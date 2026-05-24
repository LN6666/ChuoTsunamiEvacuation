# P10-B+ Weather And Stamina Rules

Weather modes are scenario-configurable:

| Mode | Movement Multiplier | Visual |
|---|---:|---|
| `clear_day` | `1.00` | normal |
| `rainy_day` | `0.75` | rain is a lightweight gameplay modifier / placeholder visual |
| `night_clear` | `0.85` | night overlay enabled |
| `night_rain` | `0.65` | night overlay enabled, rain placeholder |

Movement targets:

- normal walking speed: about `0.5 m/s`
- sprint speed: configurable, default `2.5 m/s`
- sprint target remains within `2.0-3.0 m/s`

Stamina:

- stamina appears when sprinting or recovering
- sprint drain is staged/nonlinear
- zero stamina locks sprint for 15 seconds
- after 15 seconds, stamina recovers to at least 30 percent
- after 45 seconds stopped, stamina recovers to at least 50 percent
- after 90 seconds stopped, stamina recovers to 100 percent

The system is deterministic and testable. Existing movement behavior remains unchanged unless the P10-B+ movement adapter is attached.
