# Unity project context â€” September 14, 2026

Unity 6000.6.0f1, built-in renderer, Windows x64 Mono development build; legacy Input and IMGUI. Canonical project is C:/Users/schmu/unityProjects/CyberCarGame. Existing GitHub Desktop path is a junction to this directory. No commits/pushes by this change.

Runtime composition root: GameSession. Fifteen missions / four maps / thirteen attacks. StoryCampaign owns fixed named story routes; Freeplay retains random routes. RoadNetwork has sampled cubic curves, directed city corridors and nearest-road queries. WorldBuilder builds visible collider meshes, bridge gaps and scenery. NaturalLandscape follows roads and cuts an actual service-bridge pit. TrafficSignals controls city phases. RoadsideNetwork models short-range RSUs and degraded positioning. VehicleController has four raycast suspension contacts, unconstrained 3D rotation and smooth driving inputs. TrafficAgent handles road containment, groups, signals and congestion. LabCourier drives the local attacker-mode target.

AttackDirector handles schedules/records and accepts completed DefenseChallenge authorization only. DefenseChallenge requires three evidence-based selections; wrong answers cost time. GameHud presents circular rasterized cached-road navigation through RadarPainter, diagnostics, garage and reports. DriveMusic supplies music and locally synthesized injected duck audio. ProgressStore schema 3 retains filename driver-profile-v2.json and migrates schema 2, preserving badges/unlocks; adds credits/upgrades/reward flags.

Resources/Photographic contains licensed CC0 PBR assets and readable Blender-derived FBX models. PhotoSurface triplanar normals must use WorldNormalVector. Models preserve importer rotation. Runtime materials and owned meshes have cleanup. DaySky/NightSky use photographed HDRs; night adds procedural stars and independent headlights.

RuntimeSmokeTest drives real physics for collision, drift, bridge, off-road, reverse and fall cases; mission-route completion tests also use checkpoint teleportation. Never call these manual gameplay tests. Build logs and screenshots live in Logs and Builds/Windows. The build regenerates the launch scene. Source archives exclude Library/build products. Keep the user profile untouched during tests (Testing mode).


## September 15 visual/UI update

Detailed original Blender car replaces the old mesh while preserving wheel naming and runtime animation. AI uses a lighter derivative. Cars use layer 8 to exclude them from the environment reflection probe. `SceneReflections` updates after generation, movement or lighting changes. `ArchitecturalGlass`, `LandscapeBlend` and updated `CoastalWater` supply environment shading; `RoadsideVegetation` combines grounded shoulder grass into one non-colliding mesh.

GameHud displays only active defensive controls during driving and four encountered attacks per report page. Label/button fitting and `LayoutIssues` instrumentation check clipping during Repaint. `RuntimeSmokeTest` adds `VisualProof` and `UiProof` at 1280x720, 1024x768 and 1920x1080; `-smokeTest -uiTest` runs that focused subset. Final full regression: 160 passes in `Logs/RealismFinalRuntime.log`; build `Logs/RealismBuild2.log`. The user has no manual keyboard playthrough recorded by the agent. Retain the ordinary test-mode save protection.
