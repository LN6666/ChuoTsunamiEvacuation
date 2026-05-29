# NewMap Building Collision After Whitelist

Building collision remains a required blocker.

Runtime behavior:
- Conservative building obstacle bounds are still generated from building renderers.
- Player building collision margin remains `0.05m`.
- Large frontage-style bounds are filtered or inset to avoid unexpected street-wide walls.
- Spawn validation still rejects inside-building positions.
- NPC building avoidance remains enabled.

This preserves the user requirement that buildings must collide while removing unrelated invisible blockers.
