# CyberCarGame

Windows driving and cybersecurity learning adventure, built with Unity 6000.6.0f1 and Blender-derived assets.

## Play

Run `Builds/Windows/CyberCarGame.exe` and keep its folder together. Fifteen story operations lead from courier training to a combined-attack coastal mission. Completing the campaign unlocks freeplay; previously unlocked freeplay remains available. The garage and local attacker training lab are accessible from the menu.

Follow the circular, heading-up GPS road map and its solid direction arrow. Gray roads remain visible during communication failures. Teal marks the dispatch route; a compromised route is coral. During driving, only active threat controls appear, with larger labels. After-action reports paginate encountered attacks using larger text. The next-turn instruction appears below the map. World-space GPS markers and traffic dots have been removed.

| Controls | Action |
|---|---|
| WASD / arrows | Accelerate, reverse, steer |
| Shift + steering | Drift; release for progressive grip recovery |
| Space | Brake |
| Q E R F G T Y U I O J K L | Inspect the corresponding active attack |
| 1 / 2 / 3 or diagnostic buttons | Choose an action in the current defense step |
| H | Field guide; pauses driving |
| M | Mute/unmute all music, including injected audio |
| Backspace | Return to the last checkpoint, costing 10 integrity |
| Escape | Pause menu |

Each defense requires containment, evidence verification, and safe recovery. Wrong answers leave the threat active and cost four mission seconds. Driving continues throughout diagnosis. The field guide explains the principle and the simulation's limits. Learner and Operator allow one scheduled threat; Expert allows two, Extreme three. Explicit attacker-lab launches can overlap.

Diagnostic progress is retained separately for each active encounter. Close a panel or switch threats to repair an RSU first, then resume the waiting satellite handoff. Evidence and choices remain stable; expired encounters cannot authorize a new attack. Progress stays local to the current run, and the attack continues until all three steps succeed. Threat cards and controls show completed steps. Guide, pause and garage screens isolate their controls from the screens behind them.

## Mission ratings

A completed delivery earns one star, plus one for no impacts, recovery or pedestrian contact, and one for resolving every encountered attack without a wrong diagnostic choice. Uncontained expired attacks count against the security star. Pedestrian contact caps the result at one star; failed deliveries earn none. Clean orientation runs can earn all three. The report shows defended/encountered threats and mistakes.

Story missions save their highest rating and the fastest time **at that rating**. A higher-star run outranks a faster lower-star run. Mission selection and briefings show the saved best. Freeplay receives a run rating without mixing records from different maps or difficulties; attacker practice has no campaign rating. Existing achievements and upgrade rewards are retained.

## Story and threats

| Operation | New scenario |
|---|---|
| Courier induction | Controls and crosswalk safety |
| The altered dispatch | GPS spoofing and authenticated destinations |
| Harbor under pressure | Service DoS and cached roads with RSU fallback |
| A passenger on the bus | Untrusted control-command injection |
| The locked manifest | Ransomware isolation, verified backup and recovery-key exercise |
| False friends | Sybil vehicle identities and convoy coordination |
| Yesterday's emergency | Replay of stale braking commands |
| The lighthouse update | Unsigned firmware and trusted rollback |
| City without lights | Unauthorized streetlight control at night |
| Roadside silence | Failed RSU sessions and fresh authentication |
| Under a silent sky | GNSS loss, roadside references and odometry |
| The poisoned playlist | Media injection and original synthesized duck audio |
| A familiar destination | Saved-history command fields alter the displayed destination |
| Seeing double | Phantom range readings cause false braking; independent sensor comparison |
| Operation homecoming | Combined threats on the larger coastal map at night |

Story routes pass named response locations and finish at a fixed dispatch destination. Freeplay retains seeded route variety. No real commands, ransomware, satellite interference, or file injection execute on the computer. History injection changes only the game's untrusted navigation target, never the real save file. The ransomware XOR exercise is deliberately a toy cipher, not production encryption.

## Roads, vehicles and communications

Four environments: Neon District, Container Harbor, Red Rock Pass, Beach Cliffs. Mountains blend rock and ground by slope; roadside grasses break up bare shoulders. Ocean shading includes shallow-water color, smaller ripples and shoreline foam. Environment reflections refresh after world construction, movement and lighting changes. Mixed straight and curved roads, bridges, accessible off-road terrain, ocean/coast, and photographed day/night skies. Extreme uses 81 junctions. City traffic observes a 25 mph limit, alternating one-way corridors, traffic signals and crosswalks. Harbor traffic is slower than rural traffic.

Ordinary traffic spawns in three-car groups, shares destinations and leaves following gaps. Wrong-way enemies, collisions and replayed braking can create physical queues. AI vehicles have a software road envelope; there are no invisible player-only walls. Collision audits check visible world geometry and clear driving lanes. Streetlamps have visible footings and palms are grounded to the landscape.

