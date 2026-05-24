# P9-D Final Integration

P9-D closes P9 by integrating the P9-A/B/C layers into one scene-safe gameplay chain:

1. weighted spawn runtime data
2. runtime candidate, shelter, entrance, route, and hazard proxy anchors
3. life-first target selection
4. official or non-official warning-preserving target decision
5. estimated route/proxy guidance
6. entrance, queue, and congestion evaluation
7. safe-floor / vertical evacuation proxy evaluation
8. P8 handoff-based hazard timing check
9. collapse/debris exposure-event evaluation
10. ResultPanel feedback and run log reason codes

The implementation stays adapter-based. It does not rewrite P2-P6 systems and does not mutate the high-detail scene.

Final P9 boundary:

- no P9-E/F/G stages
- no real building interior scene
- no heavy crowd package
- no official route claim
- no exact PLATEAU Unity object identity claim unless proven by a future report
- no P10 release packaging
