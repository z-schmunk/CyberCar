# Asset provenance — 2026-09-11

The star overlay is procedural and does not reproduce an astronomical star catalogue.

The original car, gameplay, road and terrain geometry, interface, shaders, music, engine/impact and tire-friction audio are first-party project work. This upgrade also includes **CC0 1.0 assets from Poly Haven**.

## Poly Haven source package

Photographed surfaces: Asphalt 02, Rocky Terrain 02, Aerial Beach 02 and Grass Path 2. Skies: Kloofendal 48d Partly Cloudy (Pure Sky) and Qwantani Night (Pure Sky). Models: Rock Face 01, Pine Sapling Medium and Street Lamp 01.

See [Poly Haven's license](https://polyhaven.com/license). The closed source package is ArtSource/Vendor/PolyHaven-2026-09-11. Its manifest records asset pages, authors, source URLs, sizes, source MD5 checks and SHA-256 digests. All 32 downloaded files were verified before texture admission. No paid service or credentials were used.

Tools/FetchNaturalAssets.py reproduces the download and verification. The game-dev plugin CLI was unavailable; verification was performed locally by this script, not by the plugin's package verifier.

ArtSource/prepare_natural_props.py imports the preserved glTF source into Blender 5.1.1, grounds the meshes and exports separate runtime FBX files. Runtime budgets: rock 12,000 triangles, streetlamp 10,000, pine 89,998. Original high-resolution geometry is retained. Docs/BlenderNaturalAssets.json records the derived files and their bounds.

Unity imports 2K surface maps and sky panoramas; model textures are 1K. Normal maps use normal-map import settings, roughness maps are linear, and imported solid meshes are readable for runtime collider creation. The runtime scene assigns the materials explicitly. Supplied props have been inspected in standalone captures; this does not imply every custom game asset is photorealistic.

## Original audio

Coastal Drive is an original 64-second, 120 BPM synthesized instrumental loop. ArtSource/create_music.py uses mathematical oscillators and seeded noise, without third-party samples or compositions. VehicleFeedback synthesizes the tire-friction loop. No third-party music attribution or purchased music rights are involved.

First-party source is supplied with the project; no blanket public repository license is assigned here. Unity runtime and built-in resources retain their applicable Unity terms. Binary source assets use the repository's Git LFS configuration.
