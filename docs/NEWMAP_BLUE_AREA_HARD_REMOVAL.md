# NewMap Blue Area Hard Removal

Normal gameplay must not show blue support/debug ground.

Runtime hard-removal now disables:
- support/grid/debug ground renderers,
- known support proxy renderers,
- large blue ground-like plane renderers.

Current expected status:
- Normal-mode visible blue support count: `0`.
- Support grid renderer active: `False`.
- Known blue debug object active: `False`.

Debug visualization must remain opt-in only.
