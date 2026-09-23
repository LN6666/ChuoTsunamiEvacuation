# P10-B+ Start And Pause Menu

P10-B+ adds scene-safe runtime-ready menu components:

- title text
- language selector support
- Start Game label
- Rules / How to Play label
- Options label
- Quit label
- Pause / Resume labels
- Return to Title label
- Force Quit label and explanation

`Esc` toggles the pause overlay when the P10-B+ menu runtime is active. The pause runtime uses `Time.timeScale = 0` while paused and restores it to `1` on resume.

The menu is built at runtime and does not mutate the high-detail scene. Manual playtest should confirm that the overlay does not obscure critical gameplay or ResultPanel content.
