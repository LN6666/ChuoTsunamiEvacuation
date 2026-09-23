# P8-E P5 Route / Candidate Geometry Handoff

P8-E records route and candidate geometry status in `Assets/Data/P8/p8e_route_candidate_geometry_handoff.json`.

Route status:

- route source is OSM/prototype route data from existing P5 outputs
- WGS84 / PLATEAU transform limitation remains
- route coordinate transform is not geometry-validated against final Unity road objects
- routes are not official evacuation routes
- routes are not guaranteed passable
- routes are not road-geometry-validated

Candidate status:

- humanitarian candidates are coordinate/data/proxy-bound
- they are not official evacuation shelters
- they are not safe/approved by default
- final scene-object anchoring is not completed in P8-E hardening

P9 may use routes as estimated prototype guidance and candidates as non-official marker inputs. P9 must not claim official route status or official building/candidate safety.
