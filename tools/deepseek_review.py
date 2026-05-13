import logging
import os
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path
from typing import Sequence

from openai import (
    APIConnectionError,
    APIStatusError,
    APITimeoutError,
    AuthenticationError,
    OpenAI,
    RateLimitError,
)


PROJECT_ROOT = Path(__file__).resolve().parents[1]
REPORT_DIR = PROJECT_ROOT / "review_reports"
LOG_DIR = PROJECT_ROOT / "logs"

MODEL_NAME = os.environ.get("DEEPSEEK_REVIEW_MODEL", "deepseek-v4-pro")
TEMPERATURE = float(os.environ.get("DEEPSEEK_REVIEW_TEMPERATURE", "0.2"))
REASONING_EFFORT = os.environ.get("DEEPSEEK_REVIEW_REASONING_EFFORT", "max")
THINKING_TYPE = os.environ.get("DEEPSEEK_REVIEW_THINKING", "enabled")
BASE_URL = os.environ.get("DEEPSEEK_API_BASE_URL", "https://api.deepseek.com")
MAX_RETRIES = int(os.environ.get("DEEPSEEK_REVIEW_MAX_RETRIES", "2"))


def setup_logging() -> None:
    LOG_DIR.mkdir(exist_ok=True)
    log_path = LOG_DIR / "deepseek_review.log"

    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[
            logging.FileHandler(log_path, encoding="utf-8"),
            logging.StreamHandler(sys.stdout),
        ],
    )


def run_command(command: Sequence[str]) -> str:
    result = subprocess.run(
        list(command),
        cwd=PROJECT_ROOT,
        text=True,
        capture_output=True,
        encoding="utf-8",
        errors="replace",
    )

    if result.returncode != 0:
        stderr_preview = (result.stderr or "").strip()[:1000]
        stdout_preview = (result.stdout or "").strip()[:1000]

        raise RuntimeError(
            "Command failed: "
            + " ".join(command)
            + "\nSTDOUT preview:\n"
            + stdout_preview
            + "\nSTDERR preview:\n"
            + stderr_preview
        )

    return result.stdout


def ensure_git_repository() -> None:
    git_dir = PROJECT_ROOT / ".git"
    if not git_dir.exists():
        raise RuntimeError(
            f"This script must be run inside a Git project. "
            f"Expected .git directory at: {git_dir}"
        )


def get_git_diff() -> str:
    staged_diff = run_command(["git", "diff", "--cached"])
    if staged_diff.strip():
        logging.info("Review target: staged git diff.")
        return staged_diff

    unstaged_diff = run_command(["git", "diff"])
    if unstaged_diff.strip():
        logging.info("Review target: unstaged git diff.")
        return unstaged_diff

    return ""


def build_review_prompt(diff_text: str) -> str:
    return f"""
You are a senior Unity C# code reviewer.

Project:
Chuo Tsunami Evacuation Game.

Project context:
This is a Unity + PLATEAU project.
The game simulates post-earthquake tsunami evacuation in Tokyo's Chuo City.
The intended gameplay includes:
- player movement
- tsunami risk boundary / moving risk wall
- risk zones
- shelter building interaction
- climb simulation
- result panels
- CSV / JSON / GeoJSON data loading
- possible Unity Editor utility scripts

Task:
Review the following git diff only.
Do not rewrite the whole project.
Do not propose unrelated features.
Do not modify files.
Focus on whether the changed code is safe, maintainable, and likely to compile in Unity.

Please check:
1. Compile errors
2. NullReferenceException risks
3. Unity lifecycle issues, such as Awake / Start / Update misuse
4. Inspector assignment risks
5. Scene / prefab reference risks
6. Performance risks, especially in Update or frequent allocations
7. Data loading risks for CSV / JSON / GeoJSON
8. Over-engineering or unnecessary complexity
9. Naming, responsibility boundaries, and maintainability
10. Concrete Codex tasks to fix the issues

Output format:

# DeepSeek Code Review

## Critical Issues
List must-fix problems only.

## Warnings
List non-fatal but important issues.

## Unity-Specific Notes
Mention Unity lifecycle, Inspector, prefab, scene, package, or editor concerns.

## Suggested Codex Tasks
Write small, actionable prompts that can be given to Codex.
Each task should specify target files and explicitly say not to modify unrelated files.

## Files That Should Not Be Changed
Mention files that should be left alone, if applicable.

## Overall Verdict
Choose one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet

Git diff to review:

----- DIFF START -----
{diff_text}
----- DIFF END -----
"""


