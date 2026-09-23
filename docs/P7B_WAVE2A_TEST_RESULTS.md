# P7-B Wave 2-A Test Results

Date: 2026-05-22

Stage: P7-B Wave 2-A

Status: Passed.

## Required Commands

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2a_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

## Results

| Check | Result | Notes |
|---|---|---|
| Wave 2-A preflight | PASS | Scene validation, Wave 2-A scope guard, and protected path check passed. |
| Unity EditMode GUI tests | PASS | 146 total / 146 passed / 0 failed / 0 skipped / 0 inconclusive. |
| Unity PlayMode GUI tests | PASS | 29 total / 29 passed / 0 failed / 0 skipped / 0 inconclusive. |
| Protected path check | PASS | `ProjectSettings`, `Packages`, `Assets/Data`, `Assets/PLATEAU`, and `Chuo_BaseMap` are untouched after reverting Unity-generated settings churn. |

## Scene Creation Note

Initial batchmode scene creation failed because Unity Licensing Client failures caused Package Manager to register 0 packages, which made existing UI and Physics references unavailable during compilation. The scene was then created with the GUI launch path:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/create_p7b_benchmark_scene.ps1 -LaunchMode Gui
```

Unity generated temporary `ProjectSettings` churn during scene creation and test runs. Those changes were reverted. The final protected-path check is clean.

## Initial Scope Confirmation

Wave 2-A is limited to the isolated `P7Benchmark` scene skeleton and metrics harness. It does not import real assets, modify `Chuo_BaseMap`, change gameplay rules, or complete EXE profiling.
