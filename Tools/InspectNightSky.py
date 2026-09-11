import bpy,numpy as np
p='C:/Users/schmu/unityProjects/CyberCarGame/Assets/Resources/Photographic/qwantani_night_puresky.hdr'
i=bpy.data.images.load(p);a=np.array(i.pixels[:]).reshape(-1,4)[:,:3];print('HDR_STATS',np.percentile(a,[0,25,50,75,90,99,100]),flush=True)
