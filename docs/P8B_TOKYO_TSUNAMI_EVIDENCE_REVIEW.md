# P8-B Tokyo Tsunami Evidence Review

Date: 2026-05-24.

## Finding

Chuo City does not appear to provide a standalone tsunami hazard map equivalent to flood hazard maps. P8-B must not interpret that as absence of usable tsunami evidence.

Tokyo Metropolitan Government sources are the primary tsunami evidence candidates for Chuo:

- Tokyo damage estimation digital map: https://www.higaisoutei.metro.tokyo.lg.jp/mydmgpred.html
- Tokyo 2022 damage estimation report page: https://www.bousai.metro.tokyo.lg.jp/taisaku/torikumi/1000902/1021571.html
- Chuo City tsunami/liquefaction page: https://www.city.chuo.lg.jp/a0034/bousaianzen/bousai/saigaitaiou/saigaijoho/tunamiekijouka.html
- Tokyo disaster guidebook maximum tsunami-height reference: https://www.bousai.metro.tokyo.lg.jp/content/e_book_2025/2025-12_GDP_GuideBook_jp/pageindices/index6.html

The available public source review indicates that the Tokyo damage estimation interface/report source family includes tsunami estimation layers relevant to Chuo, including maximum tsunami height and maximum inundation depth. 東京都「首都直下地震等による東京の被害想定」 is the broader source family/report context. The actual P8-B extracted tsunami spatial layers are 大正関東地震 and 南海トラフ巨大地震 case 1.

For P8-B spatial extraction, the usable official source was Tokyo Open Data ward-area tsunami CSVs, not a manual web-map scrape. The CSVs were accessible through direct HTTPS with no token, login, registration, API key, G-Spatial token, real-estate-library token, or browser-only manual download. The current extracted CSV scenario set does not include a 都心南部直下地震 tsunami layer; that scenario is documented as not available / not a tsunami scenario in current extracted layers rather than as missing project work.

## Chuo-Specific Evidence

Chuo City states that Tokyo's damage estimation report shows tsunami numerical simulation results. This makes the Tokyo metropolitan report/map source family the primary P8-B evidence candidate for Chuo, while keeping the extracted scenario names separate from the broader report title.

Public references report Chuo maximum tsunami height values around 2.12m to 2.46m, depending on source/scenario. P8-B records:

- 2.42m as a current Tokyo/Chuo Nankai Trough maximum tsunami-height reference.
- 2.46m as a supplementary older/public PDF maximum tsunami-height reference.
- 2.12m as a Taisho Kanto earthquake Chuo maximum tsunami-height reference from Tokyo damage-estimation report material.

These are maximum tsunami-height references. They are not a full spatial inundation-depth grid and are not by themselves an extracted Chuo inundation polygon layer.

The extracted P8-B layer now stores spatial maximum inundation depth separately from maximum tsunami height:

- Taisho Kanto earthquake: maximum spatial inundation depth 2.0436m, maximum tsunami height 2.1287m.
- Nankai Trough megathrust earthquake case 1: maximum spatial inundation depth 2.2629m, maximum tsunami height 2.4223m.

No 都心南部直下地震-specific tsunami inundation-depth, arrival-time, or tsunami-height layer is present in `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json`.

## Source Registry Decision

`Assets/Data/P8/tsunami_hazard_evidence_registry.json` includes:

- `tokyo_damage_estimation_map_tsunami`
- `tokyo_damage_estimation_report_tsunami`
- `chuo_city_tsunami_liquefaction_page`
- `supplementary_pdf_tokyo_bay_tsunami_height_chuo`
- `flood_proxy_chuo_hazard_map`
- `mlit_n03_chuo_admin_boundary`

`flood_proxy_chuo_hazard_map` is kept only as a non-tsunami proxy fallback. It is not the primary P8-B tsunami driver.

## Safety Rule

P8-B may proceed with `p8cGateDecision=PASS` because official spatial mesh data has been extracted into project data. It still must not claim full real-time fluid simulation, academic hydrodynamic modeling, or that the derived bbox boundary is an official inundation contour.
