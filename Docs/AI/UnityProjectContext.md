# Project context

New independent project at `C:/Users/schmu/unityProjects/CyberCarGame`; the prior CyberCar folder was not modified. Baseline: Unity-generated empty project, no inherited scripts/scenes, no source-control history or existing custom Console failures.

Unity 6000.6.0f1. Built-in renderer, Standard materials, linear color, legacy keyboard Input API, IMGUI scaled HUD. Windows x64 / Mono. No multiplayer, network service, third-party gameplay package or Unity MCP bridge. Editor operations run through Unity's batch CLI and the project's Editor builder. Blender 5.1.1 generates the original car FBX and blend source.

GameSession owns run state and composes WorldBuilder, VehicleController, TrafficAgent, AttackDirector and GameHud with direct references. RoadNetwork handles deterministic graph routing. ProgressStore owns versioned local PlayerPrefs keys under CyberCarGame.v1. RuntimeSmokeTest is opt-in using -smokeTest and disables progression writes. Reset and re-entry destroy prior map/car objects after deactivation.

Gameplay: six operations (orientation, spoofing, DoS, CAN injection, ransomware, Sybil), three maps (city, harbor, canyon), three freeplay difficulties. Completing mission six unlocks freeplay. Freeplay changes map and difficulty; no track editor. Attacks introduced newest-first; adaptive contextual-bandit exploration follows once each available threat has appeared. Road geometry and mission paths come from the same graph.

Rollback: remove or restore only this new project. No shared scenes, former game assets, system settings or other Unity project modified. Build command regenerates the sole launch scene and updates company/product, window, renderer quality and Windows backend settings for this project. Unity owns metadata generation.

Verification target: Windows player compiles; all maps connected; physics acceleration and collision; five attacks affect state and correct defense clears them; cooldown, pause, win/fail lifecycle; readable UI screenshots; source and Blender assets present. Human feel/balance and exhaustive traffic behavior remain separate from automated regression evidence.
