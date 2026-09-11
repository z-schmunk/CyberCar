import bpy,json,math,shutil
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[1]
source=root/'ArtSource/Vendor/PolyHaven-2026-09-11'
out=root/'Assets/Resources/Photographic'; models=out/'Models';models.mkdir(parents=True,exist_ok=True)
receipts=[]
for asset,name,budget in [('rock_face_01','RockFace',12000),('street_lamp_01','StreetLamp',10000),('pine_sapling_medium','PineSapling',90000)]:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.gltf(filepath=str(source/asset/(asset+'.gltf')))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 original=sum(len(o.data.polygons) for o in meshes)
 for obj in meshes:
  bpy.context.view_layer.objects.active=obj;obj.select_set(True)
  if original>budget:
   mod=obj.modifiers.new('Runtime triangle budget','DECIMATE');mod.ratio=budget/original
   bpy.ops.object.modifier_apply(modifier=mod.name)
  for poly in obj.data.polygons:poly.use_smooth=True
  obj.select_set(False)
 corners=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
 minimum=Vector(tuple(min(c[i] for c in corners) for i in range(3)));maximum=Vector(tuple(max(c[i] for c in corners) for i in range(3)))
 offset=Vector(((minimum.x+maximum.x)/2,(minimum.y+maximum.y)/2,minimum.z))
 # Apply grounding to root objects so imported hierarchy remains intact.
 for obj in bpy.context.scene.objects:
  if obj.parent is None:obj.location-=offset
 bpy.ops.object.select_all(action='SELECT')
 dest=models/(name+'.fbx')
 bpy.ops.export_scene.fbx(filepath=str(dest),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,path_mode='AUTO')
 tris=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
 if tris>budget*1.35 or tris==0:raise RuntimeError('Triangle budget failed '+asset+' '+str(tris))
 receipts.append({'source':asset,'output':str(dest.relative_to(root)),'triangles':tris,'bounds':[list(minimum),list(maximum)],'normalization':'Ground centered; original preserved; decimated for runtime'})
 for f in (source/asset/'textures').glob('*'):
  shutil.copy2(f,out/f.name)
 print('EXPORTED',asset,tris,flush=True)
(root/'Docs/BlenderNaturalAssets.json').write_text(json.dumps(receipts,indent=2))