Four sprung tire contact points allow pitch, roll, slopes and airborne motion. Steering/throttle smoothing, reverse braking, progressive drift recovery and reduced bounce improve handling. Impacts transfer momentum, reduce integrity and deform the car body. This is an arcade suspension model, not a calibrated automotive simulator or soft-body crash model.

Player and traffic cars have brighter front and rear running lamps at night. Brake lights brighten further, and vehicle lighting remains independent of compromised streetlights. AI road containment runs on physics steps using Rigidbody positions, so render interpolation cannot delay a road-boundary correction.

Roads use photographed asphalt with darker coloring, subtle wheel-path polishing and resurfacing patches that follow the actual curves. Lane paint and crosswalk paint have small worn areas. City sidewalks extend to their foundations and have matching collision surfaces, with decorative drainage grates along road edges. Ordinary traffic shares six metallic paint finishes. A camera lighting pass softens bright highlights and adds restrained lamp glow before the HUD is drawn, preserving readable navigation and text.

Manual recovery handles ordinary off-road trouble. A stationary overturned car or a car applying throttle while stuck automatically recovers after seven seconds. Recovery is disabled after entering the broken-bridge fall zone; driving off the unfinished deck causes an actual gravity-driven fall and mission failure. Ocean loss also ends the mission. Visible guardrails remain physical obstacles; unguarded roads allow off-road exploration.

RSUs use a 115-meter gameplay coverage radius. Contact changes with distance and can be attacked. A satellite-denial defense switches to authenticated roadside references plus odometry; it does not stop the jammer. Outside RSU range, estimated position can drift while cached roads stay visible. When RSU loss overlaps satellite loss, repair the roadside link first. Real DSRC is a communication link; surveyed-reference positioning here is an educational assumption, not a DSRC guarantee.

## Garage, saves and achievements

First completion of each story operation grants 180 credits; freeplay wins grant 40. Three upgrade lines have three tiers: tires improve grip, reinforcement reduces impact damage, and the secure console extends diagnostic time. Costs are 100/200/300 credits. Twenty-two achievements include first-time containment of all thirteen attacks, drift, safe driving, hidden cache and Extreme driver. Extreme driver requires winning Extreme freeplay, containing every available day/night attack, and hitting no pedestrians.

Progress, badges, credits, upgrades and music volume save atomically with a backup at:
`%USERPROFILE%/AppData/LocalLow/CyberCarLearning/CyberCarGame/driver-profile-v2.json`.
The stable filename now contains schema version 4. Version 2 and 3 profiles and legacy PlayerPrefs migrate without deleting earlier progress; mission records initialize empty. A corrupt primary file falls back to `.bak`. Mid-race positions and unfinished diagnostics are not saved; saves are local, not cloud synchronized.

## Attacker training

The local lab drives a courier with AI while you spend twelve points on attacks (two per launch). Delay arrival or disable the courier; the courier attempts a staged defense every twenty seconds. This is a local educational challenge. Multiplayer is not implemented. The main attack director uses a small contextual learning table, not a pretrained neural driving model.

## Assets and project

CC0 Poly Haven PBR surfaces, panoramas, rock, pine and streetlamp models were verified and processed through Blender. Original sources, hashes and author credits are retained under `ArtSource/Vendor` and `Docs/PolyHavenManifest.json`. The car now uses curved Blender coachwork, detailed tire tread, rims, brake hardware, mirrors and glazing. The player mesh has 78,428 triangles; traffic uses a 23,716-triangle derivative. Both retain thirteen mesh objects. Building glass has mullions, varied panes and night windows, with additional entrances and neighborhood buildings. These remain authored game assets rather than a claim of full photorealism. Original generated synthwave music and synthesized duck/tire/engine audio contain no third-party recordings.

Canonical project: `C:/Users/schmu/unityProjects/CyberCarGame`. GitHub Desktop's existing `C:/Users/schmu/AndroidStudioProjects/CyberCar` directory junction points here. Remote: `https://github.com/z-schmunk/CyberCar.git`. Changes remain uncommitted and unpushed.

- `Assets`, `Packages`, `ProjectSettings`: Unity source; open `Assets/Scenes/CyberCar.unity`.
- `ArtSource`: Blender/source assets and generation scripts.
- `Builds/Windows`: playable build and regression captures.
- `Docs`: architecture, provenance, education references, validation.
- `Backups`: verified source archive and pre-change snapshots.
- `Tools/PackageSource.py`: refreshes and verifies the portable source zip.
- `Logs`: build and runtime evidence; `Launchers`: local shortcuts.

The latest original car source is `ArtSource/create_car_realistic.py` and `CyberInterceptor-Realism.blend`; `create_traffic_lod.py` derives the traffic mesh. The original earlier car source is retained. Blender studio preview and asset receipts are under `ArtSource` and `Docs`.

Use **CyberCar > Build Windows game** in Unity or `CyberCar.Editor.ProjectBuilder.Build` in batch mode. The builder regenerates the launch scene; preserve custom scenes separately. `-smokeTest` runs automated regression checks without writing real progression. See `Docs/Validation.md` for the exact verified build and limitations.
