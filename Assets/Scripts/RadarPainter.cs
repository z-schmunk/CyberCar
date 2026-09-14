using UnityEngine;
namespace CyberCar {
 // Rasterize connected road strokes once per refresh to avoid rotated IMGUI seam artifacts.
 public sealed class RadarPainter {
 const int Size=320;const float Radius=156,Range=170;readonly Color[] pixels=new Color[Size*Size];
 public readonly Texture2D Texture=new Texture2D(Size,Size,TextureFormat.RGBA32,false);
 float next;Vector3 origin,forward,right;readonly Color background=new Color(.035f,.06f,.08f,.98f);
 Vector2 Map(Vector3 p){Vector3 d=p-origin;return new Vector2(160+Vector3.Dot(d,right)*Radius/Range,160+Vector3.Dot(d,forward)*Radius/Range);}
 void Stroke(Vector2 a,Vector2 b,float width,Color color){
 float r=width/2;int x0=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.x,b.x)-r-1)),x1=Mathf.Min(Size-1,Mathf.CeilToInt(Mathf.Max(a.x,b.x)+r+1));
 int y0=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(a.y,b.y)-r-1)),y1=Mathf.Min(Size-1,Mathf.CeilToInt(Mathf.Max(a.y,b.y)+r+1));Vector2 d=b-a;float len=d.sqrMagnitude;
 for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){Vector2 p=new Vector2(x+.5f,y+.5f);float t=len<.0001f?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/len);float alpha=Mathf.Clamp01(r+.5f-Vector2.Distance(p,a+t*d));if(alpha>0)pixels[y*Size+x]=Color.Lerp(pixels[y*Size+x],color,alpha);}
 }
 void Path(Vector3[] path,float width,Color color){for(int i=1;i<path.Length;i++)Stroke(Map(path[i-1]),Map(path[i]),width,color);}
 public void Refresh(GameSession session){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.1f;origin=session.Comms.EstimatedPosition;forward=Vector3.ProjectOnPlane(session.Player.transform.forward,Vector3.up).normalized;right=Vector3.Cross(Vector3.up,forward);
 for(int i=0;i<pixels.Length;i++)pixels[i]=background;
 var graph=session.World.Network;foreach(var e in graph.Edges)Path(graph.Path(e.x,e.y),9,new Color(.32f,.4f,.44f));
 Color route=new Color(.13f,.93f,.83f);
 if(session.Attacks.Has(CyberAttack.Spoofing)||session.Attacks.Has(CyberAttack.DestinationInjection)){
 var diversion=graph.TravelPath(graph.Route(graph.Nearest(origin),graph.Columns-1));Path(diversion.ToArray(),5,new Color(1,.56f,.5f));Stroke(Map(graph.Nodes[graph.Columns-1]),Map(graph.Hazard),5,new Color(1,.56f,.5f));
 }else for(int n=Mathf.Max(1,session.RouteIndex);n<session.Route.Count;n++)Path(graph.Path(session.Route[n-1],session.Route[n]),5,route);
 for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(160,160));pixels[y*Size+x].a*=Mathf.Clamp01(Radius-d);}
 Texture.SetPixels(pixels);Texture.Apply(false,false);
 }
 public void Dispose(){Object.Destroy(Texture);}
 }
}
