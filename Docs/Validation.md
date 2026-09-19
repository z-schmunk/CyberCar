# Validation — September 18, 2026: road and lighting presentation

**224 automated checks passed** in the delivered Windows build. No C# compilation errors/warnings, shader errors, or runtime errors/exceptions were observed in the final build/run. Git whitespace validation passed.

- Build: Logs/PresentationBuild3.log; Unity 6000.6.0f1, Windows x64 Mono development build; 276,521,800 bytes reported by Unity.
- Full regression: Logs/PresentationFinalRuntime3.log ends with CYBERCAR_SMOKE_SUCCESS / 224.
- Earlier presentation candidate: Logs/PresentationFocused1.log, 46 checks. Final full coverage includes the later drainage and sidewalk support additions.
- Previous mission-update validation: Docs/Validation-Mission-2026-09-18.md.

## Changes and evidence

RoadSurface reuses the existing licensed photographed asphalt textures. Mesh UV2 coordinates carry lateral meters and cumulative path distance, so subtle tire wear and resurfacing patches follow curved roads. Markings use a separate worn-paint shader. Intersections retain the same darker asphalt color family. All four maps, including an Extreme coastal map, passed material support, continuous wear-coordinate and marking checks.

City sidewalks now extend to their foundation, with unchanged top height and matching BoxCollider surfaces. Raycasts verify collider/visible-top alignment. Full lane-clearance and visible-collider audits pass with these colliders. Decorative drain grates are combined into one mesh with no collision shapes. Traffic uses six shared metallic finishes; the runtime verifies visible ordinary cars use at least four distinct finishes.

DrivingPresentation adds highlight extraction, two reduced-resolution blur passes and restrained film response before IMGUI. Bloom width is capped at 640 pixels, and temporary render targets are released after each frame. A before/after framebuffer check confirms the pass affects the rendered world. The previous blackout lamp pixel test still passes with the camera effect enabled. This is a visual treatment, not a new render pipeline or a physically calibrated camera.

The 18 new presentation checks supplement the prior 206 checks. The full suite still covers physical acceleration/collisions, reverse/drift, off-road suspension, bridge driving/falls, traffic containment, pedestrians, thirteen staged cyber defenses, dependent RSU repair, mission ratings, save migration/backup recovery, achievements and actual AI courier delivery.

## UI and visual review

Thirty UI checks pass at 1280x720, 1024x768 and 1920x1080: menu, garage, briefing, driving, threat cards, pause, diagnostics, guide, reports and achievements. Bounds and text height are checked during repaint. IMGUI remains outside the camera lighting pass.

Executable captures were inspected for city road wear, grounded sidewalk edges, drainage, coastal night lighting, and readable HUD/navigation. Evidence: Builds/Windows/presentation-map-0.png through presentation-map-3.png, presentation-street-detail.png, and ui-*.png. These are runtime captures, not mockups.

## Delivery and limits

All source and assets remain under CyberCarGame in the existing repository. Before-change backup: Backups/Before-Road-Presentation-2026-09-18.zip. Refreshed source backup is ZIP-verified; Backups/Delivery.json records its SHA-256 and the delivered assembly hash. No commit or push was performed. No new external assets, paid services, packages or save schema changes were introduced. Automated tests do not write the player's progress.

This is an incremental realism pass. Existing authored geometry and arcade driving remain. Automated tests and screenshot inspection do not constitute a manual keyboard playthrough or a GPU performance benchmark. Some campaign-completion checks use checkpoint teleportation. Decorative drain grates have no collision shapes; the underlying road supplies the physical surface. Only the existing Windows built-in-renderer target was validated.
