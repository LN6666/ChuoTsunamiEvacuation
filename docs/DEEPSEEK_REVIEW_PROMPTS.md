# Milestone 2-01 — Data-Driven Rules Integration Review Prompt

You are reviewing the Unity project ChuoTsunamiEvacuation.

Review target:
Milestone 2-01 — Data-Driven Rules Integration.

Context:
The First Playable Prototype was already working before this change. This milestone should integrate JSON-driven tsunami event rules, test shelter rules, anti-camping settings, and result metrics while preserving the existing gameplay loop.

Please review the changed files for:

### 1. First Playable regression safety

- Does third-person movement remain untouched?
- Does the camera remain untouched?
- Does manual T tsunami start still work?
- Does shelter entry with E still work?
- Does stair-climbing simulation still work?
- Does success/failure ResultPanel still work?

### 2. Tsunami config correctness

- Are tsunami_event_config.json values loaded correctly?
- Does countdown start only after tsunami warning/event start?
- Does manualStartEnabled work correctly?
- Does randomStartEnabled work only when enabled?
- Are randomStartMinSeconds and randomStartMaxSeconds handled safely?
- Does the game fail safely if both manual and random start are disabled?

### 3. Shelter data correctness

- Are test shelters loaded from JSON?
- Is shelterId lookup robust?
- Are missing shelter IDs handled with safe defaults and warnings?
- Do canEnter, entryDelaySeconds, climbTimeSeconds, crowdingDelaySeconds, and failureReason affect gameplay correctly?
- Does this avoid importing real Chuo Ward shelters prematurely?

### 4. Anti-camping scope control

- Is anti_camping_config.json loaded correctly?
- Is antiCampingEnabled false by default?
- Is any anti-camping behavior minimal and config-gated?
- Does it avoid inventing a complex anti-cheat/scoring system?

### 5. ResultMetrics usefulness

- Does ResultMetrics capture explainable result data?
- Is it connected to the result flow without over-engineering?
- Does the ResultPanel show useful fields without unnecessary UI redesign?

### 6. Runtime loading and fallback

- Are Application.dataPath + "/Data/..." paths acceptable for this Editor-stage prototype?
- Are missing/invalid/partial JSON files handled safely?
- Are Debug.LogWarning messages clear enough?
- Are there any Unity serialization risks?

### 7. Scope control

Confirm it does not add:
- multiplayer
- mobile support
- chat
- NPC crowds
- real street spawning
- full Chuo Ward facility import
- commercial graphics polish
- PLATEAU import changes

Please classify findings as:
- Blocking issues
- Non-blocking risks
- Suggested improvements for the next milestone

Focus only on correctness, maintainability, and scope control for Milestone 2-01.
