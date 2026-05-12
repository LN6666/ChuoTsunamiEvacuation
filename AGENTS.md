# AGENTS.md

## Project

ChuoTsunamiEvacuation is a Unity + PLATEAU serious game prototype.

The project simulates post-earthquake tsunami evacuation behavior in Tokyo's Chuo City.
Unity is used as the interaction and visualization platform.
PLATEAU CityGML is used as the 3D urban model source.

## Main Development Rule

Do not write large systems without a confirmed Markdown plan.

Before implementing any core system, check:

- docs/GAMEPLAY_SPEC.md
- docs/TASKS.md
- docs/ARCHITECTURE.md
- docs/DATA_SCHEMA.md, if available
- docs/DECISIONS.md, if available

## Agent Roles

### Codex

Codex is the main coding agent.

Codex may:
- create Unity C# scripts
- modify specified files
- fix compile errors
- create small editor utilities
- improve maintainability

Codex must not:
- modify unrelated files
- touch Library, Temp, Logs, Builds, or PLATEAU raw data
- introduce large third-party dependencies without permission
- implement real tsunami fluid simulation
- implement full crowd simulation unless explicitly requested
- rewrite the whole project without approval

### DeepSeek V4 Pro

DeepSeek is used as a code review and architecture review tool.

DeepSeek should:
- review git diff
- detect compile risks
- detect NullReferenceException risks
- check Unity lifecycle issues
- check architecture problems
- suggest small Codex tasks

DeepSeek should not:
- directly modify project files
- rewrite the whole codebase

## Coding Principles

- Keep scripts small and testable.
- Prefer serialized fields for Unity Inspector assignment.
- Add null checks for scene references.
- Avoid heavy logic in Update.
- Keep gameplay logic separate from UI logic.
- Keep PLATEAU-specific logic isolated.
- Use clear class and method names.
- Prefer simple implementation first, then refactor.

## Git Rules

- Commit after each stable milestone.
- Keep commits small and meaningful.
- Do not commit API keys.
- Do not commit raw PLATEAU data.
- Do not commit generated large Unity scenes.
- Do not commit review_reports or logs.

## Current Project Policy

The generated PLATEAU base map scene is stored locally on the cloud desktop data disk.
It is not tracked by GitHub because it is too large.

Generated scene:
Assets/Scenes/Chuo_BaseMap.unity

Local PLATEAU data:
D:\PLATEAU_DATA\Chuo_2025_CityGML
