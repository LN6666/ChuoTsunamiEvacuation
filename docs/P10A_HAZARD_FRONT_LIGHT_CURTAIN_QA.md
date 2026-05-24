# P10-A Hazard Front And Light Curtain QA

P10-A verifies that P9 consumes P8 hazard/front handoff conservatively and does not reimplement P8.

Checks:

- `arrivalTimeSeconds` and hazard timing remain gameplay inputs through P8/P9 handoff adapters.
- Inundation depth or hazard intensity status is not recalculated by P10-A.
- Risk front and light curtain remain visual/cinematic coordination elements.
- `visualHeightMeters` must not be described as physical tsunami height.
- Gameplay outcomes use hazard timing and hazard status conservatively.

Visual QA checklist for P10-B/P10-D:

- light curtain position relative to waterfront and inundation area
- sweep direction
- coverage of intended risk area
- UI obstruction
- frame-time impact
- consistency of risk warning copy with the current scenario

Known limitation:

- Final visual QA and performance impact measurement belong to P10-B/P10-D.
