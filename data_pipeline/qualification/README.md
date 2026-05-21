# Qualification Workspace

This directory stores Phase 5 qualification rulebooks, schemas, planning files, controlled fixtures, and processed qualification outputs.

P5-F adds high-rise humanitarian vertical evacuation candidate screening artifacts:

- `highrise_humanitarian_candidate_rulebook.json`
- `highrise_humanitarian_candidate_schema.json`
- `highrise_humanitarian_candidate_sources_plan.json`
- `highrise_humanitarian_candidates_sample.json`

P5-F is planning/schema/fixture work only. It does not download data, process full Chuo high-rise records, parse new CityGML, run GIS routing, modify Unity, or change `sourceMode`.

Do not commit raw official datasets, downloaded archives, cache files, temporary GIS exports, `.venv`, or large generated spatial files here.

Future outputs should be validated, documented, and Unity-ready before any Unity integration milestone consumes them.
