# NewMap Building Floating After Ground Raise

Building floating is now treated as a visual alignment problem against the accepted gameplay ground cover, not an official PLATEAU correction.

Policy:
- Do not move imported buildings in this pass.
- Raise the gameplay cover/support toward sampled building bases.
- Report any remaining floating or embedded buildings honestly.
- Do not re-enable the failed adaptive DEM/relief support grid.

If major outlier clusters remain after the ground raise, the next safe option is local patching or manual exclusion, not global random building movement.

Validated player-smoke summary:
- building-like base samples checked: 10,787
- ground-cover raise offset: 3m
- remaining average positive gap: 0.45m
- remaining max sampled positive gap: 3.12m
- imported buildings moved: false

This means the broad visible gap is reduced, but a manual visual check is still required for outlier buildings.
