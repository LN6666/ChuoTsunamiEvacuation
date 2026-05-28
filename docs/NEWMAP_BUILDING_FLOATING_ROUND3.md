# NewMap Building Floating Round 3

Generated: 2026-05-29T00:00:00+09:00

Manual issue: player and buildings appeared to float together, with a visible blue ground-like surface.

Fix strategy:
- Remove the visible support surface from normal gameplay.
- Align runtime support to the sampled transport/road height.
- Apply a measured runtime-only 1.97m Y correction to imported building renderer transforms when the road/building base offset is detected.
- Preserve imported PLATEAU transforms unless a consistent global offset is proven.
- Keep duplicate/debug/proxy ground visuals hidden.

No PLATEAU data was reimported and no release scene asset was rewritten for this correction.

Player smoke measured support-to-road and support-to-building-base deltas of 0.00m. Manual camera-visible confirmation is still required because runtime bounds/sampling cannot prove every building base and road surface across the full imported map.

Known limitation:
- The correction is a uniform 1.97m runtime offset from sampled transport ground to sampled building bases.
- Building detection relies on imported `bldg_`/building naming.
- If visible floating remains for unnamed meshes or local terrain variation, add a manual override list or per-building probe in a later task.
