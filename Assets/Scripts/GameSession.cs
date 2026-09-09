using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    public enum GameState{Menu,Briefing,Driving,Debrief,Achievements}
    public sealed class GameSession:MonoBehaviour
    {
        public static readonly string[] MapNames={"NEON DISTRICT","CONTAINER HARBOR","RED ROCK PASS"};
        public static readonly string[] LevelNames={"Driver orientation","Trust, but verify","Signal lost","Hands on the wheel","Recovery protocol","Zero trust delivery"};
        public static readonly string[] Briefings={
            "Deliver your car to the security campus. Follow the teal GPS gates. Learn steering, braking and collisions before threats begin.",
            "A hostile vehicle is broadcasting a counterfeit GPS route. Watch road signs and verify the signed route with Q when the destination changes.",
            "Navigate the harbor while attackers spoof GPS and flood the navigation channel. Q verifies your route; E enables offline navigation.",
            "Cross the canyon. Untrusted CAN commands can interfere with steering. R isolates the control bus. Keep driving while defending.",
            "Return through the harbor under a ransomware campaign. F restores a clean backup. Previous attacks remain in play.",
            "Deliver to the security campus under all five attacks. G authenticates V2X identities to disrupt the hostile convoy. Expect overlapping threats."};
        public GameState State {get;private set;}=GameState.Menu;
        public bool Paused {get;private set;}
        public bool Running=>State==GameState.Driving&&!Paused;
        public WorldBuilder World {get;private set;}
        public VehicleController Player {get;private set;}
        public AttackDirector Attacks {get;private set;}
        public readonly List<VehicleController> Cars=new List<VehicleController>();
        public List<int> Route {get;private set;}
        public int RouteIndex {get;private set;}=1;
        public int Level {get;private set;}
        public int Difficulty {get;private set;}
        public bool Freeplay {get;private set;}
        public float Elapsed {get;private set;}
        public float Limit=>Level==0&&!Freeplay?300:Difficulty==0?240:Difficulty==1?190:150;
        public int Crashes {get;private set;}
        public bool Won {get;private set;}
        public string EndReason {get;private set;}="";
        public string Notice {get;private set;}="";
        public float NoticeUntil;
        public bool NoticeWarning;
        public bool Testing;
        public bool InputEnabled=true;
        public float Distance=>Vector3.Distance(Player.transform.position,World.Network.Nodes[World.Network.Finish]);
        public Vector3 Target=>Attacks.Has(CyberAttack.Spoofing)?World.Network.Hazard:World.Network.Nodes[Route[Mathf.Min(RouteIndex,Route.Count-1)]];
        float damageTimer,recoverCooldown;
        void Awake()
        {
            Application.targetFrameRate=60;
            var args=System.Environment.GetCommandLineArgs();Testing=System.Array.IndexOf(args,"-smokeTest")>=0;ProgressStore.Testing=Testing;
            var cam=new GameObject("Chase camera");var camera=cam.AddComponent<Camera>();camera.fieldOfView=62;camera.farClipPlane=700;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.1f,.18f,.24f);cam.AddComponent<AudioListener>();cam.AddComponent<ChaseCamera>().Session=this;
            gameObject.AddComponent<GameHud>().Session=this;
            Prepare(0,0,0,false);State=GameState.Menu;
            if(Testing)gameObject.AddComponent<RuntimeSmokeTest>().Session=this;
        }
        public void Prepare(int level,int map,int difficulty,bool freeplay)
        {
            Time.timeScale=1;Paused=false;
            if(World){World.gameObject.SetActive(false);Destroy(World.gameObject);}
            foreach(var car in Cars)if(car){car.gameObject.SetActive(false);Destroy(car.gameObject);}Cars.Clear();
            Level=level;Difficulty=difficulty;Freeplay=freeplay;Elapsed=0;Crashes=0;RouteIndex=1;damageTimer=0;recoverCooldown=0;Notice="";Won=false;
            var world=new GameObject("Map - "+MapNames[map]);World=world.AddComponent<WorldBuilder>();World.Generate(map);World.CreateSecret();Route=World.Network.MissionRoute();
            Attacks=new AttackDirector(this);
            Player=Spawn("PLAYER / Interceptor",World.Network.Nodes[0]+Vector3.right*2,Quaternion.Euler(0,90,0),true,false);
            int enemies=level==0&&!freeplay?0:1+difficulty+(level>=4?1:0);
            for(int i=0;i<enemies;i++)
            {
                int n=i==0?2:(i*5+2)%World.Network.Nodes.Count;
                var car=Spawn("HOSTILE / "+i,World.Network.Nodes[n]+new Vector3(3,0,0),Quaternion.Euler(0,270,0),false,true);
                car.gameObject.AddComponent<TrafficAgent>().Initialize(this,true,i);
            }
            for(int i=0;i<3+difficulty*2;i++)
            {
                int n=(i*3+6)%World.Network.Nodes.Count;
                var car=Spawn("TRAFFIC / "+i,World.Network.Nodes[n]+new Vector3(-3,0,0),Quaternion.identity,false,false);
                car.gameObject.AddComponent<TrafficAgent>().Initialize(this,false,i);
            }
            State=GameState.Briefing;World.Beacon.position=Target+Vector3.up*.08f;
        }
        VehicleController Spawn(string name,Vector3 pos,Quaternion rot,bool player,bool hostile)
        {
            var go=new GameObject(name);go.transform.SetPositionAndRotation(pos+Vector3.up*.1f,rot);var car=go.AddComponent<VehicleController>();car.IsPlayer=player;car.Session=this;car.SetupVisual(player?World.PlayerPaint:hostile?World.EnemyPaint:World.TrafficPaint);Cars.Add(car);return car;
        }
        public void SelectLevel(int level)
        {
            int[] maps={0,0,1,2,1,0};Prepare(level,maps[level],level<2?0:level<5?1:2,false);
        }
        public void Begin(){State=GameState.Driving;foreach(var car in Cars)car.Driving=true;Notify("Follow the teal GPS gates. Reach "+World.Network.Names[World.Network.Finish]+".");}
        public void Menu(){Paused=false;Time.timeScale=1;State=GameState.Menu;foreach(var car in Cars)car.Driving=false;}
        public void ShowAchievements(){State=GameState.Achievements;}
        public void TogglePause(){if(State!=GameState.Driving)return;Paused=!Paused;Time.timeScale=Paused?0:1;}
        void Update()
        {
            if(InputEnabled&&Input.GetKeyDown(KeyCode.Escape))TogglePause();
            if(!Running)return;
            Elapsed+=Time.deltaTime;recoverCooldown-=Time.deltaTime;Attacks.Tick(Time.deltaTime);
            if(InputEnabled)
            {
                Player.Throttle=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
                Player.Steer=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);Player.Brake=Input.GetKey(KeyCode.Space);
                KeyCode[] keys={KeyCode.Q,KeyCode.E,KeyCode.R,KeyCode.F,KeyCode.G};for(int i=0;i<5;i++)if(Input.GetKeyDown(keys[i]))Attacks.Defend(i);
                if(Input.GetKeyDown(KeyCode.Backspace))Recover();
            }
            Player.ControlNoise=Attacks.Has(CyberAttack.ControlInjection)?.42f:0;Player.EngineLimit=Attacks.Has(CyberAttack.Ransomware)?.35f:1;
            if(Attacks.Has(CyberAttack.ControlInjection)||Attacks.Has(CyberAttack.Ransomware))
            {damageTimer+=Time.deltaTime;if(damageTimer>3){Player.Damage(2+Difficulty);damageTimer=0;}}
            bool gpsAvailable=!Attacks.Has(CyberAttack.DenialOfService);
            World.Beacon.gameObject.SetActive(gpsAvailable);World.Beacon.position=Target+Vector3.up*.08f;
            World.Beacon.localScale=new Vector3(7+Mathf.Sin(Time.time*3)*.5f,.05f,7+Mathf.Sin(Time.time*3)*.5f);
            if(Vector3.Distance(Player.transform.position,World.Network.Nodes[Route[RouteIndex]])<12)
            {
                RouteIndex++;
                if(RouteIndex>=Route.Count){RouteIndex=Route.Count-1;Finish(true,"Delivery verified");return;}
            }
            if(World.Secret&&World.Secret.gameObject.activeSelf)
            {
                World.Secret.Rotate(15*Time.deltaTime,50*Time.deltaTime,0);
                if(Vector3.Distance(Player.transform.position,World.Secret.position)<5){World.Secret.gameObject.SetActive(false);Award(7);}
            }
            if(Player.Health<=0)Finish(false,"Vehicle disabled");
            else if(Player.transform.position.y<-12)Finish(false,"Vehicle lost beyond the road");
            else if(Elapsed>=Limit)Finish(false,"Delivery window expired");
        }
        public void Recover()
        {
            if(!Running||recoverCooldown>0)return;
            int last=Mathf.Max(0,RouteIndex-1);Vector3 direction=World.Network.Nodes[Route[Mathf.Min(last+1,Route.Count-1)]]-World.Network.Nodes[Route[last]];
            Player.Recover(World.Network.Nodes[Route[last]],Quaternion.LookRotation(direction));Player.Damage(10);recoverCooldown=5;Notify("Recovery used: -10 integrity",true);
        }
        public void OnCrash(float severity){Crashes++;Notify("Impact recorded. Keep a safe following distance.",true);}
        public void Award(int badge){if(ProgressStore.Award(badge))Notify("ACHIEVEMENT / "+ProgressStore.Achievements[badge]);}
        public void Notify(string text,bool warning=false){Notice=text;NoticeWarning=warning;NoticeUntil=Time.unscaledTime+5;}
        public void Finish(bool won,string reason)
        {
            if(State!=GameState.Driving)return;Won=won;EndReason=reason;State=GameState.Debrief;Paused=false;Time.timeScale=1;
            foreach(var car in Cars){car.Driving=false;car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;}
            if(won){Award(0);if(Crashes==0)Award(6);if(!Freeplay){ProgressStore.Complete(Level);if(Level==5)Award(8);}}
        }
        void OnDestroy(){Time.timeScale=1;}
    }
}
