# DeepSeek Review Prompt: P8-B Chuo Tsunami Spatial Extraction Gate

Review the current git diff for P8-B spatial extraction gate hardening.

Context:

- P8 has exactly four stages: P8-A, P8-B, P8-C, P8-D.
- The task must not proceed to P8-C unless P8-B creates an evidence-backed Chuo tsunami spatial layer or explicitly marks P8-C blocked.
- Tokyo Metropolitan Government tsunami evidence must be prioritized. Chuo standalone tsunami-map absence is not evidence absence.
- The implementation found and used official Tokyo Open Data tsunami CSVs plus official MLIT N03 Chuo boundary data.

Check for A-level blockers:

- P8 stage count remains exactly four: P8-A, P8-B, P8-C, P8-D. No P8-E/F/G.
- No scene mutation, especially no reset/loss/staging of `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- `Assets/Scenes/Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, and `Assets/PLATEAU` are untouched.
- No P8-C infrastructure hazard interaction implementation yet.
- No P8-D collapse proxy gameplay implementation yet.
- No P9/P10 systems.
- Tokyo metropolitan tsunami source is prioritized.
- Chuo standalone tsunami-map absence is not treated as evidence absence.
- `maxTsunamiHeightMeters` is not confused with an `inundationDepthMeters` grid.
- No false claim of full real-time fluid simulation or academic hydrodynamic modeling.
- No false claim that a derived bbox is an official inundation contour.
- `sourceMode=official_tsunami_metropolitan` is used only with evidence source metadata and `extractionStatus=extracted`.
- P8-C gate is `PASS` only because extracted spatial mesh records exist; otherwise the code/docs would require `BLOCKED` or explicit conditional user override.
- Risk-front config/status reports official metropolitan source identified, spatial extraction status, and `official_spatial` driver.
- Cinematic `visualHeightMeters` remains separate from physical tsunami height/depth/water-level fields.
- Tests and preflights pass.

Expected artifacts:

- `tools/p8/extract_p8b_chuo_tsunami_layer.py`
- `tools/p8/run_p8b_evidence_spatial_gate.ps1`
- `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json`
- `Assets/Data/P8/tsunami_hazard_evidence_registry.json`
- `Assets/Data/P8/risk_front_visualization_config.json`
- `docs/P8B_TOKYO_TSUNAMI_SPATIAL_EXTRACTION.md`
- `docs/P8B_CHUO_INUNDATION_DEPTH_LAYER_CONSTRUCTION.md`
- `docs/P8B_OFFICIAL_TSUNAMI_SOURCE_DECISION.md`
- `docs/P8B_TO_P8C_GATE_DECISION.md`
- updated EditMode/PlayMode tests for extracted spatial provenance and gate behavior.

Return:

- PASS/FAIL verdict.
- A-level blockers, if any.
- B/C-level follow-ups only if they do not block this P8-B spatial extraction gate.
