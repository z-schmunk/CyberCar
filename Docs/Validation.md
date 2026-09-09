# Validation record

September 9, 2026. Windows x64, Unity 6000.6.0f1, Mono development player. Blender 5.1.1. Fresh project baseline; previous CyberCar project untouched.

## Evidence

- Unity compiled all runtime and Editor scripts and produced `Builds/Windows/CyberCarGame.exe` and its dependencies.
- `CyberCar.Editor.ProjectBuilder.Build` created the startup scene and checked every map node is reachable from its start.
- The standalone executable ran its opt-in `-smokeTest` suite: **41 checks passed**, with **zero runtime errors or exceptions** on the final gameplay implementation.
- PhysX actually accelerated the car, collided it with a wall, and collided two dynamic cars with momentum transfer. These were real physics steps, not mocked collision callbacks.
- Every attack activated. Spoofing redirected GPS to the hazard, DoS hid the GPS gate, injection changed steering, and ransomware reduced engine power. Correct defense cleared each attack, isolation/backup restored control, and cooldowns rejected duplicate activation.
- Pause froze mission time. Checkpoint traversal completed all three maps; those route-completion checks repositioned the car at each checkpoint and do not represent a full manual drive.
- Destruction, cliff fall and timer expiry produced failure. Uncontained attacks remained in reports as missed. Each campaign operation scheduled its newly introduced attack first.
- Runtime screenshots were inspected for the three maps, HUD and after-action report at 1440 x 900. Initial visual review identified the imported car's reversed direction; corrected with an explicit visual rotation. Improved text contrast and aligned report columns.
- 19 Unity metadata files checked; no duplicate GUIDs. Startup scene is enabled in build settings.

## Fixed during validation

The first player run revealed a shader stripped from the dynamic impact effect. A serialized Resources material now includes it in builds; subsequent runtime logs contain no exceptions. The initial hidden-window screenshot attempt failed; visible standalone runs successfully captured the game. The Computer Use helper could not start (`failed to launch codex app-server: program not found`) after prescribed retries/reset; no manual keyboard/mouse UI automation was claimed.

## Remaining limits

- Human playtesting of handling, campaign balance, route readability and long traffic encounters remains necessary.
- Automated controls assign gameplay input values; physical keyboard/button interaction was not independently exercised through desktop automation.
- Unlock and achievement persistence is implemented through versioned PlayerPrefs. Tests intentionally skip writes to player progression; persistence across full application restarts was not exercised.
- No gamepad/mobile, neural-network training, ML-Agents, soft-body damage or network attack integration.
- Visual review covers the sampled runtime views; it is not an exhaustive inspection of every road location or resolution.
- No measured performance budget or production release certification is claimed.

## Reproduce

Use **CyberCar > Build Windows game** in Unity, or run the Editor with `-batchmode -nographics -quit -projectPath C:/Users/schmu/unityProjects/CyberCarGame -executeMethod CyberCar.Editor.ProjectBuilder.Build -logFile PATH`.

Run `Builds/Windows/CyberCarGame.exe -smokeTest -screen-width 1440 -screen-height 900 -screen-fullscreen 0 -logFile PATH`. The test exits with 0 on success and 2 on a failed check. Read `smoke-results.txt` and the log; do not infer success from a stale result file. Runtime captures are beside the executable.

Logs: `Logs/BuildDelivery.log` and `Logs/RuntimeDelivery.log`; earlier attempts retained for diagnosis. The final delivery build changes only report layout from the preceding successful gameplay suite; the delivery suite is rerun before handoff.

## Status

Playable prototype validated by a Windows build, runtime checks and sampled visual inspection. Manual acceptance/playtesting remains open.
