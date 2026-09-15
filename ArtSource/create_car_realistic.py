"""Original, meter-scale interceptor body; no external models. Blender 5.x."""
import bpy, bmesh, math, os, json, hashlib
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(name,color,metal=0,rough=.4):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=rough;return m
paint=mat('BodyPaint',(.02,.16,.18),.72,.23);glass=mat('Glass',(.024,.042,.056),.48,.09);rubber=mat('Rubber',(.018,.021,.024),0,.82);trim=mat('Alloy',(.44,.48,.51),.93,.2);dark=mat('DarkTrim',(.025,.03,.036),.3,.33);red=mat('Taillight',(.5,.009,.006),.15,.2);light=mat('Headlight',(.83,.93,1),.35,.14);brake=mat('BrakeCaliper',(.36,.035,.012),.5,.35)
def finish(o,name,m,smooth=True):
 o.name=name;o.data.materials.append(m)
 if smooth:
  for f in o.data.polygons:f.use_smooth=True
 return o
def box(name,p,s,m,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);finish(o,name,m,False)
 if bevel:
  mod=o.modifiers.new('Rolled edge','BEVEL');mod.width=bevel;mod.segments=3;bpy.ops.object.modifier_apply(modifier=mod.name)
  mod=o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
def cylinder(name,p,r,depth,m,vertices=64):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=p,rotation=(0,math.pi/2,0));return finish(bpy.context.object,name,m)
def mesh(name,verts,faces,m):
 data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update();bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(data);bm.free();o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o);return finish(o,name,m)
def tube(name,points,r,m):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=12;c.bevel_depth=r;c.bevel_resolution=3
 if name=='Window seal':
  s=c.splines.new('POLY');s.points.add(len(points)-1)
  for b,p in zip(s.points,points):b.co=(*p,1)
 else:
  s=c.splines.new('BEZIER');s.bezier_points.add(len(points)-1)
  for b,p in zip(s.bezier_points,points):b.co=p;b.handle_left_type='AUTO';b.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);bpy.context.collection.objects.link(o);o.data.materials.append(m);bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o.select_set(False);return o
# +Y forward / +Z up. A continuous loft replaces stacked boxes.
rings=[(-2.35,.80,.43,.91),(-2.18,.99,.40,1.02),(-1.48,1.07,.38,1.12),(-.70,1.045,.38,1.14),(.20,1.035,.38,1.10),(.95,1.025,.40,1.04),(1.50,1.04,.42,1.015),(2.14,.995,.44,.98),(2.34,.82,.48,.90)]
verts=[];faces=[]
for y,w,b,t in rings:
 for x,z in [(-w*.9,b),(-w,b+.12),(-w,t-.13),(-w*.90,t-.025),(-w*.5,t+.02),(0,t+.035),(w*.5,t+.02),(w*.9,t-.025),(w,t-.13),(w,b+.12),(w*.9,b),(0,b)]:verts.append((x,y,z))
N=12
for i in range(len(rings)-1):
 for j in range(N):faces.append((i*N+j,i*N+(j+1)%N,(i+1)*N+(j+1)%N,(i+1)*N+j))
