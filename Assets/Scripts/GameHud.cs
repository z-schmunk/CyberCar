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
        bool initialized;bool nightChoice;Texture2D radarDisc;
        void Update(){if(Session!=null&&Session.InputEnabled&&Input.GetKeyDown(KeyCode.H)){lesson=Mathf.Max(0,Session.LastLesson);Session.SetGuide(!Session.GuideOpen);}}
        void Init()
        {
            if(initialized)return;initialized=true;
            radarDisc=new Texture2D(256,256,TextureFormat.RGBA32,false);var pixels=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(128,128));pixels[y*256+x]=new Color(1,1,1,Mathf.Clamp01(128-d));}radarDisc.SetPixels(pixels);radarDisc.Apply(false,true);
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
        void Menu()
        {
            Box(0,66,650,834,ink);Header("DRIVING + CYBER DEFENSE");
            Label(52,102,570,100,"TRUST YOUR SKILLS.\nVERIFY YOUR GPS.",39);
            Label(52,215,550,60,"Nine operations. Four worlds. Day and night.",21,muted);
            Label(52,294,540,28,"CAMPAIGN / NINE OPERATIONS",16,teal);
            for(int i=0;i<GameSession.LevelCount;i++)
            {
                bool unlocked=i<=ProgressStore.Unlocked;string status=i<ProgressStore.Unlocked?" / COMPLETE":unlocked?" / READY":" / LOCKED";
                if(Button(52,332+i*46,545,41,(i+1).ToString("00")+"   "+GameSession.LevelNames[i]+status,false,unlocked))Session.SelectLevel(i);
            }
            if(Button(52,776,264,48,"ACHIEVEMENTS"))Session.ShowAchievements();
            if(Button(330,776,267,48,"QUIT"))Application.Quit();
            Label(53,842,560,35,"SHIFT / drift     H / field guide     M / music",16,muted);
            if(Button(906,105,451,54,"CYBER FIELD GUIDE"))Session.SetGuide(true);
            Box(878,477,510,386,ink);Label(906,503,450,42,"FREEPLAY",30);
            Label(906,546,450,30,"Fresh routes. Extreme expands the world.",17,muted);
            if(Button(906,585,451,42,GameSession.MapNames[mapChoice]))mapChoice=(mapChoice+1)%GameSession.MapNames.Length;
            if(Button(906,638,451,42,"DIFFICULTY / "+new[]{"LEARNER","OPERATOR","EXPERT","EXTREME"}[difficultyChoice]))difficultyChoice=(difficultyChoice+1)%4;
            if(Button(906,691,451,42,"TIME / "+(nightChoice?"NIGHT":"DAY")))nightChoice=!nightChoice;
            if(Button(906,752,451,59,ProgressStore.FreeplayUnlocked?"START FREEPLAY >":"COMPLETE CAMPAIGN TO UNLOCK",true,ProgressStore.FreeplayUnlocked))Session.Prepare(GameSession.LevelCount-1,mapChoice,difficultyChoice,true,night:nightChoice);
            Label(906,830,450,25,"Progress and achievements save automatically",14,muted);
        }
        void Briefing()
        {
            Box(0,0,1440,900,new Color(.02f,.04f,.06f,.86f));Header("OPERATION BRIEFING");
            Label(140,139,1160,35,(Session.Freeplay?"FREEPLAY":"OPERATION "+(Session.Level+1).ToString("00"))+"   /   "+GameSession.MapNames[Session.World.Network.Map],19,teal);
            Label(140,194,1150,70,Session.Freeplay?"Your route. Your rules.":GameSession.LevelNames[Session.Level],48);
            Label(140,288,1100,104,Session.Freeplay?"Reach the destination through every GPS gate. All applicable threats are enabled; the attack director learns which attacks you fail to contain.":GameSession.Briefings[Session.Level],25);
            Box(140,417,1160,1,muted);
            Label(140,448,470,38,"DRIVE",20,teal);Label(140,496,470,147,"W / S or arrows   Accelerate / reverse\nA / D or arrows   Steer\nSPACE   Brake    SHIFT   Drift\nBACKSPACE   Recover (-10 integrity)",21);
            Label(710,448,560,38,"DEFEND WHILE DRIVING",20,teal);
            Label(710,496,560,172,"Q   Verify GPS       E   Offline navigation\nR   Isolate CAN bus\nF   Restore backup\nG   Authenticate vehicle identities\nT   Reject replay     Y   Verify firmware\nU   Restore local lighting",21);
            Label(140,687,1120,40,"Route: "+(Session.RouteMeters/1000).ToString("0.0")+" km / Seed "+Session.RouteSeed+". H opens detailed attack and defense lessons.",19,muted);
            if(Button(140,767,340,66,"DEPLOY  >",true))Session.Begin();
            if(Button(502,767,240,66,"BACK"))Session.Menu();
        }
        void Hud()
        {
            Header((Session.Freeplay?"FREEPLAY":("OP "+(Session.Level+1).ToString("00")))+"  /  "+GameSession.MapNames[Session.World.Network.Map]);
            Box(28,94,426,98,ink);Label(48,110,385,24,"DESTINATION / "+Session.World.Network.Names[Session.World.Network.Finish].ToUpper(),15,teal);
            Label(48,143,225,42,Mathf.RoundToInt(Session.Distance)+" m to delivery",25);
            Label(304,148,140,40,(Mathf.Max(0,Session.Limit-Session.Elapsed)).ToString("0")+" s left",20,muted);
            int row=0;
            foreach(var r in Session.Attacks.Records)
            {
                if(!r.Active)continue;float y=211+row*121;Box(28,y,426,112,ink);Box(28,y,4,112,coral);
                Label(47,y+12,378,28,AttackDirector.Names[(int)r.Type]+" / "+Mathf.CeilToInt(r.Remaining)+"s",18,coral);
                Label(47,y+46,379,50,AttackDirector.Symptoms[(int)r.Type],18);
                if(Session.Difficulty==0)Label(47,y+85,379,26,"USE "+AttackDirector.Keys[(int)r.Type]+" / "+AttackDirector.Defenses[(int)r.Type],15,teal);
                row++;
            }
            if(row==0){Box(28,211,296,40,ink);Label(46,220,276,28,"SYSTEMS NOMINAL / stay alert",16,teal);}
            Minimap(1067,94,345);
            if(Session.NoticeUntil>Time.unscaledTime)
            {Box(402,649,636,84,ink);Label(422,663,596,67,Session.Notice,17,Session.NoticeWarning?coral:teal);}
            Box(28,751,257,120,ink);Label(47,766,215,65,Mathf.RoundToInt(Mathf.Abs(Session.Player.Speed)*3.6f).ToString("000")+"  km/h",35);
            Label(47,821,215,27,"INTEGRITY   "+Mathf.CeilToInt(Session.Player.Health)+"%",17);
            Box(47,852,215,5,new Color(.25f,.3f,.32f));Box(47,852,215*Session.Player.Health/100,5,Session.Player.Health<35?coral:teal);
            string[] shortNames={"VERIFY GPS","OFFLINE MAP","ISOLATE BUS","BACKUP","VERIFY V2X","FRESHNESS","FIRMWARE","LIGHTING"};
            button.fontSize=15;
            for(int i=0;i<AttackDirector.Count;i++)
            {
                float x=306+i*137;bool active=Session.Attacks.Has((CyberAttack)i);float cd=Session.Attacks.Cooldowns[i];
                if(Button(x,784,129,66,AttackDirector.Keys[i]+"\n"+shortNames[i]+(cd>0?" "+Mathf.CeilToInt(cd)+"s":""),active&&Session.Difficulty==0,cd<=0))Session.Attacks.Defend(i);
            }
            button.fontSize=18;
            Box(306,854,1090,30,ink);Label(308,859,1090,27,"WASD / DRIVE   SHIFT / DRIFT   SPACE / BRAKE   H / GUIDE   M / MUSIC   ESC / PAUSE",15,muted);
            if(Session.Player.IsDrifting)Label(47,710,300,35,"DRIFT / "+Session.Player.DriftSeconds.ToString("0.0")+" s",21,teal);
            if(Session.InHotspot)Label(1067,615,345,60,"TRAFFIC HOTSPOT\nAttack pressure increased",18,coral);
            if(Session.Player.Health<30)Box(0,66,1440,5,coral);
        }
        void Minimap(float x,float y,float size)
        {
            var center=new Vector2(x+size/2,y+size/2+28);float radius=size*.46f;
            GUI.color=teal;GUI.DrawTexture(new Rect(center.x-radius-2,center.y-radius-2,(radius+2)*2,(radius+2)*2),radarDisc);
            GUI.color=ink;GUI.DrawTexture(new Rect(center.x-radius,center.y-radius,radius*2,radius*2),radarDisc);GUI.color=Color.white;
            Label(x,y,size,28,Session.Attacks.Has(CyberAttack.Spoofing)?"NAV / UNVERIFIED":"NAV / LOCAL ROUTE",18,Session.Attacks.Has(CyberAttack.Spoofing)?coral:teal);
            var network=Session.World.Network;Vector3 driver=Session.Player.transform.position;
            Vector3 forward=Vector3.ProjectOnPlane(Session.Player.transform.forward,Vector3.up).normalized,right=Vector3.Cross(Vector3.up,forward);
            const float range=170;
            Vector2 Map(Vector3 p){Vector3 d=p-driver;return center+new Vector2(Vector3.Dot(d,right),-Vector3.Dot(d,forward))*(radius/range);}
            void Clipped(Vector2 a,Vector2 b,float width,Color color){
                Vector2 d=b-a,f=a-center;float aa=Vector2.Dot(d,d);if(aa<.001f)return;float bb=2*Vector2.Dot(f,d),cc=Vector2.Dot(f,f)-(radius-7)*(radius-7);
                float discriminant=bb*bb-4*aa*cc;if(discriminant<0)return;
                float t0=Mathf.Max(0,(-bb-Mathf.Sqrt(discriminant))/(2*aa)),t1=Mathf.Min(1,(-bb+Mathf.Sqrt(discriminant))/(2*aa));
                if(t0<=t1)Line(a+d*t0,a+d*t1,width,color);
            }
            bool lost=Session.Attacks.Has(CyberAttack.DenialOfService);
            if(!lost){
                foreach(var edge in network.Edges){var path=network.Path(edge.x,edge.y);for(int i=1;i<path.Length;i++)Clipped(Map(path[i-1]),Map(path[i]),4,new Color(.28f,.36f,.39f));}
                if(Session.Attacks.Has(CyberAttack.Spoofing))Clipped(center,Map(network.Hazard),5,coral);
                else for(int n=Mathf.Max(1,Session.RouteIndex);n<Session.Route.Count;n++){var path=network.Path(Session.Route[n-1],Session.Route[n]);for(int i=1;i<path.Length;i++)Clipped(Map(path[i-1]),Map(path[i]),5,teal);}
                foreach(var car in Session.Cars){if(!car.gameObject.activeSelf||car.IsPlayer)continue;Vector2 p=Map(car.transform.position);if(Vector2.Distance(p,center)<radius-10)Box(p.x-3,p.y-3,6,6,car.GetComponent<TrafficAgent>()?.Hostile==true?coral:muted);}
                foreach(var person in Session.Citizens)if(person&&person.IsCrossing){Vector2 p=Map(person.transform.position);if(Vector2.Distance(p,center)<radius-10)Box(p.x-2,p.y-2,4,4,new Color(1,.8f,.35f));}
                Vector2 target=Map(Session.Target);if(Vector2.Distance(target,center)>radius-14)target=center+(target-center).normalized*(radius-14);Box(target.x-5,target.y-5,10,10,teal);
                Line(center+new Vector2(-6,6),center+new Vector2(0,-10),3,Color.white);Line(center+new Vector2(0,-10),center+new Vector2(6,6),3,Color.white);
                Vector2 north=new Vector2(Vector3.Dot(Vector3.forward,right),-Vector3.Dot(Vector3.forward,forward))*(radius-24);
                Label(center.x+north.x-8,center.y+north.y-10,20,24,"N",15,Color.white);
            }else Label(center.x-105,center.y-45,210,100,"SIGNAL LOST\nE / offline map",23,coral);
            Box(x-5,center.y+radius+10,size+10,80,ink);Label(x,center.y+radius+16,size,34,lost?"Navigation unavailable":Mathf.RoundToInt(Vector3.Distance(driver,Session.Target))+" m / next gate",22,Color.white);
            Label(x,center.y+radius+52,size,30,"GATE "+Session.RouteIndex+" / "+(Session.Route.Count-1)+"   "+(Session.Night?"NIGHT":"DAY"),16,muted);
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
                float y=306+i*51;Box(86,y-8,1265,1,new Color(.2f,.28f,.31f));
                Label(86,y+5,278,46,AttackDirector.Names[i],17);
                Label(367,y+5,215,47,total==0?"Not encountered":defended+" / "+total+" contained",18,total==0?muted:defended==total?teal:coral);
                Label(600,y+3,744,53,AttackDirector.Lessons[i],16,muted);
            }
            Label(86,735,1230,36,"Physical impacts are consequences in this simulation; each cyber defense models a real security principle.",18,muted);
            if(Session.Won&&!Session.Freeplay&&Session.Level<GameSession.LevelCount-1)
            {if(Button(86,788,368,66,"NEXT OPERATION  >",true))Session.SelectLevel(Session.Level+1);}
            else if(Button(86,788,368,66,"RETRY / DRIVE AGAIN",true))Session.Prepare(Session.Level,Session.World.Network.Map,Session.Difficulty,Session.Freeplay,night:Session.Night);
            if(Button(476,788,330,66,"MISSION SELECT"))Session.Menu();
            if(Session.Won&&Session.Level==GameSession.LevelCount-1&&!Session.Freeplay)Label(844,805,500,40,"FREEPLAY UNLOCKED",26,teal);
        }
        void Achievements()
        {
            Box(0,0,1440,900,ink);Header("DRIVER RECORD / AUTOMATICALLY SAVED");Label(84,107,1200,65,"ACHIEVEMENTS",45);
            for(int i=0;i<ProgressStore.Achievements.Length;i++)
            {
                float x=i<9?86:750,y=209+(i%9)*55;bool earned=ProgressStore.Has(i);Box(x,y,605,1,new Color(.18f,.26f,.29f));
                Label(x,y+15,605,42,(earned?"[ VERIFIED ] ":"[ LOCKED ] ")+(i==7&&!earned?"Hidden cache":ProgressStore.Achievements[i]),21,earned?teal:muted);
            }
            Label(86,732,1270,45,"Extreme driver: win EXTREME freeplay, contain every available attack, and hit no pedestrians.",20,muted);
            if(Button(86,798,360,65,"BACK"))Session.Menu();
        }
        void Guide()
        {
            Box(0,0,1440,900,ink);Header("CYBER FIELD GUIDE / DRIVING PAUSED");
            Label(60,106,1320,60,"Understand the attack. Choose the defense.",36);
            for(int i=0;i<AttackDirector.Count;i++)if(Button(60,207+i*73,335,60,AttackDirector.Keys[i]+" / "+AttackDirector.Names[i],lesson==i))lesson=i;
            Label(450,211,900,50,AttackDirector.Names[lesson],32,teal);
            Label(450,286,890,420,AttackDirector.Explanations[lesson],24);
            Label(450,713,900,55,"IN GAME / "+AttackDirector.Keys[lesson]+"  "+AttackDirector.Defenses[lesson],22,teal);
            if(Button(60,800,335,62,"RETURN / H",true))Session.SetGuide(false);
            Label(450,811,220,40,"MUSIC VOLUME",18,muted);
            float volume=GUI.HorizontalSlider(new Rect(680,824,330,25),ProgressStore.MusicVolume,0,.5f);
            if(Mathf.Abs(volume-ProgressStore.MusicVolume)>.005f)ProgressStore.MusicVolume=volume;
        }
        void OnDestroy(){if(radarDisc)Destroy(radarDisc);}
        void Pause()
        {
            Box(0,0,1440,900,new Color(0,0,0,.7f));Box(440,230,560,431,ink);Label(481,270,480,65,"CONNECTION PAUSED",34);
            if(Button(481,378,478,66,"RESUME  >",true))Session.TogglePause();
            if(Button(481,466,478,66,"RESTART OPERATION"))Session.Prepare(Session.Level,Session.World.Network.Map,Session.Difficulty,Session.Freeplay,night:Session.Night);
            if(Button(481,554,478,66,"MISSION SELECT"))Session.Menu();
        }
    }
}
