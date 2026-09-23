# DeepSeek Review Prompt: P10-B Runtime Smoke Profiling Optimization

You are reviewing the P10-B diff for ChuoTsunamiEvacuation.

Verify:

- P10-B does not build the final Windows EXE.
- Windows EXE build is correctly deferred to P10-C.
- No P10-E, P10-F, or P10-G stage was created.
- The green ground frame feature is tsunami-start triggered and runtime-safe.
- Official and non-official evacuation-related buildings are marked without semantic false claims.
- Non-official humanitarian candidates remain `isOfficialShelter=false`, `nonOfficialWarningRequired=true`, and not safe/approved by default.
- Green frames are documented as evacuation-related marker/proxy rectangles, not official approval or exact building footprint proof.
- The implementation does not mutate PLATEAU assets or the high-detail scene.
- Performance/profiling/stress readiness is meaningful.
- Optimizations are low-risk and test-backed.
- No major new system, real indoor scene, P7/P8/P9 reimplementation, package import, or final release/archive work was introduced.
- P5 routes remain estimated prototype guidance, not official evacuation routes.
- Protected paths are clean.
- Tests cover green frame trigger behavior, non-official warning preservation, proxy fallback, pooling, and performance hooks.
- No A-level blockers remain.

Also list any B-level follow-ups for the user manual playtest before P10-C.
