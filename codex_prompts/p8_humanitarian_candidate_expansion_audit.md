# Codex Prompt Trace: P8 Humanitarian Candidate Expansion Audit

Task name: Expand and select non-official high-rise humanitarian candidate points after tsunami evidence wording correction.

Scope:

- Use local project files and local PLATEAU/high-detail metadata only.
- Expand `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md` beyond the original five P5 sample candidates when local evidence exists.
- Create `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`.
- Keep every candidate non-official, warning-required, manual-review-required, and data-only.
- Allocate lifecycle to P8-C/P8-D/P8-E/P9 docs without implementing P8-D or P9.
- Verify P8 has exactly five stages: P8-A through P8-E.

Strict boundaries:

- Do not claim candidates are official evacuation shelters.
- Do not mark candidates safe or approved.
- Do not change gameplay success/failure rules.
- Do not implement P8-D collapse/damage proxy.
- Do not implement P9 selectable gameplay.
- Do not modify `Chuo_BaseMap.unity`, ProjectSettings, Packages, Assets/PLATEAU, or reset `P7_HighDetail_Chuo.unity`.
- Do not download live external datasets.
- Do not fabricate candidate names; use fallback PLATEAU building IDs where names are absent.

Required validation:

- `tools/p8/run_p8_humanitarian_candidate_audit_preflight.ps1`
- `tools/p8/run_p8bc_consolidation_preflight.ps1`
- `tools/p8/run_p8c_preflight.ps1`
- `tools/p8/run_p8b_evidence_spatial_gate.ps1`
- `tools/p8/run_p8a_preflight.ps1`
- Unity EditMode GUI tests
- Unity PlayMode GUI tests
- DeepSeek review using `deepseek_review_prompt_p8_humanitarian_candidate_audit.md`
