# NewMap Performance Spike Retest

Generated: 2026-05-27T04:03:12

Decision: `spike_remaining_but_documented`

- Player.log errors: 0
- Player.log warnings: 0
- Average FPS: 72.72
- Max frame: 11273.06 ms
- Memory focus this task: informational only

Bootstrap line:

~~~text
NewMap runtime bootstrap completed. renderers=0 colliders=0 groundSupportProxy=False collisionSupportProxy=True meshColliderShutdown=disabled_runtime_startup activeRuntimeTargets=19
~~~

Bootstrap timing line:

~~~text
NewMap runtime bootstrap timings: boundsMs=0 spawnSupportMs=13 systemsMs=49 targetsMs=105 configureMs=2 totalMs=169
~~~

MeshCollider shutdown line:

~~~text

~~~

Attribution: Runtime bootstrap timing is shown above. If the max frame remains above 2 seconds while bootstrap total is below 2 seconds, the remaining spike is attributed to large Chuo_BaseMap Unity/PLATEAU scene activation and render startup before regular gameplay.
