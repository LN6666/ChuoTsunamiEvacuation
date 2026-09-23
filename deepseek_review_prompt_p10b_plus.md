# DeepSeek Review Prompt: P10-B+ UI Localization Weather Stamina

Review the P10-B+ diff for ChuoTsunamiEvacuation.

Verify:

- P10-B+ remains UI/UX/polish scope and is not a new official stage.
- No P10-E, P10-F, or P10-G was created.
- No final Windows EXE build output, release package, or archive work was created.
- Localization supports English and Japanese, language switching, English fallback, and missing-key safety.
- Start menu, pause menu, options/language switching, force quit explanation, and scrollable rules UI requirements are covered.
- English and Japanese game rules Markdown files are exported.
- UI overflow risks are handled or clearly documented.
- Background asset policy is safe and no unlicensed internet image is committed.
- Weather movement modifiers match the requested values: rain 0.75, night 0.85, night rain 0.65.
- Stamina/sprint system is deterministic and testable, including staged drain, 15 second lockout, 30/50/100 percent recovery milestones, and composition with weather.
- Avatar presentation and mobility profile policy is ethically safe.
- Female speed modifier is not hard-coded or enabled by default; if present, it is optional and documented as a scenario assumption, not a real-world claim.
- Existing P2 movement/camera/E interaction and P9/P10-B systems are not broken.
- No P7/P8/P9 systems are reimplemented.
- Protected paths are clean.
- Tests meaningfully cover localization, UI flow, weather, stamina, and avatar/mobility policy.
- No A-level blockers remain.

Also list B-level follow-ups for user manual playtest before P10-C.
