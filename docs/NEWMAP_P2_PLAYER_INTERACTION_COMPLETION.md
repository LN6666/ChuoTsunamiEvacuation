# NewMap P2 Player Interaction Completion

JSON: `Assets/Data/P10/newmap_p2_player_interaction_completion.json`

Final status: `completed_with_documented_runtime_proxy`

The new map runtime creates a visible humanoid player marker, active third-person camera, movement controls, E interaction, and ResultPanel flow. Grounding uses a documented runtime support proxy because the reset PLATEAU scene does not provide reliable gameplay walkable surfaces.

Movement policy:

- Tourism: walk 2 m/s, sprint 10 m/s, stamina disabled.
- Evacuation: walk 1 m/s, sprint 5 m/s, stamina enabled.

Normal route fall recovery target: 0.
