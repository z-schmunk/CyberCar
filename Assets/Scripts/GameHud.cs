using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    public sealed class GameHud:MonoBehaviour
    {
        public GameSession Session;
        readonly Color ink=new Color(.035f,.06f,.08f,.96f),panel=new Color(.055f,.09f,.115f,.94f),muted=new Color(.72f,.83f,.87f),teal=new Color(.13f,.93f,.83f),coral=new Color(1,.56f,.5f);
        GUIStyle text,button;
        int mapChoice,difficultyChoice=1;
        bool initialized;
        void Init()
        {
            if(initialized)return;initialized=true;
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
            if(Session.Paused)Pause();
            GUI.matrix=Matrix4x4.identity;
        }
        void Header(string right)
        {Box(0,0,1440,66,ink);Label(34,20,650,32,"CYBERCAR    /    SECURE THE ROUTE",19,teal);Label(1010,22,400,28,right,15,muted);}
        void Menu()
        {
            Box(0,66,650,834,ink);Header("DRIVING + CYBER DEFENSE SIMULATION");
            Label(52,108,570,100,"TRUST YOUR SKILLS.\nVERIFY YOUR GPS.",39);
            Label(52,228,530,52,"Drive the delivery. Detect the deception.\nBring the car home.",20,muted);
            Label(52,316,540,28,"CAMPAIGN / SIX OPERATIONS",16,teal);
            for(int i=0;i<6;i++)
            {
                bool unlocked=i<=ProgressStore.Unlocked;
                string status=i<ProgressStore.Unlocked?"  /  COMPLETE":unlocked?"  /  READY":"  /  LOCKED";
                if(Button(52,356+i*64,545,54,(i+1).ToString("00")+"   "+GameSession.LevelNames[i]+status,false,unlocked))Session.SelectLevel(i);
            }
            if(Button(52,772,264,48,"ACHIEVEMENTS"))Session.ShowAchievements();
            if(Button(330,772,267,48,"QUIT"))Application.Quit();
            Label(53,841,560,35,"WASD / drive     SPACE / brake     ESC / pause",16,muted);
            Box(878,500,510,353,ink);Label(906,526,450,42,"FREEPLAY",30);Label(906,575,440,48,"Choose a map and threat intensity.",19,muted);
            bool available=ProgressStore.Unlocked>=6;
            if(Button(906,625,451,48,GameSession.MapNames[mapChoice]))mapChoice=(mapChoice+1)%3;
            if(Button(906,686,451,48,"DIFFICULTY / "+new[]{"LEARNER","OPERATOR","EXPERT"}[difficultyChoice]))difficultyChoice=(difficultyChoice+1)%3;
            if(Button(906,752,451,59,available?"START FREEPLAY  >":"COMPLETE CAMPAIGN TO UNLOCK",true,available))Session.Prepare(5,mapChoice,difficultyChoice,true);
            Label(906,826,450,25,"Adaptive opponents / Local single player",14,muted);
        }
        void Briefing()
        {
            Box(0,0,1440,900,new Color(.02f,.04f,.06f,.86f));Header("OPERATION BRIEFING");
            Label(140,139,1160,35,(Session.Freeplay?"FREEPLAY":"OPERATION "+(Session.Level+1).ToString("00"))+"   /   "+GameSession.MapNames[Session.World.Network.Map],19,teal);
            Label(140,194,1150,70,Session.Freeplay?"Your route. Your rules.":GameSession.LevelNames[Session.Level],48);
            Label(140,288,1100,104,Session.Freeplay?"Reach the destination through every GPS gate. All five threats are enabled; the attack director learns which attacks you fail to contain.":GameSession.Briefings[Session.Level],25);
            Box(140,417,1160,1,muted);
            Label(140,448,470,38,"DRIVE",20,teal);Label(140,496,470,147,"W / S or arrows   Accelerate / reverse\nA / D or arrows   Steer\nSPACE   Brake\nBACKSPACE   Recover (-10 integrity)",21);
            Label(710,448,560,38,"DEFEND WHILE DRIVING",20,teal);
            Label(710,496,560,172,"Q   Verify GPS       E   Offline navigation\nR   Isolate CAN bus\nF   Restore backup\nG   Authenticate vehicle identities",21);
            Label(140,687,1120,40,"Teal gates mark route checkpoints. Amber caches hide off the main route.",19,muted);
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
                if(!r.Active)continue;float y=211+row*135;Box(28,y,426,123,ink);Box(28,y,4,123,coral);
                Label(47,y+12,378,28,AttackDirector.Names[(int)r.Type]+" / "+Mathf.CeilToInt(r.Remaining)+"s",18,coral);
                Label(47,y+46,379,50,AttackDirector.Symptoms[(int)r.Type],18);
                if(Session.Difficulty==0)Label(47,y+96,379,26,"USE "+new[]{"Q","E","R","F","G"}[(int)r.Type]+" / "+AttackDirector.Defenses[(int)r.Type],15,teal);
                row++;
            }
            if(row==0){Box(28,211,296,40,ink);Label(46,220,276,28,"SYSTEMS NOMINAL / stay alert",16,teal);}
            Minimap(1067,94,345);
            if(Session.NoticeUntil>Time.unscaledTime)
            {Box(402,668,636,64,ink);Label(422,682,596,49,Session.Notice,19,Session.NoticeWarning?coral:teal);}
            Box(28,751,257,120,ink);Label(47,766,215,65,Mathf.RoundToInt(Mathf.Abs(Session.Player.Speed)*3.6f).ToString("000")+"  km/h",35);
            Label(47,821,215,27,"INTEGRITY   "+Mathf.CeilToInt(Session.Player.Health)+"%",17);
            Box(47,852,215,5,new Color(.25f,.3f,.32f));Box(47,852,215*Session.Player.Health/100,5,Session.Player.Health<35?coral:teal);
            string[] shortNames={"VERIFY GPS","OFFLINE MAP","ISOLATE BUS","RESTORE","VERIFY V2X"};
            for(int i=0;i<5;i++)
            {
                float x=306+i*186;bool active=Session.Attacks.Has((CyberAttack)i);float cd=Session.Attacks.Cooldowns[i];
                if(Button(x,784,174,66,new[]{"Q","E","R","F","G"}[i]+"  "+shortNames[i]+(cd>0?"\n"+Mathf.CeilToInt(cd)+"s":""),active,cd<=0))Session.Attacks.Defend(i);
            }
            Label(308,859,1090,27,"WASD / DRIVE     SPACE / BRAKE     BACKSPACE / RECOVER     ESC / PAUSE",15,muted);
            if(Session.Player.Health<30)Box(0,66,1440,5,coral);
        }
        void Minimap(float x,float y,float size)
        {
            Box(x,y,size,size+79,ink);Label(x+18,y+15,size-35,30,"NAV / "+(Session.Attacks.Has(CyberAttack.Spoofing)?"UNVERIFIED":"SIGNED ROUTE"),17,Session.Attacks.Has(CyberAttack.Spoofing)?coral:teal);
            if(Session.Attacks.Has(CyberAttack.DenialOfService))
            {Label(x+32,y+120,size-64,130,"SIGNAL UNAVAILABLE\n\nE / use offline map",23,coral);return;}
            Rect rect=new Rect(x+25,y+64,size-50,size-74);var network=Session.World.Network;
            float maxX=0,maxZ=0;foreach(var node in network.Nodes){maxX=Mathf.Max(maxX,node.x);maxZ=Mathf.Max(maxZ,node.z);}maxX+=60;maxZ+=25;
            Vector2 Map(Vector3 p)=>new Vector2(rect.x+10+(p.x+10)/(maxX+20)*(rect.width-20),rect.y+rect.height-(p.z+10)/(maxZ+20)*rect.height);
            foreach(var e in network.Edges)Line(Map(network.Nodes[e.x]),Map(network.Nodes[e.y]),3,new Color(.24f,.34f,.39f));
            if(Session.Attacks.Has(CyberAttack.Spoofing))Line(Map(Session.Player.transform.position),Map(network.Hazard),4,coral);
            else for(int i=Mathf.Max(1,Session.RouteIndex);i<Session.Route.Count;i++)Line(Map(network.Nodes[Session.Route[i-1]]),Map(network.Nodes[Session.Route[i]]),4,teal);
            foreach(var car in Session.Cars)
            {
                Vector2 p=Map(car.transform.position);Box(p.x-3,p.y-3,6,6,car.IsPlayer?Color.white:car.GetComponent<TrafficAgent>()?.Hostile==true?coral:muted);
            }
            Vector2 target=Map(Session.Target);Box(target.x-5,target.y-5,10,10,teal);
            Vector2 player=Map(Session.Player.transform.position);Vector2 dir=new Vector2(Session.Player.transform.forward.x,-Session.Player.transform.forward.z);Line(player,player+dir*14,3,Color.white);
            Label(x+18,y+size+8,size-30,27,"GATE "+Session.RouteIndex+" / "+(Session.Route.Count-1)+"    N ^",18);
            Label(x+18,y+size+40,size-30,30,"TEAL route   RED hostile   WHITE you",13,muted);
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
            for(int i=0;i<5;i++)
            {
                int total=0,defended=0;foreach(var r in Session.Attacks.Records)if((int)r.Type==i){total++;if(r.Defended)defended++;}
                float y=314+i*77;Box(86,y-8,1265,1,new Color(.2f,.28f,.31f));
                Label(86,y+7,278,56,AttackDirector.Names[i],19);
                Label(367,y+7,215,57,total==0?"Not encountered":defended+" / "+total+" contained",18,total==0?muted:defended==total?teal:coral);
                Label(600,y+3,744,69,AttackDirector.Lessons[i],18,muted);
            }
            Label(86,711,1230,41,"Physical impacts are consequences in this simulation; each cyber defense models a real security principle.",18,muted);
            if(Session.Won&&!Session.Freeplay&&Session.Level<5)
            {if(Button(86,788,368,66,"NEXT OPERATION  >",true))Session.SelectLevel(Session.Level+1);}
            else if(Button(86,788,368,66,"RETRY / DRIVE AGAIN",true))Session.Prepare(Session.Level,Session.World.Network.Map,Session.Difficulty,Session.Freeplay);
            if(Button(476,788,330,66,"MISSION SELECT"))Session.Menu();
            if(Session.Won&&Session.Level==5&&!Session.Freeplay)Label(844,805,500,40,"FREEPLAY UNLOCKED",26,teal);
        }
        void Achievements()
        {
            Box(0,0,1440,900,ink);Header("DRIVER RECORD");Label(84,107,1200,65,"ACHIEVEMENTS",45);
            for(int i=0;i<ProgressStore.Achievements.Length;i++)
            {
                float y=203+i*60;bool earned=ProgressStore.Has(i);Box(86,y,1268,1,new Color(.18f,.26f,.29f));
                Label(90,y+15,1150,42,(earned?"[ VERIFIED ]   ":"[ LOCKED ]     ")+(i==7&&!earned?"Hidden cache":ProgressStore.Achievements[i]),23,earned?teal:muted);
            }
            if(Button(86,786,360,65,"BACK"))Session.Menu();
        }
        void Pause()
        {
            Box(0,0,1440,900,new Color(0,0,0,.7f));Box(440,230,560,431,ink);Label(481,270,480,65,"CONNECTION PAUSED",34);
            if(Button(481,378,478,66,"RESUME  >",true))Session.TogglePause();
            if(Button(481,466,478,66,"RESTART OPERATION"))Session.Prepare(Session.Level,Session.World.Network.Map,Session.Difficulty,Session.Freeplay);
            if(Button(481,554,478,66,"MISSION SELECT"))Session.Menu();
        }
    }
}
