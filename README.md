# CyberCarGame

A local Windows driving game about defending connected vehicles. Created with Unity 6000.6.0f1 and Blender 5.1.1.

## Project organization and GitHub Desktop

The canonical project folder is `C:/Users/schmu/unityProjects/CyberCarGame`.

- `Assets`, `Packages`, `ProjectSettings`: Unity source and settings.
- `ArtSource`: Blender model and its generation script.
- `Builds/Windows`: playable executable and generated runtime captures.
- `Docs/Validation/Captures`: reviewed screenshots; `Docs/Validation` also holds test results.
- `Docs/Delivery`: original delivery documents and receipt.
- `Tools/PackageSource.py`: rebuild and verify a portable source backup.
- `Tools/Archive`: original session packaging script retained for completeness.
- `Backups`: source archives and backup receipt, kept locally.
- `Launchers`: shortcuts to play the game or open Unity, kept locally.
- `Logs`: project creation, build and runtime evidence, kept locally.

This folder tracks `main` from `https://github.com/z-schmunk/CyberCar.git`. The repository's previous Unity template remains in Git history; the local changes replace it with this game. The original Unity Git attributes and Git LFS configuration are preserved. Builds, caches, backups and machine-specific shortcuts are ignored by Git.

GitHub Desktop's saved CyberCar location was `C:/Users/schmu/AndroidStudioProjects/CyberCar`, which no longer existed. That path is now a directory junction pointing at the canonical project folder above. It contains no second copy of the game and allows the existing Desktop repository entry to resolve the project.

The connection and organization do not commit or push changes. Review them in GitHub Desktop before publishing. Run `python Tools/PackageSource.py` from this folder to refresh `Backups/CyberCarGame-Source.zip`.

## Play

Run `Builds/Windows/CyberCarGame.exe`. Keep its entire Windows folder together.

Start with **Driver orientation**, then complete six progressively harder operations to unlock freeplay. Each mission introduces its newest attack first. Reach every teal GPS gate, then the destination, before time or vehicle integrity runs out. Freeplay offers three maps and three difficulty settings.

| Control | Action |
|---|---|
| W/S or Up/Down | Accelerate, brake into reverse |
| A/D or Left/Right | Steer |
| Space | Brake |
| Q | Verify signed GPS route; counter spoofing |
| E | Switch to cached offline map; counter denial of service |
| R | Isolate compromised CAN controller; counter injected commands |
| F | Restore clean backup; counter ransomware |
| G | Authenticate V2X identities; counter Sybil convoy |
| Backspace | Recover at last checkpoint, costs 10 integrity |
| Escape | Pause / resume |

You can also click defense buttons. Each defense has a cooldown. Incorrect diagnostics have a short cooldown. Learner difficulty displays defense hints; higher difficulties remove these hints and increase traffic and pressure. Expert permits overlapping attacks.

## Included

- Arcade Rigidbody driving with momentum, lateral grip, braking, physical impacts, integrity loss, impact particles, engine/impact audio, and simple body compression.
- Blender-authored interceptor with body, cabin, spoiler, wheels and materials; original `.blend` plus reproducible generation script.
- Neon District: urban blocks and a closed service-road trap.
- Container Harbor: causeways over water, cranes and cargo islands.
- Red Rock Pass: elevated roads, rock mesas, guardrails and an unfinished bridge trap.
- Six campaign operations with persistent unlocks.
- Five simulated attack types, simultaneous driving/defense, GPS minimap, destination gates, pursuit vehicles and ambient traffic.
- Per-level reports of encountered, contained and missed attacks, with brief explanations.
- Nine persistent achievements, including first defense of each attack and a hidden encrypted cache.
- Freeplay after the campaign, with map and difficulty selection.

## Editing in Unity / VS Code

Add this folder to Unity Hub and open it with **6000.6.0f1**. Open `Assets/Scenes/CyberCar.unity`, then press Play. The scene has a single composition root; maps, vehicles and HUD are generated when the game starts, so an empty edit-time scene is expected.

Edit C# files under `Assets/Scripts`. Use **CyberCar > Build Windows game** to regenerate the launch scene and executable. That menu writes the launch scene and relevant build/player settings, so save any intended custom scene elsewhere first. `ArtSource/create_car.py` regenerates `CyberInterceptor.blend` and the FBX under Resources using Blender.

| File | Responsibility |
|---|---|
| GameSession.cs | Campaign/freeplay flow, timers, checkpoints, defense input |
| VehicleController.cs | Physics, impacts, audio, vehicle condition |
| RoadNetwork.cs | Connected map graphs and shortest paths |
| WorldBuilder.cs | Map geometry, signs, markers, impact effect |
| TrafficAgent.cs | Road routing, pursuit and traffic steering |
| AttackDirector.cs | Threat states, defenses, cooldowns and online attack selection |
| GameHud.cs | Menus, briefing, GPS, HUD, reports and achievements |
| ProgressStore.cs | Versioned local campaign/achievement keys |
| RuntimeSmokeTest.cs | Opt-in standalone regression scenarios |
| Editor/ProjectBuilder.cs | Scene generation, road validation and Windows build |

## AI and educational scope

Enemy driving uses graph routing and reactive steering. Attack selection uses a small **online contextual bandit**: after the introductory attack sequence, it explores and learns attack expiry versus containment at low/high player speed. Learning resets each run. This is a working lightweight learning algorithm, not a pretrained neural network or Unity ML-Agents training project.

Cyber events are safe local game simulations. There is no network interception or real vehicle interaction. Sybil represents forged vehicle identities coordinating hostile vehicles; authenticating senders temporarily stops their pursuit. Ramming itself is a physical consequence, not a cyber attack. Recovery and isolation are simplified to one-button actions so players can continue driving.

This is a playable prototype with procedural environments. Vehicle damage is not soft-body deformation; suspension and animated wheels are not simulated. Keyboard/mouse is supported; gamepad bindings and accessibility remapping are not yet included. Traffic is reactive and can become stuck; recovery handles stalled cars. No external paid assets, services, or credentials are required.

## Validation

The builder checks connectivity from the start to every node in all three maps. Run the executable with `-smokeTest` to exercise acceleration, a real collision, all attack/defense pairs, cooldowns, pause, checkpoint completions, vehicle destruction and cliff failure. Test mode does not write campaign progress. Screenshots and `smoke-results.txt` are written beside the executable.

See `Docs/Validation.md` for the final executed results and remaining limitations.

## Asset provenance

All first-party C#, procedural level geometry, synthesized audio and the Blender interceptor were authored for this project during this session. No third-party asset downloads were used. Unity runtime and built-in resources retain their existing Unity terms. Keep `Assets`, `Packages`, `ProjectSettings`, `ArtSource` and `Docs` in backups; Unity can regenerate `Library`, `Temp`, `Logs` and IDE files.
