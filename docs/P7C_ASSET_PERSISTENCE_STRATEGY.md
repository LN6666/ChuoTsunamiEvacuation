# P7-C Asset Persistence Strategy

## Operating Context

P8/P9/P10 will continue on this cloud PC, but the cloud PC may be deleted after the project is complete. The final packaged game will be stored on cloud drive.

## What GitHub Should Preserve

Commit normally:

- source code
- docs
- tools
- tests
- prompts and review prompts
- key scene definitions
- small metadata assets
- reproducible project state

## What Requires Care

Do not blindly push large generated/imported city assets into normal Git history.

Large game assets should be committed only if they are already accepted by the current Git/LFS strategy or explicitly required for the project state. Otherwise they should be archived via cloud drive, release package, or an approved LFS workflow.

## What Must Not Be Committed

Do not commit:

- Unity `Library`
- `Temp`
- `Obj`
- `Build` outputs
- `Logs`
- profiler binary captures
- cache directories
- generated Windows EXE packages

## Release Recovery Requirement

P10 must create a release/recovery package before VM deletion. That package should include the final Windows EXE, required runtime assets, configuration, and enough documentation to rebuild or recover the project state.

P7-C classifies its own outputs as code/docs/tests/scene metadata unless actual large PLATEAU assets are separately approved for Git/LFS.
