# Expansion validation — 2026-09-10

## Scope

Eight missions, four maps, seven threats/defenses, drifting, curved bridges, randomized delivery routes, citizens, traffic hotspots, local JSON progression, original music, and larger Extreme freeplay with a dedicated achievement. This extends the existing Unity/Blender prototype.

## Automated evidence

The Unity builder verifies connectivity on all four maps, 32 seeded routes per map (128 total), route adjacency and diversity, and 81-node Extreme maps. The Windows player compiles with Unity 6000.6.0f1.

The standalone regression suite passed **66 assertions** on the gameplay implementation, with no runtime errors/exceptions:
- Rigidbody acceleration, wall collision/integrity loss, car-to-car momentum transfer.
- All seven attack activations, correct defenses, cooldowns, state restoration.
- Pause and field-guide pause/resume.
- Each map's gate/delivery lifecycle (teleported checkpoints).
- Actual drift initiation, lateral slip and release.
- Music asset imported with more than 60 seconds of audio.
- Isolated save/reload of unlocks and achievements; corrupt-file fallback to backup.
- Expanded Extreme map, all-seven-defense eligibility and failure on pedestrian contact.
- Actual moving-car contact with a pedestrian trigger.
- Physical traversal of a curved, elevated beach-cliff bridge.
- Destroyed-car, cliff and timer failures; missed-attack recording; all seven campaign introductions.

A bridge test initially failed: support tops penetrated the drivable mesh, stopping the car at the crest. Piers were lowered beneath the deck. The corrected drive reached all 13 sampled waypoints, climbed about 3.44 m, and retained 100 integrity. The failing diagnostic log is retained for traceability.

Final delivered-build evidence is appended below after the final visual pass.

## Visual inspection

Reviewed standalone captures of all four maps, the seven-topic guide, seven-row after-action report, eight-mission menu and fifteen-achievement screen. Corrected excessive ocean specular shimmer and camera carryover between maps. Coastal sand/rock materials, arching palm fronds and shoreline rocks complete the final visual pass.

The screenshots are runtime captures, not mockups. The report screenshot uses accelerated checkpoint testing; its short completion time is not a real player performance claim.

## Limits

No manual keyboard playthrough was performed: native Computer Use was unavailable. The bridge and drifting checks use real physics with scripted input; complete map wins use checkpoint teleportation. Human driving feel, difficulty balance, every route's practical traffic behavior and long-session performance remain unverified. Graph reachability and the physical bridge check establish bounded evidence, not exhaustive traversability.

Pedestrians and scenery remain procedural prototype assets; this is not a photorealistic environment. Vehicle collision damage is not soft-body deformation. Traffic is reactive and may jam. No hardware-wide performance benchmark is claimed.

Progress tests use a separate file under the build folder and do not modify the user's normal progression. Achievement eligibility is tested alongside disk persistence; a manual, genuine Extreme victory has not been claimed. Old PlayerPrefs are retained for migration.

## Final delivered build

- Build log: Logs/ExpansionDeliveryBuild.log — successful Windows build, 159,910,672 reported bytes; graph validation passed.
- Runtime log: Logs/ExpansionDeliveryRuntime.log — CYBERCAR_SMOKE_SUCCESS / 66 after the final coastal visual changes; zero runtime errors/exceptions.
- Final bridge telemetry: all 13 waypoints completed at 100 integrity (see the runtime results extract).
- Git diff whitespace check passed. GitHub Desktop's historical path still resolves to the canonical project via its junction. No commit or push was performed by this task.
- Docs/Validation/ExpansionResults.txt preserves the compact assertion output; Captures contains final runtime images.
