using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CyberCar {
 public sealed class NaturalLandscape:MonoBehaviour {
 struct RoadSample {public Vector3 A,B;public bool Bridge;public float T;}
 readonly Dictionary<Vector2Int,List<RoadSample>> cells=new Dictionary<Vector2Int,List<RoadSample>>();
 readonly List<Mesh> owned=new List<Mesh>();RoadNetwork graph;WorldBuilder world;
 const float Bin=32;
 public void Build(WorldBuilder owner){
 world=owner;graph=world.Network;
 foreach(var edge in graph.Edges){var p=graph.Path(edge.x,edge.y);for(int i=1;i<p.Length;i++){
 var s=new RoadSample{A=p[i-1],B=p[i],Bridge=graph.IsBridge(edge.x,edge.y),T=(i-.5f)/(p.Length-1)};
 int x0=Mathf.FloorToInt((Mathf.Min(s.A.x,s.B.x)-60)/Bin),x1=Mathf.FloorToInt((Mathf.Max(s.A.x,s.B.x)+60)/Bin);
 int z0=Mathf.FloorToInt((Mathf.Min(s.A.z,s.B.z)-60)/Bin),z1=Mathf.FloorToInt((Mathf.Max(s.A.z,s.B.z)+60)/Bin);
 for(int x=x0;x<=x1;x++)for(int z=z0;z<=z1;z++){var key=new Vector2Int(x,z);if(!cells.TryGetValue(key,out var list)){list=new List<RoadSample>();cells[key]=list;}list.Add(s);}
 }}
 float min=-300,max=graph.Size+430;int count=Mathf.CeilToInt((max-min)/4);float step=(max-min)/count;
 var vertices=new Vector3[(count+1)*(count+1)];var triangles=new int[count*count*6];int t=0;
 for(int z=0;z<=count;z++)for(int x=0;x<=count;x++){int n=z*(count+1)+x;float px=min+x*step,pz=min+z*step;vertices[n]=new Vector3(px,Height(px,pz),pz);if(x<count&&z<count){triangles[t++]=n;triangles[t++]=n+count+1;triangles[t++]=n+1;triangles[t++]=n+1;triangles[t++]=n+count+1;triangles[t++]=n+count+2;}}
 var mesh=new Mesh{name="Continuous mountain and coast",indexFormat=IndexFormat.UInt32};mesh.vertices=vertices;mesh.triangles=triangles;var uv=new Vector2[vertices.Length];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x,vertices[i].z);mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();owned.Add(mesh);
 var go=new GameObject("Visible landscape collision");go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshCollider>().sharedMesh=mesh;
 var material=world.Photos.Surface(graph.Map==1?"aerial_beach_02":"rocky_terrain_02",8);go.AddComponent<MeshRenderer>().sharedMaterial=material;
 // A continuous sandy coastline sits below the cliff roads and meets the ocean.
 if(graph.Map==3)BeachStrip();
 var rng=new System.Random(315+graph.Map);
 for(int i=0;i<(graph.Map>=2?32:10);i++){
 float x=(float)rng.NextDouble()*graph.Size,z=(float)rng.NextDouble()*graph.Size;
 float distance=RoadDistance(x,z,out _,out _);if(distance<24)continue;float y=Height(x,z);
 if(y<0&&graph.Map==1)continue;
 world.Photos.Prop(i%3==0?"RockFace":"PineSapling",new Vector3(x,y-.25f,z),i%3==0?6+(float)rng.NextDouble()*8:5+(float)rng.NextDouble()*6,rng.Next(360));
 }
 }
 public float Height(float x,float z){
 float noise=Mathf.PerlinNoise(x*.008f+28,z*.008f+19)*.62f+Mathf.PerlinNoise(x*.024f+7,z*.024f+14)*.26f+Mathf.PerlinNoise(x*.065f+40,z*.065f+10)*.12f;
 float baseHeight;
 if(graph.Map==1){float shore=-50+Mathf.Sin(x*.009f)*10;baseHeight=-11+Mathf.Exp(-Mathf.Pow((z-shore)/49,2))*11+noise*.5f;}
 else{
 float ridge=1-Mathf.Abs(Mathf.PerlinNoise(x*.005f+72,z*.005f+33)*2-1);
 float mountain=Mathf.Exp(-Mathf.Pow((x-graph.Size*.6f)/270,2)-Mathf.Pow((z-graph.Size-130)/230,2))*185;
 baseHeight=-8+noise*34+Mathf.Pow(ridge,3)*35+mountain;
 if(graph.Map==3){float coast=-17+Mathf.Sin(x*.012f)*4;baseHeight=Mathf.Lerp(-38,baseHeight,Mathf.SmoothStep(0,1,(z-coast+20)/27));}
 }
 float d=RoadDistance(x,z,out float roadHeight,out float bridgeDepth);float half=graph.RoadWidth/2;
 if(d<half+55){float shoulder=Mathf.Clamp01((d-half-10)/45);baseHeight=Mathf.Lerp(roadHeight-.4f-bridgeDepth,baseHeight,Mathf.SmoothStep(0,1,shoulder));}
 return baseHeight;
 }
 float RoadDistance(float x,float z,out float height,out float bridgeDepth){
 float best=float.MaxValue;height=0;bridgeDepth=0;
 if(!cells.TryGetValue(new Vector2Int(Mathf.FloorToInt(x/Bin),Mathf.FloorToInt(z/Bin)),out var list))return best;
 Vector2 p=new Vector2(x,z);
 foreach(var s in list){Vector2 a=new Vector2(s.A.x,s.A.z),b=new Vector2(s.B.x,s.B.z),delta=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,delta)/delta.sqrMagnitude);float d=Vector2.Distance(p,a+delta*t);if(d<best){best=d;height=Mathf.Lerp(s.A.y,s.B.y,t);bridgeDepth=s.Bridge?Mathf.Pow(Mathf.Sin(s.T*Mathf.PI),2)*20:0;}}
 return best;
 }
 void BeachStrip(){
 int count=120;var v=new Vector3[(count+1)*3];var tris=new List<int>();float size=graph.Size+600;
 for(int i=0;i<=count;i++){float x=-300+i*size/count;float shore=-35+Mathf.Sin(x*.012f)*5;v[i*3]=new Vector3(x,-25,shore);v[i*3+1]=new Vector3(x,-30,shore-25);v[i*3+2]=new Vector3(x,-34,shore-48);if(i>0)for(int j=0;j<2;j++){int a=(i-1)*3+j,b=i*3+j;tris.AddRange(new[]{a,a+1,b,b,a+1,b+1});}}
 var mesh=new Mesh{name="Irregular sandy shoreline"};mesh.vertices=v;mesh.triangles=tris.ToArray();mesh.RecalculateNormals();owned.Add(mesh);var go=new GameObject("Beach shoreline");go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=world.Photos.Surface("aerial_beach_02",6);go.AddComponent<MeshCollider>().sharedMesh=mesh;
 }
 void OnDestroy(){foreach(var m in owned)if(m)Destroy(m);}
 }
}