# P9-C Safe-Floor Vertical Evacuation Proxy

P9-C does not load a real interior scene. The vertical evacuation flow is:

1. reach official shelter or non-official candidate entrance proxy
2. evaluate entrance status and delay
3. evaluate safe-floor proxy status
4. compare completion time against hazard-arrival timing
5. resolve success, delayed success, or failure

Supported safe-floor statuses:

- `available`
- `unavailable`
- `unknown`
- `below_required_height`
- `hazard_warning`
- `crowd_over_capacity`

Primary reason codes:

- `vertical_evacuation_complete_proxy`
- `safe_floor_available_success`
- `safe_floor_unavailable_failure`
- `safe_floor_unknown_warning`
- `safe_floor_below_required_height_failure`
- `vertical_evacuation_delayed_by_queue`
- `vertical_evacuation_delayed_by_crowd`

Unknown and warning states remain configurable. The sample config treats unknown and hazard warning as warning-only, but unavailable, below-required-height, and over-capacity can fail.
