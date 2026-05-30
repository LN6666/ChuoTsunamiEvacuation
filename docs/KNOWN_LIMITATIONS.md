# Known Limitations

This project is for educational and prototype demonstration use only. It is not for real emergency use.

- Route guidance is estimated prototype guidance, not an official evacuation route.
- There is no official or GIS-grade road route certification.
- Non-official high-rise candidates are not official shelters.
- Green frames do not mean official approval or safety certification.
- Safe-floor, vertical evacuation, crowd, collapse, and debris systems are gameplay proxy systems unless a target is explicitly verified by source data.
- The gameplay ground cover/support is a gameplay representation, not GIS-grade terrain.
- Some building visual alignment, material appearance, and labels may remain incomplete.
- Cached/enriched building labels may be incomplete or imperfect.
- Runtime does not perform web name lookup. Online name matching is preprocessing-only.
- Performance and memory use may require a capable Windows PC.
- First startup can have a loading or stutter spike while the map and runtime systems initialize. The final local sample recorded 2 frames over 66ms and a max sampled frame of about 1036ms during the 180-second runtime performance window.
- NPCs use broad distribution with far static-proxy behavior for performance.
- The tsunami is represented by a risk boundary/front, not by a fluid simulation.
- Use this build for presentation, manual playtest, and prototype evaluation only.
