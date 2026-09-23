# NewMap Completion Hardening Blocker List

JSON: `Assets/Data/P10/newmap_completion_hardening_blockers.json`

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

A-level blockers: none identified from the current reports or code inspection.

B-level blockers:

- `B_MEMORY_PRIVATE_HIGH`: hardening temp player sample reached 21,950,038,016 bytes private memory.
- `B_FRAME_SPIKE_OVER_2S`: hardening Player.log sample reported max frame 20,655.05 ms.
- `B_NO_ACTIVE_OFFICIAL_TARGET`: no official shelter has a verified new-map anchor.
- `B_LOCAL_NON_OFFICIAL_TRAINING_ONLY`: active targets are local non-official training targets only.
- `B_ROUTE_GUIDANCE_LOCAL_PROTOTYPE`: route guidance is local estimated prototype guidance only.
- `B_MANUAL_PLAYTEST_NOT_DONE_AFTER_HARDENING`: automated validation ran, but human manual playtest has not been performed.

No disabled or out-of-map target may appear as active gameplay.
