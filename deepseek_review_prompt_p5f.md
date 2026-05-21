# DeepSeek Review Prompt: P5-F High-Rise Humanitarian Candidate Screening 1.0

Review the current git diff for the Unity project `ChuoTsunamiEvacuation`.

Focus milestone:

P5-F - High-rise Humanitarian Vertical Evacuation Candidate Screening 1.0

## Context

This is a Unity + PLATEAU serious game prototype for tsunami evacuation behavior in Tokyo Chuo City.

P5-F is data/schema/rulebook/planning only. It should create a conservative foundation for identifying high-rise buildings that may serve as humanitarian emergency vertical evacuation candidates during tsunami risk.

This is a life-first emergency access concept, but it must not confuse non-official buildings with official/designated evacuation shelters.

## Expected P5-F Outputs

- `docs/P5F_HIGHRISE_HUMANITARIAN_CANDIDATES.md`
- `data_pipeline/qualification/highrise_humanitarian_candidate_rulebook.json`
- `data_pipeline/qualification/highrise_humanitarian_candidate_schema.json`
- `data_pipeline/qualification/highrise_humanitarian_candidate_sources_plan.json`
- `data_pipeline/qualification/highrise_humanitarian_candidates_sample.json`
- `data_pipeline/tests/test_highrise_humanitarian_candidates.py`

## Required Status Taxonomy

Review whether these statuses are clear and safely separated:

- `official_confirmed`
- `official_confirmed_with_review`
- `humanitarian_strong_candidate`
- `humanitarian_candidate_with_review`
- `humanitarian_weak_candidate`
- `unknown`
- `not_recommended`

## Hard Safety Boundaries

Flag any violation as A-level/blocking:

- Do not label non-official high-rise buildings as official shelters.
- Do not modify Unity gameplay in P5-F.
- Do not modify `Assets/Scenes/Chuo_BaseMap.unity`.
- Do not modify PLATEAU imported files or raw PLATEAU data.
- Do not modify `Assets/Data`.
- Do not modify `ProjectSettings`.
- Do not modify `Packages`.
- Do not change the default `sourceMode`.
- Do not download large raw GIS files.
- Do not aggressively scrape websites.
- Do not commit raw/cache/download/tmp/.venv/large data.
- Do not implement tsunami fluid simulation, full crowd simulation, or new gameplay rules.

## Review Focus

Please review for:

- Whether official/designated facilities and humanitarian emergency candidates are clearly separate layers.
- Whether non-official candidate records can accidentally be interpreted as official shelters.
- Whether warning/manual-review rules are strong enough for life-first but uncertain emergency access.
- Whether `publicAccessStatus = unknown` is handled correctly: not a hard exclusion, but a manual-review and warning trigger.
- Whether seismic evidence, public access, and management agreement uncertainty are preserved.
- Whether schema fields are adequate for future real Chuo high-rise screening.
- Whether the scoring/classification method is conservative and avoids false certainty.
- Whether the source plan avoids large downloads, scraping, and unsafe runtime data use.
- Whether tests cover official/humanitarian separation, schema validation, warning policy, and unknown access behavior.
- Whether documentation accurately states limitations and non-claims.

## Validation Context

System Python in the current environment did not have `pytest` or `jsonschema` installed. JSON syntax checks passed for the new P5-F JSON artifacts. Focused pytest and JSON Schema validation should be run in the project-approved Python environment when dependencies are available.

## Expected Output

Return:

1. Overall verdict: PASS, PASS WITH B-LEVEL FOLLOW-UPS, or BLOCKED.
2. A-level blockers, if any, with file/line references.
3. B-level follow-ups, if any, with concise rationale.
4. Small Codex fix tasks, if any.
5. Confirmation whether P5-F is stable enough to commit after any required fixes and tests.
