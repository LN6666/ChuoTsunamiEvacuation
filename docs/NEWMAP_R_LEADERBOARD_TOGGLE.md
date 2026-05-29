# NewMap R Leaderboard Toggle

Config: `Assets/Data/P10/newmap_leaderboard_toggle_config.json`

- Enabled: true
- Toggle key: `R`
- Press `R`: show shelter ranking panel.
- Press `R` again: hide shelter ranking panel.
- Auto-refresh while visible: yes, using the shelter direct-line ranking interval.
- Ignored while start menu, pause, rules, or result UI is active.

The panel uses the existing mixed shelter/candidate distance ranking. Official and non-official targets are not separated. The nearest target remains the same target whose direct line is red. Wording stays as estimated prototype guidance and does not claim official evacuation routes.

Player smoke result:
- `r_leaderboard_toggle_show_hide`: passed
- `shelter_direct_lines_created`: passed with 95 rankable targets and 0 line colliders
