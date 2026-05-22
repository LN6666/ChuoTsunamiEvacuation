# P7-D Asset Archive And Recovery Plan

## GitHub Contents

GitHub should preserve:

- source code
- docs
- tools
- tests
- prompts and review prompts
- key scene definitions
- small metadata assets
- reproducible project state

## Local Contents For P8/P9/P10

The cloud PC may keep large imported city assets locally while P8/P9/P10 continue. Those assets must not be assumed recoverable after VM deletion unless archived.

## Cloud Archive Requirement

Before VM deletion, archive to cloud drive or a release package:

- final Windows EXE
- required runtime city assets
- required configuration files
- final scene and asset manifests
- validation/profiling reports

## Large Asset Strategy

Large city assets, build outputs, profiler binaries, and final EXE packages should use cloud drive, release packages, or approved LFS only where appropriate. Do not blindly commit them to normal Git history.

## Never Commit

- `Library`
- `Temp`
- `Obj`
- build folders
- `Logs`
- cache directories
- profiler binary captures
- temporary generated outputs
