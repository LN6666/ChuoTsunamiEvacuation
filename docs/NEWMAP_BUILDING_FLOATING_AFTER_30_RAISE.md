# NewMap Building Floating After 30 Percent Raise

Building geometry is not moved in this pass. Floating is reduced by raising the gameplay ground/ground cover/support from the previous effective `3.3m` level to `4.29m`.

Measured fields:
- Sampled building count.
- Average/max floating gap before the 30 percent pass.
- Average/max floating gap after the 30 percent pass.
- Improved, still-floating, and embedding-risk counts where available.

The report intentionally does not claim a complete building-floating fix. Remaining outlier clusters require manual visual review because this is a gameplay cover/support adjustment, not GIS-grade terrain reconstruction.

Latest runtime retest:
- Sampled buildings: `10787`
- Average gap before/after: `0.32m` -> `0.06m`
- Max gap before/after: `2.82m` -> `1.83m`
- Remaining floating/outlier count source: `59` runtime ground-raise outliers
- Embedded-risk count: `0`
