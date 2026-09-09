using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    public sealed class WorldBuilder:MonoBehaviour
    {
        public RoadNetwork Network {get;private set;}
        public Material PlayerPaint {get;private set;}
        public Material EnemyPaint {get;private set;}
        public Material TrafficPaint {get;private set;}
        public Transform Beacon {get;private set;}
        public Transform Secret {get;private set;}
        readonly List<Material> materials=new List<Material>();
        Material road,concrete,cyan,amber,white,steel,glass;
        public Material Mat(string name,Color color,float metal=0,bool emission=false)
        {
            var m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",.32f);
            if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*1.5f);}
            materials.Add(m);return m;
        }
        public GameObject Box(string name,Vector3 position,Vector3 scale,Material mat,bool solid=true)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(transform,false);o.transform.position=position;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;
            if(!solid)Destroy(o.GetComponent<Collider>());return o;
        }
        public void Generate(int map)
        {
            Network=new RoadNetwork(map);
            road=Mat("Graphite asphalt",new Color(.075f,.095f,.12f));concrete=Mat("Concrete",new Color(.25f,.29f,.31f));
            cyan=Mat("Navigation teal",new Color(.06f,.92f,.85f),.3f,true);amber=Mat("Warning amber",new Color(1,.5f,.1f),.2f,true);
            white=Mat("Road paint",new Color(.67f,.75f,.76f));steel=Mat("Architecture",new Color(.12f,.19f,.24f),.35f);
            glass=Mat("Lit windows",new Color(.2f,.48f,.58f),.45f,true);
            PlayerPaint=Mat("Player teal",new Color(.015f,.67f,.68f),.65f);EnemyPaint=Mat("Hostile coral",new Color(.9f,.09f,.12f),.5f);TrafficPaint=Mat("Traffic ivory",new Color(.65f,.7f,.73f),.4f);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.48f,.56f,.64f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0023f;
            RenderSettings.fogColor=map==2?new Color(.35f,.3f,.27f):new Color(.1f,.19f,.25f);
            var sun=new GameObject("Evening sunlight");sun.transform.SetParent(transform);sun.transform.rotation=Quaternion.Euler(37,-35,0);
            var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;light.color=new Color(1,.85f,.69f);light.shadows=LightShadows.Soft;
            if(map==0)Box("City foundation",new Vector3(128,-2,128),new Vector3(650,4,650),Mat("Ground",new Color(.15f,.19f,.19f)));
            else Box(map==1?"Harbor water":"Canyon floor",new Vector3(150,map==1?-5:-28,150),new Vector3(1100,2,1100),Mat("Valley",map==1?new Color(.025f,.2f,.27f):new Color(.32f,.2f,.15f)),false);
            foreach(Vector3 n in Network.Nodes)Box("Intersection",n+Vector3.down*.28f,new Vector3(19,.5f,19),road);
            foreach(var e in Network.Edges)
            {
                Vector3 a=Network.Nodes[e.x],b=Network.Nodes[e.y],mid=(a+b)/2;float length=Vector3.Distance(a,b);
                bool vertical=Mathf.Abs(a.z-b.z)>1;
                Box("Road",mid+Vector3.down*.28f,vertical?new Vector3(17,.5f,length):new Vector3(length,.5f,17),road);
                for(float d=11;d<length-8;d+=9)
                {
                    Vector3 p=Vector3.Lerp(a,b,d/length)+Vector3.up*.005f;
                    Box("Center dash",p,vertical?new Vector3(.16f,.025f,3.4f):new Vector3(3.4f,.025f,.16f),white,false);
                }
                Vector3 side=vertical?Vector3.right:Vector3.forward;
                foreach(int sign in new[]{-1,1})
                {
                    Box("Edge line",mid+side*sign*7.3f,vertical?new Vector3(.13f,.03f,length-18):new Vector3(length-18,.03f,.13f),white,false);
                    if(map==2)Box("Guardrail",mid+side*sign*8.7f+Vector3.up*.65f,vertical?new Vector3(.3f,1.1f,length-20):new Vector3(length-20,1.1f,.3f),concrete);
                    if(map==0)Box("Sidewalk",mid+side*sign*10.2f,vertical?new Vector3(3,.25f,length-19):new Vector3(length-19,.25f,3),concrete);
                }
                if(map!=0)Box("Bridge pier",mid+Vector3.down*8,new Vector3(4,15,4),concrete);
            }
            var rng=new System.Random(710+map);int cols=map==1?4:5,rows=map==1?6:5;float space=map==2?78:64;
            for(int z=0;z<rows-1;z++)for(int x=0;x<cols-1;x++)
            {
                Vector3 center=new Vector3((x+.5f)*space,0,(z+.5f)*space);
                if(map==0)
                {
                    float h=12+rng.Next(28);
                    Box("Office block",center+Vector3.up*h/2,new Vector3(36,h,36),steel);
                    Box("Roof machinery",center+Vector3.up*(h+1),new Vector3(12,2,15),concrete);
                    for(float y=4;y<h;y+=4)
                    {
                        Box("Window ribbon",center+new Vector3(0,y,-18.03f),new Vector3(30,1.1f,.03f),glass,false);
                        Box("Window ribbon",center+new Vector3(-18.03f,y,0),new Vector3(.03f,1.1f,30),glass,false);
                    }
                }
                else if(map==1)
                {
                    Box("Cargo island",center+Vector3.down*.75f,new Vector3(41,1.5f,41),concrete);
                    for(int k=0;k<4;k++)
                    {
                        var mat=k%2==0?steel:Mat("Rust container",new Color(.53f,.24f,.13f));
                        Vector3 p=center+new Vector3((k%2)*15-8,2.5f,(k/2)*15-8);
                        Box("Shipping container",p,new Vector3(10,5,12),mat);
                        for(int r=-4;r<=4;r+=2)Box("Container rib",p+new Vector3(r,0,-6.02f),new Vector3(.13f,4.7f,.1f),concrete,false);
                    }
                    if(x==1){Box("Crane mast",center+new Vector3(18,15,18),new Vector3(2,30,2),amber);Box("Crane jib",center+new Vector3(2,30,18),new Vector3(34,2,2),amber);}
                }
                else
                {
                    var rock=GameObject.CreatePrimitive(PrimitiveType.Cylinder);rock.name="Canyon mesa";rock.transform.SetParent(transform);rock.transform.position=center+Vector3.down*7;
                    rock.transform.localScale=new Vector3(44,12+rng.Next(8),44);rock.GetComponent<Renderer>().sharedMaterial=Mat("Sandstone",new Color(.38f+(float)rng.NextDouble()*.1f,.26f,.19f));
                }
            }
            if(map==0)
            {
                for(int i=0;i<4;i++)
                {
                    foreach(Vector3 p in new[]{new Vector3(i*64+32,0,-34),new Vector3(-34,0,i*64+32),new Vector3(i*64+32,0,290),new Vector3(290,0,i*64+32)})
                    {
                        float h=18+i*7;Box("Perimeter tower",p+Vector3.up*h/2,new Vector3(37,h,37),steel);
                        for(float y=4;y<h;y+=4)
                        {
                            Box("Perimeter glazing",p+new Vector3(0,y,18.53f),new Vector3(31,1.4f,.04f),glass,false);
                            Box("Perimeter glazing",p+new Vector3(0,y,-18.53f),new Vector3(31,1.4f,.04f),glass,false);
                            Box("Perimeter glazing",p+new Vector3(18.53f,y,0),new Vector3(.04f,1.4f,31),glass,false);
                        }
                    }
                }
            }
            for(int i=0;i<Network.Nodes.Count;i+=2)
            {
                Vector3 p=Network.Nodes[i]+new Vector3(-11,0,-11);
                Box("Streetlight",p+Vector3.up*4,new Vector3(.23f,8,.23f),steel);
                Box("Lamp",p+new Vector3(1,8,0),new Vector3(2.7f,.15f,.4f),cyan,false);
            }
            // A visible dead-end branch gives spoofing a physical consequence.
            Vector3 fork=Network.Nodes[cols-1];Box("Untrusted service road",fork+Vector3.right*22+Vector3.down*.28f,new Vector3(44,.5f,13),road);
            if(map==0)Box("Service road barrier",Network.Hazard+Vector3.up*1.5f,new Vector3(2,3,14),amber);
            Sign(fork+new Vector3(18,0,-9),map==0?"ROAD CLOSED":"BRIDGE OUT",amber);
            Sign(Network.Nodes[0]+new Vector3(-10,0,10),"OPERATIONS",cyan);
            Sign(Network.Nodes[Network.Finish]+new Vector3(10,0,10),Network.Names[Network.Finish].ToUpper(),cyan);
            var beacon=GameObject.CreatePrimitive(PrimitiveType.Cylinder);beacon.name="GPS target";beacon.transform.SetParent(transform);beacon.transform.localScale=new Vector3(7,.05f,7);beacon.GetComponent<Renderer>().sharedMaterial=cyan;Destroy(beacon.GetComponent<Collider>());Beacon=beacon.transform;
        }
        void Sign(Vector3 point,string text,Material mat)
        {
            Box("Sign post",point+Vector3.up*2,new Vector3(.25f,4,.25f),steel);
            Box("Sign board",point+Vector3.up*4,new Vector3(10,2,.2f),steel);
            var o=new GameObject(text);o.transform.SetParent(transform);o.transform.position=point+new Vector3(0,4,-.12f);o.transform.rotation=Quaternion.Euler(0,180,0);
            var t=o.AddComponent<TextMesh>();t.text=text;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.fontSize=48;t.characterSize=.09f;t.color=mat.color;
        }
        public void CreateSecret()
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name="Hidden encrypted cache";o.transform.SetParent(transform);o.transform.position=Network.Nodes[Network.Map==1?8:10]+new Vector3(5,1.4f,5);o.transform.localScale=Vector3.one*1.3f;
            o.GetComponent<Renderer>().sharedMaterial=amber;Destroy(o.GetComponent<Collider>());Secret=o.transform;
        }
        public static void Sparks(Vector3 point)
        {
            var obj=new GameObject("Impact sparks");obj.transform.position=point;var p=obj.AddComponent<ParticleSystem>();
            p.Stop();var main=p.main;main.duration=.4f;main.loop=false;main.startLifetime=.45f;main.startSpeed=6;main.startSize=.12f;main.startColor=new Color(1,.6f,.2f);main.gravityModifier=1.5f;
            var em=p.emission;em.rateOverTime=0;em.SetBursts(new[]{new ParticleSystem.Burst(0,22)});
            var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("Impact");p.Play();Destroy(obj,1);
        }
        void OnDestroy(){foreach(var m in materials)if(m)Destroy(m);}
    }
}