faces.extend([tuple(range(N-1,-1,-1)),tuple((len(rings)-1)*N+j for j in range(N))]);body=mesh('Sculpted monocoque',verts,faces,paint)
bpy.context.view_layer.objects.active=body;sub=body.modifiers.new('Coachwork curvature','SUBSURF');sub.levels=2;bpy.ops.object.modifier_apply(modifier=sub.name)
for x in [-1.04,1.04]:
 for y in [-1.46,1.48]:
  cutter=cylinder('Arch cutter',(x,y,.49),.535,.7,rubber,96);bpy.context.view_layer.objects.active=body;mod=body.modifiers.new('Open tire arch','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter;bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cutter,do_unlink=True)
mod=body.modifiers.new('Panel edge radii','BEVEL');mod.width=.015;mod.segments=3;bpy.context.view_layer.objects.active=body;bpy.ops.object.modifier_apply(modifier=mod.name)
# Glazed greenhouse and curved painted roof, attached A/C pillars and window seals.
mesh('Windshield',[(-.88,.94,1.075),(.88,.94,1.075),(.755,.19,1.58),(-.755,.19,1.58)],[(0,1,2,3)],glass)
mesh('Rear glass',[(-.755,-.62,1.61),(.755,-.62,1.61),(.89,-1.36,1.125),(-.89,-1.36,1.125)],[(0,1,2,3)],glass)
roof=mesh('Crowned roof',[(-.755,.22,1.59),(0,.23,1.665),(.755,.22,1.59),(-.765,-.22,1.645),(0,-.22,1.7),(.765,-.22,1.645),(-.755,-.65,1.61),(0,-.67,1.665),(.755,-.65,1.61)],[(0,1,4,3),(1,2,5,4),(3,4,7,6),(4,5,8,7)],paint)
for sign in [-1,1]:
 pts=[(sign*.885,.88,1.085),(sign*.758,.18,1.585),(sign*.758,-.64,1.615),(sign*.905,-1.33,1.13)]
 mesh('Side glazing',pts,[(0,1,2,3)],glass)
 tube('Window seal',pts+[pts[0]],.007,dark)
 mesh('A pillar panel',[(sign*.88,.96,1.075),(sign*.94,.92,1.075),(sign*.805,.20,1.59),(sign*.755,.20,1.59)],[(0,1,2,3)],paint)
 mesh('C pillar panel',[(sign*.755,-.62,1.615),(sign*.805,-.69,1.60),(sign*.98,-1.40,1.09),(sign*.89,-1.34,1.125)],[(0,1,2,3)],paint)
 tube('B pillar',[(sign*.9,-.33,1.115),(sign*.765,-.33,1.63)],.04,dark)
 tube('Door seam',[(sign*1.035,-.68,.51),(sign*1.037,-.70,.85),(sign*.985,-.72,1.095)],.008,rubber)
 tube('Sill',[(sign*1.025,-.90,.43),(sign*1.03,0,.40),(sign*1.02,.91,.44)],.045,dark)
 box('Flush door handle',(sign*1.008,-.50,1.027),(.035,.23,.04),trim,.014)
 tube('Mirror mount',[(sign*.9,.69,1.21),(sign*1.10,.69,1.19),(sign*1.19,.66,1.22)],.037,dark)
 box('Mirror housing',(sign*1.20,.65,1.22),(.25,.31,.15),paint,.065)
 box('Mirror glass',(sign*1.20,.493,1.23),(.20,.014,.095),glass,.022)
 # High-resolution tires, deep rims, brake disc and ten swept alloy spokes.
 for y in [-1.46,1.48]:
  parts=[];x=sign*1.035
  bpy.ops.mesh.primitive_torus_add(major_segments=80,minor_segments=16,major_radius=.355,minor_radius=.115,location=(x,y,.49),rotation=(0,math.pi/2,0));parts.append(finish(bpy.context.object,'Tire',rubber))
  parts.append(cylinder('Sidewall',(x,y,.49),.435,.215,rubber))
  parts.append(cylinder('Rim barrel',(x+sign*.09,y,.49),.326,.075,dark))
  parts.append(cylinder('Brake disc',(x+sign*.126,y,.49),.273,.012,trim))
  for radius in [.323,.296]:
   bpy.ops.mesh.primitive_torus_add(major_segments=64,minor_segments=8,major_radius=radius,minor_radius=.013,location=(x+sign*.15,y,.49),rotation=(0,math.pi/2,0));parts.append(finish(bpy.context.object,'Rim lip',trim))
  for n in range(10):
   angle=n*math.tau/10
   spoke=box('Swept spoke',(x+sign*.153,y+math.sin(angle)*.18,.49+math.cos(angle)*.18),(.028,.036,.24),trim,.01);spoke.rotation_euler.x=-angle+.14;parts.append(spoke)
  parts.append(cylinder('Hub',(x+sign*.17,y,.49),.075,.035,dark,32))
  for n in range(5):
   angle=n*math.tau/5;parts.append(cylinder('Lug',(x+sign*.19,y+math.sin(angle)*.053,.49+math.cos(angle)*.053),.012,.013,trim,12))
  # Shallow tread grooves use dark shoulder strips, no expensive texture fetch.
  for n in range(36):
   a=n*math.tau/36;groove=box('Tread',(x,y+math.sin(a)*.465,.49+math.cos(a)*.465),(.17,.016,.008),dark,.002);groove.rotation_euler.x=-a;parts.append(groove)
  bpy.ops.object.select_all(action='DESELECT')
  for o in parts:o.select_set(True)
  bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='Wheel_'+str(sign)+'_'+str(y);bpy.context.scene.cursor.location=(x,y,.49);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
  box('Caliper',(x+sign*.13,y+.20,.51),(.075,.09,.26),brake,.025)
# Lamps, grille, shut lines and a restrained integrated rear lip.
for x in [-.62,.62]:
 box('Lamp enclosure',(x,2.245,.92),(.66,.12,.16),dark,.04)
 box('Headlight',(x,2.311,.952),(.60,.018,.035),light,.014)
 box('Headlight projector',(x*.82,2.318,.895),(.16,.023,.06),light,.015)
 box('Rear enclosure',(x,-2.224,1.00),(.69,.12,.14),dark,.03)
 box('Taillight',(x,-2.288,1.025),(.63,.02,.045),red,.015)
 box('Exhaust trim',(x,-2.32,.48),(.24,.16,.12),trim,.035)
box('Front lower intake',(0,2.31,.635),(1.50,.055,.19),dark,.035)
for x in [-.65+i*.10 for i in range(14)]:box('Intake grille fin',(x,2.345,.635),(.019,.018,.155),trim,.004)
box('Rear diffuser',(0,-2.305,.44),(1.28,.15,.12),dark,.025)
for x in [-.48,-.24,0,.24,.48]:box('Diffuser strake',(x,-2.27,.42),(.025,.30,.13),dark,.012)
tube('Integrated rear lip',[(-.90,-2.03,1.065),(0,-2.10,1.11),(.90,-2.03,1.065)],.027,paint)
box('Rear light bar',(0,-2.33,1.012),(.57,.018,.025),red,.009)
for y in [-2.354,2.358]:box('Number plate',(0,y,.79),(.48,.022,.13),trim,.01)
tube('Hood shut line',[(-.86,.92,1.074),(-.86,1.8,1.015),(-.70,2.15,.99)],.006,dark)
tube('Hood shut line',[(.86,.92,1.074),(.86,1.8,1.015),(.70,2.15,.99)],.006,dark)
# Join static pieces by material to keep cars inexpensive in heavy traffic.
for m in list(bpy.data.materials):
 objects=[o for o in bpy.context.scene.objects if o.type=='MESH' and not o.name.startswith('Wheel_') and len(o.data.materials)==1 and o.data.materials[0]==m]
 if not objects:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();bpy.context.object.name=m.name if m.name in ['Headlight','Taillight'] else 'Coachwork_'+m.name
bpy.ops.object.select_all(action='DESELECT')
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in objects)
coords=[o.matrix_world@Vector(c) for o in objects for c in o.bound_box]
bounds=[[min(c[i] for c in coords),max(c[i] for c in coords)] for i in range(3)]
assert triangles<110000 and len(objects)<=20,(triangles,len(objects))
assert bounds[2][0]>-.05 and bounds[2][1]<1.8,bounds
blend=os.path.join(ROOT,'ArtSource','CyberInterceptor-Realism.blend');fbx=os.path.join(ROOT,'Assets','Resources','Art','CyberInterceptor.fbx')
bpy.ops.wm.save_as_mainfile(filepath=blend)
bpy.ops.export_scene.fbx(filepath=fbx,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
# Offscreen studio preview is separate from runtime validation.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,0));floor=bpy.context.object;floor.data.materials.append(mat('Preview floor',(.075,.09,.10),0,.5))
world=bpy.context.scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.28,.34,.4,1);world.node_tree.nodes['Background'].inputs[1].default_value=.45
for location,power,size in [((4,-3,7),1700,5),((-4,1,4),1100,4),((0,5,7),1400,4)]:
 bpy.ops.object.light_add(type='AREA',location=location);lamp=bpy.context.object;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=size;lamp.rotation_euler=(Vector((0,0,.7))-lamp.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(6.6,-8.1,4));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,.78))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=60;bpy.context.scene.camera=camera
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40;scene.cycles.use_denoising=True;scene.render.resolution_x=1280;scene.render.resolution_y=800;scene.render.resolution_percentage=100;scene.render.filepath=os.path.join(ROOT,'ArtSource','CyberInterceptor-Realism-preview.png');bpy.ops.render.render(write_still=True)
receipt={'triangles':triangles,'mesh_objects':len(objects),'bounds_xyz_m':bounds,'fbx_sha256':hashlib.sha256(open(fbx,'rb').read()).hexdigest(),'source':'Original procedural Blender geometry; no external asset','date':'2026-09-15'}
open(os.path.join(ROOT,'Docs','RealismCarReceipt.json'),'w').write(json.dumps(receipt,indent=2));print('REALISM_CAR_EXPORTED',receipt)
