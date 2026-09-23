# P9-C Reference Model Review

Policy: `reference_only`. P9-C uses these projects and models only as design references. No external package/code is used.

## References

| Reference | What informed P9-C | P9-C boundary |
| --- | --- | --- |
| JR-Morgan/Crowd-Evacuation-Simulation: https://github.com/JR-Morgan/Crowd-Evacuation-Simulation | Unity evacuation workflow, result metrics, density/evacuation-time feedback, and BIM-oriented scenario structure. | No Unity/BIM workflow code is imported. P9-C does not build real building interiors or a full agent behavior model. |
| TUNAMI-EVAC: https://github.com/erick2307/TUNAMI-EVAC | NetLogo tsunami evacuation ABM framing, tsunami evacuation scenario inputs, and long-run evacuation analysis. | P9-C is not NetLogo, not calibrated ABM, and not official behavior prediction. |
| armostafizi/EvacuationModel: https://github.com/armostafizi/EvacuationModel | Near-field tsunami evacuation concepts, horizontal/vertical evacuation choice, shelter locations, road network failure, and mortality-rate study framing. | P9-C uses deterministic gameplay reason codes, not a real mortality model or calibrated transportation model. |
| fabhiansan/tsunami_simulation: https://github.com/fabhiansan/tsunami_simulation | Agent movement, shelter occupancy/capacity, death-count style outputs, route algorithms, and GeoJSON-style export concepts. | P9-C does not import Rust code, pathfinding engines, tsunami propagation, or output systems. |
| Project-PLATEAU/evacuation-simulation-tools: https://github.com/Project-PLATEAU/evacuation-simulation-tools | 3D city model based flood evacuation simulation, time-series person movement logs, road-network evacuation, congestion, and inundation overlay concepts. | P9-C does not claim official PLATEAU simulation parity or reimplement the PLATEAU use-case tools. |
| Social Force Model: https://arxiv.org/abs/cond-mat/9805244 | Conceptual crowd pressure, bottleneck, personal-space, and boundary interaction concepts. | P9-C does not implement calibrated Social Force Model equations. |
| RVO/ORCA: https://gamma-web.iacs.umd.edu/ORCA/ | Conceptual local multi-agent avoidance and reciprocal collision-avoidance framing. | P9-C does not import RVO/ORCA libraries and does not implement full ORCA/RVO navigation. |

## Design Choice

P9-C implements deterministic rule-based gameplay proxies:

- target selection
- congestion score
- queue delay
- entrance bottleneck
- shelter/candidate capacity pressure
- vertical evacuation choice
- life-safety outcome reason codes

P9-C must not claim:

- scientifically validated crowd simulation
- calibrated social force model
- full ORCA/RVO navigation
- official evacuation behavior prediction
- real mortality model

The result is deliberately conservative: transparent reason codes and repeatable outcomes for gameplay testing, not a validated social simulation engine.
