# P10-B++ Anti-Aliasing And Quality Audit

## Current Readable State

Read-only audit found:

- `ProjectSettings/QualitySettings.asset` contains `antiAliasing: 0` for listed quality entries.
- `ProjectSettings/QualitySettings.asset` contains `vSyncCount: 0` for listed quality entries.
- URP assets exist under `Assets/Settings/`.
- `Assets/Settings/PC_RPAsset.asset` and `Assets/Settings/Mobile_RPAsset.asset` contain `m_MSAA: 1`.
- Scene cameras found in the checked scenes contain `m_AllowMSAA: 1`.

P10-B++ does not change ProjectSettings or render pipeline assets.

## Honest AA Status

P10-B++ does not claim that TAA, FXAA, or MSAA is active in the final player.

The readable settings are mixed: QualitySettings MSAA appears off, while URP/camera assets show MSAA allowance fields. The final visual result must be verified in P10-C built-player QA.

## Recommendations

Low:

- Keep debug labels off.
- Use minimal or no AA until FPS and memory are stable.
- Use lower NPC, marker, and green frame caps.

Medium:

- Default P10-C profiling target.
- Use moderate AA only if the built player confirms stable 1 percent low FPS and memory.
- Keep green frame and marker caps bounded.

High:

- Use only after Medium passes.
- Check visual quality, shimmer, and edge crawling against FPS, memory, and stutter.
- Do not raise render settings in P10-B++.

## P10-C Checks

- Record active quality preset.
- Record `QualitySettings.antiAliasing`, `QualitySettings.vSyncCount`, and `Application.targetFrameRate`.
- Visually inspect building edges, green frames, UI text, and light curtain shimmer.
- Compare FPS and 1 percent low with AA off/minimal and with the chosen visual setting.
