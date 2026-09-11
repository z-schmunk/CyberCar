using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CyberCar {
 public sealed class WorldEnvironment:MonoBehaviour {
 public bool Night{get;private set;}public bool Blackout{get;private set;}
 public int LitStreetlights{get;private set;}
 sealed class Lamp {public Light Light;}
 readonly List<Lamp> lamps=new List<Lamp>();GameSession session;WorldBuilder world;Light sun;Material sky;float refresh;
 public void Build(WorldBuilder owner){
 world=owner;sun=world.GetComponentInChildren<Light>();
 foreach(var e in world.Network.Edges){
 var path=world.Network.Path(e.x,e.y);
 for(int i=4;i<path.Length-2;i+=8){
 Vector3 forward=(path[i+1]-path[i-1]).normalized,side=Vector3.Cross(Vector3.up,forward).normalized;
 int sign=(i+e.x)%2==0?1:-1;Vector3 position=path[i]+side*sign*(world.Network.RoadWidth/2+2);
 world.Photos.Prop("StreetLamp",position,8,Mathf.Atan2(-side.x*sign,-side.z*sign)*Mathf.Rad2Deg);
 var o=new GameObject("Streetlight pool");o.transform.SetParent(transform);o.transform.position=position+Vector3.up*7.1f;o.transform.rotation=Quaternion.Euler(90,0,0);
 var light=o.AddComponent<Light>();light.type=LightType.Spot;light.spotAngle=120;light.innerSpotAngle=65;light.range=34;light.intensity=1.7f;light.color=new Color(1,.8f,.55f);light.shadows=LightShadows.None;light.enabled=false;
 lamps.Add(new Lamp{Light=light});
 }}
 }
 public void Configure(GameSession game,bool night){
 session=game;Night=night;if(sky)Destroy(sky);
 var template=Resources.Load<Material>(night?"NightSky":"DaySky");if(!template)throw new System.InvalidOperationException("Missing panorama sky material");
 sky=new Material(template);RenderSettings.skybox=sky;
 sun.intensity=night?.09f:1.1f;sun.color=night?new Color(.52f,.64f,1):new Color(1,.95f,.87f);sun.transform.rotation=Quaternion.Euler(night?36:48,night?-65:-32,0);
 RenderSettings.ambientMode=AmbientMode.Trilight;
 RenderSettings.ambientSkyColor=night?new Color(.035f,.045f,.085f):new Color(.5f,.62f,.75f);
 RenderSettings.ambientEquatorColor=night?new Color(.015f,.025f,.045f):new Color(.35f,.4f,.45f);
 RenderSettings.ambientGroundColor=night?new Color(.012f,.016f,.026f):new Color(.18f,.17f,.16f);
 RenderSettings.fogColor=night?new Color(.018f,.028f,.048f):new Color(.6f,.7f,.8f);RenderSettings.fogDensity=night?.001f:.00065f;
 DynamicGI.UpdateEnvironment();Refresh();
 }
 void Update(){if(!session)return;bool blackout=Night&&session.Attacks!=null&&session.Attacks.Has(CyberAttack.Blackout);if(blackout!=Blackout){Blackout=blackout;refresh=0;}refresh-=Time.deltaTime;if(refresh<=0){Refresh();refresh=.25f;}}
 void Refresh(){
 LitStreetlights=0;Vector3 driver=session&&session.Player?session.Player.transform.position:Vector3.zero;
 bool powered=Night&&!Blackout;world.Photos.SetLampPower(powered);
 foreach(var lamp in lamps){lamp.Light.enabled=powered&&(lamp.Light.transform.position-driver).sqrMagnitude<110*110;if(lamp.Light.enabled)LitStreetlights++;}
 }
 void OnDestroy(){if(sky)Destroy(sky);}
 }
}