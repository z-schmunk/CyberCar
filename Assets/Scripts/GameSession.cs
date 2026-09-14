using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public enum GameState{Menu,Briefing,Driving,Debrief,Achievements}
 public sealed class GameSession:MonoBehaviour {
 public const int LevelCount=15;
 public static readonly string[] MapNames={"NEON DISTRICT","CONTAINER HARBOR","RED ROCK PASS","BEACH CLIFFS"};
 public static readonly string[] LevelNames=StoryCampaign.Titles;
 public static readonly string[] Briefings=StoryCampaign.Story;
 public GameState State{get;private set;}=GameState.Menu;public bool Paused{get;private set;}public bool Running=>State==GameState.Driving&&!Paused;
 public WorldBuilder World{get;private set;}public VehicleController Player{get;private set;}public AttackDirector Attacks{get;private set;}
 public readonly List<VehicleController> Cars=new List<VehicleController>();public readonly List<PedestrianAgent> Citizens=new List<PedestrianAgent>();
 public List<int> Route{get;private set;}public int RouteIndex{get;private set;}=1;
 public bool Night{get;private set;}public int Level{get;private set;}public int Difficulty{get;private set;}public bool Freeplay{get;private set;}public int RouteSeed{get;private set;}
 public float Elapsed{get;private set;}public float Limit{get;private set;}public float RouteMeters{get;private set;}
 public int Recoveries{get;private set;}public int Crashes{get;private set;}public int CivilianStrikes{get;private set;}public int LastLesson=-1;public bool Won{get;private set;}public bool ExtremeAwardEligible=>Freeplay&&Difficulty==3&&World.Network.Columns==9&&CivilianStrikes==0&&Attacks.AllDefended;
 public string EndReason{get;private set;}="";public string Notice{get;private set;}="";public float NoticeUntil;public bool NoticeWarning;public bool Testing;public bool InputEnabled=true;
 public bool InHotspot=>World&&Player&&World.Network.NearbyHotspot(Player.transform.position)>=0;
 public bool ReplayBraking=>Attacks!=null&&(Attacks.Has(CyberAttack.Replay)&&Mathf.Sin(Elapsed*2)>0||Player&&Player.GetComponent<CarSensors>()&&Player.GetComponent<CarSensors>().FalseEmergencyBrake);
 public float Distance=>Vector3.Distance(Player.transform.position,World.Network.Nodes[World.Network.Finish]);
 public Vector3 Target=>(Attacks.Has(CyberAttack.Spoofing)||Attacks.Has(CyberAttack.DestinationInjection))?World.Network.Hazard:World.Network.Nodes[Route[Mathf.Min(RouteIndex,Route.Count-1)]];
 public DefenseChallenge Defense{get;private set;}public RoadsideNetwork Comms{get;private set;}public bool AttackerMode{get;private set;}public int AttackBudget{get;private set;}=12;float labDefenseTimer;
 float damageTimer,recoverCooldown,stuckTimer;readonly HashSet<int> ambushes=new HashSet<int>();
 void Awake(){
 Application.targetFrameRate=60;var args=System.Environment.GetCommandLineArgs();Testing=System.Array.IndexOf(args,"-smokeTest")>=0;ProgressStore.Testing=Testing;
 var cam=new GameObject("Chase camera");var camera=cam.AddComponent<Camera>();camera.fieldOfView=62;camera.farClipPlane=1600;camera.clearFlags=CameraClearFlags.Skybox;cam.AddComponent<AudioListener>();cam.AddComponent<ChaseCamera>().Session=this;
 gameObject.AddComponent<GameHud>().Session=this;gameObject.AddComponent<DriveMusic>().Session=this;
 Prepare(0,0,0,false);State=GameState.Menu;if(Testing)gameObject.AddComponent<RuntimeSmokeTest>().Session=this;
 }
 public void Prepare(int level,int map,int difficulty,bool freeplay,int? seed=null,bool? night=null,bool attacker=false){
 Time.timeScale=1;Paused=false;GuideOpen=false;AttackerMode=attacker;AttackBudget=12;labDefenseTimer=0;stuckTimer=0;if(World){World.gameObject.SetActive(false);Destroy(World.gameObject);}
 foreach(var car in Cars)if(car){car.gameObject.SetActive(false);Destroy(car.gameObject);}Cars.Clear();Citizens.Clear();ambushes.Clear();
 Level=Mathf.Clamp(level,0,LevelCount-1);Night=night??(!freeplay&&(level==8||level==14));Difficulty=Mathf.Clamp(difficulty,0,3);Freeplay=freeplay;Elapsed=0;Crashes=0;Recoveries=0;CivilianStrikes=0;RouteIndex=1;damageTimer=0;recoverCooldown=0;Notice="";Won=false;LastLesson=-1;RouteSeed=seed??(Testing?1729:System.Environment.TickCount&int.MaxValue);
 var world=new GameObject("Map - "+MapNames[map]);World=world.AddComponent<WorldBuilder>();World.Generate(map,Difficulty==3);World.CreateSecret();Route=freeplay?World.Network.MissionRoute(RouteSeed):StoryCampaign.Route(World.Network,Level);RouteMeters=World.Network.RouteLength(Route);
 Limit=Level==0&&!Freeplay?300:RouteMeters/(Difficulty==0?8:Difficulty==1?9:Difficulty==2?10:11)+(Difficulty==3?55:120);
 Attacks=new AttackDirector(this,RouteSeed);Defense=new DefenseChallenge(this,RouteSeed);Comms=new RoadsideNetwork(this);World.Environment.Configure(this,Night);
 Player=Spawn("PLAYER",World.Network.Nodes[0]+Vector3.forward*-2.7f,Quaternion.Euler(0,90,0),true,false);
 Player.gameObject.AddComponent<CarSensors>();
 if(attacker)Player.gameObject.AddComponent<LabCourier>().Session=this;
 int enemies=level==0&&!freeplay?0:1+Difficulty+(level>=4?1:0);
 for(int i=0;i<enemies;i++){int n=i==0?2:(i*5+2)%World.Network.Nodes.Count;var car=Spawn("HOSTILE "+i,World.Network.Nodes[n]+Vector3.forward*2.7f,Quaternion.Euler(0,270,0),false,true);car.gameObject.AddComponent<TrafficAgent>().Initialize(this,true,i);}
 for(int i=0;i<TrafficCount;i++){int n=3+((i/3)*7)%(World.Network.Nodes.Count-4);var route=World.Network.Route(n,(n+1)%World.Network.Nodes.Count);var path=World.Network.Path(route[0],route[1]);int k=4+(i%3)*4;Vector3 forward=(path[k+1]-path[k-1]).normalized;Vector3 pos=path[k]+Vector3.Cross(Vector3.up,forward)*2.7f;var car=Spawn("PLATOON "+(i/3+1)+" / "+(i%3+1),pos,Quaternion.LookRotation(forward),false,false);car.gameObject.AddComponent<TrafficAgent>().Initialize(this,false,i+20);}
 int citizens=Level==0&&!freeplay?6:12+Difficulty*4;
 for(int i=0;i<Mathf.Min(citizens,World.Crossings.Count);i++){var o=new GameObject("Citizen "+i);o.transform.SetParent(World.transform);var person=o.AddComponent<PedestrianAgent>();person.Initialize(this,World.Crossings[i],i);Citizens.Add(person);}
 Comms.Tick(0);World.AddDestinationSigns();State=GameState.Briefing;World.Beacon.gameObject.SetActive(false);
 }
 public int TrafficCount=>Level==0&&!Freeplay?6:12+Difficulty*5;
 VehicleController Spawn(string name,Vector3 pos,Quaternion rot,bool player,bool hostile){var go=new GameObject(name);go.transform.SetPositionAndRotation(pos+Vector3.up*.1f,rot);var car=go.AddComponent<VehicleController>();car.IsPlayer=player;car.Session=this;car.SetupVisual(player?World.PlayerPaint:hostile?World.EnemyPaint:World.TrafficPaint);Cars.Add(car);return car;}
 public void SelectLevel(int level){Prepare(level,StoryCampaign.Maps[level],level<2?0:level<5?1:level<14?2:3,false);}
 public void Begin(){State=GameState.Driving;foreach(var car in Cars)car.Driving=true;Notify("Follow the route arrow. Yield at crosswalks. Select threats, then use 1-3 to diagnose.");}
 public void Menu(){Paused=false;Time.timeScale=1;State=GameState.Menu;foreach(var car in Cars)car.Driving=false;ProgressStore.Save();}
 public void ShowAchievements(){State=GameState.Achievements;}
 public bool GuideOpen{get;private set;}bool guideWasPaused;
 public void SetGuide(bool open){if(open==GuideOpen)return;GuideOpen=open;if(State==GameState.Driving){if(open){guideWasPaused=Paused;Paused=true;}else Paused=guideWasPaused;Time.timeScale=Paused?0:1;}}
 public void TogglePause(){if(GuideOpen){SetGuide(false);return;}if(State!=GameState.Driving)return;Paused=!Paused;Time.timeScale=Paused?0:1;}
 void Update(){
 if(InputEnabled&&Input.GetKeyDown(KeyCode.Escape))TogglePause();if(!Running)return;
 Elapsed+=Time.deltaTime;recoverCooldown-=Time.deltaTime;Attacks.Tick(Time.deltaTime);if(Defense.Open&&!Attacks.Has((CyberAttack)Defense.Attack))Defense.Close();
 Comms.Tick(Time.deltaTime);
 if(InputEnabled&&!AttackerMode){Player.Throttle=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);Player.Steer=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);Player.Brake=Input.GetKey(KeyCode.Space);Player.Drift=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);for(int i=0;i<AttackDirector.Count;i++)if(Input.GetKeyDown(AttackDirector.KeyCodes[i]))Defense.Begin(i);if(Input.GetKeyDown(KeyCode.Backspace))Recover();if(Defense.Open){if(Input.GetKeyDown(KeyCode.Alpha1))Defense.Choose(0);if(Input.GetKeyDown(KeyCode.Alpha2))Defense.Choose(1);if(Input.GetKeyDown(KeyCode.Alpha3))Defense.Choose(2);}}
 if(AttackerMode){labDefenseTimer+=Time.deltaTime;if(labDefenseTimer>20){foreach(var r in Attacks.Records)if(r.Active){Defense.Begin((int)r.Type);for(int step=0;step<3&&Defense.Open;step++)Defense.Choose(Defense.EvidenceAnswer);break;}labDefenseTimer=0;}}
 Player.ControlNoise=Attacks.Has(CyberAttack.ControlInjection)?.42f:0;Player.EngineLimit=Attacks.Has(CyberAttack.Ransomware)?.35f:Attacks.Has(CyberAttack.Firmware)?.65f:1;Player.SteeringPolarity=Attacks.Has(CyberAttack.Firmware)?-1:1;
 if(Player.DriftSeconds>=20)Award(12);
 if(Attacks.Has(CyberAttack.ControlInjection)||Attacks.Has(CyberAttack.Ransomware)){damageTimer+=Time.deltaTime;if(damageTimer>4){Player.Damage(1+Difficulty);damageTimer=0;}}
 World.Beacon.gameObject.SetActive(false);
 bool fallen=World.FatalFall(Player.transform.position);
 if(fallen){Finish(AttackerMode,AttackerMode?"Lab courier diverted into the closed bridge":"Vehicle lost beyond the road");return;}
 if(!Player.Upright&&Player.Body.linearVelocity.magnitude<2||Player.Grounded&&Mathf.Abs(Player.Speed)<.4f&&Mathf.Abs(Player.Throttle)>.5f)stuckTimer+=Time.deltaTime;else stuckTimer=0;
 if(stuckTimer>7){Recover();stuckTimer=0;}
 if(Level>0||Freeplay){int hot=World.Network.NearbyHotspot(Player.transform.position);if(hot>=0&&ambushes.Add(hot)){Notify("TRAFFIC HOTSPOT / coordinated attack pressure increased",true);for(int i=0;i<2;i++){int n=Mathf.Clamp(hot+(i==0?1:World.Network.Columns),0,World.Network.Nodes.Count-1);var car=Spawn("INTERCHANGE HOSTILE",World.Network.Nodes[n]+Vector3.right*(i==0?3:-3),Quaternion.identity,false,true);car.Driving=true;car.gameObject.AddComponent<TrafficAgent>().Initialize(this,true,Cars.Count+40);}}}
 if(Vector3.Distance(Player.transform.position,World.Network.Nodes[Route[RouteIndex]])<12){RouteIndex++;if(RouteIndex>=Route.Count){RouteIndex=Route.Count-1;Finish(!AttackerMode,AttackerMode?"Lab courier resisted and delivered":"Delivery verified");return;}}
 if(World.Secret&&World.Secret.gameObject.activeSelf){World.Secret.Rotate(15*Time.deltaTime,50*Time.deltaTime,0);if(Vector3.Distance(Player.transform.position,World.Secret.position)<5){World.Secret.gameObject.SetActive(false);Award(7);}}
 if(Player.Health<=0)Finish(AttackerMode,"Vehicle disabled");else if(Elapsed>=Limit)Finish(AttackerMode,AttackerMode?"Lab objective: delivery delayed":"Delivery window expired");
 }
 public void ElapsedPenalty(float seconds){Elapsed+=seconds;}
 public void LaunchLab(int attack){if(!AttackerMode||!Running||AttackBudget<2||Attacks.Has((CyberAttack)attack))return;AttackBudget-=2;Attacks.Launch((CyberAttack)attack);}
 public void OnCivilianStrike(){CivilianStrikes++;Elapsed+=15;Player.Damage(15);Notify("CROSSWALK SAFETY VIOLATION / +15 seconds, -15 integrity",true);if(Difficulty==3||CivilianStrikes>=3)Finish(false,"Civilian safety limit exceeded");}
 public void Recover(){if(!Running||recoverCooldown>0||World.InFatalZone(Player.transform.position))return;int last=Mathf.Max(0,RouteIndex-1);Vector3 direction=World.Network.Path(Route[last],Route[Mathf.Min(last+1,Route.Count-1)] )[1]-World.Network.Nodes[Route[last]];Player.Recover(World.Network.Nodes[Route[last]],Quaternion.LookRotation(direction));Player.Damage(10);Recoveries++;recoverCooldown=5;Notify("Recovery used: -10 integrity",true);}
 public void OnCrash(float severity){Crashes++;Notify("Impact recorded. Keep a safe following distance.",true);}
 public void Award(int badge){if(ProgressStore.Award(badge))Notify("ACHIEVEMENT / "+ProgressStore.Achievements[badge]);}
 public void Notify(string text,bool warning=false){Notice=text;NoticeWarning=warning;NoticeUntil=Time.unscaledTime+6;}
 public void Finish(bool won,string reason){if(State!=GameState.Driving)return;Won=won;EndReason=reason;State=GameState.Debrief;Paused=false;Time.timeScale=1;foreach(var car in Cars){car.Driving=false;car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;}if(won&&!AttackerMode){ProgressStore.Reward(Level,Freeplay);Award(0);if(Crashes==0)Award(6);if(CivilianStrikes==0)Award(13);if(ExtremeAwardEligible)Award(11);if(!Freeplay){ProgressStore.Complete(Level);if(Level==5)Award(8);if(Level==7)Award(14);if(Level==8)Award(16);}}ProgressStore.Save();}
 void OnApplicationQuit(){ProgressStore.Save();}
 void OnDestroy(){Time.timeScale=1;}
 }
}
