using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public sealed class WorldBuilder:MonoBehaviour {
 public RoadNetwork Network{get;private set;} public Material PlayerPaint{get;private set;} public Material EnemyPaint{get;private set;} public Material TrafficPaint{get;private set;}
 public Transform Beacon{get;private set;} public Transform Secret{get;private set;}
 public readonly List<CrossingSite> Crossings=new List<CrossingSite>();
 public PhotographicMaterials Photos{get;private set;} public WorldEnvironment Environment{get;private set;}
 readonly List<Object> owned=new List<Object>();Material road,concrete,cyan,amber,white,steel,rock,sand,leaves;
 public Material Mat(string name,Color color,float metal=0,bool emission=false){var m=new Material(Shader.Find("Standard")){name=name,color=color,enableInstancing=true};m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",.25f);if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color);}owned.Add(m);return m;}
 public GameObject Box(string name,Vector3 p,Vector3 scale,Material mat,bool solid=true){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(transform,false);o.transform.position=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)Destroy(o.GetComponent<Collider>());return o;}
 public GameObject Segment(string name,Vector3 a,Vector3 b,float width,float height,Material mat,bool solid=false){var o=Box(name,(a+b)/2,new Vector3(width,height,Vector3.Distance(a,b)+.08f),mat,solid);o.transform.rotation=Quaternion.LookRotation(b-a);return o;}
 GameObject Ribbon(string name,Vector3[] path,float width,Material mat,bool solid=true) {
 var vertices=new Vector3[path.Length*4];var triangles=new List<int>();
 for(int i=0;i<path.Length;i++){Vector3 tangent=path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(i-1,0)],side=Vector3.Cross(Vector3.up,tangent.normalized).normalized*width/2;vertices[i*4]=path[i]-side;vertices[i*4+1]=path[i]+side;vertices[i*4+2]=path[i]-side-Vector3.up*.6f;vertices[i*4+3]=path[i]+side-Vector3.up*.6f;
 if(i==0)continue;int a=(i-1)*4,b=i*4;triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1,a+2,a+3,b+2,a+3,b+3,b+2,a,a+2,b,a+2,b+2,b,a+1,b+1,a+3,a+3,b+1,b+3});}
 var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles.ToArray();var uv=new Vector2[vertices.Length];for(int j=0;j<uv.Length;j++)uv[j]=new Vector2(vertices[j].x,vertices[j].z);mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();owned.Add(mesh);
 var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;if(solid)go.AddComponent<MeshCollider>().sharedMesh=mesh;return go;
 }
 public void Generate(int map,bool extreme=false) {
 Network=new RoadNetwork(map,extreme);float size=Network.Size,w=Network.RoadWidth;
 Photos=gameObject.AddComponent<PhotographicMaterials>();
 road=Mat("Asphalt",new Color(.1f,.11f,.12f));concrete=Mat("Concrete",new Color(.4f,.41f,.4f));cyan=Mat("GPS",new Color(.04f,.7f,.6f),.1f,true);amber=Mat("Warning",new Color(.95f,.56f,.12f));white=Mat("Markings",new Color(.75f,.73f,.66f));steel=Mat("Steel",new Color(.17f,.2f,.23f),.65f);rock=Mat("Rock",new Color(.42f,.32f,.24f));sand=Mat("Beach sand",new Color(.66f,.55f,.36f));leaves=Mat("Palm leaves",new Color(.13f,.23f,.08f));
 PlayerPaint=Mat("Player paint",new Color(.03f,.27f,.3f),.7f);EnemyPaint=Mat("Enemy paint",new Color(.5f,.045f,.03f),.6f);TrafficPaint=Mat("Traffic paint",new Color(.55f,.59f,.62f),.4f);
 var sun=new GameObject("Daylight");sun.transform.SetParent(transform);sun.transform.rotation=Quaternion.Euler(42,-30,0);var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.shadows=LightShadows.Soft;
 if(map==0)Box("City foundation",new Vector3(size/2,-2.6f,size/2),new Vector3(size+300,4,size+300),concrete);
 if(map==1||map==3)Coast(map,size);
 foreach(var node in Network.Nodes)Box("Intersection",node-Vector3.up*.3f,new Vector3(w+2,.6f,w+2),road);
 int edgeIndex=0;
 foreach(var e in Network.Edges) {
 var path=Network.Path(e.x,e.y);Ribbon("Road",path,w,road);
 for(int i=1;i<path.Length;i++) {
 Vector3 a=path[i-1],b=path[i],f=(b-a).normalized,side=Vector3.Cross(Vector3.up,f).normalized,mid=(a+b)/2;
 if(i>3&&i<path.Length-3){if(i%2==0)Segment("Center dash",mid-f*1.5f+Vector3.up*.035f,mid+f*1.5f+Vector3.up*.035f,.15f,.018f,white);
 foreach(int sign in new[]{-1,1}){Segment("Edge line",a+side*sign*(w/2-1)+Vector3.up*.025f,b+side*sign*(w/2-1)+Vector3.up*.025f,.12f,.018f,white);
 if(map>=2||Network.IsBridge(e.x,e.y)){Segment("Guardrail",a+side*sign*(w/2+.25f)+Vector3.up*.75f,b+side*sign*(w/2+.25f)+Vector3.up*.75f,.22f,.4f,steel,true);Box("Guardrail post",mid+side*sign*(w/2+.25f)+Vector3.up*.45f,new Vector3(.16f,.9f,.16f),steel,false);}
 else if(map==0)Segment("Sidewalk",a+side*sign*(w/2+1.3f),b+side*sign*(w/2+1.3f),2.3f,.2f,concrete);
 }}
 }
 if(Network.IsBridge(e.x,e.y)) {
 int quarter=(path.Length-1)/4;
 for(int k=quarter;k<=quarter*3;k+=quarter)Box("Bridge pier",path[k]-Vector3.up*12.8f,new Vector3(2.5f,24,2.5f),concrete);
 foreach(int k in new[]{quarter,quarter*3}){var side=Vector3.Cross(Vector3.up,(path[k+1]-path[k-1]).normalized);foreach(int sign in new[]{-1,1}){Vector3 p=path[k]+side*sign*(w/2+1);Box("Bridge tower",p+Vector3.up*7,new Vector3(.6f,14,.6f),steel,false);Segment("Stay cable",p+Vector3.up*13,path[path.Length/2]+side*sign*(w/2+1),.09f,.09f,steel);}}
 }
 if(edgeIndex++%4==1&&!Network.IsBridge(e.x,e.y)) {
 int cross=5;Vector3 center=path[cross]+Vector3.up*.04f,forward=(path[cross+1]-path[cross-1]).normalized,side=Vector3.Cross(Vector3.up,forward);
 for(float d=-w/2+1;d<w/2;d+=1.4f){var stripe=Box("Crosswalk",center+side*d,new Vector3(.7f,.025f,4),white,false);stripe.transform.rotation=Quaternion.LookRotation(forward);}
 Crossings.Add(new CrossingSite{Center=center,Forward=forward,Width=w,Phase=Crossings.Count*2.3f});
 foreach(int sign in new[]{-1,1}){Vector3 p=center+side*sign*(w/2+1.5f);Box("Crossing post",p+Vector3.up*1.5f,new Vector3(.12f,3,.12f),steel,false);Box("Crossing beacon",p+Vector3.up*3,new Vector3(.45f,.45f,.3f),amber,false);}
 }
 }
 var rng=new System.Random(825+map);
 for(int z=0;z<Network.Rows-1;z++)for(int x=0;x<Network.Columns-1;x++){
 Vector3 p=new Vector3((x+.5f)*96,0,(z+.5f)*96);
 if(map==0){float h=14+rng.Next(38);Box("Office block",p+Vector3.up*h/2,new Vector3(48,h,46),steel);Box("Roof machinery",p+Vector3.up*(h+1.2f),new Vector3(12,2.4f,8),concrete);}
 else if(map==1){
 if(x==Network.Columns/2-1)continue;Box("Cargo island",p-Vector3.up*.8f,new Vector3(64,1.6f,64),concrete);
 for(int k=0;k<6;k++){Vector3 c=p+new Vector3((k%3)*15-15,2.5f,k/3*22-11);Box("Container",c,new Vector3(11,5,18),k%2==0?steel:amber);for(int rib=-5;rib<=5;rib+=2)Box("Container rib",c+new Vector3(rib,0,-9.04f),new Vector3(.12f,4.8f,.08f),concrete,false);}
 if(z%2==0){Box("Crane mast",p+new Vector3(27,18,22),new Vector3(1.4f,36,1.4f),amber);Box("Crane jib",p+new Vector3(5,36,22),new Vector3(46,1.4f,1.4f),amber);Segment("Crane cable",p+new Vector3(-17,36,22),p+new Vector3(-17,12,22),.08f,.08f,steel);}
 }
 }
 foreach(int n in Network.Hotspots){Vector3 p=Network.Nodes[n];Box("Interchange apron",p-Vector3.up*(map>=2?.9f:.35f),new Vector3(w+14,.5f,w+14),road,map<2);Sign(p+new Vector3(-w,0,-14),"TRAFFIC CONTROL",cyan);}
 Vector3 fork=Network.Nodes[Network.Columns-1];Segment("Untrusted service road",fork-Vector3.up*.3f,Network.Hazard-Vector3.up*.3f,11,.6f,road,true);
 if(map==0)Box("Service road barrier",Network.Hazard+Vector3.up*1.5f,new Vector3(2,3,14),amber);
 Sign(fork+new Vector3(17,0,-8),map==0?"ROAD CLOSED":"BRIDGE OUT",amber);
 Sign(Network.Nodes[0]+new Vector3(-10,0,10),"OPERATIONS",cyan);Sign(Network.Nodes[Network.Finish]+new Vector3(12,0,10),Network.Names[Network.Finish].ToUpper(),cyan);
 if(map==3){Vector3 p=Network.Nodes[Network.Finish]+new Vector3(26,0,22);Box("Lighthouse foundation",p-Vector3.up,new Vector3(38,2,38),concrete);var tower=GameObject.CreatePrimitive(PrimitiveType.Cylinder);tower.transform.SetParent(transform);tower.transform.position=p+Vector3.up*12;tower.transform.localScale=new Vector3(8,12,8);tower.GetComponent<Renderer>().sharedMaterial=white;Box("Lighthouse lantern",p+Vector3.up*25,new Vector3(6,3,6),cyan,false);}
 var beacon=GameObject.CreatePrimitive(PrimitiveType.Cylinder);beacon.name="GPS target";beacon.transform.SetParent(transform);beacon.transform.localScale=new Vector3(7,.05f,7);beacon.GetComponent<Renderer>().sharedMaterial=cyan;Destroy(beacon.GetComponent<Collider>());Beacon=beacon.transform;
 gameObject.AddComponent<VisualUpgrade>().Apply(this);
 if(map>0)gameObject.AddComponent<NaturalLandscape>().Build(this);
 Environment=gameObject.AddComponent<WorldEnvironment>();Environment.Build(this);
 }
 void Coast(int map,float size) {
 float sea=map==3?-32:-7;
 var water=new Material(Resources.Load<Shader>("CoastalWater"));owned.Add(water);
 int count=40;var vertices=new Vector3[(count+1)*(count+1)];var tris=new List<int>();
 for(int z=0;z<=count;z++)for(int x=0;x<=count;x++){int n=z*(count+1)+x;vertices[n]=new Vector3(-900+x*(size+1800)/count,sea,-900+z*(size+1800)/count);if(x<count&&z<count)tris.AddRange(new[]{n,n+count+1,n+1,n+1,n+count+1,n+count+2});}
 var mesh=new Mesh{name="Ocean waves"};mesh.vertices=vertices;mesh.triangles=tris.ToArray();var uv=new Vector2[vertices.Length];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x/10,vertices[i].z/10);mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateTangents();owned.Add(mesh);var ocean=new GameObject("Open ocean");ocean.transform.SetParent(transform);ocean.AddComponent<MeshFilter>().sharedMesh=mesh;ocean.AddComponent<MeshRenderer>().sharedMaterial=water;
 for(int x=-1;x<Network.Columns;x++){
 Vector3 high=new Vector3(x*96+48,map==3?-22:-.9f,-21),low=new Vector3(x*96+48,sea-.15f,-100);
 for(int k=0;k<3;k++){Vector3 p=new Vector3(x*96+20+k*22,map==3?-24:-2,-36-k*8);Palm(p,8+(x+k+3)%4,(x+k)*.9f);}
 for(int k=0;k<4;k++){var stone=GameObject.CreatePrimitive(PrimitiveType.Sphere);stone.name="Shore boulder";stone.transform.SetParent(transform);stone.transform.position=new Vector3(x*96+k*23,sea+1,-86+Mathf.Sin(x*4+k)*9);stone.transform.localScale=new Vector3(4+k%2*3,2+k%3,3);stone.transform.rotation=Quaternion.Euler(k*13,x*19,12);stone.GetComponent<Renderer>().sharedMaterial=rock;Destroy(stone.GetComponent<Collider>());}
 }
 }
 void Palm(Vector3 p,float height,float phase){
 Vector3 top=p+new Vector3(Mathf.Sin(phase)*1.5f,height,Mathf.Cos(phase));Vector3 previous=p;
 for(int i=1;i<=6;i++){float t=i/6f;Vector3 next=Vector3.Lerp(p,top,t)+Vector3.right*Mathf.Sin(t*Mathf.PI)*.55f;Segment("Palm trunk",previous,next,.32f-t*.12f,.32f-t*.12f,rock);previous=next;}
 var vertices=new List<Vector3>();var indices=new List<int>();
 for(int leaf=0;leaf<9;leaf++){float angle=phase+leaf*Mathf.PI*2/9;Vector3 dir=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle)),side=Vector3.Cross(Vector3.up,dir);
 for(int j=0;j<9;j++){float t=j/8f;Vector3 center=top+dir*(t*5)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*1.2f-t*t*2);float width=Mathf.Sin(t*Mathf.PI)*.48f;vertices.Add(center-side*width);vertices.Add(center+side*width);
 if(j>0){int n=vertices.Count-4;indices.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}
 }
 int frontVertices=vertices.Count,frontIndices=indices.Count;vertices.AddRange(vertices.ToArray());for(int i=0;i<frontIndices;i+=3)indices.AddRange(new[]{indices[i+2]+frontVertices,indices[i+1]+frontVertices,indices[i]+frontVertices});
 var mesh=new Mesh{name="Arching palm crown"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();owned.Add(mesh);var crown=new GameObject("Palm fronds");crown.transform.SetParent(transform);crown.AddComponent<MeshFilter>().sharedMesh=mesh;crown.AddComponent<MeshRenderer>().sharedMaterial=leaves;
 }
 void Sign(Vector3 p,string value,Material mat){Box("Sign post",p+Vector3.up*2,new Vector3(.2f,4,.2f),steel,false);Box("Sign board",p+Vector3.up*4,new Vector3(11,2,.16f),steel,false);var o=new GameObject(value);o.transform.SetParent(transform);o.transform.position=p+new Vector3(0,4,-.1f);o.transform.rotation=Quaternion.Euler(0,180,0);var t=o.AddComponent<TextMesh>();t.text=value;t.anchor=TextAnchor.MiddleCenter;t.fontSize=48;t.characterSize=.085f;t.color=mat.color;}
 public void CreateSecret(){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name="Hidden encrypted cache";o.transform.SetParent(transform);o.transform.position=Network.Nodes[Network.Columns*2]+new Vector3(5,1.4f,5);o.transform.localScale=Vector3.one*1.3f;o.GetComponent<Renderer>().sharedMaterial=amber;Destroy(o.GetComponent<Collider>());Secret=o.transform;}
 public static void Sparks(Vector3 point){var o=new GameObject("Impact sparks");o.transform.position=point;var p=o.AddComponent<ParticleSystem>();p.Stop();var m=p.main;m.duration=.4f;m.loop=false;m.startLifetime=.45f;m.startSpeed=6;m.startSize=.12f;m.startColor=new Color(1,.6f,.2f);m.gravityModifier=1.5f;var e=p.emission;e.rateOverTime=0;e.SetBursts(new[]{new ParticleSystem.Burst(0,22)});p.GetComponent<ParticleSystemRenderer>().sharedMaterial=Resources.Load<Material>("Impact");p.Play();Destroy(o,1);}
 void OnDestroy(){foreach(var obj in owned)if(obj)Destroy(obj);}
 }
 public sealed class CrossingSite {public Vector3 Center,Forward;public float Width,Phase;}
}
