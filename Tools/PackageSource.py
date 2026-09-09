"""Create a portable source backup in this project's Backups folder."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import hashlib
import json

root = Path(__file__).resolve().parents[1]
destination = root / 'Backups'
destination.mkdir(exist_ok=True)
archive = destination / 'CyberCarGame-Source.zip'
files = []
for folder in ('Assets', 'Packages', 'ProjectSettings', 'ArtSource', 'Docs', 'Tools'):
    files.extend(p for p in (root / folder).rglob('*')
                 if p.is_file() and not p.name.endswith(('.blend1', '.pyc'))
                 and '__pycache__' not in p.parts)
files.extend(root / name for name in ('README.md', '.gitignore', '.gitattributes')
             if (root / name).is_file())
with ZipFile(archive, 'w', ZIP_DEFLATED, compresslevel=6) as package:
    for path in files:
        package.write(path, Path('CyberCarGame') / path.relative_to(root))
with ZipFile(archive) as package:
    if package.testzip() is not None:
        raise RuntimeError('Backup verification failed')
receipt = {
    'source_files': len(files),
    'source_zip_bytes': archive.stat().st_size,
    'source_sha256': hashlib.sha256(archive.read_bytes()).hexdigest(),
    'executable': 'Builds/Windows/CyberCarGame.exe',
}
(destination / 'Delivery.json').write_text(json.dumps(receipt, indent=2), encoding='utf-8')
print('Verified source backup:', archive)