def call_deepseek(prompt: str) -> str:
    api_key = os.environ.get("DEEPSEEK_API_KEY")

    if not api_key:
        raise EnvironmentError(
            'DEEPSEEK_API_KEY is not set. Run this in PowerShell:\n'
            'setx DEEPSEEK_API_KEY "your_api_key_here"\n'
            "Then close and reopen PowerShell."
        )

    client = OpenAI(
        api_key=api_key,
        base_url=BASE_URL,
    )

    last_error: Exception | None = None

    for attempt in range(1, MAX_RETRIES + 2):
        try:
            logging.info(
                "Calling DeepSeek review model. model=%s reasoning_effort=%s thinking=%s attempt=%s",
                MODEL_NAME,
                REASONING_EFFORT,
                THINKING_TYPE,
                attempt,
            )

            response = client.chat.completions.create(
                model=MODEL_NAME,
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "You are a careful Unity C# code reviewer. "
                            "Be concise, specific, and practical. "
                            "Do not rewrite the entire project."
                        ),
                    },
                    {
                        "role": "user",
                        "content": prompt,
                    },
                ],
                temperature=TEMPERATURE,
                reasoning_effort=REASONING_EFFORT,
                extra_body={"thinking": {"type": THINKING_TYPE}},
            )

            return response.choices[0].message.content or ""

        except (APIConnectionError, APITimeoutError, RateLimitError) as exc:
            last_error = exc
            logging.warning(
                "Transient DeepSeek API error on attempt %s: %s",
                attempt,
                type(exc).__name__,
            )

            if attempt <= MAX_RETRIES:
                time.sleep(2 * attempt)
                continue

            raise RuntimeError(
                f"DeepSeek API request failed after retries: {type(exc).__name__}"
            ) from exc

        except AuthenticationError as exc:
            raise RuntimeError(
                "DeepSeek authentication failed. "
                "Please check DEEPSEEK_API_KEY."
            ) from exc

        except APIStatusError as exc:
            status_code = getattr(exc, "status_code", "unknown")
            raise RuntimeError(
                f"DeepSeek API returned an error status: {status_code}"
            ) from exc

    raise RuntimeError("DeepSeek API request failed.") from last_error


def save_review_report(review_text: str) -> Path:
    REPORT_DIR.mkdir(exist_ok=True)

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    report_path = REPORT_DIR / f"deepseek_review_{timestamp}.md"

    report_path.write_text(review_text, encoding="utf-8")

    return report_path


def main() -> int:
    setup_logging()

    try:
        ensure_git_repository()

        diff_text = get_git_diff()

        if not diff_text.strip():
            logging.info("No staged or unstaged git diff found. Nothing to review.")
            print("No staged or unstaged git diff found. Nothing to review.")
            print("Tip: Make code changes first, or run git add <file> to stage changes.")
            return 0

        prompt = build_review_prompt(diff_text)
        review_text = call_deepseek(prompt)
        report_path = save_review_report(review_text)

        print(review_text)
        print()
        print(f"Review saved to: {report_path}")

        logging.info("Review saved to: %s", report_path)
        return 0

    except Exception as exc:
        logging.exception("DeepSeek review failed.")
        print()
        print("DeepSeek review failed.")
        print(f"Reason: {exc}")
        print()
        print("Detailed error log:")
        print(LOG_DIR / "deepseek_review.log")
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
