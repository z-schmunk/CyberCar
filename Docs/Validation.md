# Validation — September 15, 2026

**160 automated checks passed** in the exact delivered Windows build. No runtime errors or exceptions, C# compilation errors/warnings, or shader errors were observed. `git diff --check` passed.

- Build: `Logs/RealismBuild2.log`, Unity 6000.6.0f1, Windows x64 Mono development build; 276,303,694 bytes reported by Unity.
- Full run: `Logs/RealismFinalRuntime.log` ends with `CYBERCAR_SMOKE_SUCCESS / 160`.
- Focused visual/UI run: `Logs/RealismUi2.log`, 28 checks passed.
- Car production: `Logs/RealismCarFinal.log`, `Docs/RealismCarReceipt.json` and `Docs/TrafficCarReceipt.json`. Blender production passed geometry bounds/triangle/object limits. Blender reported forward-looking API deprecation warnings, not export failures.

## Gameplay regression

The prior 132 checks still pass: physical driving, collisions and momentum transfer, drifting, reverse motion, curved bridge traversal, actual bridge falls, off-road suspension, automatic rollover recovery, visible world-collider audits and clear sampled lanes across four maps. They also cover thirteen staged cyber defenses, RSU range/fallback, sensor obstacles, wrong answers, one-way routing, traffic signal phases, AI containment, local attacker budget and an actual AI courier delivery, story graph destinations, persistence and backup recovery, pedestrians, Extreme eligibility and day/night lighting behavior. Campaign completion checks include checkpoint teleportation; they are not all full driving playthroughs.

## Visual and interface checks

Four additional checks verify the detailed car's bounded mesh count, four animated wheels, environment reflection capture and slope-blended terrain. Twenty-four layout checks cover menu, garage, briefing, normal driving, a multi-attack diagnostic, field guide, after-action report and achievements at each of:

- 1280 × 720
- 1024 × 768
- 1920 × 1080

Rendered text height and control bounds are checked during GUI repaint. The first run caught the diagnostic close button wrapping because of its padding; compact-button padding was corrected. Runtime screenshots were inspected, including the 720p diagnostic, 4:3 report, city and cliff driving. Background panels were added beneath navigation/status text to maintain contrast over scenery. Active-threat controls and paginated reports use more room per item.

Evidence images are in `Builds/Windows/ui-*.png`, `realism-*.png`, and `smoke-*.png`. They come from the executable. `ArtSource/CyberInterceptor-Realism-preview.png` is explicitly a separate Blender studio render.

## Limits

Automated tests and inspected captures do not constitute a manual keyboard playthrough or exhaustive collision/performance testing. UI checks cover the listed sizes and states, not every window dimension or every possible string. The graphics are a realism upgrade; the world and car remain authored game approximations. No claim of full photorealism, production vehicle dynamics, multiplayer or real cybersecurity attacks is made. Existing saves remain protected during tests. No commit or push was performed.
