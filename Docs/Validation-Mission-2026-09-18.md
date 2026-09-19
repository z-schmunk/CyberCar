# Validation — September 18, 2026

**206 automated checks passed in the delivered Windows build.** No runtime errors/exceptions, C# compilation errors/warnings, or shader errors were observed in the final build/run. Git diff whitespace validation passed.

- Build: Logs/MissionBuild5.log, Unity 6000.6.0f1, Windows x64 Mono development build; 276,327,284 bytes reported by Unity.
- Full run: Logs/MissionFinalRuntime5.log, ending CYBERCAR_SMOKE_SUCCESS / 206.
- Focused candidate runs: Logs/MissionFocused4.log passed 65 mission/UI checks; Logs/MissionTraffic3.log passed six AI recovery cases. The full final run repeats their coverage.
- Prior validation is preserved in Docs/Validation-2026-09-15.md.

## New coverage

Thirty-four mission checks cover independent diagnostic progress, close/reopen and threat switching, cooldown retry, wrong-answer counting, expired evidence rejection, dependent RSU/satellite repairs, report integration, rating rules, personal-best ordering, actual disk persistence, schema 2/3 migration, malformed records, backup recovery, test-mode write protection and run reset. Four of these checks cover front/rear lamp materials, stronger braking emission, independent player/traffic lighting during blackout, and visible red pixels inside the projected rear-lamp bounds.

Six additional AI checks move a traffic car outside either corner of the road network and below the road, at each of 30 and 120 target FPS. Both physical and rendered positions must recover to the roads. These are frame-rate targets, not a performance benchmark.

Thirty UI checks cover menu, garage, briefing, driving, three simultaneous threat cards, pause, diagnostic, field guide, report and achievements at 1280x720, 1024x768 and 1920x1080. Repaint-time text bounds and control bounds pass. Screenshots of the report, 4:3 diagnostic, garage, threat cards and blackout were inspected. Guide/pause/garage now render only their own controls, preventing background controls from receiving their clicks; no manual click-through test is claimed.

The prior driving, collision, drift, reverse, off-road, physical bridge traversal/fall, rollover recovery, pedestrians, all thirteen defenses, road graphs/signals, actual AI courier delivery, visible world-collider audits, lighting, asset and persistence regressions remain covered.

## Issues found and corrected

The first full candidate failed AI road containment immediately after a forced physics displacement. Boundary handling had read the interpolated rendered transform in Update. It now runs in FixedUpdate using Rigidbody.position, and replans from that same physical pose. The original reproduction and six targeted recovery cases pass in the final build. Diagnostic logs demonstrate a physics/render pose gap immediately after displacement; the final test checks both converge onto the road.

A material-state lamp test passed while the screenshot still showed dark lenses. An explicit Resources/VehicleLamp shader replaced runtime Standard emission variants for the lamp meshes. The final framebuffer test and inspected blackout capture show red tail lights. This is why material-state checks alone were insufficient. A Unity camera-lookup deprecation warning in candidate build 4 was corrected before final build 5.

## Delivery and limits

Source, assets and documentation remain in CyberCarGame and its existing GitHub Desktop repository. A pre-change snapshot is Backups/Before-Mission-Polish-2026-09-17.zip; the refreshed source archive is ZIP-verified and SHA-256 recorded in Backups/Delivery.json. Tests do not write the real player's progress. No commit or push was performed.

Automated physics tests and inspected captures are not a manual keyboard playthrough or exhaustive collision/performance testing. Some campaign completion cases use checkpoint teleportation. UI checks cover the listed sizes/states, not every possible string or input sequence. Partial diagnostics survive switching within a run, not quitting; freeplay ratings are not pooled across different maps/difficulties. The educational attacks remain inert local simulations, and the car remains an arcade physics model.
