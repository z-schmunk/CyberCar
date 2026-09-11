# CyberCarGame

A local Windows cyber-security driving game. Unity 6000.6.0f1 / Blender 5.1.1.

## Play

Run `Builds/Windows/CyberCarGame.exe`; keep its Windows folder together.
Complete nine missions to unlock freeplay. Returning players retain earlier unlocks, achievements and previously unlocked freeplay.

Reach each teal GPS gate, then the destination, before time or integrity runs out. Every operation after orientation generates a fresh route through several districts. The circular GPS shows a heading-up local view within 170 meters. An edge marker points toward the next gate. Map geometry is fixed, while each delivery route changes.

| Control | Action |
|---|---|
| WASD / arrow keys | Accelerate, reverse, steer |
| Shift while steering at speed | Drift; release to restore grip |
| Space | Brake |
| Q | Verify route: GPS spoofing |
| E | Offline map: navigation denial of service |
| R | Isolate controller: CAN command injection |
| F | Restore clean backup: ransomware |
| G | Authenticate V2X identities: Sybil convoy |
| T | Check message freshness: replay attack |
| Y | Trusted firmware recovery: malicious update |
| U | Restore trusted local streetlight control: lighting takeover |
| H | Open/close educational field guide; pauses driving |
| M | Mute/unmute original soundtrack |
| Backspace | Recover at previous gate; costs 10 integrity |
| Escape | Pause/resume or close guide |

Defense buttons also respond to mouse clicks. Successful defenses have an eight-second cooldown; mismatched diagnostics have a three-second cooldown. The field guide explains the attack, why its defense works, and what the simplified simulation omits.

## Campaign and maps

| Mission | Environment | New lesson |
|---|---|---|
| Driver orientation | Neon District | Driving, drifting and crosswalk safety |
| Trust, but verify | Neon District | GPS spoofing |
| Signal lost | Container Harbor | Denial of service and offline fallback |
| Hands on the wheel | Red Rock Pass | Control-bus injection |
| Recovery protocol | Container Harbor | Ransomware recovery |
| Zero trust delivery | Neon District | Sybil identities |
| Yesterday's commands | Container Harbor | Replayed brake commands |
| Signed at the edge | Beach Cliffs | Malicious firmware |
| Lights out | Neon District at night | Infrastructure lighting takeover |

City, harbor and canyon use 36 junctions; beach cliffs uses 49. Rural maps have irregular junction placement, fewer cross-connectors, smooth road curves and level junction approaches. Continuous mountainous terrain replaces block-shaped mesas. Harbor and cliffs include ocean and sandy shorelines; the cliff destination is a lighthouse.

Orientation has six ambient cars and six citizens. Other missions have 12–27 ambient vehicles by difficulty, additional enemies, crosswalk pedestrians, and three traffic interchanges. Ordinary vehicles favor these interchanges. First entry triggers hostile reinforcements; attack pressure also increases there. Replay attacks can disrupt surrounding traffic.

Yield to pedestrians. Contact costs 15 integrity and 15 seconds; three violations fail a normal run. Extreme fails on the first violation. Physical collisions transfer momentum and damage cars.

## Freeplay and achievements

Choose any of four maps, Day or Night, and Learner, Operator, Expert or Extreme difficulty. Expert permits two concurrent cyber attacks; Extreme permits three, uses a larger 81-junction map, more traffic and tighter time allowances.

**Extreme driver:** win an Extreme freeplay delivery, defend every available attack type during that run (seven by day, eight at night), and hit no pedestrians. This is deliberately harder than simply reaching the destination.

Seventeen achievements include first delivery, first successful defense of each attack, a collision-free delivery, hidden cache, original-campaign veteran, 20 cumulative drift seconds within one run, safe delivery without pedestrian contact, coastal-campaign completion, Extreme driver, lighting recovery and night delivery.

## Night driving and visual assets

The Lights out mission and Night freeplay use a photographed night panorama with a subtle procedural star field, warm streetlight pools and independent headlights. Lighting takeover disables streetlights; U isolates remote commands and restores the local lighting schedule. The field guide explains authentication, authorization, segmentation and recovery. Daylight uses a photographed cloud panorama.

