# P10-A High-Detail Scene QA

Protected scene:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

P10-A checks:

- scene file exists locally
- protected path is clean in Git status
- P9 runtime markers and final flow can be validated through scene-safe proxy tests
- no direct scene mutation is required for P10-A
- manual high-detail smoke steps are documented for P10-B/P10-D

P10-A does not:

- open and save the high-detail scene
- modify `Chuo_BaseMap.unity`
- modify `ProjectSettings`, `Packages`, or `Assets/PLATEAU`
- archive the scene
- build the final Windows EXE

Manual high-detail smoke checklist for P10-B/P10-D:

1. Open `P7_HighDetail_Chuo` locally without saving unless an approved scene change is required.
2. Confirm renderable high-detail baseline objects are visible.
3. Add or enable only scene-safe runtime bootstrap/proxy markers if needed.
4. Verify player spawn, marker visibility, entrance proxy interaction, hazard timing, and ResultPanel feedback through runtime flow.
5. Observe risk front/light curtain placement, sweep direction, inundation coverage, UI obstruction, and visual performance.
6. Record runtime warnings/errors and do not commit generated scene churn.
7. If the scene is local-only or dirty, record the archive/backup requirement for P10-C.

Automated P10-A status:

- `Assets/Data/P10/p10a_high_detail_scene_qa_status.json`
