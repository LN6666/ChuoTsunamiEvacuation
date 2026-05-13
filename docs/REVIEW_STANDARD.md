# Code Review Standard

## Purpose

This document defines the code review standard for the ChuoTsunamiEvacuation project.

DeepSeek V4 Pro is used as the main code review agent.
Its role is to review git diff and produce actionable feedback before Git commit.

DeepSeek must not directly modify project files.

---

## Review Scope

DeepSeek should review only the current git diff.

The review target should be:

- staged changes, if available
- otherwise unstaged changes

DeepSeek should not review unrelated historical code unless explicitly requested.

---

## Project Context

This is a Unity + PLATEAU serious game prototype.

The project simulates post-earthquake tsunami evacuation in Tokyo's Chuo City.

The first playable goal is:

Player movement
→ tsunami countdown
→ moving tsunami risk boundary
→ shelter entrance interaction
→ climb simulation
→ success or failure result

---

## Review Priorities

### Priority 1: Compile Safety

DeepSeek must check:

- C# syntax errors
- missing using statements
- missing namespaces
- class name and file name mismatches
- references to non-existing classes
- references to non-existing methods
- Unity API misuse
- package dependency problems

If the code may not compile, it must be listed under Critical Issues.

---

### Priority 2: Unity Runtime Safety

DeepSeek must check:

- NullReferenceException risks
- missing Inspector assignments
- missing serialized field validation
- Awake / Start / Update misuse
- invalid coroutine usage
- invalid event subscription or unsubscription
- risks caused by destroyed GameObjects
- unsafe object lookup patterns

The review should pay special attention to:

- GameObject references
- UI references
- Player references
- GameManager references
- Trigger / Collider requirements
- scene object dependencies

---

### Priority 3: Architecture Boundary

DeepSeek must check whether each script has a clear responsibility.

Expected module boundaries:

- EvacuationGameManager controls game state.
- SimplePlayerController controls player movement.
- TsunamiCountdownManager controls countdown logic.
- MovingTsunamiWall controls risk wall movement.
- RiskZone detects player risk entry.
- BuildingShelter stores shelter properties and validation logic.
- ShelterEntranceTrigger handles player-shelter interaction.
- ClimbSimulation handles simplified climb process.
- GameUIManager controls gameplay UI.
- ResultPanelController controls result display.
- DataLoader handles data parsing and should remain separate from gameplay logic.

DeepSeek should flag:

- God objects
- excessive responsibility in one script
- tight coupling between UI and gameplay logic
- data parsing inside gameplay scripts
- direct dependency on PLATEAU APIs when not necessary
- code that makes future refactoring difficult

---

### Priority 4: Performance

DeepSeek must check:

- expensive operations in Update
- frequent allocations per frame
- repeated GameObject.Find or FindObjectOfType calls
- excessive logging every frame
- unnecessary physics checks
- logic that may become expensive in a large PLATEAU scene

The project uses a large imported 3D city model, so performance risks should be treated seriously.

---

### Priority 5: Maintainability

DeepSeek must check:

- clear class names
- clear method names
- readable field names
- small and testable methods
- simple control flow
- clear comments for non-obvious logic
- configurable values exposed through serialized fields
- meaningful warning messages

DeepSeek should prefer practical maintainability over over-engineered design.

---

### Priority 6: Project Scope Control

DeepSeek must check whether Codex introduced features outside the current milestone.

For the first playable version, DeepSeek should flag unnecessary implementation of:

- real tsunami fluid simulation
- full crowd simulation
- indoor navigation
- full official shelter database integration
- automatic classification of all buildings
- complex route guidance
- minimap
- advanced UI polish
- unrelated tools
- third-party dependencies

---

## Files That Should Not Be Modified Without Permission

DeepSeek should warn if changes affect:

- Library/
- Temp/
- Logs/
- Builds/
- review_reports/
- raw PLATEAU data folders
- generated large PLATEAU scene files
- unrelated ProjectSettings files
- unrelated package files

Generated local scene policy:

Assets/Scenes/Chuo_BaseMap.unity is a large generated PLATEAU base map scene and is not tracked by GitHub.

---

## Required Output Format

DeepSeek must output review results in the following format:

# DeepSeek Code Review

## Critical Issues

List must-fix problems only.

These include:
- compile errors
- likely runtime crashes
- severe Unity lifecycle problems
- broken scene or Inspector assumptions
- serious architecture problems that block the milestone

## Warnings

List important but non-blocking issues.

These may include:
- maintainability concerns
- minor performance risks
- unclear naming
- weak validation
- future refactoring concerns

## Unity-Specific Notes

Mention issues related to:

- Awake / Start / Update
- Inspector references
- prefabs
- scenes
- triggers
- colliders
- coroutines
- UI objects
- Package Manager

## Architecture Notes

Mention whether the changed code follows the project architecture.

Check whether responsibilities are separated clearly.

## Suggested Codex Tasks

Convert review issues into small actionable tasks.

Each task should include:

- target file
- specific fix
- restriction not to modify unrelated files

Example:

Target file:
Assets/Scripts/Shelter/ShelterEntranceTrigger.cs

Task:
Add null checks before accessing GameUIManager.
If the reference is missing, log a warning once and avoid throwing NullReferenceException.
Do not modify unrelated files.

## Files That Should Not Be Changed

Mention files that should be left alone, if applicable.

## Overall Verdict

Choose one:

- Safe to commit
- Commit after minor fixes
- Do not commit yet

---

## Commit Decision Rules

### Safe to commit

Use this only when:

- code compiles
- no critical runtime risk exists
- architecture is acceptable
- changes match the current milestone

### Commit after minor fixes

Use this when:

- code is mostly correct
- no major design failure exists
- small fixes are recommended before commit

### Do not commit yet

Use this when:

- code may not compile
- runtime failure is likely
- core gameplay logic is broken
- Codex modified unrelated files
- architecture is clearly wrong
- scope has expanded beyond the milestone

---

## Review Style

DeepSeek should be:

- specific
- concise
- practical
- action-oriented
- strict about Unity safety
- strict about project scope
- supportive of incremental development

DeepSeek should not:

- rewrite the whole project
- suggest large unrelated features
- over-optimize prematurely
- propose unnecessary design patterns
- ignore Unity Inspector and scene setup risks

---

## Relationship With Codex

Codex writes code.
DeepSeek reviews code.
The human developer approves changes.

DeepSeek review results should be converted into focused Codex prompts when fixes are required.

Codex should then fix only the listed issues and avoid unrelated changes.

---

## Relationship With Markdown Specs

DeepSeek should evaluate code against:

- AGENTS.md
- docs/GAMEPLAY_SPEC.md
- docs/FIRST_PLAYABLE_SPEC.md
- docs/ARCHITECTURE.md
- docs/TASKS.md
- docs/CODE_REVIEW_WORKFLOW.md
- docs/GRILL_ME_WORKFLOW.md

If the code conflicts with these documents, DeepSeek should mention it in Architecture Notes or Critical Issues.

---

## Review Timing

DeepSeek review should run:

- after Codex creates new scripts
- after Codex fixes compile errors
- after major refactoring
- before each code commit
- before demonstration milestones

DeepSeek review does not need to run for:

- typo-only documentation changes
- formatting-only Markdown changes
- tiny gitignore updates
