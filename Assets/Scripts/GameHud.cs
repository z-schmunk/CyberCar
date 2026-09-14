using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    public sealed class GameHud:MonoBehaviour
    {
        public GameSession Session;
        readonly Color ink=new Color(.035f,.06f,.08f,.96f),panel=new Color(.055f,.09f,.115f,.94f),muted=new Color(.72f,.83f,.87f),teal=new Color(.13f,.93f,.83f),coral=new Color(1,.56f,.5f);
        GUIStyle text,button;
        int mapChoice,difficultyChoice=1,lesson;
        bool initialized;bool nightChoice,garage;Texture2D radarDisc,arrow;RadarPainter radar;
        void Update(){if(Session!=null&&Session.InputEnabled&&Input.GetKeyDown(KeyCode.H)){lesson=Mathf.Max(0,Session.LastLesson);Session.SetGuide(!Session.GuideOpen);}}
        void Init()
        {
            if(initialized)return;initialized=true;radar=new RadarPainter();
            radarDisc=new Texture2D(256,256,TextureFormat.RGBA32,false);var pixels=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(128,128));pixels[y*256+x]=new Color(1,1,1,Mathf.Clamp01(128-d));}radarDisc.SetPixels(pixels);radarDisc.Apply(false,true);
 arrow=new Texture2D(48,48,TextureFormat.RGBA32,false);var ap=new Color[48*48];for(int y=0;y<48;y++)for(int x=0;x<48;x++){float width=(47-y)*.38f;bool inside=Mathf.Abs(x-23.5f)<width&&y>7+Mathf.Abs(x-23.5f)*.45f;ap[y*48+x]=inside?Color.white:Color.clear;}arrow.SetPixels(ap);arrow.Apply(false,true);
            Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text=new GUIStyle{font=font,wordWrap=true,richText=false};text.normal.textColor=Color.white;
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=18,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(18,12,5,5),wordWrap=true};button.normal.textColor=Color.white;
        }
        void Box(float x,float y,float w,float h,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture);GUI.color=old;}
        void Label(float x,float y,float w,float h,string value,int size=18,Color? color=null)
        {text.fontSize=size;text.normal.textColor=color??Color.white;text.fontStyle=size>=32?FontStyle.Bold:FontStyle.Normal;GUI.Label(new Rect(x,y,w,h),value,text);}
        bool Button(float x,float y,float w,float h,string value,bool primary=false,bool enabled=true)
        {
            Box(x,y,w,h,primary?teal:panel);Box(x,y,3,h,enabled?teal:muted);GUI.enabled=enabled;
            button.normal.background=Texture2D.whiteTexture;button.hover.background=Texture2D.whiteTexture;button.active.background=Texture2D.whiteTexture;
            GUI.backgroundColor=primary?teal:new Color(.15f,.23f,.27f);button.normal.textColor=primary?ink:Color.white;button.hover.textColor=ink;
            bool hit=GUI.Button(new Rect(x+3,y,w-3,h),value,button);GUI.backgroundColor=Color.white;GUI.enabled=true;return hit;
        }
        void OnGUI()
        {
            if(Session==null||Session.Player==null)return;Init();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(Session.State==GameState.Menu)Menu();
            else if(Session.State==GameState.Briefing)Briefing();
            else if(Session.State==GameState.Achievements)Achievements();
            else if(Session.State==GameState.Debrief)Report();
            else Hud();
            if(Session.Paused&&!Session.GuideOpen)Pause();
            if(Session.GuideOpen)Guide();
            GUI.matrix=Matrix4x4.identity;
        }
        void Header(string right)
        {Box(0,0,1440,66,ink);Label(34,20,650,32,"CYBERCAR    /    SECURE THE ROUTE",19,teal);Label(1010,22,400,28,right,15,muted);}
        void Menu(){
 Box(0,0,1440,900,ink);Header("STORY CAMPAIGN / AUTOMATIC SAVE");
 Label(48,100,890,65,"THE RESPONSE COURIER",42);Label(50,170,820,55,"Restore the city's trusted network. Fifteen connected operations.",22,muted);
 for(int i=0;i<GameSession.LevelCount;i++){float x=50+(i/8)*423,y=248+(i%8)*58;bool unlocked=i<=ProgressStore.Unlocked;
 if(Button(x,y,402,49,(i+1).ToString("00")+"  "+GameSession.LevelNames[i]+(unlocked?"":" / LOCKED"),false,unlocked))Session.SelectLevel(i);}
 if(Button(50,756,255,55,"ACHIEVEMENTS"))Session.ShowAchievements();if(Button(325,756,255,55,"GARAGE / "+ProgressStore.Credits+" CR"))garage=!garage;if(Button(600,756,253,55,"QUIT"))Application.Quit();
 Box(924,240,470,574,panel);Label(948,259,420,44,"FREEPLAY",30);
 if(Button(947,324,421,50,GameSession.MapNames[mapChoice]))mapChoice=(mapChoice+1)%4;
 if(Button(947,389,421,50,new[]{"LEARNER","OPERATOR","EXPERT","EXTREME / LARGE MAP"}[difficultyChoice]))difficultyChoice=(difficultyChoice+1)%4;
 if(Button(947,454,421,50,nightChoice?"NIGHT":"DAY"))nightChoice=!nightChoice;
 if(Button(947,527,421,60,ProgressStore.FreeplayUnlocked?"START FREEPLAY >":"FINISH STORY TO UNLOCK",true,ProgressStore.FreeplayUnlocked))Session.Prepare(GameSession.LevelCount-1,mapChoice,difficultyChoice,true,night:nightChoice);
 if(Button(947,621,421,60,"ATTACKER TRAINING LAB"))Session.Prepare(14,mapChoice,1,true,night:nightChoice,attacker:true);
 Label(949,699,415,90,"Local AI courier challenge: spend six attack launches to delay a delivery. No multiplayer connection.",18,muted);
 if(Button(949,103,419,60,"CYBER FIELD GUIDE"))Session.SetGuide(true);
 Label(50,840,1250,35,"WASD drive  /  SHIFT drift  /  1-3 diagnostic choices  /  H field guide  /  M audio",18,muted);
 if(garage){Box(330,225,775,480,ink);Label(365,251,690,55,"GARAGE / "+ProgressStore.Credits+" CREDITS",32,teal);
 string[] names={"Tires / smoother grip recovery","Impact reinforcement / 10% less damage per tier","Secure console / +5 seconds per threat per tier"};
 for(int i=0;i<3;i++){int level=ProgressStore.Upgrade(i);if(Button(365,337+i*83,700,68,names[i]+"\nTier "+level+" / "+(level>=3?"MAX":ProgressStore.UpgradeCost(i)+" CR"),false,level<3&&ProgressStore.Credits>=ProgressStore.UpgradeCost(i)))ProgressStore.Purchase(i);}
 if(Button(365,614,700,58,"CLOSE GARAGE"))garage=false;}
        }
        void Briefing()
        {
            Box(0,0,1440,900,new Color(.02f,.04f,.06f,.86f));Header("OPERATION BRIEFING");
            Label(140,139,1160,35,(Session.Freeplay?"FREEPLAY":"OPERATION "+(Session.Level+1).ToString("00"))+"   /   "+GameSession.MapNames[Session.World.Network.Map],19,teal);
            Label(140,194,1150,70,Session.AttackerMode?"Red-team delivery lab":Session.Freeplay?"Your route. Your rules.":GameSession.LevelNames[Session.Level],48);
            Label(140,288,1100,104,Session.AttackerMode?"Watch an AI courier drive. Spend twelve points on up to six attacks (two points each). Delay its delivery or disable it before it reaches the destination. The courier attempts recovery every twenty seconds.":Session.Freeplay?"Reach the destination through every route checkpoint. Multiple threats can overlap on Expert and Extreme. Cached roads remain visible during signal loss.":GameSession.Briefings[Session.Level],25);
            Box(140,417,1160,1,muted);
            Label(140,448,470,38,"DRIVE",20,teal);Label(140,496,470,147,"W / S or arrows   Accelerate / reverse\nA / D or arrows   Steer\nSPACE   Brake    SHIFT   Drift\nBACKSPACE   Recover (-10 integrity)",21);
            Label(710,448,560,38,"DEFEND WHILE DRIVING",20,teal);
            Label(710,496,560,172,"Select an active threat to inspect it.\nChoose 1, 2 or 3 for each diagnostic step.\nDriving continues during diagnosis.\nRSU handoffs require range below 115 m.\nH pauses to explain every attack.\nGPS always retains cached roads.",21);
            Label(140,687,1120,40,"Route: "+(Session.RouteMeters/1000).ToString("0.0")+" km / Seed "+Session.RouteSeed+". H opens detailed attack and defense lessons.",19,muted);
            if(Button(140,767,340,66,"DEPLOY  >",true))Session.Begin();
            if(Button(502,767,240,66,"BACK"))Session.Menu();
        }
        void Hud()
        {
            Header((Session.Freeplay?"FREEPLAY":("OP "+(Session.Level+1).ToString("00")))+"  /  "+GameSession.MapNames[Session.World.Network.Map]);
            Box(28,94,426,98,ink);Label(48,110,385,24,"DISPATCH / "+Session.World.Network.Names[Session.World.Network.Finish].ToUpper(),15,teal);
            Label(48,143,225,42,Mathf.RoundToInt(Session.Distance)+" m to delivery",25);
            Label(304,148,140,40,(Mathf.Max(0,Session.Limit-Session.Elapsed)).ToString("0")+" s left",20,muted);
            int row=0;
            foreach(var r in Session.Attacks.Records)
            {
                if(!r.Active||Session.Defense.Open&&!Session.AttackerMode)continue;float y=211+row*121;Box(28,y,426,112,ink);Box(28,y,4,112,coral);
                Label(47,y+12,378,28,AttackDirector.Names[(int)r.Type]+" / "+Mathf.CeilToInt(r.Remaining)+"s",18,coral);
                Label(47,y+46,379,50,AttackDirector.Symptoms[(int)r.Type],17);
                button.fontSize=13;if(Button(330,y+78,108,29,"INSPECT"))Session.Defense.Begin((int)r.Type);button.fontSize=18;
                if(Session.Difficulty==0)Label(47,y+85,379,26,"USE "+AttackDirector.Keys[(int)r.Type]+" / "+AttackDirector.Defenses[(int)r.Type],15,teal);
                row++;
            }
            if(row==0&&!Session.Defense.Open){Box(28,211,296,40,ink);Label(46,220,276,28,"SYSTEMS NOMINAL / stay alert",16,teal);}
            Minimap(1067,94,345);
            if(Session.NoticeUntil>Time.unscaledTime)
            {Box(402,649,636,84,ink);Label(422,663,596,67,Session.Notice,17,Session.NoticeWarning?coral:teal);}
            Box(28,751,257,120,ink);Label(47,766,215,65,Mathf.RoundToInt(Mathf.Abs(Session.Player.Speed)*2.236936f).ToString("000")+"  mph",35);
            Label(47,821,215,27,"INTEGRITY   "+Mathf.CeilToInt(Session.Player.Health)+"%",17);
            Box(47,852,215,5,new Color(.25f,.3f,.32f));Box(47,852,215*Session.Player.Health/100,5,Session.Player.Health<35?coral:teal);
            button.fontSize=13;
            for(int i=0;i<AttackDirector.Count;i++){
                float x=306+(i%7)*157,y=747+(i/7)*57;bool active=Session.Attacks.Has((CyberAttack)i);
                if(Button(x,y,149,51,AttackDirector.Keys[i]+" / "+AttackDirector.Names[i],active,Session.AttackerMode||active)){
                    if(Session.AttackerMode)Session.LaunchLab(i);else Session.Defense.Begin(i);
                }
            }
            button.fontSize=18;
            Box(306,861,1090,35,ink);Label(309,867,1090,27,Session.AttackerMode?"ATTACKER LAB / "+Session.AttackBudget+" POINTS LEFT":"WASD / DRIVE   SHIFT / DRIFT   1-3 / DIAGNOSE   BACKSPACE / RECOVER   H / GUIDE",15,muted);
            Box(1063,569,350,59,ink);Label(1075,583,326,55,"RSU "+Session.Comms.NearestName+" / "+Session.Comms.NearestDistance.ToString("0")+" m / "+(Session.Comms.Connected?"CONNECTED":"NO LINK"),17,Session.Comms.Connected?teal:coral);
            if(Session.Attacks.Has(CyberAttack.SensorAttack)){Box(504,99,485,88,ink);Label(520,111,454,72,"SENSOR CONFLICT\n"+Session.Player.GetComponent<CarSensors>().Readout,20,coral);}
            if(Session.Defense.Open&&!Session.AttackerMode)Diagnostic();
            if(Session.Player.IsDrifting)Label(47,710,300,35,"DRIFT / "+Session.Player.DriftSeconds.ToString("0.0")+" s",21,teal);
            if(Session.InHotspot)Label(1067,615,345,60,"TRAFFIC HOTSPOT\nAttack pressure increased",18,coral);
            if(Session.Player.Health<30)Box(0,66,1440,5,coral);
        }
        void Minimap(float x,float y,float size)
        {
            var center=new Vector2(x+size/2,y+size/2+28);float radius=size*.46f;
            GUI.color=teal;GUI.DrawTexture(new Rect(center.x-radius-2,center.y-radius-2,(radius+2)*2,(radius+2)*2),radarDisc);
            GUI.color=ink;GUI.DrawTexture(new Rect(center.x-radius,center.y-radius,radius*2,radius*2),radarDisc);GUI.color=Color.white;
            Label(x,y,size,28,(Session.Attacks.Has(CyberAttack.Spoofing)||Session.Attacks.Has(CyberAttack.DestinationInjection))?"NAV / UNVERIFIED":"NAV / LOCAL ROUTE",18,(Session.Attacks.Has(CyberAttack.Spoofing)||Session.Attacks.Has(CyberAttack.DestinationInjection))?coral:teal);
            var network=Session.World.Network;Vector3 driver=Session.Comms.EstimatedPosition;
            Vector3 forward=Vector3.ProjectOnPlane(Session.Player.transform.forward,Vector3.up).normalized,right=Vector3.Cross(Vector3.up,forward);
            bool lost=!Session.Comms.SatelliteAvailable&&!Session.Comms.Connected;
            {
                if(Event.current.type==EventType.Repaint)radar.Refresh(Session);
                GUI.DrawTexture(new Rect(center.x-radius,center.y-radius,radius*2,radius*2),radar.Texture);
                GUI.color=Color.white;GUI.DrawTexture(new Rect(center.x-15,center.y-20,30,36),arrow);GUI.color=Color.white;
                Vector2 north=new Vector2(Vector3.Dot(Vector3.forward,right),-Vector3.Dot(Vector3.forward,forward))*(radius-24);
                Label(center.x+north.x-8,center.y+north.y-10,20,24,"N",15,Color.white);
            }
            Box(x-5,center.y+radius+10,size+10,80,ink);Label(x,center.y+radius+16,size,34,Mathf.RoundToInt(Vector3.Distance(driver,Session.Target))+" m / "+TurnInstruction(),22,Color.white);
            Label(x,center.y+radius+52,size,30,Session.Comms.Source+(lost?" +/- "+Session.Comms.Uncertainty.ToString("0")+" m":""),16,muted);
        }
        string TurnInstruction(){
 if(Session.Attacks.Has(CyberAttack.Spoofing)||Session.Attacks.Has(CyberAttack.DestinationInjection))return "unverified detour";
 int index=Session.RouteIndex;if(index>=Session.Route.Count-1)return "arrive at dispatch";
 var graph=Session.World.Network;var inbound=graph.Path(Session.Route[index-1],Session.Route[index]);var outbound=graph.Path(Session.Route[index],Session.Route[index+1]);
 float angle=Vector3.SignedAngle(inbound[inbound.Length-1]-inbound[inbound.Length-3],outbound[2]-outbound[0],Vector3.up);
 return Mathf.Abs(angle)<28?"continue straight":angle>0?"turn right":"turn left";
        }
        void Diagnostic(){
 var d=Session.Defense;Box(28,211,426,437,ink);Label(48,230,336,54,AttackDirector.Names[d.Attack]+" / "+(d.Stage+1)+" OF 3",18,teal);
 button.fontSize=13;if(Button(399,219,40,34,"X")){d.Close();button.fontSize=18;return;}
 Label(48,290,386,112,d.Evidence,18);
 button.fontSize=15;for(int i=0;i<3;i++)if(Button(48,410+i*65,386,58,(i+1)+"  "+d.Choices[i]))d.Choose(i);button.fontSize=18;
 Label(48,616,386,25,"Driving continues / keys 1, 2, 3",15,muted);
        }
        void Line(Vector2 a,Vector2 b,float width,Color color)
        {
            var matrix=GUI.matrix;Vector2 delta=b-a;GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,a);Box(a.x,a.y-width/2,delta.magnitude,width,color);GUI.matrix=matrix;
        }
        void Report()
        {
            Box(0,0,1440,900,ink);Header("AFTER-ACTION REPORT");
            Label(84,103,1230,72,Session.Won?"DELIVERY SECURED.":"OPERATION INTERRUPTED.",46,Session.Won?teal:coral);
            Label(86,184,1250,40,Session.EndReason+"   /   "+Session.Elapsed.ToString("0.0")+" s   /   "+Session.Crashes+" impacts   /   "+Mathf.CeilToInt(Session.Player.Health)+"% integrity",22,muted);
            Label(86,264,275,30,"THREAT",16,teal);Label(367,264,220,30,"OUTCOME",16,teal);Label(600,264,744,30,"WHAT YOU LEARNED",16,teal);
            for(int i=0;i<AttackDirector.Count;i++)
            {
                int total=0,defended=0;foreach(var r in Session.Attacks.Records)if((int)r.Type==i){total++;if(r.Defended)defended++;}
                float y=295+i*32;Box(86,y-8,1265,1,new Color(.2f,.28f,.31f));
                Label(86,y+5,278,32,AttackDirector.Names[i],14);
                Label(367,y+5,215,32,total==0?"Not encountered":defended+" / "+total+" contained",18,total==0?muted:defended==total?teal:coral);
                Label(600,y+3,744,32,AttackDirector.Lessons[i],13,muted);
            }
            Label(86,735,1230,36,"Physical impacts are consequences in this simulation; each cyber defense models a real security principle.",18,muted);
            if(Session.Won&&!Session.Freeplay&&Session.Level<GameSession.LevelCount-1)
            {if(Button(86,788,368,66,"NEXT OPERATION  >",true))Session.SelectLevel(Session.Level+1);}
            else if(Button(86,788,368,66,"RETRY / DRIVE AGAIN",true))Session.Prepare(Session.Level,Session.World.Network.Map,Session.Difficulty,Session.Freeplay,night:Session.Night,attacker:Session.AttackerMode);
            if(Button(476,788,330,66,"MISSION SELECT"))Session.Menu();
            if(Session.Won&&Session.Level==GameSession.LevelCount-1&&!Session.Freeplay)Label(844,805,500,40,"FREEPLAY UNLOCKED",26,teal);
        }
        void Achievements()
        {
            Box(0,0,1440,900,ink);Header("DRIVER RECORD / AUTOMATICALLY SAVED");Label(84,107,1200,65,"ACHIEVEMENTS",45);
            for(int i=0;i<ProgressStore.Achievements.Length;i++)
            {
                float x=i<11?86:750,y=209+(i%11)*45;bool earned=ProgressStore.Has(i);Box(x,y,605,1,new Color(.18f,.26f,.29f));
                Label(x,y+15,605,42,(earned?"[ VERIFIED ] ":"[ LOCKED ] ")+(i==7&&!earned?"Hidden cache":ProgressStore.Achievements[i]),18,earned?teal:muted);
            }
            Label(86,732,1270,45,"Extreme driver: win EXTREME freeplay, contain every available attack, and hit no pedestrians.",20,muted);
            if(Button(86,798,360,65,"BACK"))Session.Menu();
        }
        void Guide()
        {
            Box(0,0,1440,900,ink);Header("CYBER FIELD GUIDE / DRIVING PAUSED");
            Label(60,106,1320,60,"Understand the attack. Choose the defense.",36);
            for(int i=0;i<AttackDirector.Count;i++)if(Button(60,190+i*44,335,39,AttackDirector.Keys[i]+" / "+AttackDirector.Names[i],lesson==i))lesson=i;
            Label(450,211,900,50,AttackDirector.Names[lesson],32,teal);
            Label(450,286,890,420,AttackDirector.Explanations[lesson],24);
            Label(450,713,900,55,"IN GAME / "+AttackDirector.Keys[lesson]+"  "+AttackDirector.Defenses[lesson],22,teal);
            if(Button(60,800,335,62,"RETURN / H",true))Session.SetGuide(false);
            Label(450,811,220,40,"MUSIC VOLUME",18,muted);
            float volume=GUI.HorizontalSlider(new Rect(680,824,330,25),ProgressStore.MusicVolume,0,.5f);
            if(Mathf.Abs(volume-ProgressStore.MusicVolume)>.005f)ProgressStore.MusicVolume=volume;
        }
        void OnDestroy(){if(radarDisc)Destroy(radarDisc);if(arrow)Destroy(arrow);radar?.Dispose();}
        void Pause()
        {
            Box(0,0,1440,900,new Color(0,0,0,.7f));Box(440,230,560,431,ink);Label(481,270,480,65,"CONNECTION PAUSED",34);
            if(Button(481,378,478,66,"RESUME  >",true))Session.TogglePause();
            if(Button(481,466,478,66,"RESTART OPERATION"))Session.Prepare(Session.Level,Session.World.Network.Map,Session.Difficulty,Session.Freeplay,night:Session.Night,attacker:Session.AttackerMode);
            if(Button(481,554,478,66,"MISSION SELECT"))Session.Menu();
        }
    }
}
