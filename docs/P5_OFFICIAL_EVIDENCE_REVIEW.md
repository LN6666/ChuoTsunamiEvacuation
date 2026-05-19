# P5 Official Evidence Review

## Purpose

P5 needs evidence-based qualification of PLATEAU buildings as evacuation buildings for Chuo Ward evacuation decision validation.

The central boundary is that official designation and candidate qualification are different claims. A building can be an officially confirmed evacuation building only when official evidence supports that claim. Literature, reports, PLATEAU attributes, and routing data may support candidate status, warnings, or manual review, but they must not be used to label a non-official building as officially designated.

## Evidence Hierarchy

P5 should evaluate evidence in this priority order:

1. Official evacuation shelter / disaster facility data.
2. Official hazard/disaster maps and administrative disaster documents.
3. PLATEAU building geometry and attributes.
4. Academic papers and policy/technical reports.
5. OSM / routing data as auxiliary evidence.

When evidence conflicts, official designation and official hazard/disaster documents take precedence over derived candidate criteria. Lower-priority sources can add context, route estimates, or warnings, but they cannot override official designation status without manual review.

## Official Evidence Families

Planned official source families:

- Evacuation shelter / designated facility lists: official records of shelters, evacuation places, tsunami evacuation buildings, welfare shelters, or disaster-related public facilities.
- Tsunami / flood / storm surge hazard maps: official maps or GIS layers that define hazard context, inundation depth classes, target scenarios, or affected areas.
- Evacuation area / evacuation building maps if available: official map products that may identify evacuation destinations or facility categories.
- Administrative disaster prevention plans: disaster management plans, evacuation guidance, facility role definitions, and policy constraints that define how evacuation buildings should be interpreted.
- Source update / provenance metadata: source organization, update date, publication date, license, download/manual collection policy, and source URL.

Recorded local candidate references from `data_pipeline/sources/source_candidates.json`:

- Chuo City Open Data - Designated Emergency Shelter List: `https://www.city.chuo.lg.jp/kusei/gaiyou/toukeidate/opendata.html`
- Tokyo Disaster Prevention Map Shelter List: `https://catalog.data.metro.tokyo.lg.jp/dataset/t000003d0000000093/resource/4d800809-d51c-41e1-8311-f708a488978e`
- Chuo City Disaster Prevention Portal Facility Map: `https://bosai.city.chuo.lg.jp/`
- Chuo City Flood Hazard Map and Inundation Record Map: `https://www.city.chuo.lg.jp/a0011/bousaianzen/bousai/bousaitaisaku/suigaisonae/kouzuihazardmap/kozui02.html`
- Tokyo Storm Surge Inundation Assumption Area Data: `https://catalog.data.metro.tokyo.lg.jp/dataset/t000015d1700000007`
- Tokyo Tsunami Inundation Assumption Maps: `https://www.bousai.metro.tokyo.lg.jp/taisaku/torikumi/1000216/1023301/1023364/index.html`
- MLIT Real Estate Information Library - Tsunami Inundation Information: `https://www.reinfolib.mlit.go.jp/help/contents/`

These URLs are recorded candidate references only. P5-A1 does not download, scrape, or verify them. License, update date, data format, authority, and Chuo applicability must be verified in P5-A2 or later manual source collection before ingestion.

## Literature/Report Evidence Families

Planned literature and report evidence categories:

- Vertical evacuation building criteria: candidate suitability rules for vertical evacuation, including height, floors, use type, and expected sheltering function where documented.
- Safe floor / height criteria: minimum floor or elevation assumptions, freeboard or depth-related thresholds, and whether the criterion is hazard-specific.
- Capacity / crowding: expected capacity, occupancy constraints, bottlenecks, or crowding risk considerations.
- Accessibility / walking distance: time-to-reach, walking distance, slope/barrier assumptions, and route network constraints.
- Building structure / robustness if evidence exists: structural type, age, earthquake resilience, or public guidance on acceptable building classes.
- Vulnerable population or time-to-evacuate considerations: elderly, disabled, children, tourists, day/night population, warning time, and evacuation speed assumptions.

Literature/report evidence should be cited and converted into reviewable rule proposals. It should not become primary evidence for official designation.

## What Official Evidence Can Prove

Official evidence may support:

- `official_confirmed` when the official source directly identifies the facility/building and the PLATEAU match is unambiguous.
- `official_confirmed_with_review` when the official source likely identifies the building but matching or metadata ambiguity remains.
- `sourceUpdatedAt` when publication or update date metadata is available.
- `disasterTypes` when the source lists supported disaster categories.
- `capacity` / `safeFloor` when official data includes capacity, floor, elevation, or facility-use fields.

Official evidence may still require manual review if the source is stale, ambiguous, map-only, address-only, or spatially inconsistent with PLATEAU geometry.

## What Literature/Report Evidence Can Prove

Literature/report evidence may support:

- `strong_candidate`
- `weak_candidate`
- `manualReviewNeeded`
- `warnings`

Examples include a building that meets documented vertical evacuation height criteria, has plausible access within a time threshold, or lies outside a hazard depth class under a reviewed scenario. These are candidate claims only. Literature/report evidence alone must not label a building as officially designated.

## Manual Review Needs

`manualReviewNeeded` should be true when:

- shelter point is not inside a PLATEAU building footprint
- nearest building match distance is too large
- multiple building candidates are plausible
- official sources conflict with each other
- safe floor or capacity is missing
- match confidence is low
- candidate is non-official
- source update date or publication date is missing
- source geometry is map/PDF/manual-reference only
- hazard source applicability to Chuo Ward is unclear
- routing data is OSM-derived or otherwise non-official

## P5-A2 Follow-Up

P5-A2 should formalize:

- official source family definitions and required metadata
- official confirmation rules
- candidate suitability rules
- `official_confirmed_with_review` triggers
- manual review flag definitions
- confidence levels and thresholds
- safe floor / height rule structure
- capacity and accessibility rule structure
- source conflict handling
- planned qualification output schema
- minimum validation checks before P5-B can implement matching/routing
