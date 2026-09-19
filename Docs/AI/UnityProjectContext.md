# Unity project context â€” September 14, 2026

Unity 6000.6.0f1, built-in renderer, Windows x64 Mono development build; legacy Input and IMGUI. Canonical project is C:/Users/schmu/unityProjects/CyberCarGame. Existing GitHub Desktop path is a junction to this directory. No commits/pushes by this change.

Runtime composition root: GameSession. Fifteen missions / four maps / thirteen attacks. StoryCampaign owns fixed named story routes; Freeplay retains random routes. RoadNetwork has sampled cubic curves, directed city corridors and nearest-road queries. WorldBuilder builds visible collider meshes, bridge gaps and scenery. NaturalLandscape follows roads and cuts an actual service-bridge pit. TrafficSignals controls city phases. RoadsideNetwork models short-range RSUs and degraded positioning. VehicleController has four raycast suspension contacts, unconstrained 3D rotation and smooth driving inputs. TrafficAgent handles road containment, groups, signals and congestion. LabCourier drives the local attacker-mode target.

AttackDirector handles schedules/records and accepts completed DefenseChallenge authorization only. DefenseChallenge requires three evidence-based selections; wrong answers cost time. GameHud presents circular rasterized cached-road navigation through RadarPainter, diagnostics, garage and reports. DriveMusic supplies music and locally synthesized injected duck audio. ProgressStore schema 3 retains filename driver-profile-v2.json and migrates schema 2, preserving badges/unlocks; adds credits/upgrades/reward flags.

Resources/Photographic contains licensed CC0 PBR assets and readable Blender-derived FBX models. PhotoSurface triplanar normals must use WorldNormalVector. Models preserve importer rotation. Runtime materials and owned meshes have cleanup. DaySky/NightSky use photographed HDRs; night adds procedural stars and independent headlights.

RuntimeSmokeTest drives real physics for collision, drift, bridge, off-road, reverse and fall cases; mission-route completion tests also use checkpoint teleportation. Never call these manual gameplay tests. Build logs and screenshots live in Logs and Builds/Windows. The build regenerates the launch scene. Source archives exclude Library/build products. Keep the user profile untouched during tests (Testing mode).


## September 15 visual/UI update

Detailed original Blender car replaces the old mesh while preserving wheel naming and runtime animation. AI uses a lighter derivative. Cars use layer 8 to exclude them from the environment reflection probe. `SceneReflections` updates after generation, movement or lighting changes. `ArchitecturalGlass`, `LandscapeBlend` and updated `CoastalWater` supply environment shading; `RoadsideVegetation` combines grounded shoulder grass into one non-colliding mesh.

GameHud displays only active defensive controls during driving and four encountered attacks per report page. Label/button fitting and `LayoutIssues` instrumentation check clipping during Repaint. `RuntimeSmokeTest` adds `VisualProof` and `UiProof` at 1280x720, 1024x768 and 1920x1080; `-smokeTest -uiTest` runs that focused subset. Final full regression: 160 passes in `Logs/RealismFinalRuntime.log`; build `Logs/RealismBuild2.log`. The user has no manual keyboard playthrough recorded by the agent. Retain the ordinary test-mode save protection.

## September 17 mission polish

AttackRecord now owns diagnostic stage, evidence, choices and answer. DefenseChallenge selects an encounter without resetting it; expiry cannot authorize a new encounter of the same type. Final recovery stays at stage two on cooldown and authorizes only the synchronous Defend call. Mistakes are counted per run. Steps are verified selections; gameplay effects still end only after the complete defense.

MissionRating snapshots delivery, safe-driving and security results at Finish. Story personal bests prioritize stars, then elapsed time at the same rating. ProgressStore schema 4 adds MissionBest[15], keeps the stable profile filename, migrates versions 2/3 and normalizes invalid/partial record arrays. Test mode blocks live record writes. Freeplay ratings are not pooled into story records; attacker lab receives no rating.

GameHud shows stored ratings in mission selection/briefing and run ratings in reports. Diagnostics show per-threat progress. Guide, pause and garage render only the active screen's controls. VehicleFeedback owns per-car front/rear emissive lamp materials; night running lamps remain independent of blackout, with stronger braking emission. TrafficAgent contains AI cars in FixedUpdate using Rigidbody.position, and plans after recovery from the same physical pose.

RuntimeSmokeTest adds MissionProof (diagnostic continuity, dependent RSU repair, ratings, schema migration/backup, actual lamp materials), TrafficProof (three recovery positions at 30/120 target fps), and threat-card/pause layouts at three sizes. Focused flags require -smokeTest: -missionTest and -trafficTest. See current Docs/Validation.md for final run evidence.

VehicleLamp is an explicit Resources shader for emissive lens meshes. Runtime Standard emission properties alone passed a material-state check but remained dark in the player capture; the dedicated shader fixes the rendered output. MissionProof also reads the rendered rear-lamp bounds and requires visible red pixels during a blackout. Test camera lookup uses FindAnyObjectByType, avoiding the deprecated ordering-dependent API in Unity 6000.6.

## September 18 road and lighting presentation

WorldBuilder road ribbons carry lateral meters and cumulative path distance in UV2 plus endpoint fades in vertex red. RoadSurface retains existing licensed asphalt textures and uses those coordinates for traffic wear and repair patches. VisualUpgrade assigns a separate RoadPaint cutout material only to road lines/crosswalks. City sidewalk thickness reaches the foundation while retaining the same top height. StreetDetails builds all decorative city drain covers into one vertex-colored mesh with RoadFurniture; it adds no colliders and owns its mesh/material. TrafficFinish lazily creates six shared metallic body paints owned by the world.

DrivingPresentation is attached to the existing chase camera. Its built-in-pipeline OnRenderImage performs highlight extraction, two reduced-resolution blur passes and a restrained film response/composite. Two temporary targets are released in finally; bloom width is capped at 640. The material is owned by the camera and destroyed with it. IMGUI draws after this pass. Unsupported shaders fall back to a plain blit. No rendering packages, new external assets or save schema changes were introduced.

PresentationProof verifies material support, path coordinates, markings on all four maps, grounded city sidewalks, combined drains, shared traffic paint variety, a framebuffer change from the camera pass and bounded glow targets. It captures presentation-map-0..3 and presentation-street-detail. The focused entry point is -smokeTest -presentationTest; the full regression also runs this proof before the existing UI matrix.

City sidewalk segments now have visible matching BoxColliders, with their top still 0.1 m above the road. PresentationProof raycasts their tops to verify visual/physical agreement. Drain grates remain non-colliding decorative surface detail. Earlier physical-road lane audits still apply to the added sidewalk colliders.
