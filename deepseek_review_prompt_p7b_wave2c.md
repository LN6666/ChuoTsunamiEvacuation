# DeepSeek Review Prompt: P7-B Wave 2-C 53393690 Sandbox Import

Review the current git diff for PBL7 / P7-B Wave 2-C.

Task:

Full `53393690` LOD3 candidate sandbox import.

## Human Approval Boundary

The human developer explicitly approved importing the full candidate package `53393690` because 605 MB is acceptable.

This approval applies only to candidate `53393690` inside:

`Assets/P7Benchmark/Imported/53393690/`

It does not approve full Chuo import, fallback candidate import, `Assets/PLATEAU` changes, `Assets/Data` changes, `Chuo_BaseMap.unity` changes, production scene integration, `ProjectSettings` changes, `Packages` changes, or P8/P9 systems.

## Required Checks

Confirm:

- only candidate `53393690` was imported,
- import target is only `Assets/P7Benchmark/Imported/53393690/`,
- no fallback candidate package was imported,
- no `Assets/PLATEAU` changes exist,
- no `Assets/Data` changes exist,
- no `Chuo_BaseMap.unity` changes exist,
- no `ProjectSettings` or `Packages` changes exist,
- no production scene integration occurred,
- no existing gameplay scripts changed,
- no full Chuo import occurred,
- P7 remains exactly five stages: P7-0, P7-A, P7-B, P7-C, P7-D,
- no P7-E, P7-F, or P7-G stages were created,
- no P8/P9 systems were implemented,
- Unity EditMode and PlayMode GUI tests passed,
- visual quality claims remain conservative,
- LOD3 visual and geometry quality remain unverified unless actual renderable proof exists,
- LOD4 is not assumed available,
- production integration remains future gated work.

## Tooling Review

Review:

- `tools/p7/import_p7b_53393690_sandbox.ps1`
- `tools/p7/run_p7b_wave2c_preflight.ps1`
- `tools/p7/check_p7_scope.ps1`

Confirm the import script:

- locates only candidate `53393690`,
- preserves source directory structure under the sandbox target,
- refuses non-`53393690` candidates,
- refuses unsafe target paths,
- logs file count and bytes,
- writes `docs/P7B_WAVE2C_IMPORT_LOG.md`,
- does not touch protected production paths.

Confirm the preflight:

- runs strict Wave 2-C scope guard mode,
- verifies imported files are only under the approved sandbox path,
- verifies protected paths are clean,
- verifies Git LFS tracking for large imported files,
- exits non-zero on failure.

## Validation Evidence

Expected commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2c_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Classify findings:

- A-level: must fix before commit/push.
- B-level: follow-up allowed after commit if no safety issue.
- C-level: note or polish only.

Return an overall verdict and state whether commit/push is safe.
