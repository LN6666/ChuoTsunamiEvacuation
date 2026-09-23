# P9-C Entrance Queue Congestion Rules

Entrance gameplay is external proxy gameplay. The player reaches an entrance marker, and P9-C evaluates a deterministic state object.

Supported statuses:

- `open`
- `crowded`
- `blocked`
- `hazard_affected`
- `closed`

Delay model:

- queue delay = queue length times scenario seconds per person
- congestion delay = crowd density and route congestion score times configured factors
- total delay is capped by `maxDelayCapSeconds`
- blocked/closed entrances fail only when configured
- hazard-affected entrances fail only when configured
- crowd delay can fail only when it exceeds the hazard-arrival safety window

Primary reason codes:

- `entrance_open`
- `entrance_crowded_delay`
- `entrance_blocked_failure`
- `queue_delay_applied`
- `crowd_congestion_delay`
- `delayed_by_crowd_congestion`
- `failed_due_to_crowd_delay`

This is not a full crowd dynamics model. It is a bounded gameplay proxy.
