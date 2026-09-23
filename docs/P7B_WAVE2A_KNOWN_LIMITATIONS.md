# P7-B Wave 2-A Known Limitations

Date: 2026-05-22

Stage: P7-B Wave 2-A

## Limitations

| ID | Severity | Limitation | Follow-up |
|---|---|---|---|
| P7B-W2A-L01 | Medium | The benchmark scene skeleton has no real PLATEAU geometry. Primitive placeholders cannot validate LOD3 geometry quality, import cost, material cost, or visual correctness. | Wave 2-B or a later approved task must define and approve any real LOD3 candidate import. |
| P7B-W2A-L02 | Medium | Metrics recorder FPS and 1 percent low values are approximate and sample-based. They are not a Unity Profiler replacement. | Use Unity Profiler and benchmark records before making asset-scope decisions. |
| P7B-W2A-L03 | High | Wave 2-A does not approve importing LOD3 candidate data, copying real assets, or generating PLATEAU-derived Unity assets. | Require explicit Wave 2-B approval before any real asset import. |
| P7B-W2A-L04 | Medium | LOD4 is not assumed available. Current inventory found no LOD4 path/name hits. | Keep LOD4 claims blocked until source evidence exists. |
| P7B-W2A-L05 | Medium | Unity scene creation may require GUI or batchmode validation depending on local Unity licensing and package-manager behavior. | Validate with `tools/p7/run_p7b_wave2a_preflight.ps1` and GUI EditMode/PlayMode tests. |
| P7B-W2A-L06 | High | Unity launches can sometimes create line-ending or serialization churn in `ProjectSettings` or `Packages`. | Revert any such churn unless a later prompt explicitly approves it. |
| P7B-W2A-L07 | Medium | Windows EXE profiling is not completed in Wave 2-A. | Keep EXE profiling for later P7 work and P7-D closeout. |

