# Random Tsunami Event and Anti-Camping Design Rules

## Purpose

This document records future gameplay rules for random tsunami timing, shelter uncertainty, and anti-camping behavior.

These rules are not all required for the first prototype, but they define the intended direction of the game design.

---

## Core Principle

The game should not treat evacuation as a simple race from a known starting point to a known safe building.

The intended experience is:

- the player is initially a normal pedestrian in the city
- the tsunami warning occurs unexpectedly
- the player must quickly understand the situation
- shelter availability is uncertain
- entering a building is not immediate safety
- the player only succeeds after reaching a safe upper floor

---

## Rule A: Shelter Status Is Hidden Before Warning

Before the tsunami warning starts, the player should not know exactly which buildings are usable as shelters.

During the PreEvent phase:

- shelter rank should not be shown
- usability should not be shown
- official / candidate shelter status should not be fully shown
- pressing E should not immediately start evacuation
- buildings may look like normal city buildings

After the tsunami warning starts:

- shelter status can be revealed
- usable / unusable state can be shown
- rank and failure reason can be shown
- the player can start shelter entry if conditions allow

Reason:
This prevents the player from planning around fully known safe buildings before the disaster happens.

---

## Rule D: Shelter Entry Requires Time

Pressing E near a shelter is not instant safety.

Shelter entry should include one or more time costs:

- entry delay
- climb time
- congestion delay
- confirmation delay

Example fields:

- entryDelaySeconds
- climbTimeSeconds
- crowdingDelaySeconds
- confirmationDelaySeconds

The player becomes safe only after the required climb / safe-floor process is completed.

Reason:
In a real tsunami evacuation situation, reaching a building entrance is not enough. The evacuee must reach a safe upper floor before the risk arrives.

---

## Rule E: Anti-Camping Near Shelter Entrances

Standing near a shelter entrance before the tsunami warning should not become a guaranteed advantage.

If the player camps inside or very near a shelter entrance before the tsunami warning, the game may mark that shelter as camped by the player.

Anti-camping rule:

If the player is already inside a ShelterEntrance trigger when the tsunami warning starts, or has stayed inside it for longer than a configured camping threshold before the warning, that shelter becomes unavailable to this player for the current round.

Result:

- the player cannot enter that pre-camped shelter
- the UI should show a message such as:
  "This entrance is not available because you were already waiting here before the warning. Find another shelter."
- the player must search for another usable shelter

Possible parameters:

- antiCampingEnabled: true
- preWarningCampingThresholdSeconds: 5
- blockCampedShelterForRound: true

Reason:
This prevents the player from exploiting prior knowledge by waiting at a known shelter entrance before the disaster occurs.

Design note:
This rule should be used carefully. The goal is not to punish normal movement near buildings, but to prevent obvious shelter camping.

---

## Rule F: Ordinary Pedestrian Task Before Disaster

Before the tsunami warning, the player should have an ordinary city activity or movement goal.

Examples:

- walk to a station
- go to an office
- go to a convenience store
- cross a street
- follow a route
- explore the city

The tsunami warning may occur while the player is performing this ordinary task.

Reason:
This makes the disaster feel unexpected and prevents the player from simply waiting near shelters.

---

## Random Tsunami Warning Timing

In future versions, the tsunami warning should be triggered randomly.

Example rule:

- the game starts in PreEvent state
- after a random delay, the tsunami warning starts
- possible delay range: 30 to 120 seconds
- after the warning starts, the evacuation countdown begins
- the tsunami risk wall starts moving after warning

Prototype rule:

- for testing, key T can manually start the tsunami warning
- when T is pressed, the countdown starts
- before T is pressed, the countdown should not decrease

Future rule:

- T key remains as a debug tool
- random warning becomes the normal gameplay trigger

---

## Shelter Availability Randomization

After the warning starts, shelter availability may be randomized based on shelter rank.

Example:

S rank:
- very high chance of being usable

A rank:
- high chance of being usable

B rank:
- medium chance of being usable

C rank:
- low chance of being usable

D rank:
- usually unavailable

Possible post-warning states:

- usable
- entrance blocked
- structurally damaged
- crowded
- unknown
- unavailable

Reason:
This prevents the player from relying on one memorized building every round.

---

## First Implementation Timing

Implement now:

- countdown starts only after tsunami warning
- T key starts warning in prototype
- shelter entry is not instant safety
- success only after climb completion
- failure if tsunami reaches player or active shelter entrance before climb completion

Implement later:

- random warning delay
- hidden shelter status before warning
- shelter availability randomization
- anti-camping rule
- ordinary pedestrian task before warning

---

## Summary Rule

The player should not win by simply waiting at a known safe building.

The player should win by reacting to an unexpected warning, finding a usable shelter, entering it, and reaching a safe floor before the risk arrives.
