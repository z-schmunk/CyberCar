"""Fetch a closed, verified CC0 asset set from Poly Haven. No credentials or paid services."""
import hashlib,json,urllib.request,concurrent.futures,shutil
from pathlib import Path
root=Path(__file__).resolve().parents[1]
ref=root/'ArtSource/Reference'
catalog=json.loads((ref/'polyhaven-catalog.json').read_text())
data=json.loads((ref/'polyhaven-files.json').read_text())
models=json.loads((ref/'polyhaven-model-files.json').read_text())
package=root/'ArtSource/Vendor/PolyHaven-2026-09-11'
package.mkdir(parents=True,exist_ok=True)
jobs=[];receipts=[]
for asset,items in data.items():
    if 'hdri' in items:
        f=items['hdri']['2k']['hdr'];jobs.append((asset,asset+'.hdr',f,True))
    else:
        for channel,out in [('Diffuse','diffuse'),('nor_gl','normal'),('Rough','rough')]:
            f=items[channel]['2k']['jpg'];jobs.append((asset,asset+'_'+out+'.jpg',f,True))
for asset,items in models.items():
    gltf=items['gltf']['1k']['gltf']
    jobs.append((asset,asset+'/'+asset+'.gltf',gltf,False))
    for name,f in gltf.get('include',{}).items():jobs.append((asset,asset+'/'+name,f,False))
def fetch(job):
    asset,name,f,admit=job;p=package/name;p.parent.mkdir(parents=True,exist_ok=True)
    raw=p.read_bytes() if p.exists() else urllib.request.urlopen(urllib.request.Request(f['url'],headers={'User-Agent':'CyberCarGame/1.0 (local educational game)'}),timeout=90).read()
    if len(raw)!=f['size'] or hashlib.md5(raw).hexdigest()!=f['md5']:raise ValueError('Integrity failure '+name)
    p.write_bytes(raw)
    return {'asset':asset,'file':name,'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest(),'source_url':f['url'],'source_md5':f['md5'],'admit_texture':admit}
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:receipts=list(pool.map(fetch,jobs))
metadata={'name':'CyberCar photographed surfaces and props','version':'2026-09-11','license':'CC0-1.0','license_url':'https://polyhaven.com/license','source':'Poly Haven','assets':{a:{'page':'https://polyhaven.com/a/'+a,'authors':catalog[a].get('authors',{}),'dimensions':catalog[a].get('dimensions')} for a in sorted({r['asset'] for r in receipts})},'files':receipts}
(package/'manifest.json').write_text(json.dumps(metadata,indent=2))
(package/'LICENSE.txt').write_text('Assets from Poly Haven are CC0 1.0: https://polyhaven.com/license\nhttps://creativecommons.org/publicdomain/zero/1.0/\nAuthors and source URLs are preserved in manifest.json.\n')
# Verify every source byte before admitting textures to the exact Unity resource directory.
destination=root/'Assets/Resources/Photographic';destination.mkdir(parents=True,exist_ok=True)
for r in receipts:
    p=package/r['file'];assert hashlib.sha256(p.read_bytes()).hexdigest()==r['sha256']
    if r['admit_texture']:
        target=destination/r['file']
        if target.exists() and hashlib.sha256(target.read_bytes()).hexdigest()!=r['sha256']:raise ValueError('Destination collision '+str(target))
        shutil.copy2(p,target);assert hashlib.sha256(target.read_bytes()).hexdigest()==r['sha256']
(root/'Docs/PolyHavenManifest.json').write_text(json.dumps(metadata,indent=2))
print('Verified',len(receipts),'files;',sum(r['bytes'] for r in receipts),'bytes; CC0 source package retained.')
