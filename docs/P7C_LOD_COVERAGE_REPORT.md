# P7-C LOD Coverage Report

## Evidence Rules

This report separates target LOD from actual loaded LOD. A target setting is not evidence that a Unity scene contains renderable geometry.

Average LOD3 is not achieved in the current evidence. Current evidence is a scene shell, local PLATEAU source category folders, and the earlier raw P7Benchmark import for candidate `53393690`.

## Coverage Table

| Category | Target LOD | Path/name evidence | Renderable Unity object evidence | Status |
|---|---:|---|---|---|
| Buildings | LOD3 | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\bldg`; P7Benchmark raw candidate `53393690` | Not verified in `P7_HighDetail_Chuo.unity` | Target only, pending import |
| Roads | LOD3 | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\tran` | Not verified | Target only, pending import |
| Bridges | LOD3 | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\brid` | Not verified | Target only, pending import |
| Underground | LOD3 if available | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\ubld` | Not verified | Target only, pending import |
| City furniture | LOD2 or LOD3 | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\frn` | Not verified | Target only, pending import |
| Water | LOD1 | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\wtr` | Not verified | Target only, pending import |
| Vegetation | LOD3 if available | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\veg` | Not verified | Target only, pending import |
| Relief | Import terrain relief | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\dem` | Not verified | Target only, pending import |
| Disaster risk | Import | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\fld` | Not verified | Target only, pending import |
| Land use | Import | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\luse` | Not verified | Target only, pending import |
| Urban planning decision | LOD1 | `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\urf` | Not verified | Target only, pending import |

## Conclusion

P7-C prepares coverage targets and validators. It does not yet prove average LOD3, LOD4, or renderable high-detail city coverage. P7-D must only run final Windows EXE profiling after actual imported assets are visible in the high-detail scene.
