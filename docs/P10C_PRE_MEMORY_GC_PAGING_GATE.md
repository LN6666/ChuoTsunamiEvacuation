# P10-C-Pre Memory, GC, And Paging Gate

## Checked

- P10-B++ metrics use bounded ring buffers for frame samples.
- P10-C-Pre runtime metrics reuse the P10-B++ ring buffer instead of unbounded sample history.
- P10-C-Pre quality profiles cap NPC, marker, and green frame counts.
- Green frame warmup and pooling remain in place.

## Built Player Measurements

Use the temporary profile script when a test build exists:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_pre_built_player_profile.ps1
```

Manual Windows checks:

- Task Manager: watch memory, CPU, GPU, and disk while loading and during tsunami start.
- Resource Monitor: check hard faults/sec and disk queue symptoms.
- Details tab: record working set and private memory for the player process.
- Player.log: check for repeated errors, warnings, missing assets, or NullReferenceException spam.

Do not modify the system pagefile in this task.

## P10-C-Pre Result

The temporary player process-level profile ran for 30 seconds with 29 samples:

- max working set: 608.05 MB
- max private memory: 1003.33 MB
- private memory growth during startup/sample window: 828.73 MB
- Player.log warnings/errors: 0/0 after copying `Assets/Data` into the temporary build
- paging symptoms: not observed by script; still needs manual Resource Monitor confirmation

The memory growth value is a short startup-window process delta, not proof of unbounded growth. A longer manual run should verify stabilization before official P10-C packaging.

## Blockers

- Memory grows continuously during a short run.
- Disk activity or hard faults coincide with persistent freezes.
- Player.log repeats errors or exceptions.
- Low profile cannot remain responsive during hazard/green-frame/light-curtain activation.
