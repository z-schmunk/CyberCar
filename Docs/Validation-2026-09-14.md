# Validation — September 14, 2026

Final Windows development build: Unity 6000.6.0f1, built-in renderer, x64 Mono.

**PASS: 132 automated runtime checks.** No runtime errors or exceptions were observed. The final build completed successfully with no C# compilation warnings or errors. `git diff --check` passed.

Evidence:
- `Logs/StoryFinalBuild2.log`: exact delivered build, 273,579,032 bytes reported by Unity.
- `Logs/StoryFinalRuntime.log`: full final regression and success marker.
- `Builds/Windows/smoke-results.txt`: portable summary.
- `Builds/Windows/smoke-*.png` and `bridge-probe.png`: actual executable captures.

## Coverage

- Real acceleration, braking, momentum-transfer collisions, drift/slip and reverse motion.
- Four visible-world material/collider audits and sampled lane-clearance checks across every generated road edge; four map delivery completions use checkpoint teleportation.
- Physical traversal of a curved elevated beach-cliff bridge, off-road mountain driving with pitch changes, an actual broken-bridge fall, and an overturned car's recorded automatic recovery.
- All thirteen attack activations and successful staged defenses, rejected direct one-call bypass, wrong-answer penalty, simultaneous threats, and defense cooldown behavior.
- RSU contact in range, contact loss outside range, blocked out-of-range handshakes, satellite fallback after returning to coverage, and saved-route validation.
- Visible obstacle measurements from virtual car sensors and injected phantom range readings.
- Directed one-way routing, traffic signal phase priority, and AI return to the road envelope.
- Local attacker launch budget and an unattacked AI courier physically completing a delivery route.
- Every story mission reaches its named destination through legal graph edges; newest threat introductions, pause/guide behavior, integrity and time failures.
- Night streetlights, independent headlights during lighting takeover, and restoration after a staged defense.
- Disk round-trip of unlocks, badges, currency and upgrades; corrupt-primary recovery from the backup, using isolated test files.
- Extreme map size and all-applicable-attack achievement eligibility, real pedestrian contact, music import, and lifecycle cleanup.

## Practical limits

These are automated tests and inspected screenshots, not a manual keyboard playthrough. They do not prove every possible collision, traffic jam or recovery situation. No multiplayer, network security service, actual GPS receiver, real ransomware, or real command execution is implemented. The attacker challenge is local. Sensors sample the physics scene; the XOR exercise is a toy cipher. Suspension and crashes are arcade approximations, and the car/buildings remain stylized. Long-session performance and difficulty tuning still benefit from human playtesting.

Existing saves are protected by test mode. The source archive includes source assets, Unity metadata, documentation and tools. Build products and logs stay in their canonical project folders. GitHub Desktop still resolves the existing CyberCar junction to this project; no commit or push was made.
