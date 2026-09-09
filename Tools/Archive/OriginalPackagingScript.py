from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import hashlib, shutil, json

root=Path(r'C:\Users\schmu\unityProjects\CyberCarGame')
out=Path(r'C:\Users\schmu\Documents\Codex\2026-09-09\computer-plugin-computer-use-openai-bundled\outputs')
archive=out/'CyberCarGame-Source.zip'
files=[]
for folder in ('Assets','Packages','ProjectSettings','ArtSource','Docs'):
    files.extend(p for p in (root/folder).rglob('*') if p.is_file() and not p.name.endswith('.blend1'))
files.extend([root/'README.md',root/'.gitignore'])
with ZipFile(archive,'w',ZIP_DEFLATED,compresslevel=6) as z:
    for p in files:z.write(p,Path('CyberCarGame')/p.relative_to(root))
with ZipFile(archive) as z:
    assert z.testzip() is None
    assert 'CyberCarGame/Assets/Scenes/CyberCar.unity' in z.namelist()
shutil.copy2(root/'README.md',out/'CyberCarGame-Guide.md')
shutil.copy2(root/'Docs/Validation.md',out/'CyberCarGame-Validation.md')
for name in ('smoke-gameplay.png','smoke-menu.png','smoke-report.png','smoke-map-0.png','smoke-map-1.png','smoke-map-2.png','smoke-results.txt'):
    shutil.copy2(root/'Builds/Windows'/name,out/name.replace('smoke-','CyberCarGame-'))
receipt={'source_files':len(files),'source_zip_bytes':archive.stat().st_size,'source_sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'executable':str(root/'Builds/Windows/CyberCarGame.exe')}
(out/'CyberCarGame-Delivery.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))
