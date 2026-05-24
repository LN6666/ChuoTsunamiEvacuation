# P8-C To P8-D/E Handoff

Date: 2026-05-24.

## P8-C Output

P8-C provides:

- P2-P6 new-map smoke/proxy adaptation status.
- Risk-front/hazard-layer-driven infrastructure hazard states.
- Proxy categories for roads, buildings, bridges, underground, entrances, waterfront, open space, shelter proxies, navigation target proxies, humanitarian candidate proxies, and high-rise candidate markers.
- Non-official humanitarian candidate name list for user review.

P8-C does not implement gameplay success/failure rule changes, P8-D collapse proxy, P8-E final closeout, P9 final gameplay, real indoor scenes, congestion, or P10 packaging.

## P8-D Allocation

P8-D should handle infrastructure damage, blockage, and lightweight collapse proxy only after the user reviews the humanitarian candidate name list.

Humanitarian/high-rise candidates may participate in:

- building warning proxy
- entrance blocked proxy
- low-floor inundation warning proxy
- damage proxy status

P8-D must not implement real structural collapse and must not claim candidate buildings are official shelters.

## P8-E Allocation

P8-E should verify final P8 closure before P9 starts.

P8-E should confirm humanitarian/high-rise candidates are persistently and explicitly visible in the future game or are handed off to P9 with clear marker requirements. All such candidates need explicit non-official labeling and disclaimer text.

## P9 Allocation

P9 decides whether reviewed candidates become life-first selectable vertical evacuation targets in real gameplay.

P9 must not present them as official shelters. Because real indoor scene gameplay is cancelled, P9 should use entrance / safe floor / evacuation-complete proxy flow for any vertical evacuation candidate.
