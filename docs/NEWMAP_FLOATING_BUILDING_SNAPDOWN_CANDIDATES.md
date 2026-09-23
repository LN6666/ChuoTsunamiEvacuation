# NewMap Floating Building Snapdown Candidates

- Reference surface: accepted gameplay ground cover at Y=0.
- Candidate rule: runtime scans enabled building-like renderers, groups them by safe `bldg`/building root, and excludes roads, ground cover, support colliders, water, air walls, markers, UI, player, and NPCs.
- Floating threshold: 0.5m above ground cover.
- Extreme offset guard: candidates above 8.0m are skipped until manually classified safe.
- Runtime player does not write object-level scene files; aggregate candidate counts are written to Player.log and parsed into P10 reports.

This is gameplay visual alignment only. It is not a PLATEAU elevation correction.
