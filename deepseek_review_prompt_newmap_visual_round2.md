# DeepSeek Review Prompt: NewMap Visual Round 2

Review the staged git diff for the NewMap manual visual/control fix round 2.

Verify:

- Latest manual feedback is explicitly acknowledged.
- Mouse/camera look rotates only while holding the configured mouse button and dragging.
- Mouse movement alone does not rotate the camera.
- Tourism and Evacuation modes both support drag look.
- Menu/rules/pause/result states preserve normal cursor use.
- Ground/support height alignment is fixed or honestly documented.
- Building floating/clipping sources are reduced without moving imported PLATEAU geometry unsafely.
- Building material/photo-pasted issue is audited without false LOD2 claims.
- Clear daytime lighting remains playable.
- Night mode darkens the sky while keeping buildings readable.
- UI, markers, green frames, warning semantics, and route wording remain intact.
- Debug/test objects are hidden in normal player mode.
- P2-P10 gameplay regressions are checked.
- No final release/archive is created.
- No PBL11/P10-E/P10-F/P10-G milestone is created.
- ProjectSettings/Packages are not modified unless necessary and documented.
- No A-level blockers remain.

Focus on Unity compile risks, runtime lifecycle, NullReference risks, scene/bootstrap risks, Player.log/tooling accuracy, and whether docs/status JSON overclaim manual-visible success.
