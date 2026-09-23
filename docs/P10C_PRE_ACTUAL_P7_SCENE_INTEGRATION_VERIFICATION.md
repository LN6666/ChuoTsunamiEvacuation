# P10-C-Pre Actual P7 Scene Integration Verification

Status: PASS_WITH_LIMITATIONS

Verification time: 2026-05-25T21:40:06

Built player: D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe

Player log: D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\P10CPre_ActualP7SceneIntegration.log

## Evidence
- [P10CPreStartupDiagnostics] phase=startup_menu_built P10-C-Pre playable startup diagnostics: scene=P7_HighDetail_Chuo, UI=True, player=0, camera=1, P2-P10 limitations=11.
- [P10CPreStartupDiagnostics] scene=Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity, target=Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity, targetActive=True, canLoad=True, preBootstrapRenderers=0, placeholderLikely=True
- [P10CPreStartupDiagnostics] dataRoot=D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre_Data\Data, P8=True, P9=True, P10=True, localizationLoaded=True
- [P10CPreStartupDiagnostics] counts cameras=1, players=0, canvases=1, eventSystems=1, shelters=0, entrances=0, resultPanels=0
- [P10CPreStartupDiagnostics] integration p4RealShelters=0, p5Qualified=0, p5Candidates=0, p6Npc=0, p8RiskFrontLoaded=False, p9Crowd=0, p9SpawnMarkers=0, p9EntranceSafeFloorMarkers=0, p9CollapseDebrisZones=0, p10GreenFrames=0
- [P10CPreStartupDiagnostics] phase=start_game_completed P10-C-Pre playable startup diagnostics: scene=P7_HighDetail_Chuo, UI=True, player=1, camera=1, P2-P10 limitations=1.
- [P10CPreStartupDiagnostics] scene=Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity, target=Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity, targetActive=True, canLoad=True, preBootstrapRenderers=0, placeholderLikely=True
- [P10CPreStartupDiagnostics] dataRoot=D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre_Data\Data, P8=True, P9=True, P10=True, localizationLoaded=True
- [P10CPreStartupDiagnostics] counts cameras=1, players=1, canvases=2, eventSystems=1, shelters=34, entrances=34, resultPanels=1
- [P10CPreStartupDiagnostics] integration p4RealShelters=5, p5Qualified=27, p5Candidates=5, p6Npc=1, p8RiskFrontLoaded=True, p9Crowd=16, p9SpawnMarkers=12, p9EntranceSafeFloorMarkers=4, p9CollapseDebrisZones=3, p10GreenFrames=2

## Limitations Or Blockers
- P9 target scene appears to be the small placeholder/status shell rather than the actual 22GB P7 high-detail scene copy.
