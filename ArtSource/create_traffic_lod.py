import bpy,json,hashlib
from pathlib import Path
r=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(r/'ArtSource/CyberInterceptor-Realism.blend'))
for o in bpy.context.scene.objects:
 if o.type!='MESH':continue
 bpy.context.view_layer.objects.active=o
 if len(o.data.polygons)>400:
  m=o.modifiers.new('Traffic mesh budget','DECIMATE');m.ratio=.28;bpy.ops.object.modifier_apply(modifier=m.name)
path=r/'Assets/Resources/Art/CyberInterceptorTraffic.fbx'
bpy.ops.export_scene.fbx(filepath=str(path),object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH')
assert triangles<30000
(r/'Docs/TrafficCarReceipt.json').write_text(json.dumps({'triangles':triangles,'source':'CyberInterceptor-Realism.blend','sha256':hashlib.sha256(path.read_bytes()).hexdigest()},indent=2))
print('TRAFFIC_LOD',triangles)
