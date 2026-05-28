# NewMap Manual Playtest Checklist

Generated: 2026-05-29T00:00:00+09:00

Readiness decision: `ready_with_documented_visual_limitations`

- Start the NewMap spawn/mouse-fix temp player.
- Verify mouse movement without pressing a mouse button does not rotate the camera.
- Verify holding Left Mouse Button and dragging rotates the camera in Tourism Mode.
- Verify holding Left Mouse Button and dragging rotates the camera in Evacuation Mode.
- Verify holding Right Mouse Button and dragging rotates the camera in Tourism Mode.
- Verify holding Right Mouse Button and dragging rotates the camera in Evacuation Mode.
- Verify releasing the mouse button stops camera rotation.
- Verify Escape pause unlocks cursor and Resume restores drag-look availability.
- Verify Start Menu to gameplay transition restores camera control.
- Verify random spawn starts on road/playable ground/support surface.
- Verify the player is not inside a building at spawn.
- Verify the player can move immediately after spawn.
- Verify fallback safe spawn works if random placement fails.
- Walk toward the map edge and confirm invisible air walls block leaving the imported map.
- Confirm no visible wall appears at the playable boundary.
- Confirm NPCs remain inside the playable area.
- Verify clear day brightness is playable.
- Verify night sky is dark.
- Verify night buildings remain readable and are not black silhouettes.
- Return from night/rain to clear day and confirm brightness is restored.
- Confirm no visible local training proxy/test/debug objects in normal manual mode.
- Confirm official shelter markers and non-official candidate warnings remain visible.
- Confirm green frames and estimated route guidance appear only as active guidance.
- Confirm buildings/materials are not magenta or unreadably dark.
- Confirm building material/photo-texture appearance is acceptable or explicitly rejected as a PLATEAU/import limitation.
- Confirm player stands on or very near the visible city ground.
- Confirm no obvious invisible support-plane height mismatch.
- Confirm the blue support/collision plane is no longer visible.
- Confirm the support/collision proxy is invisible in normal mode.
- Confirm road/ground material is not a blue debug material.
- Confirm buildings no longer appear visibly floating near spawn/test area.
- Confirm no severe support-plane clipping near spawn or active targets.
- Confirm building/road names appear only if source metadata or the local enrichment cache provides real names.
- Confirm no fabricated road/building names appear; if no source name exists, no generic label should be shown.
- Confirm official shelter names are readable.
- Confirm non-official candidate labels are clearly non-official.
- Confirm label clutter is controlled and labels disappear at distance.
- Confirm NPCs appear broadly around the player within about 1000m.
- Confirm NPCs are not concentrated into one blob.
- Confirm NPCs are not too close to player spawn.
- Confirm NPCs do not fall through the map.
- Confirm NPC humanoid visuals are visible.
- Confirm Tourism Mode has ambient NPCs but no crowd failure.
- Confirm Evacuation Mode crowd/congestion still works and remains bounded.
- Confirm FPS impact is acceptable.
- Confirm Player.log has no errors or exceptions.
