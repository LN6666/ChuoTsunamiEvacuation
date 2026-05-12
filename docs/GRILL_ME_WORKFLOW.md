# Grill-Me Workflow

## Purpose

Grill-me is a design pressure-test step before coding.

The goal is to expose unclear requirements, hidden assumptions, edge cases, and scope creep before Codex writes code.

This project uses grill-me before implementing any core gameplay system.

## When To Use Grill-Me

Use grill-me before implementing:

- PLATEAU import strategy
- player control system
- tsunami risk boundary
- risk zones
- shelter interaction
- shelter data loading
- result panel
- crowd simulation
- route guidance
- data schema changes
- major refactoring

## Grill-Me Process

1. State the module goal.
2. Ask design questions.
3. Identify assumptions.
4. Identify failure cases.
5. Decide what is in scope and out of scope.
6. Record decisions in Markdown.
7. Generate a Codex prompt.
8. Implement only after the design is clear.

## Standard Questions

For every core system, ask:

1. What is the goal of this system?
2. What is the minimum playable version?
3. What should this system not do?
4. What data does it need?
5. What other systems does it depend on?
6. What happens if required references are missing?
7. What happens if data is invalid?
8. What are the success conditions?
9. What are the failure conditions?
10. How will we test it in Unity?
11. What should Codex be forbidden from changing?
12. What should DeepSeek review focus on?

## Tsunami Risk Boundary Questions

Before implementation, answer:

- From which direction does the risk boundary move?
- Does it move linearly or in stages?
- Does contact cause immediate failure?
- Is there a warning period?
- Is the countdown connected to wall movement?
- Is the wall visual only or does it trigger gameplay logic?
- How should the UI explain it?
- What is out of scope?

## Shelter System Questions

Before implementation, answer:

- What makes a building usable?
- Are official shelters different from candidate high-rise buildings?
- What does S / A / B / C / D rank mean?
- Can the player enter any building?
- What happens if the building is damaged?
- How is climb time decided?
- Does the shelter have capacity?
- What failure reason is shown?

## Player Interaction Questions

Before implementation, answer:

- How does the player know a building is interactable?
- What key is used to enter?
- Does entering a building switch scenes?
- Can the player move during climb simulation?
- What happens if the player leaves the trigger area?
- What UI prompt is shown?

## Result Panel Questions

Before implementation, answer:

- What is shown on success?
- What is shown on failure?
- Is elapsed time shown?
- Is selected shelter shown?
- Is failure reason shown?
- Is the result useful for PBL presentation?

## Output Of Grill-Me

Each grill-me session should produce:

- design decision summary
- scope boundary
- data requirements
- Codex prompt
- DeepSeek review checklist
- Git commit plan

## Rule

If the module design is unclear, do not let Codex implement it yet.
