# DeepSeek Review Prompt: NewMap Non-Official Name Fix

Review the current git diff for the NewMap non-official candidate name enrichment pass.

Verify:
- Missing non-official candidate names were audited.
- Coordinate-based preprocessing enrichment was actually run or honestly reported unavailable.
- `Assets/Data/P10/newmap_name_cache.json` was updated.
- Runtime loads the local cache only.
- Runtime scripts do not perform web requests.
- Japanese/Kanji main-name normalization is enforced.
- No fabricated names, full addresses, GML/building IDs, or coordinate strings are visible in normal gameplay.
- Non-official warning semantics are preserved and candidates are not promoted to official shelters.
- No final release/archive was created.
- No P10-E/F/G artifacts were created.
- No A-level blockers remain.
