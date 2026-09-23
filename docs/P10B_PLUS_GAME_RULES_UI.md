# P10-B+ Game Rules UI

The rules panel is runtime-ready and uses localized text. Long rules text is placed inside a Unity `ScrollRect` and configured for wrapping.

Rules can be opened from the start menu and pause menu. The exported Markdown versions are:

- `docs/GAME_RULES_EN.md`
- `docs/GAME_RULES_JA.md`

The rules explicitly state:

- tsunami is represented by a risk front, not a fluid simulation
- official shelters and non-official humanitarian candidates are different
- green ground frames are evacuation-related markers, not official approval
- P5 routes are estimated prototype guidance, not official evacuation routes
- vertical evacuation is a proxy flow, not a real indoor scene
- collapse/debris fatality is an exposure-event gameplay proxy, not a structural or mortality model
