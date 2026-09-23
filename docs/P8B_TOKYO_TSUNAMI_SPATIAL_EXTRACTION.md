# P8-B Tokyo Tsunami Spatial Extraction

Date: 2026-05-24.

## Source Decision

Tokyo metropolitan evidence is the primary source candidate for Chuo. Chuo standalone tsunami-map absence is not evidence absence.

Inspected usable sources:

- Tokyo Open Data tsunami distribution CSVs for ward areas.
- MLIT National Land Numerical Information N03 administrative boundary data for Tokyo.
- Chuo City page referencing Tokyo Metropolitan Government damage estimation report.
- Tokyo Metropolitan Government damage estimation report page.
- Supplementary public PDF height references.

No token, API key, registration, login, G-Spatial token, real-estate-library API token, or browser-only manual download was required for the extraction path used by P8-B.

## Files Used

Tokyo Open Data CSV files:

- `2_kubu_taisyoukanntoujisinn_sinnsuisinn.csv`: Taisho Kanto maximum inundation depth.
- `0_kubu_taisyoukanntoujisinn_toutatujikann.csv`: Taisho Kanto arrival time.
- `1_kubu_taisyoukanntoujisinn_tunamidaka.csv`: Taisho Kanto maximum tsunami height.
- `2_kubu_nannkaitorafucase1_sinnsuisinn.csv`: Nankai Trough case 1 maximum inundation depth.
- `0_kubu_nannkaitorafucase1_toutatujikann.csv`: Nankai Trough case 1 arrival time.
- `1_kubu_nannkaitorafucase1_tunamidaka.csv`: Nankai Trough case 1 maximum tsunami height.

Administrative boundary:

- MLIT N03 Tokyo 2024 ZIP, using the Chuo City polygon (`N03_004=中央区`, `N03_007=13102`).

## Extraction Result

`tools/p8/extract_p8b_chuo_tsunami_layer.py` clips Tokyo tsunami mesh points to Chuo and generates `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json`.

Extracted scenarios:

- Taisho Kanto earthquake: 1123 Chuo grid samples, maximum spatial inundation depth 2.0436m, maximum tsunami height 2.1287m.
- Nankai Trough megathrust earthquake case 1: 1256 Chuo grid samples, maximum spatial inundation depth 2.2629m, maximum tsunami height 2.4223m.

`maxTsunamiHeightMeters` is not the same as full `inundationDepthMeters` grid. maxTsunamiHeightMeters is not the same as full inundationDepthMeters grid. Maximum inundation depth was spatially extracted from the depth CSV. Maximum tsunami height is stored separately.

## Access Report

| Source | Access Method | Token/Login Needed | Data Needed | Expected File Type | Required For PASS |
|---|---|---:|---|---|---:|
| Tokyo Open Data tsunami CSVs | Direct HTTPS CSV download | No | depth, arrival, height mesh records | CSV, CP932/Shift-JIS | Yes |
| MLIT N03 Chuo boundary | Direct HTTPS ZIP download | No | Chuo administrative polygon | ZIP with GeoJSON/Shapefile/GML | Yes |
| Chuo City tsunami page | Public HTML reference | No | provenance bridge to Tokyo report | HTML | Evidence quality |
| Tokyo damage estimation report | Public report page/PDF references | No | scenario/source context | HTML/PDF | Evidence quality |
| Supplementary PDF | Public PDF reference | No | maximum-height reference | PDF | Evidence quality |

If a future source requires an API token, login, registration, G-Spatial token, real-estate-library API token, or browser-only manual download, stop and report the access requirement before using it.
