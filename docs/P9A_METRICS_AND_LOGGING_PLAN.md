# P9-A Metrics And Logging Plan

P9-A documents future metrics only. It does not implement final run logging or final failure mechanics.

## Future Metrics

- spawned agent count,
- active crowd count,
- entrance queue length,
- average evacuation proxy time,
- congestion hotspot proxy,
- failed/blocked entrance count,
- delay reason,
- evacuation failure reason draft.

## P9-A Runtime Boundary

P9-A may create neutral debug summaries for sample data and proxy state. These summaries are for validation and development only.

P9-A must not:

- write final gameplay run logs,
- create final failure reasons,
- make NPC/crowd state affect player success/failure,
- treat P8-D/E damage/collapse outputs as final.

## Future Implementation

P9-B can add runtime prototype counters for spawned and active agents. P9-C can connect congestion, entrance-blocked state, vertical evacuation status, and final failure reasons after P8-D/E handoff is stable.
