# Unity project context — 2026-09-11

Canonical folder: C:/Users/schmu/unityProjects/CyberCarGame. GitHub Desktop's historical AndroidStudioProjects/CyberCar path remains a junction to this folder. No commit or push was performed by this task.

Unity 6000.6.0f1, built-in renderer, linear color, keyboard input, IMGUI. Windows x64/Mono development build. Blender 5.1.1 prepares source meshes. CLI builds regenerate the launch scene and relevant player/quality settings; pixel light count is eight for night illumination.

GameSession owns nine operations, four maps and four freeplay difficulties with Day/Night selection. Mission 9 introduces lighting takeover. U isolates remote lighting control and restores a trusted local schedule. Headlights remain independent. Seven attacks are available by day, eight by night. Extreme requires containing every available type for the chosen time of day.

RoadNetwork uses irregular rural junctions, reduced cross-connectors, cubic curves, level junction approaches and elevated bridges. NaturalLandscape creates continuous terrain with graded road corridors and coastal shorelines. WorldBuilder shares sampled paths with physics, traffic and the local GPS. PhotographicMaterials owns per-world CC0 materials and runtime model instances. WorldEnvironment manages photographed skies, nearby streetlight pools and blackout state. VehicleFeedback controls lamps, drift smoke and tire sound; ChaseCamera avoids obstacles and varies field of view with speed.

GameHud draws a circular heading-up GPS with a 170 m local radius and clipped roads. It includes nine missions, eight defense buttons/guide topics and seventeen achievements. ProgressStore retains schema v2, resizes old badge arrays, preserves existing unlocks/freeplay and adds new slots without renumbering. It saves long-term progression, not mid-race position.

RuntimeSmokeTest: 90 assertions after this upgrade, including material/collider audits, sampled lane clearance, physical bridge driving, drift, collisions, all defenses, lighting loss/recovery, independent headlights and save recovery. Check Docs/Validation.md for the exact final run. Full map delivery tests teleport gates; human handling/balance and exhaustive world collision behavior remain unverified.

Source provenance: Docs/PolyHavenManifest.json and Docs/BlenderNaturalAssets.json. Original downloaded bytes remain in ArtSource/Vendor. The game-dev CLI was unavailable; local hash verification and Blender/Unity validation were used. Tools/FetchNaturalAssets.py and ArtSource/prepare_natural_props.py reproduce assets. Backups/Before-Natural-World-2026-09-11.zip preserves the pre-upgrade source.
