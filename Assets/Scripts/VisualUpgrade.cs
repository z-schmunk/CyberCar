using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CyberCar
{
    public sealed class VisualUpgrade:MonoBehaviour
    {
        readonly List<Object> owned=new List<Object>();
        Material Detail(string name,Color color,int style,float scale,float gloss=.2f)
        {
            var tex=new Texture2D(256,256,TextureFormat.RGB24,true);tex.name=name;tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=8;
            var pixels=new Color[256*256];var rng=new System.Random(314+style);
            for(int y=0;y<256;y++)for(int x=0;x<256;x++)
            {
                float noise=.8f+(float)rng.NextDouble()*.35f;Color c=color*noise;
                if(style==1) // Glazed facade with mullions, floor slabs and occasional interior lighting.
                {
                    bool frame=x%64<5||y%64<5;
                    c=frame?new Color(.23f,.25f,.26f):Color.Lerp(new Color(.07f,.13f,.18f),new Color(.32f,.42f,.48f),y%64/64f)*noise;
                    if((x/64+y/64*3)%7==0&&!frame)c=new Color(.48f,.4f,.26f)*noise;
                }
                if(style==2&&((x+y/64*17)%128<2||y%64<2))c*=.48f;
                if(style==3)c*=.65f+.4f*Mathf.PerlinNoise(x/25f,y/6f);
                pixels[y*256+x]=c;
            }
            tex.SetPixels(pixels);tex.Apply(true,true);owned.Add(tex);
            var mat=new Material(Resources.Load<Shader>("SurfaceDetail")){name=name};mat.SetTexture("_MainTex",tex);mat.SetFloat("_Scale",scale);mat.SetFloat("_Smoothness",gloss);owned.Add(mat);return mat;
        }
        public void Apply(WorldBuilder world)
        {
            int map=world.Network.Map;
            var asphalt=world.Photos.Surface("asphalt_02",4);
            var paving=Detail("Concrete joints",new Color(.46f,.45f,.42f),2,4);
            var facade=Detail("Glazed building facade",Color.white,1,13,.65f);
            var rock=world.Photos.Surface("rocky_terrain_02",6);
            var ground=world.Photos.Surface("grass_path_2",5);
            var sand=world.Photos.Surface("aerial_beach_02",6);
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>())
            {
                string n=renderer.gameObject.name;
                if(n=="Road"||n=="Intersection"||n=="Untrusted service road")renderer.sharedMaterial=asphalt;
                else if(n=="Office block"||n=="Perimeter tower")renderer.sharedMaterial=facade;
                else if(n=="Sidewalk"||n=="Cargo island"||n=="Bridge pier")renderer.sharedMaterial=paving;
                else if(n=="Canyon mesa"||n=="Canyon floor"||n=="Coastal escarpment"||n=="Shore boulder")renderer.sharedMaterial=rock;
                else if(n=="Beach")renderer.sharedMaterial=sand;
                else if(n=="City foundation")renderer.sharedMaterial=ground;
                else if(n=="Window ribbon"||n=="Perimeter glazing")renderer.enabled=false;
                else if(n=="Harbor water")
                {
                    var water=Detail("Harbor ripples",new Color(.07f,.24f,.3f),3,12,.88f);water.SetFloat("_Metallic",.35f);renderer.sharedMaterial=water;
                }
            }
            var sky=new Material(Shader.Find("Skybox/Procedural"));owned.Add(sky);
            sky.SetColor("_SkyTint",new Color(.5f,.5f,.5f));sky.SetColor("_GroundColor",new Color(.37f,.35f,.32f));sky.SetFloat("_AtmosphereThickness",1.1f);sky.SetFloat("_Exposure",.95f);sky.SetFloat("_SunSize",.035f);
            RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.55f,.66f,.78f);RenderSettings.ambientEquatorColor=new Color(.39f,.42f,.46f);RenderSettings.ambientGroundColor=new Color(.2f,.19f,.17f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.56f,.65f,.72f);RenderSettings.fogDensity=.0007f;
            var sunlight=world.GetComponentInChildren<Light>();sunlight.transform.rotation=Quaternion.Euler(38,-32,0);sunlight.intensity=1.35f;sunlight.color=new Color(1,.94f,.84f);sunlight.shadowStrength=.7f;sunlight.shadowBias=.035f;
            // A local reflection capture supplies actual surroundings to the metallic car paint.
            var probeObject=new GameObject("Environment reflection");probeObject.transform.SetParent(world.transform);probeObject.transform.position=new Vector3(30,5,10);
            var probe=probeObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            probe.resolution=128;probe.size=new Vector3(2200,200,2200);probe.boxProjection=false;probe.clearFlags=ReflectionProbeClearFlags.Skybox;probe.RenderProbe();
            world.PlayerPaint.color=new Color(.035f,.22f,.25f);world.PlayerPaint.SetFloat("_Glossiness",.86f);world.PlayerPaint.SetFloat("_Metallic",.72f);
            world.EnemyPaint.SetFloat("_Glossiness",.78f);world.TrafficPaint.SetFloat("_Glossiness",.72f);
            // Road furniture uses the same graph as gameplay and stays outside driveable lanes.
            foreach(var edge in world.Network.Edges)
            {
                Vector3 a=world.Network.Nodes[edge.x],b=world.Network.Nodes[edge.y],forward=(b-a).normalized,side=Vector3.Cross(Vector3.up,forward);
                if(map==0)
                {
                    Vector3 p=world.Network.Path(edge.x,edge.y)[6]+side*13;
                    world.Box("Tree planter",p+Vector3.up*.2f,new Vector3(3,.4f,3),paving,false);
                    if((edge.x+edge.y)%3==0)world.Photos.Prop("PineSapling",p,6,(edge.x*37)%360);
                }
            }
            if(map>=2)
            {
                foreach(var filter in world.GetComponentsInChildren<MeshFilter>())if(filter.name=="Canyon mesa")
                {
                    var mesh=Instantiate(filter.sharedMesh);owned.Add(mesh);var vertices=mesh.vertices;
                    for(int i=0;i<vertices.Length;i++){Vector3 v=vertices[i];float r=.87f+.24f*Mathf.PerlinNoise(v.x*5+filter.transform.position.x,v.z*5+filter.transform.position.z);v.x*=r;v.z*=r;vertices[i]=v;}
                    mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=mesh;
                }
            }
        }
        void OnDestroy(){foreach(var obj in owned)if(obj)Destroy(obj);}
    }
}
