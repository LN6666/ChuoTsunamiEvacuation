# P7-B Wave 2-C Test Results

Date: 2026-05-23

## Required Commands

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2c_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

## Current Results

Wave 2-C preflight: PASS.

EditMode GUI tests: PASS.

```text
total=146 passed=146 failed=0 skipped=0 inconclusive=0
```

PlayMode GUI tests: PASS.

```text
total=29 passed=29 failed=0 skipped=0 inconclusive=0
```

## Unity Churn Handling

Unity generated temporary `ProjectSettings` churn during GUI validation:

- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/ShaderGraphSettings.asset`
- `ProjectSettings/ProjectSettings.asset`

This churn was reverted and must not be staged.

Unity generated `.meta` files for the approved sandbox import path under `Assets/P7Benchmark/Imported/53393690/`. Those files are expected Unity import metadata for the sandbox package.

## Expected Scope Checks

- `Chuo_BaseMap.unity` untouched.
- `ProjectSettings` clean.
- `Packages` clean.
- `Assets/PLATEAU` unchanged.
- `Assets/Data` unchanged.
- Existing gameplay scripts unchanged.
- Production scenes unchanged.
- Imported files only under `Assets/P7Benchmark/Imported/53393690/`.

## Unity Test Rationale

Unity GUI EditMode and PlayMode tests are required for Wave 2-C because files under `Assets/` changed.
