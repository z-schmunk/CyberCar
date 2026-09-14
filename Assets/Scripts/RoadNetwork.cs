using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public sealed class RoadNetwork {
 public readonly List<Vector3> Nodes=new List<Vector3>();
 public readonly List<Vector2Int> Edges=new List<Vector2Int>();
 public readonly List<string> Names=new List<string>();
 public readonly List<int> Hotspots=new List<int>();
 readonly Dictionary<long,Vector3[]> paths=new Dictionary<long,Vector3[]>();
 public int Map{get;private set;} public int Columns{get;private set;} public int Rows=>Columns;
 public float Spacing=>96; public float Size=>(Columns-1)*Spacing; public float RoadWidth=>Map==3?13:18;
 public int Finish{get;private set;} public Vector3 Hazard{get;private set;}
 public RoadNetwork(int map,bool extreme=false) {
 Map=map;Columns=extreme?9:map==3?7:6;
 for(int z=0;z<Rows;z++)for(int x=0;x<Columns;x++){int n=Nodes.Count;float warp=map==0?10:map==1?14:30;float px=x*Spacing+Mathf.Sin(z*.83f)*warp, pz=z*Spacing+Mathf.Sin(x*.91f)*warp;float y=map>=2?Mathf.Sin(x*.63f)*5+z*1.5f:0;Nodes.Add(new Vector3(px,y,pz));Names.Add("District "+(z+1)+" / Junction "+(x+1));if(x>0)Edges.Add(new Vector2Int(n-1,n));if(z>0&&(map<2||x==0||x==Columns-1||x%3==z%3))Edges.Add(new Vector2Int(n-Columns,n));}
 Finish=Nodes.Count-1;Names[0]="Operations garage";Names[Finish]=map==0?"Security campus":map==1?"Harbor control":map==2?"Summit observatory":"Clifftop lighthouse";
 Hotspots.Add(Columns+2);Hotspots.Add((Rows/2)*Columns+Columns/2);Hotspots.Add((Rows-2)*Columns+Columns-2);
 foreach(int n in Hotspots)Names[n]="Traffic interchange "+(Hotspots.IndexOf(n)+1);
 Hazard=Nodes[Columns-1]+Vector3.right*120;
 }
 public bool IsBridge(int a,int b)=>Map!=0&&a/Columns==b/Columns&&Mathf.Min(a%Columns,b%Columns)==Columns/2-1;
 public Vector3[] Path(int a,int b) {
 long key=((long)a<<32)|(uint)b;if(paths.TryGetValue(key,out var stored))return stored;
 bool reversed=a>b;int lo=Mathf.Min(a,b),hi=Mathf.Max(a,b);Vector3 from=Nodes[lo],to=Nodes[hi],side=Vector3.Cross(Vector3.up,(to-from).normalized);
 float curve=((lo+hi)%3==0?0:Map==0?5:Map==1?8:13)*Mathf.Sin(lo*1.713f+hi*.317f);const int steps=24;var result=new Vector3[steps+1];
 bool horizontal=lo/Columns==hi/Columns;int stride=horizontal?1:Columns;
 Vector3 before=(horizontal?lo%Columns>0:lo>=Columns)?Nodes[lo-stride]:from-(to-from);
 Vector3 after=(horizontal?hi%Columns<Columns-1:hi<Nodes.Count-Columns)?Nodes[hi+stride]:to+(to-from);
 Vector3 c1=from+(to-before).normalized*Vector3.Distance(from,to)/3,c2=to-(after-from).normalized*Vector3.Distance(from,to)/3;
 for(int i=0;i<=steps;i++){float t=i/(float)steps,u=1-t,bend=Mathf.Sin(t*Mathf.PI);bend*=bend;result[i]=u*u*u*from+3*u*u*t*c1+3*u*t*t*c2+t*t*t*to+side*curve*bend;float grade=Mathf.Clamp01((t-.15f)/.7f);result[i].y=Mathf.SmoothStep(from.y,to.y,grade);if(IsBridge(lo,hi))result[i].y+=3.5f*Mathf.Pow(Mathf.Sin(grade*Mathf.PI),2);}
 if(reversed)System.Array.Reverse(result);paths[key]=result;return result;
 }
 public int Nearest(Vector3 p){int best=0;float distance=float.MaxValue;for(int i=0;i<Nodes.Count;i++){float d=(Nodes[i]-p).sqrMagnitude;if(d<distance){best=i;distance=d;}}return best;}
 public List<int> Route(int start,int end,bool wrongWay=false) {
 var q=new Queue<int>();var prev=new Dictionary<int,int>();q.Enqueue(start);prev[start]=-1;
 while(q.Count>0){int n=q.Dequeue();if(n==end)break;foreach(var e in Edges){int k=e.x==n?e.y:e.y==n?e.x:-1;if(k<0||prev.ContainsKey(k)||!wrongWay&&!CanTravel(n,k))continue;prev[k]=n;q.Enqueue(k);}}
 var route=new List<int>();if(!prev.ContainsKey(end))return route;for(int n=end;n!=-1;n=prev[n])route.Add(n);route.Reverse();return route;
 }
 public bool OneWay(int a,int b)=>Map==0&&a/Columns==b/Columns&&(a/Columns==1||a/Columns==3);
 public bool CanTravel(int a,int b)=>!OneWay(a,b)||(a/Columns==1?b>a:a>b);
 public float SpeedLimit=>Map==0?11.18f:Map==1?15.65f:22.35f;
 public Vector3 ClosestRoad(Vector3 p,out Vector3 tangent,out float distance){
 Vector3 best=Nodes[0];tangent=Vector3.forward;float sq=float.MaxValue;
 foreach(var e in Edges){var path=Path(e.x,e.y);for(int i=1;i<path.Length;i++){Vector3 a=path[i-1],d=path[i]-a;Vector3 sample=a+d*Mathf.Clamp01(Vector3.Dot(p-a,d)/d.sqrMagnitude);float value=(p-sample).sqrMagnitude;if(value<sq){sq=value;best=sample;tangent=d.normalized;}}}
 distance=Mathf.Sqrt(sq);return best;
 }
 public List<int> MissionRoute(int seed=42,bool orientation=false) {
 var rng=new System.Random(seed);var stops=new List<int>();
 if(orientation){stops.Add(2);stops.Add(Columns+2);Finish=Columns+1;Names[Finish]="Training depot";}
 else {int h=Columns/2;stops.Add(rng.Next(h,Columns)+rng.Next(0,h)*Columns);stops.Add(rng.Next(h,Columns)+rng.Next(h,Rows)*Columns);stops.Add(rng.Next(0,h)+rng.Next(h,Rows)*Columns);if(rng.Next(2)==0){int swap=stops[0];stops[0]=stops[2];stops[2]=swap;}stops.Insert(1,Hotspots[rng.Next(Hotspots.Count)]);}
 stops.Add(Finish);var result=new List<int>{0};int current=0;foreach(int stop in stops){var part=Route(current,stop);if(part.Count>1){part.RemoveAt(0);result.AddRange(part);}current=stop;}return result;
 }
 public List<Vector3> TravelPath(List<int> route){var points=new List<Vector3>();for(int i=1;i<route.Count;i++){var part=Path(route[i-1],route[i]);for(int j=i==1?0:1;j<part.Length;j++)points.Add(part[j]);}return points;}
 public float RouteLength(List<int> route){float length=0;var p=TravelPath(route);for(int i=1;i<p.Count;i++)length+=Vector3.Distance(p[i-1],p[i]);return length;}
 public int NearbyHotspot(Vector3 p,float radius=75){foreach(int n in Hotspots)if(Vector3.Distance(p,Nodes[n])<radius)return n;return -1;}
 }
}
