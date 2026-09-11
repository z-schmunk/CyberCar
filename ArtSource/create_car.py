import bpy, math, os
from mathutils import Vector

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

def material(name, color, metallic=0, rough=.4):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Metallic'].default_value=metallic; p.inputs['Roughness'].default_value=rough
    return m
paint=material('BodyPaint',(.025,.55,.57),.65)
glass=material('Glass',(.018,.045,.07),.7,.16)
black=material('Rubber',(.013,.017,.025),0,.8)
metal=material('Alloy',(.3,.38,.42),.85)
light=material('Headlight',(.7,1,1),.3)
red=material('Taillight',(1,.045,.08),.2)

def box(name,loc,scale,mat,bevel=.05):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object; o.name=name; o.dimensions=scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    o.data.materials.append(mat)
    if bevel:
        b=o.modifiers.new('Machined edges','BEVEL'); b.width=bevel; b.segments=4
        bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=b.name)
    return o

# Blender: +Y is forward, +Z up. Unity export converts to +Z forward, +Y up.
box('Chassis',(0,0,.48),(2.1,4.6,.5),black)
box('Body',(0,0,.83),(2.2,4.7,.62),paint,.16)
box('Hood',(0,1.4,1.17),(2.06,1.55,.15),paint)
verts=[(-.91,-1.12,1.08),(.91,-1.12,1.08),(.91,1.0,1.08),(-.91,1.0,1.08),(-.75,-.8,1.77),(.75,-.8,1.77),(.75,.48,1.77),(-.75,.48,1.77)]
mesh=bpy.data.meshes.new('CabinMesh'); mesh.from_pydata(verts,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(3,7,6,2),(0,4,7,3),(1,2,6,5)]); mesh.update()
o=bpy.data.objects.new('Cabin',mesh); bpy.context.collection.objects.link(o); o.data.materials.append(glass)
box('Roof',(0,-.16,1.79),(1.58,1.35,.11),paint)
for x in [-.96,.96]:
    box('CabinPillar',(x,-.22,1.39),(.06,.13,.57),paint)
for x in [-.69,.69]:
    box('Headlight',(x,2.355,.97),(.65,.035,.14),light,.02)
    box('Taillight',(x,-2.355,.98),(.65,.035,.14),red,.02)
for x in [-1.09,1.09]:
    for y in [-1.46,1.48]:
        bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=.48,depth=.29,location=(x,y,.5),rotation=(0,math.pi/2,0))
        o=bpy.context.object; o.name='Wheel'; o.data.materials.append(black)
        bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=.29,depth=.305,location=(x,y,.5),rotation=(0,math.pi/2,0))
        o=bpy.context.object; o.name='Hub'; o.data.materials.append(metal)
box('RearWing',(0,-1.92,1.39),(2.28,.34,.12),black)
for x in [-.75,.75]: box('WingStrut',(x,-1.92,1.23),(.08,.16,.28),black)
box('FrontSplitter',(0,2.34,.54),(2.28,.24,.12),black)
box('RoofStripe',(0,-.16,1.85),(.2,1.25,.015),light,.0)

# Refine the production car: wheel arches, alloys, trim and mirrors.
body=bpy.data.objects.get('Body')
for x in [-1.09,1.09]:
    for y in [-1.46,1.48]:
        bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=.53,depth=.65,location=(x,y,.5),rotation=(0,math.pi/2,0))
        cutter=bpy.context.object
        mod=body.modifiers.new('Wheel arch','BOOLEAN'); mod.operation='DIFFERENCE'; mod.object=cutter
        bpy.context.view_layer.objects.active=body; bpy.ops.object.modifier_apply(modifier=mod.name)
        bpy.data.objects.remove(cutter,do_unlink=True)
        for spoke in range(5):
            angle=spoke*math.tau/5
            o=box('Alloy spoke',(x*1.16,y+math.sin(angle)*.16,.5+math.cos(angle)*.16),(.03,.09,.31),metal,.015)
            o.rotation_euler.x=-angle
for x in [-1,1]:
    box('Mirror stem',(x*1.12,.65,1.15),(.2,.08,.07),black,.02)
    box('Mirror housing',(x*1.24,.65,1.19),(.22,.3,.16),paint,.06)
    box('Door handle',(x*1.105,-.45,1.02),(.025,.26,.05),metal,.015)
    box('Side skirt',(x*1.075,0,.43),(.13,2.1,.16),black,.045)
    box('Door shut line',(x*1.102,-.82,.89),(.012,.015,.38),black,.002)
for y in [-2.36,2.36]:
    box('Number plate',(0,y,.7),(.54,.028,.17),metal,.012)
box('Front grille',(0,2.365,.88),(.65,.035,.16),black,.025)
for x in [-.28,-.14,0,.14,.28]:box('Grille fin',(x,2.39,.87),(.025,.022,.13),metal,.004)
box('Rear light bar',(0,-2.37,1.00),(.72,.02,.055),red,.015)
for x in [-.68,.68]:box('Exhaust tip',(x,-2.37,.45),(.2,.14,.12),metal,.035)
# Lower cabin silhouette while retaining the collision dimensions.
for obj in bpy.context.scene.objects:
    if obj.type=='MESH':
        for v in obj.data.vertices:
            world=obj.matrix_world @ v.co
            if world.z>1.1:
                world.z=1.1+(world.z-1.1)*.76
                v.co=obj.matrix_world.inverted() @ world

os.makedirs(os.path.join(ROOT,'Assets','Resources','Art'),exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource','CyberInterceptor.blend'))
bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'Assets','Resources','Art','CyberInterceptor.fbx'),use_selection=False,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
print('CAR_EXPORTED',sum(len(o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'))