Photographed 2K asphalt, rock, sand and ground textures use normal and roughness maps. CC0 Poly Haven rocks, pine and streetlamp models were reduced in Blender for runtime use. Source files, author credits and hashes are retained in ArtSource/Vendor and Docs/PolyHavenManifest.json. The existing car and city buildings remain stylized; this is a realism upgrade, not a claim of complete photorealism.

Brake lamps respond to braking, including replay-induced braking. Drifting produces tire smoke and synthesized friction audio. The follow camera expands its view with speed and avoids solid obstructions.

## Saved progress

Unlocks, achievements and music volume save automatically to:
`%USERPROFILE%/AppData/LocalLow/CyberCarLearning/CyberCarGame/driver-profile-v2.json`.

Writes replace the file atomically and retain `.bak`. A malformed primary save falls back to that backup. The old `CyberCarGame.v1.` PlayerPrefs keys are migrated without deleting them. This saves long-term progress, not a suspended mid-race position. Saves are local to this Windows user, not cloud synced.

## Project organization and GitHub Desktop

Canonical folder: `C:/Users/schmu/unityProjects/CyberCarGame`.

- `Assets`, `Packages`, `ProjectSettings`: Unity project.
- `ArtSource`: original Blender car and reproducible car/music generation scripts.
- `Builds/Windows`: playable build and generated test captures.
- `Docs`: guides, architecture, validation and asset provenance.
- `Tools/PackageSource.py`: verified portable source backup.
- `Backups`: source archives, including the pre-expansion snapshot.
- `Launchers`: local shortcuts; `Logs`: build/runtime evidence.

The existing GitHub Desktop CyberCar entry resolves through the directory junction at `C:/Users/schmu/AndroidStudioProjects/CyberCar`. Its target is the canonical folder, with remote `https://github.com/z-schmunk/CyberCar.git`. This is one project copy. Changes have not been committed or pushed. Builds, caches, backups and local launchers are ignored; original Git attributes/LFS configuration remain.

## Editing and rebuilding

Open this folder in Unity Hub using **6000.6.0f1**. Open `Assets/Scenes/CyberCar.unity`, then Play. Maps, vehicles and HUD are composed at runtime.

Edit C# under `Assets/Scripts`. **CyberCar > Build Windows game** regenerates the launch scene and build/player settings; save custom scenes separately. The builder checks four connected maps, 128 seeded routes and 81-node Extreme maps.

`ArtSource/create_car.py` regenerates the Blender/FBX car. `python ArtSource/create_music.py` synthesizes the original soundtrack without samples or paid services. See `Docs/AssetProvenance.md`.

Run the executable with `-smokeTest` for automated regression coverage. This mode prevents normal progression writes and uses isolated save-test files. Run `python Tools/PackageSource.py` from this folder to refresh the verified source archive.

## Architecture and limits

GameSession owns missions, gates and timers. RoadNetwork shares sampled curved paths between road meshes, traffic and minimap. WorldBuilder creates geometry. VehicleController provides Rigidbody arcade handling, drift, impact response and wheel animation. TrafficAgent steers graph routes and pursues locally. PedestrianAgent controls timed crossings. AttackDirector tracks defenses and a small contextual bandit that learns containment versus expiry during each run. ProgressStore manages versioned JSON; DriveMusic manages music; GameHud renders menus, guide and reports.

This remains a procedural prototype rather than a photorealistic simulator. Damage is simple body compression; there is no detailed suspension or soft-body crash simulation. Human pedestrians are simple articulated figures. Traffic may jam and uses recovery behavior. Keyboard/mouse controls are supported; gamepad bindings are not implemented.

Cyber events are entirely local educational simulations. Real vehicles do not expose these defenses as instant keyboard actions. Ramming is a physical consequence, not itself a cyber attack. The enemy learning model is a lightweight online contextual bandit, not a pretrained neural network. See `Docs/EducationReferences.md` for primary sources and `Docs/Validation.md` for executed evidence and limitations.
