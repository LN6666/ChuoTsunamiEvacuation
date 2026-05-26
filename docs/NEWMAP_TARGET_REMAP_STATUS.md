# NewMap Target Remap Status

Target remap output:

`Assets/Data/P10/newmap_target_remap_status.json`

Summary:

- Active runtime targets: 4 local non-official training proxies.
- Old P3/P4 sample shelter targets: disabled.
- Old P5 qualified shelter/candidate targets: disabled.
- Old P5 route targets: disabled.

Disable reason:

The reset `Chuo_BaseMap` scene exposes UUID-style map object names and no verified mapping to the old P5 `13102-bldg-*` identifiers. Route geometry remains WGS84/prototype without a verified Unity/PLATEAU transform. Spawning those targets as active gameplay would be a false completion claim.

Active local proxies are used only for mechanics validation and are explicitly non-official. The fourth active proxy is a crowd-delay training target added for P6/P9 hardening; it is not an official shelter or route.
