using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CyberCar {
 public sealed class RoadsideVegetation:MonoBehaviour {
 Mesh mesh;Material material;
 public void Build(WorldBuilder world){if(world.Network.Map<2)return;var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();var rng=new System.Random(642+world.Network.Map);
 foreach(var edge in world.Network.Edges){if(world.Network.IsBridge(edge.x,edge.y))continue;var path=world.Network.Path(edge.x,edge.y);for(int k=5;k<path.Length-5;k+=4){Vector3 tangent=(path[k+1]-path[k-1]).normalized,side=Vector3.Cross(Vector3.up,tangent);
 foreach(int sign in new[]{-1,1}){Vector3 center=path[k]+side*sign*(world.Network.RoadWidth/2+4+(float)rng.NextDouble()*5);
 for(int blade=0;blade<26;blade++){Vector3 basePoint=center+new Vector3((float)rng.NextDouble()*2-1,0,(float)rng.NextDouble()*2-1);basePoint.y=world.GroundHeight(basePoint)-.13f;
 float height=.22f+(float)rng.NextDouble()*.5f,width=.012f+(float)rng.NextDouble()*.018f,angle=(float)rng.NextDouble()*Mathf.PI*2;Vector3 across=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*width,bend=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle))*.12f;
 int a=vertices.Count;vertices.Add(basePoint-across);vertices.Add(basePoint+across);vertices.Add(basePoint+Vector3.up*height*.55f+bend*.3f-across*.5f);vertices.Add(basePoint+Vector3.up*height*.55f+bend*.3f+across*.5f);vertices.Add(basePoint+Vector3.up*height+bend);
 Color color=Color.Lerp(new Color(.23f,.27f,.11f),new Color(.46f,.40f,.23f),(float)rng.NextDouble());for(int j=0;j<5;j++)colors.Add(color);
 triangles.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3,a+2,a+4,a+3});
 }
 }
 }}
 mesh=new Mesh{name="Grounded shoulder grass",indexFormat=IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
 material=new Material(Resources.Load<Shader>("RoadGrass"));var o=new GameObject("Non-colliding shoulder grass");o.transform.SetParent(transform,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
 void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
