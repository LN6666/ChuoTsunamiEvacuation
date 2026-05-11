import os
import subprocess
from pathlib import Path
from datetime import datetime
from openai import OpenAI


PROJECT_ROOT = Path(__file__).resolve().parents[1]
REPORT_DIR = PROJECT_ROOT / "review_reports"


def run_command(command: list[str]) -> str:
    result = subprocess.run(
        command,
        cwd=PROJECT_ROOT,
        text=True,
        capture_output=True,
        encoding="utf-8",
        errors="replace",
    )

    if result.returncode != 0:
        raise RuntimeError(
            f"Command failed: {' '.join(command)}\n\nSTDOUT:\n{result.stdout}\n\nSTDERR:\n{result.stderr}"
        )

    return result.stdout


def get_git_diff() -> str:
    """
    Review staged changes first.
    If there are no staged changes, review unstaged changes.
    """
    staged_diff = run_command(["git", "diff", "--cached"])
    if staged_diff.strip():
        return staged_diff

    unstaged_diff = run_command(["git", "diff"])
    if unstaged_diff.strip():
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
        base_url="https://api.deepseek.com",
    )

    response = client.chat.completions.create(
        model="deepseek-v4-pro",
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
        temperature=0.2,
    )

    return response.choices[0].message.content or ""


def save_review_report(review_text: str) -> Path:
    REPORT_DIR.mkdir(exist_ok=True)

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    report_path = REPORT_DIR / f"deepseek_review_{timestamp}.md"

    report_path.write_text(review_text, encoding="utf-8")

    return report_path


def main() -> None:
    diff_text = get_git_diff()

    if not diff_text.strip():
        print("No staged or unstaged git diff found. Nothing to review.")
        print("Tip: Make some code changes first, or run git add <file> to stage changes.")
        return

    prompt = build_review_prompt(diff_text)
    review_text = call_deepseek(prompt)
    report_path = save_review_report(review_text)

    print(review_text)
    print()
    print(f"Review saved to: {report_path}")


if __name__ == "__main__":
    main()