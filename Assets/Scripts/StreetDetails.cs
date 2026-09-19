using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CyberCar {
 // Small decorative geometry is combined into one draw, without collision shapes.
 public sealed class StreetDetails:MonoBehaviour {
 readonly List<Vector3> vertices=new List<Vector3>();readonly List<int> triangles=new List<int>();readonly List<Color> colors=new List<Color>();
 Mesh mesh;Material material;public int DrainCount{get;private set;}
 void Block(Vector3 center,Vector3 size,Quaternion rotation,Color color){
 int start=vertices.Count;Vector3[] corners={new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)};
 int[] faces={0,3,2,1,5,6,7,4,4,7,3,0,1,2,6,5,3,7,6,2,4,0,1,5};
 for(int i=0;i<faces.Length;i++){vertices.Add(center+rotation*Vector3.Scale(corners[faces[i]],size*.5f));colors.Add(color);}
 for(int i=0;i<24;i+=4)triangles.AddRange(new[]{start+i,start+i+1,start+i+2,start+i,start+i+2,start+i+3});
 }
 public void Build(WorldBuilder world){
 if(world.Network.Map!=0)return;
 Color iron=new Color(.16f,.17f,.18f),rim=new Color(.19f,.16f,.13f),shadow=new Color(.025f,.03f,.035f);
 foreach(var edge in world.Network.Edges){var path=world.Network.Path(edge.x,edge.y);int i=path.Length/2;Vector3 forward=(path[i+1]-path[i-1]).normalized,side=Vector3.Cross(Vector3.up,forward);Quaternion rotation=Quaternion.LookRotation(forward);
 foreach(int sign in new[]{-1,1}){Vector3 p=path[i]+side*sign*(world.Network.RoadWidth/2-.4f);DrainCount++;
 Block(p+Vector3.up*.008f,new Vector3(.64f,.014f,1.12f),rotation,shadow);
 foreach(int s in new[]{-1,1}){Block(p+rotation*new Vector3(s*.3f,.024f,0),new Vector3(.045f,.024f,1.12f),rotation,rim);Block(p+rotation*new Vector3(0,.024f,s*.53f),new Vector3(.64f,.024f,.045f),rotation,rim);}
 for(int bar=-3;bar<=3;bar++)Block(p+rotation*new Vector3(0,.022f,bar*.14f),new Vector3(.58f,.022f,.055f),rotation,iron);
 }}
 mesh=new Mesh{name="Combined curb drainage",indexFormat=IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();
 material=new Material(Resources.Load<Shader>("RoadFurniture")){name="Weathered drain iron"};
 var group=new GameObject("Curb drainage details");group.transform.SetParent(transform,false);group.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=group.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
 vertices.Clear();triangles.Clear();colors.Clear();
 }
 void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
