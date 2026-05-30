# P11 Final Git Source Audit

Initial audit:

- Branch: `phase5-qualification-routing-plateau`
- Previous HEAD: `913d838e Finalize NewMap manual tuning`
- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Generated local base map scene remains ignored.
- `Assets/Scenes/Chuo_GroundRoad_Import_Source.unity` is a large generated scene-like file and is not staged for the final source commit.
- Unity Test Framework generated `Assets/InitTestScene*.unity` files are ignored and not staged.
- Release folder and ZIP are outside the repo and are not staged.
- P11 source commit pushed: `36a69558c1163777a4883bb646ce2201059ffc4b`

Commit include policy:

- Include `Assets/Scripts`, editor scripts, runtime JSON/configs/reports, tests, tools, docs, prompt files, package manifests, and small release status JSON.
- Exclude `Library`, `Temp`, `Logs`, `Build`, `Builds`, release folders, EXEs, player data folders, ZIPs, raw PLATEAU data, and giant generated scene artifacts.
