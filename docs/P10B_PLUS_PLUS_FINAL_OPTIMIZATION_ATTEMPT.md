# P10-B++ Final Optimization Attempt

P10-B++ is a final optimization hardening sprint after P10-B/P10-B+ and before P10-C.

It is not a new official P10 stage. The official P10 structure remains P10-A, P10-B, P10-C, and P10-D. P10-B++ does not create P10-E, P10-F, or P10-G.

## Scope

- Audit high-detail runtime risks before the Windows EXE build.
- Add low-risk, reversible runtime hardening where it is safe.
- Strengthen metrics, quality recommendations, and manual profiling checklists.
- Keep final Windows EXE build, release packaging, and archive work deferred to P10-C.
- Avoid ProjectSettings, Packages, PLATEAU data, `Chuo_BaseMap.unity`, and `P7_HighDetail_Chuo.unity` changes.

## Optimization Result

P10-B++ performed an evidence-driven optimization pass without introducing a large new runtime system:

- Green ground frame pooled objects can warm before tsunami start.
- Green frame line setup no longer allocates a temporary `Vector3[]` per frame activation.
- P10-B metrics now include a frame spike threshold and GC collection counts.
- P10-B++ adds a bounded ring buffer, frame spike detector, runtime layer toggles, and quality preset advisor.
- Debug layers remain off by default.
- Quality recommendations are config-level only and do not claim a final AA mode.
- Streaming/chunk loading is audited honestly; production chunk streaming is not implemented.

## Boundaries

- No Addressables were added.
- No AssetBundles were built.
- No additive production scene streaming was implemented.
- No high-detail scene chunk rewrite was attempted.
- No real tsunami fluid simulation was added.
- No full crowd simulation rewrite was added.
- No final Windows EXE was created.
- No release package or archive was created.

## P10-C Gate

P10-C must still run built-player profiling and record:

- load time and memory peak
- average FPS and 1 percent low FPS
- min/avg/max frame time
- frame spikes above threshold
- managed heap and total allocated memory proxy
- Player.log warning/error count
- CPU usage from Windows tools
- disk paging symptoms from Windows tools
- high-detail scene smoke and stress behavior
