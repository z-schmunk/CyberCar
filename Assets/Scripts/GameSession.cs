using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public enum GameState{Menu,Briefing,Driving,Debrief,Achievements}
 public sealed class GameSession:MonoBehaviour {
 public const int LevelCount=9;
 public static readonly string[] MapNames={"NEON DISTRICT","CONTAINER HARBOR","RED ROCK PASS","BEACH CLIFFS"};
 public static readonly string[] LevelNames={"Driver orientation","Trust, but verify","Signal lost","Hands on the wheel","Recovery protocol","Zero trust delivery","Yesterday's commands","Signed at the edge","Lights out"};
 public static readonly string[] Briefings={
 "Learn the car on a short route. Yield to people crossing the road. Hold LEFT SHIFT while steering at speed to drift; release it to regain grip.",
 "Follow a fresh multi-district delivery route. A forged GPS route can lead to a closed road. Press Q to validate it against an authenticated reference.",
 "Explore the harbor, beaches and bridges. A signal flood can remove GPS. Press E to fall back to a cached offline map.",
 "Take the winding canyon roads. Untrusted CAN commands affect steering. Press R to isolate the compromised controller.",
 "Navigate busy harbor interchanges under ransomware. Press F to restore a verified backup. Stay alert for earlier attacks.",
 "Crowded city interchanges let a Sybil convoy coordinate against you. Press G to authenticate identities and disrupt their coordination.",
 "A busy harbor is replaying yesterday's emergency brake commands. Press T to reject old timestamps and sequence numbers. Signed messages can still be stale.",
 "Reach the beach-cliff lighthouse over winding bridges. An unsigned firmware update can reverse steering and reduce power. Press Y to roll back to trusted signed firmware.",
 "Deliver after dark. A compromised streetlight controller can turn off road lighting. Headlights remain independent. Press U to isolate remote commands and restore the trusted local lighting schedule."
 };
 public GameState State{get;private set;}=GameState.Menu;public bool Paused{get;private set;}public bool Running=>State==GameState.Driving&&!Paused;
 public WorldBuilder World{get;private set;}public VehicleController Player{get;private set;}public AttackDirector Attacks{get;private set;}
 public readonly List<VehicleController> Cars=new List<VehicleController>();public readonly List<PedestrianAgent> Citizens=new List<PedestrianAgent>();
 public List<int> Route{get;private set;}public int RouteIndex{get;private set;}=1;
 public bool Night{get;private set;}public int Level{get;private set;}public int Difficulty{get;private set;}public bool Freeplay{get;private set;}public int RouteSeed{get;private set;}
 public float Elapsed{get;private set;}public float Limit{get;private set;}public float RouteMeters{get;private set;}
 public int Crashes{get;private set;}public int CivilianStrikes{get;private set;}public int LastLesson=-1;public bool Won{get;private set;}public bool ExtremeAwardEligible=>Freeplay&&Difficulty==3&&World.Network.Columns==9&&CivilianStrikes==0&&Attacks.AllDefended;
 public string EndReason{get;private set;}="";public string Notice{get;private set;}="";public float NoticeUntil;public bool NoticeWarning;public bool Testing;public bool InputEnabled=true;
 public bool InHotspot=>World&&Player&&World.Network.NearbyHotspot(Player.transform.position)>=0;
 public bool ReplayBraking=>Attacks!=null&&Attacks.Has(CyberAttack.Replay)&&Mathf.Sin(Elapsed*2)>0;
 public float Distance=>Vector3.Distance(Player.transform.position,World.Network.Nodes[World.Network.Finish]);
 public Vector3 Target=>Attacks.Has(CyberAttack.Spoofing)?World.Network.Hazard:World.Network.Nodes[Route[Mathf.Min(RouteIndex,Route.Count-1)]];
 float damageTimer,recoverCooldown;readonly HashSet<int> ambushes=new HashSet<int>();
 void Awake(){
 Application.targetFrameRate=60;var args=System.Environment.GetCommandLineArgs();Testing=System.Array.IndexOf(args,"-smokeTest")>=0;ProgressStore.Testing=Testing;
 var cam=new GameObject("Chase camera");var camera=cam.AddComponent<Camera>();camera.fieldOfView=62;camera.farClipPlane=1600;camera.clearFlags=CameraClearFlags.Skybox;cam.AddComponent<AudioListener>();cam.AddComponent<ChaseCamera>().Session=this;
 gameObject.AddComponent<GameHud>().Session=this;gameObject.AddComponent<DriveMusic>().Session=this;
 Prepare(0,0,0,false);State=GameState.Menu;if(Testing)gameObject.AddComponent<RuntimeSmokeTest>().Session=this;
 }
 public void Prepare(int level,int map,int difficulty,bool freeplay,int? seed=null,bool? night=null){
 Time.timeScale=1;Paused=false;GuideOpen=false;if(World){World.gameObject.SetActive(false);Destroy(World.gameObject);}
 foreach(var car in Cars)if(car){car.gameObject.SetActive(false);Destroy(car.gameObject);}Cars.Clear();Citizens.Clear();ambushes.Clear();
 Level=Mathf.Clamp(level,0,LevelCount-1);Night=night??(!freeplay&&level==8);Difficulty=Mathf.Clamp(difficulty,0,3);Freeplay=freeplay;Elapsed=0;Crashes=0;CivilianStrikes=0;RouteIndex=1;damageTimer=0;recoverCooldown=0;Notice="";Won=false;LastLesson=-1;RouteSeed=seed??(Testing?1729:System.Environment.TickCount&int.MaxValue);
 var world=new GameObject("Map - "+MapNames[map]);World=world.AddComponent<WorldBuilder>();World.Generate(map,Difficulty==3);World.CreateSecret();Route=World.Network.MissionRoute(RouteSeed,level==0&&!freeplay);RouteMeters=World.Network.RouteLength(Route);
 Limit=Level==0&&!Freeplay?300:RouteMeters/(Difficulty==0?8:Difficulty==1?9:Difficulty==2?10:11)+(Difficulty==3?55:120);
 Attacks=new AttackDirector(this,RouteSeed);World.Environment.Configure(this,Night);
 Player=Spawn("PLAYER",World.Network.Nodes[0]+Vector3.forward*-2.7f,Quaternion.Euler(0,90,0),true,false);
 int enemies=level==0&&!freeplay?0:1+Difficulty+(level>=4?1:0);
 for(int i=0;i<enemies;i++){int n=i==0?2:(i*5+2)%World.Network.Nodes.Count;var car=Spawn("HOSTILE "+i,World.Network.Nodes[n]+Vector3.forward*2.7f,Quaternion.Euler(0,270,0),false,true);car.gameObject.AddComponent<TrafficAgent>().Initialize(this,true,i);}
 for(int i=0;i<TrafficCount;i++){int n=3+(i*5+3)%(World.Network.Nodes.Count-3);Vector3 pos=World.Network.Nodes[n]+new Vector3(2.7f,0,0);var car=Spawn("TRAFFIC "+i,pos,Quaternion.identity,false,false);car.gameObject.AddComponent<TrafficAgent>().Initialize(this,false,i+20);}
 int citizens=Level==0&&!freeplay?6:12+Difficulty*4;
 for(int i=0;i<Mathf.Min(citizens,World.Crossings.Count);i++){var o=new GameObject("Citizen "+i);o.transform.SetParent(World.transform);var person=o.AddComponent<PedestrianAgent>();person.Initialize(this,World.Crossings[i],i);Citizens.Add(person);}
 State=GameState.Briefing;World.Beacon.position=Target+Vector3.up*.08f;
 }
 public int TrafficCount=>Level==0&&!Freeplay?6:12+Difficulty*5;
 VehicleController Spawn(string name,Vector3 pos,Quaternion rot,bool player,bool hostile){var go=new GameObject(name);go.transform.SetPositionAndRotation(pos+Vector3.up*.1f,rot);var car=go.AddComponent<VehicleController>();car.IsPlayer=player;car.Session=this;car.SetupVisual(player?World.PlayerPaint:hostile?World.EnemyPaint:World.TrafficPaint);Cars.Add(car);return car;}
 public void SelectLevel(int level){int[] maps={0,0,1,2,1,0,1,3,0};Prepare(level,maps[level],level<2?0:level<5?1:2,false);}
 public void Begin(){State=GameState.Driving;foreach(var car in Cars)car.Driving=true;Notify("Follow the GPS gates. Yield at crosswalks. H opens the field guide.");}
 public void Menu(){Paused=false;Time.timeScale=1;State=GameState.Menu;foreach(var car in Cars)car.Driving=false;ProgressStore.Save();}
 public void ShowAchievements(){State=GameState.Achievements;}
 public bool GuideOpen{get;private set;}bool guideWasPaused;
 public void SetGuide(bool open){if(open==GuideOpen)return;GuideOpen=open;if(State==GameState.Driving){if(open){guideWasPaused=Paused;Paused=true;}else Paused=guideWasPaused;Time.timeScale=Paused?0:1;}}
 public void TogglePause(){if(GuideOpen){SetGuide(false);return;}if(State!=GameState.Driving)return;Paused=!Paused;Time.timeScale=Paused?0:1;}
 void Update(){
 if(InputEnabled&&Input.GetKeyDown(KeyCode.Escape))TogglePause();if(!Running)return;
 Elapsed+=Time.deltaTime;recoverCooldown-=Time.deltaTime;Attacks.Tick(Time.deltaTime);
 if(InputEnabled){Player.Throttle=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);Player.Steer=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);Player.Brake=Input.GetKey(KeyCode.Space);Player.Drift=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);for(int i=0;i<AttackDirector.Count;i++)if(Input.GetKeyDown(AttackDirector.KeyCodes[i]))Attacks.Defend(i);if(Input.GetKeyDown(KeyCode.Backspace))Recover();}
 Player.ControlNoise=Attacks.Has(CyberAttack.ControlInjection)?.42f:0;Player.EngineLimit=Attacks.Has(CyberAttack.Ransomware)?.35f:Attacks.Has(CyberAttack.Firmware)?.65f:1;Player.SteeringPolarity=Attacks.Has(CyberAttack.Firmware)?-1:1;
 if(Player.DriftSeconds>=20)Award(12);
 if(Attacks.Has(CyberAttack.ControlInjection)||Attacks.Has(CyberAttack.Ransomware)){damageTimer+=Time.deltaTime;if(damageTimer>4){Player.Damage(1+Difficulty);damageTimer=0;}}
 World.Beacon.gameObject.SetActive(!Attacks.Has(CyberAttack.DenialOfService));World.Beacon.position=Target+Vector3.up*.08f;
 if(Level>0||Freeplay){int hot=World.Network.NearbyHotspot(Player.transform.position);if(hot>=0&&ambushes.Add(hot)){Notify("TRAFFIC HOTSPOT / coordinated attack pressure increased",true);for(int i=0;i<2;i++){int n=Mathf.Clamp(hot+(i==0?1:World.Network.Columns),0,World.Network.Nodes.Count-1);var car=Spawn("INTERCHANGE HOSTILE",World.Network.Nodes[n]+Vector3.right*(i==0?3:-3),Quaternion.identity,false,true);car.Driving=true;car.gameObject.AddComponent<TrafficAgent>().Initialize(this,true,Cars.Count+40);}}}
 if(Vector3.Distance(Player.transform.position,World.Network.Nodes[Route[RouteIndex]])<12){RouteIndex++;if(RouteIndex>=Route.Count){RouteIndex=Route.Count-1;Finish(true,"Delivery verified");return;}}
 if(World.Secret&&World.Secret.gameObject.activeSelf){World.Secret.Rotate(15*Time.deltaTime,50*Time.deltaTime,0);if(Vector3.Distance(Player.transform.position,World.Secret.position)<5){World.Secret.gameObject.SetActive(false);Award(7);}}
 if(Player.Health<=0)Finish(false,"Vehicle disabled");else if(Player.transform.position.y<-12)Finish(false,"Vehicle lost beyond the road");else if(Elapsed>=Limit)Finish(false,"Delivery window expired");
 }
 public void OnCivilianStrike(){CivilianStrikes++;Elapsed+=15;Player.Damage(15);Notify("CROSSWALK SAFETY VIOLATION / +15 seconds, -15 integrity",true);if(Difficulty==3||CivilianStrikes>=3)Finish(false,"Civilian safety limit exceeded");}
 public void Recover(){if(!Running||recoverCooldown>0)return;int last=Mathf.Max(0,RouteIndex-1);Vector3 direction=World.Network.Path(Route[last],Route[Mathf.Min(last+1,Route.Count-1)] )[1]-World.Network.Nodes[Route[last]];Player.Recover(World.Network.Nodes[Route[last]],Quaternion.LookRotation(direction));Player.Damage(10);recoverCooldown=5;Notify("Recovery used: -10 integrity",true);}
 public void OnCrash(float severity){Crashes++;Notify("Impact recorded. Keep a safe following distance.",true);}
 public void Award(int badge){if(ProgressStore.Award(badge))Notify("ACHIEVEMENT / "+ProgressStore.Achievements[badge]);}
 public void Notify(string text,bool warning=false){Notice=text;NoticeWarning=warning;NoticeUntil=Time.unscaledTime+6;}
 public void Finish(bool won,string reason){if(State!=GameState.Driving)return;Won=won;EndReason=reason;State=GameState.Debrief;Paused=false;Time.timeScale=1;foreach(var car in Cars){car.Driving=false;car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;}if(won){Award(0);if(Crashes==0)Award(6);if(CivilianStrikes==0)Award(13);if(ExtremeAwardEligible)Award(11);if(!Freeplay){ProgressStore.Complete(Level);if(Level==5)Award(8);if(Level==7)Award(14);if(Level==8)Award(16);}}ProgressStore.Save();}
 void OnApplicationQuit(){ProgressStore.Save();}
 void OnDestroy(){Time.timeScale=1;}
 }
}
