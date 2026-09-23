# P10-B+ UI Localization Polish

P10-B+ is a polish/fix sprint after P10-B and before P10-C. It is not a new official stage, and it does not build the final Windows EXE.

The localization layer supports English and Japanese through simple JSON tables:

- `Assets/Data/P10/p10b_plus_localization_en.json`
- `Assets/Data/P10/p10b_plus_localization_ja.json`

The runtime service falls back to English when the active language is missing a key. Missing English keys return a visible bracketed key instead of throwing an exception.

All new P10-B+ UI text uses localization keys. Existing P2-P9 UI remains untouched except for an optional movement-speed provider hook in the player controller.
