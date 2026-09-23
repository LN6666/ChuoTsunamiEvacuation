# P9-A P8 Handoff Contract

P9-A defines the future contract expected from P8-D/E, but it does not require those outputs to be final.

## Expected Future Inputs From P8-D/E

P9-C expects P8-D/E to provide stable versions of:

- infrastructure hazard state,
- road/bridge/underground state,
- building warning state,
- entrance blocked state,
- low-floor inundation warning,
- collapse/damage proxy state,
- hazard `sourceMode`,
- hazard `evidenceSourceId`,
- depth status,
- intensity status,
- arrival timing status.

## P9-A Fallback Rule

If P8 handoff data is missing, malformed, or still provisional, P9-A uses a null/fallback/no-effect state:

- infrastructure hazard state: `unknown_no_effect`,
- road/bridge/underground state: `unknown_no_effect`,
- building warning state: `unknown_no_effect`,
- entrance blocked state: false,
- low-floor inundation warning: false,
- collapse/damage proxy state: `unknown_no_effect`,
- source metadata: empty,
- depth/intensity/arrival timing status: `unknown`,
- gameplay effect: none.

## Gameplay Rule Boundary

P9-A does not mutate success/failure rules. Final gameplay failure waits until P9-C, after the P8-D/E handoff contract is stable.

## Adapter Requirement

The P9 runtime adapter must accept missing P8 data and return safe defaults. The adapter may expose future handoff fields for tests and debug summaries, but it must not call player failure, shelter failure, result panel, or P2-P6 success/failure controllers in P9-A.
