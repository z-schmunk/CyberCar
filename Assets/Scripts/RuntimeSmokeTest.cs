using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CyberCar
{
    // Runs only with -smokeTest. Does not write campaign progress or achievements.
    public sealed class RuntimeSmokeTest:MonoBehaviour
    {
        public GameSession Session;
        int checks;
        int runtimeErrors;
        void OnEnable(){Application.logMessageReceived+=ObserveLog;}
        void OnDisable(){Application.logMessageReceived-=ObserveLog;}
        void ObserveLog(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error)runtimeErrors++;}
        void Check(bool condition,string message)
        {
            if(!condition){Debug.LogError("SMOKE_FAIL / "+message);Application.Quit(2);throw new Exception(message);}
            checks++;Debug.Log("SMOKE_PASS / "+message);
        }
        IEnumerator Start()
        {
            yield return null;Session.InputEnabled=false;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-trafficTest")>=0){yield return TrafficProof();Debug.Log("TRAFFIC_SUCCESS / "+checks);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-missionTest")>=0){yield return MissionProof();yield return UiProof();Check(runtimeErrors==0,"No runtime errors or exceptions");Debug.Log("MISSION_SUCCESS / "+checks);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-uiTest")>=0){yield return VisualProof();yield return UiProof();Debug.Log("UI_VISUAL_SUCCESS / "+checks);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-expansionTest")>=0){yield return ExpansionProof();Debug.Log("EXPANSION_SUCCESS / "+checks);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-bridgeTest")>=0){yield return BridgeProof();Application.Quit(0);yield break;}
            Session.SelectLevel(0);Session.Begin();
            yield return new WaitForSeconds(.5f);
            Vector3 start=Session.Player.transform.position;Session.Player.Throttle=1;
            yield return new WaitForSeconds(2.5f);
            Check(Vector3.Distance(start,Session.Player.transform.position)>15,"Car accelerates under physics");
            var wall=Session.World.Box("Smoke collision wall",Session.Player.transform.position+Session.Player.transform.forward*17+Vector3.up*2,new Vector3(2,4,15),Session.World.EnemyPaint);
            yield return new WaitForSeconds(1.6f);
            Check(Session.Crashes>0&&Session.Player.Health<100,"Physical collision causes impact and integrity loss");
            wall.SetActive(false);
            Session.Player.Recover(Vector3.zero,Quaternion.Euler(0,90,0));
            var other=Session.Cars[1];other.GetComponent<TrafficAgent>().enabled=false;other.Driving=false;
            other.Recover(new Vector3(20,0,0),Quaternion.Euler(0,270,0));
            int impacts=Session.Crashes;yield return new WaitForSeconds(3);
            Check(Session.Crashes>impacts&&other.Body.linearVelocity.magnitude>1,"Car-to-car collision transfers momentum");
            Session.Player.Throttle=0;Session.Player.Brake=true;
            yield return null;yield return null;
            Check(Session.Player.GetComponent<VehicleFeedback>().BrakeLightsActive,"Braking illuminates rear lamps");
            for(int i=0;i<AttackDirector.Count;i++)
            {
                Session.Attacks.Launch((CyberAttack)i);yield return null;
                Check(Session.Attacks.Has((CyberAttack)i),"Attack activates: "+i);
                if(i==0)Check(Vector3.Distance(Session.Target,Session.World.Network.Hazard)<.1f,"Spoofed GPS points at physical hazard");
                if(i==1)Check(!Session.World.Beacon.gameObject.activeSelf,"Signal flood removes GPS gate");
                if(i==2)Check(Session.Player.ControlNoise>0,"Injection changes steering input");
                if(i==3)Check(Session.Player.EngineLimit<1,"Ransomware restricts engine");
                if(i==6)Check(Session.Player.SteeringPolarity==-1,"Malicious firmware reverses steering");
                Check(Solve(i)&&!Session.Attacks.Has((CyberAttack)i),"Correct defense contains attack: "+i);
                Check(!Session.Attacks.Defend(i),"Cooldown prevents repeated activation: "+i);
                yield return null;
                if(i==2)Check(Session.Player.ControlNoise==0,"Bus isolation restores steering");
                if(i==3)Check(Session.Player.EngineLimit==1,"Backup restores engine power");
                if(i==6)Check(Session.Player.SteeringPolarity==1,"Signed rollback restores steering");
            }
            Session.TogglePause();float elapsed=Session.Elapsed;yield return new WaitForSecondsRealtime(.15f);Check(Session.Elapsed==elapsed,"Pause freezes mission timer");Session.TogglePause();
            for(int map=0;map<4;map++)
            {
                Session.Prepare(7,map,2,true);Session.Begin();yield return new WaitForSeconds(.15f);
                Check(Session.Cars.Count>=1+Session.TrafficCount+4,"Expert traffic and hostile cars spawn");
                yield return new WaitForSeconds(.5f);
                AuditWorld(map);
                yield return new WaitForEndOfFrame();
                var mapShot=ScreenCapture.CaptureScreenshotAsTexture();
                if(mapShot){File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-map-"+map+".png"),mapShot.EncodeToPNG());Destroy(mapShot);}
                Session.Player.Driving=false;
                while(Session.Running)
                {
                    Session.Player.Recover(Session.World.Network.Nodes[Session.Route[Session.RouteIndex]],Quaternion.identity);
                    yield return new WaitForSeconds(.06f);
                }
                Check(Session.Won,"Checkpoint delivery completes on map "+map);
                if(map==3)
                {
                    yield return new WaitForEndOfFrame();var reportShot=ScreenCapture.CaptureScreenshotAsTexture();
                    if(reportShot){File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-report.png"),reportShot.EncodeToPNG());Destroy(reportShot);}
                }
            }
            Session.SelectLevel(8);Session.Begin();yield return new WaitForSeconds(.5f);
            Check(Session.Night&&Session.World.Environment.LitStreetlights>0,"Night operation illuminates nearby road lights");
            yield return Capture("smoke-night.png");
            Session.Attacks.Launch(CyberAttack.Blackout);yield return new WaitForSeconds(.35f);
            Check(Session.World.Environment.Blackout&&Session.World.Environment.LitStreetlights==0,"Lighting takeover disables streetlights");
            int workingHeadlights=0;foreach(var light in Session.Player.GetComponentsInChildren<Light>())if(light.enabled&&light.type==LightType.Spot)workingHeadlights++;
            Check(workingHeadlights==2,"Independent headlights remain usable during blackout");
            yield return Capture("smoke-blackout.png");
            Check(Solve(7),"Local lighting defense contains takeover");yield return new WaitForSeconds(.35f);
            Check(Session.World.Environment.LitStreetlights>0&&!Session.World.Environment.Blackout,"Trusted local control restores streetlights");
            // Actual drifting on a road, followed by grip recovery.
            Session.SelectLevel(0);Session.Begin();Session.Player.Throttle=1;
            foreach(var c in Session.Citizens)c.gameObject.SetActive(false);
            yield return new WaitForSeconds(2.5f);Session.Player.Drift=true;Session.Player.Steer=.5f;
            yield return new WaitForSeconds(.7f);
            Check(Session.Player.IsDrifting&&Session.Player.DriftSeconds>.3f,"Drifting accumulates at driving speed");
            Check(Session.Player.GetComponent<VehicleFeedback>().DriftFeedbackActive,"Physical drift activates tire feedback");
            Check(Mathf.Abs(Vector3.Dot(Session.Player.Body.linearVelocity,Session.Player.transform.right))>1,"Drifting creates lateral slip");
            Session.Player.Drift=false;Session.Player.Steer=0;yield return new WaitForFixedUpdate();
            Check(!Session.Player.IsDrifting,"Releasing drift restores grip mode");
            Session.SetGuide(true);float guideTime=Session.Elapsed;yield return new WaitForSecondsRealtime(.15f);
            Check(Session.Paused&&Session.Elapsed==guideTime,"Field guide safely pauses driving");
            Check(!Session.Player.GetComponent<VehicleFeedback>().DriftFeedbackActive,"Pause silences drift feedback");
            yield return new WaitForEndOfFrame();var guideShot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-guide.png"),guideShot.EncodeToPNG());Destroy(guideShot);Session.SetGuide(false);
            Check(!Session.Paused,"Closing field guide resumes prior driving state");
            Check(Resources.Load<AudioClip>("Audio/CoastalDrive")?.length>60,"Original soundtrack imported");
            // Save/reload and recovery use an isolated profile, never the user's save.
            string savePath=Path.Combine(Application.dataPath,"..","smoke-save-test","profile.json");
            var profile=new DriverProfile{unlocked=9,freeplay=true,music=.17f};profile.badges[11]=true;profile.badges[16]=true;profile.badges[21]=true;profile.credits=420;profile.upgrades[0]=2;
            ProgressStore.WriteTo(savePath,profile);var loaded=ProgressStore.LoadFrom(savePath);
            Check(loaded.unlocked==9&&loaded.badges[11]&&loaded.badges[16]&&loaded.freeplay&&loaded.badges[21]&&loaded.credits==420&&loaded.upgrades[0]==2,"Unlocks and achievements survive disk reload");
            profile.unlocked=7;ProgressStore.WriteTo(savePath,profile);File.WriteAllText(savePath,"invalid-json");loaded=ProgressStore.LoadFrom(savePath);
            Check(loaded.unlocked==9&&loaded.badges[11],"Corrupted profile recovers previous backup");
            Session.Prepare(7,3,3,true);Session.Begin();
            Check(Session.World.Network.Nodes.Count==81,"Extreme mode creates a larger 81-node map");
            for(int attack=0;attack<AttackDirector.Count;attack++){Session.Attacks.Launch((CyberAttack)attack);Solve(attack);}
            Check(Session.ExtremeAwardEligible,"Extreme accolade requires all attack defenses and safe citizens");
            Session.OnCivilianStrike();Check(!Session.Won&&Session.State==GameState.Debrief,"Extreme pedestrian strike fails the run");
            Session.SelectLevel(0);Session.Begin();
            var citizen=Session.Citizens[0];foreach(var c in Session.Citizens)c.enabled=false;
            Vector3 personPosition=citizen.transform.position;Session.Player.Recover(personPosition-Vector3.forward*3,Quaternion.identity);Session.Player.Body.linearVelocity=Vector3.forward*12;
            yield return new WaitForSeconds(.6f);Check(Session.CivilianStrikes==1,"Physical pedestrian contact records a safety violation");
            yield return BridgeProof();
            yield return ExpansionProof();
            Session.SelectLevel(1);Session.Begin();Session.Player.Damage(100);yield return null;yield return null;Check(!Session.Won&&Session.State==GameState.Debrief,"Destroyed vehicle fails mission");
            Session.SelectLevel(3);Session.Begin();Session.Player.Recover(new Vector3(0,-40,0),Quaternion.identity);yield return new WaitForSeconds(.1f);Check(!Session.Won&&Session.State==GameState.Debrief,"Cliff fall fails mission");
            Session.SelectLevel(1);Session.Begin();Session.Attacks.Launch(CyberAttack.Spoofing);Session.Attacks.Tick(90);Check(Session.Attacks.Records.Exists(r=>r.Type==CyberAttack.Spoofing&&!r.Defended&&!r.Active),"Uncontained attack is recorded as missed");
            for(int level=1;level<=AttackDirector.Count;level++)
            {
                Session.SelectLevel(level);Session.Begin();Session.Attacks.Tick(11);
                Check(Session.Attacks.Has((CyberAttack)(level-1)),"Mission introduces its new attack first: "+level);
            }
            Session.SelectLevel(0);Session.Begin();Time.timeScale=100;
            yield return new WaitForSeconds(301);
            Check(Session.State==GameState.Debrief&&!Session.Won&&Session.EndReason=="Delivery window expired","Mission timer expiry fails delivery");Time.timeScale=1;
            Session.Prepare(7,0,1,true);Session.Begin();Session.Attacks.Launch(CyberAttack.Spoofing);
            yield return new WaitForSeconds(.5f);
            string output=Path.Combine(Application.dataPath,"..","smoke-gameplay.png");
            yield return new WaitForEndOfFrame();
            var screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            if(screenshot){File.WriteAllBytes(output,screenshot.EncodeToPNG());Destroy(screenshot);}
            Session.Menu();yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();
            screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            if(screenshot){File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-menu.png"),screenshot.EncodeToPNG());Destroy(screenshot);}
            Session.ShowAchievements();yield return new WaitForEndOfFrame();
            screenshot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-achievements.png"),screenshot.EncodeToPNG());Destroy(screenshot);
            yield return MissionProof();yield return TrafficProof();yield return VisualProof();yield return UiProof();
            Check(runtimeErrors==0,"No runtime errors or exceptions");
            File.WriteAllText(Path.Combine(Application.dataPath,"..","smoke-results.txt"),"PASS: "+checks+" checks\nPhysics, car collisions, thirteen staged attacks/defenses, drifting, pedestrian safety, four maps, curved bridge driving, extreme eligibility, save recovery, field guide, soundtrack, win/fail lifecycle, zero runtime errors.\n");
            Debug.Log("CYBERCAR_SMOKE_SUCCESS / "+checks);Application.Quit(0);
        }
        IEnumerator VisualProof(){
            Session.Prepare(7,3,1,true);Session.Begin();yield return new WaitForSeconds(1.5f);
            var source=Resources.Load<GameObject>("Art/CyberInterceptor");int meshes=source.GetComponentsInChildren<MeshFilter>().Length;
            Check(meshes<=20&&meshes>=4,"Detailed Blender car retains a bounded mesh count");
            int wheels=0;foreach(var t in Session.Player.GetComponentsInChildren<Transform>())if(t.name=="Animated wheel")wheels++;
            Check(wheels==4,"New car retains four independently animated wheel pivots");
            var reflection=Session.World.GetComponentInChildren<SceneReflections>();Check(reflection!=null&&reflection.Captures>0,"Reflection probe captures the completed environment");
            bool blend=false;foreach(var renderer in Session.World.GetComponentsInChildren<Renderer>())foreach(var material in renderer.sharedMaterials)if(material&&material.shader.name=="CyberCar/LandscapeBlend")blend=true;
            Check(blend,"Mountain terrain uses slope-blended ground and rock");
            yield return Capture("realism-cliffs.png");
            Session.SelectLevel(5);Session.Begin();yield return new WaitForSeconds(1.5f);yield return Capture("realism-city.png");
            Session.SelectLevel(8);Session.Begin();yield return new WaitForSeconds(1.5f);yield return Capture("realism-night.png");
        }
        IEnumerator CheckUi(string name){
            yield return new WaitForEndOfFrame();var hud=Session.GetComponent<GameHud>();
            if(hud.LayoutIssues.Count>0)Debug.Log("UI_LAYOUT_DETAIL / "+string.Join(" | ",hud.LayoutIssues));
            Check(hud.LayoutIssues.Count==0,"UI text and controls fit: "+name);
            var shot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Application.dataPath,"..","ui-"+name+".png"),shot.EncodeToPNG());Destroy(shot);
        }
        IEnumerator UiProof(){
            var hud=Session.GetComponent<GameHud>();
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(1920,1080)}){
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);string prefix=size.x+"x"+size.y+"-";
                Session.Menu();hud.ShowGarage(false);yield return CheckUi(prefix+"menu");
                hud.ShowGarage(true);yield return CheckUi(prefix+"garage");hud.ShowGarage(false);
                Session.SelectLevel(14);yield return CheckUi(prefix+"briefing");Session.Begin();yield return CheckUi(prefix+"driving");
                Session.Attacks.Launch(CyberAttack.Ransomware);Session.Attacks.Launch(CyberAttack.SensorAttack);Session.Attacks.Launch(CyberAttack.MusicInjection);yield return CheckUi(prefix+"threats");
                Session.TogglePause();yield return CheckUi(prefix+"pause");Session.TogglePause();
                Session.Defense.Begin(3);Session.Defense.Choose(Session.Defense.EvidenceAnswer);Session.Defense.Choose(Session.Defense.EvidenceAnswer);yield return CheckUi(prefix+"diagnostic");
                Session.SetGuide(true);hud.ShowGuideTopic(7);yield return CheckUi(prefix+"guide");Session.SetGuide(false);
                for(int i=0;i<AttackDirector.Count;i++)Session.Attacks.Launch((CyberAttack)i);Session.Finish(true,"Visual report review");yield return CheckUi(prefix+"report");
                Session.ShowAchievements();yield return CheckUi(prefix+"achievements");
            }
            Screen.SetResolution(1440,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);Session.Menu();
        }
        bool Solve(int attack){Session.Defense.Begin(attack);bool result=false;for(int i=0;i<3&&Session.Defense.Open;i++)result=Session.Defense.Choose(Session.Defense.EvidenceAnswer);return result;}
        IEnumerator MissionProof(){
            Session.Prepare(14,0,2,true);Session.Begin();yield return new WaitForSeconds(.2f);Session.enabled=false;
            Session.Attacks.Launch(CyberAttack.Ransomware);Session.Attacks.Launch(CyberAttack.MusicInjection);
            var d=Session.Defense;d.Begin(3);d.Choose(d.EvidenceAnswer);string evidence=d.Evidence;int answer=d.EvidenceAnswer;
            d.Begin(3);Check(d.Stage==1&&d.Evidence==evidence&&d.EvidenceAnswer==answer,"Re-inspecting a threat retains its evidence and completed steps");
            d.Begin(10);d.Choose(d.EvidenceAnswer);d.Choose(d.EvidenceAnswer);d.Begin(3);
            Check(d.Stage==1&&d.Evidence==evidence,"Switching simultaneous threats restores independent progress");
            d.Close();d.Begin(10);Check(d.Stage==2,"Closing a diagnostic preserves progress for that encounter");
            Session.Attacks.Cooldowns[10]=5;Check(!d.Choose(d.EvidenceAnswer)&&d.Stage==2&&Session.Attacks.Has(CyberAttack.MusicInjection),"Recovery cooldown does not advance beyond the final step");
            Session.Attacks.Cooldowns[10]=0;Check(d.Choose(d.EvidenceAnswer)&&!d.Open&&!Session.Attacks.Has(CyberAttack.MusicInjection),"Verified recovery can be retried after cooldown");
            d.Begin(3);int errors=d.Mistakes;Check(!d.Choose((d.EvidenceAnswer+1)%3)&&d.Mistakes==errors+1&&d.Stage==1&&d.Evidence==evidence,"Wrong choices count once and preserve current evidence");
            Check(Solve(3),"A partially completed diagnostic resumes and resolves normally");
            Session.Attacks.Launch(CyberAttack.Spoofing);d.Begin(0);d.Choose(d.EvidenceAnswer);
            var old=Session.Attacks.Records.Find(r=>r.Active&&r.Type==CyberAttack.Spoofing);old.Active=false;Session.Attacks.Launch(CyberAttack.Spoofing);
            Check(!d.Choose(d.EvidenceAnswer)&&!d.Open&&Session.Attacks.Has(CyberAttack.Spoofing),"Expired evidence cannot authorize a replacement attack of the same type");
            d.Begin(0);Check(d.Stage==0,"Each new encounter starts with fresh diagnostic progress");
            // Satellite fallback must wait for an RSU repair, then resume without restarting.
            Session.Player.Recover(Session.World.Network.Nodes[0],Quaternion.identity);Session.Comms.Tick(0);
            Session.Attacks.Launch(CyberAttack.SatelliteLoss);Session.Attacks.Launch(CyberAttack.RsuLoss);d.Begin(9);d.Choose(d.EvidenceAnswer);
            Check(!d.Choose(d.EvidenceAnswer)&&d.Stage==1,"Satellite handoff waits while RSU authentication is unavailable");
            Check(Solve(8),"RSU repair can finish while another diagnostic is waiting");d.Begin(9);
            Check(d.Stage==1&&Solve(9)&&Session.Comms.SatelliteFallback,"Satellite handoff resumes after repairing the roadside link");
            Session.Finish(true,"Mission rating review");Check(Session.Rating!=null&&Session.Rating.Mistakes==1&&Session.Rating.Threats==6&&Session.Rating.Defended==4,"After-action rating includes every encounter and diagnostic error");
            yield return Capture("mission-rating.png");
            var records=new System.Collections.Generic.List<AttackRecord>{new AttackRecord{Active=false,Defended=true}};
            Check(new MissionRating(true,0,0,0,0,records).Stars==3,"A clean delivery with verified defenses earns three stars");
            Check(new MissionRating(false,0,0,0,0,records).Stars==0,"A failed delivery cannot earn stars");
            Check(new MissionRating(true,1,0,0,0,records).Stars==2&&new MissionRating(true,0,1,0,0,records).Stars==2,"Impacts or recovery remove the safe-driving star");
            Check(new MissionRating(true,0,0,1,0,records).Stars==1,"Pedestrian contact caps a completed run at one star");
            Check(new MissionRating(true,0,0,0,1,records).Stars==2,"Diagnostic errors remove the verified-recovery star");
            records.Add(new AttackRecord{Active=false});Check(new MissionRating(true,0,0,0,0,records).Stars==2,"Expired uncontained attacks count against verified recovery");
            Check(new MissionRating(true,0,0,0,0,new System.Collections.Generic.List<AttackRecord>()).Stars==3,"Clean orientation delivery can earn all three stars");
            var profile=new DriverProfile();Check(ProgressStore.ImproveRecord(profile,2,2,60)&&ProgressStore.ImproveRecord(profile,2,3,90),"A safer higher-star run outranks a faster low-star run");
            Check(!ProgressStore.ImproveRecord(profile,2,2,20)&&!ProgressStore.ImproveRecord(profile,2,3,100)&&ProgressStore.ImproveRecord(profile,2,3,85),"Only a faster time at the same rating replaces a personal best");
            Check(!ProgressStore.ImproveRecord(profile,2,0,1)&&!ProgressStore.ImproveRecord(profile,2,3,float.NaN)&&!ProgressStore.ImproveRecord(profile,-1,3,1),"Invalid and failed results cannot enter mission records");
            string path=Path.Combine(Application.temporaryCachePath,"cybercar-mission-record-test.json");
            profile.unlocked=9;profile.credits=420;profile.badges[21]=true;profile.upgrades[0]=2;ProgressStore.WriteTo(path,profile);
            var loaded=ProgressStore.LoadFrom(path);Check(loaded.version==4&&loaded.best[2].stars==3&&loaded.best[2].seconds==85&&loaded.credits==420&&loaded.badges[21]&&loaded.upgrades[0]==2,"Mission records survive disk reload alongside existing progress");
            ProgressStore.WriteTo(path,profile);File.WriteAllText(path,"broken");loaded=ProgressStore.LoadFrom(path);
            Check(loaded.best[2]!=null&&loaded.best[2].seconds==85,"Corrupt profile recovers mission records from backup");
            foreach(int version in new[]{2,3}){File.WriteAllText(path,"{\"version\":"+version+",\"unlocked\":7,\"credits\":180,\"badges\":[true],\"upgrades\":[2]}");loaded=ProgressStore.LoadFrom(path);Check(loaded.version==4&&loaded.unlocked==7&&loaded.credits==180&&loaded.badges[0]&&loaded.upgrades[0]==2&&loaded.best.Length==15&&loaded.best[0]==null,"Legacy schema "+version+" keeps existing progress and initializes mission records");}
            File.WriteAllText(path,"{\"version\":4,\"best\":[{\"stars\":8,\"seconds\":-1},null,{\"stars\":2,\"seconds\":20}]}");loaded=ProgressStore.LoadFrom(path);
            Check(loaded.best[0]==null&&loaded.best[1]==null&&loaded.best[2].stars==2&&loaded.best.Length==15,"Partial or malformed record arrays normalize safely");
            Check(!ProgressStore.RecordMission(0,3,1),"Automated runs cannot write the real player's mission records");
            Session.Prepare(0,0,0,false);Check(Session.Rating==null&&Session.Defense.Mistakes==0&&!Session.Defense.Open,"Restart clears run-local rating and diagnostic state");Session.enabled=true;
            Session.Prepare(8,0,1,false,night:true);Session.Begin();yield return new WaitForSeconds(.4f);
            Material rear=null,front=null;foreach(var renderer in Session.Player.GetComponentsInChildren<Renderer>())foreach(var material in renderer.sharedMaterials){if(material.name=="Responsive rear lamps")rear=material;if(material.name=="Independent running lamps")front=material;}
            Check(rear!=null&&front!=null&&rear.GetColor("_EmissionColor").r>1&&front.GetColor("_EmissionColor").b>2,"Night running lamps illuminate actual front and rear car meshes");
            float rearGlow=rear.GetColor("_EmissionColor").r;Session.Player.Brake=true;yield return null;yield return new WaitForEndOfFrame();
            Check(rear.GetColor("_EmissionColor").r>rearGlow*2,"Braking increases rear lamp intensity above night running lights");Session.Player.Brake=false;
            Session.Attacks.Launch(CyberAttack.Blackout);yield return new WaitForSeconds(.4f);
            bool trafficLamps=false;foreach(var renderer in Session.Cars[1].GetComponentsInChildren<Renderer>())foreach(var material in renderer.sharedMaterials)if(material.name=="Responsive rear lamps"&&material.GetColor("_EmissionColor").r>1)trafficLamps=true;
            Check(Session.World.Environment.LitStreetlights==0&&rear.GetColor("_EmissionColor").r>=rearGlow&&trafficLamps,"Blackout leaves player and traffic running lights powered");
            yield return new WaitForEndOfFrame();var frame=ScreenCapture.CaptureScreenshotAsTexture();int redPixels=0;var view=FindAnyObjectByType<ChaseCamera>().GetComponent<Camera>();
            foreach(var renderer in Session.Player.GetComponentsInChildren<Renderer>())if(renderer.sharedMaterial==rear){
                var bounds=renderer.bounds;Vector2 low=new Vector2(Screen.width,Screen.height),high=Vector2.zero;
                for(int corner=0;corner<8;corner++){Vector3 p=bounds.center+Vector3.Scale(bounds.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));Vector3 screen=view.WorldToScreenPoint(p);low=Vector2.Min(low,screen);high=Vector2.Max(high,screen);}
                for(int y=Mathf.Max(0,Mathf.FloorToInt(low.y)-2);y<=Mathf.Min(frame.height-1,Mathf.CeilToInt(high.y)+2);y++)for(int x=Mathf.Max(0,Mathf.FloorToInt(low.x)-2);x<=Mathf.Min(frame.width-1,Mathf.CeilToInt(high.x)+2);x++){Color pixel=frame.GetPixel(x,y);if(pixel.r>.5f&&pixel.r>pixel.g*2.5f&&pixel.r>pixel.b*2.5f)redPixels++;}
            }
            Destroy(frame);Check(redPixels>=6,"Rear running lamps produce visible red pixels during blackout");
            yield return Capture("mission-night-lights.png");
        }
        IEnumerator ExpansionProof(){
            bool storyValid=true;for(int level=0;level<GameSession.LevelCount;level++){var graph=new RoadNetwork(StoryCampaign.Maps[level],level==14);var route=StoryCampaign.Route(graph,level);storyValid&=route.Count>1&&route[route.Count-1]==graph.Finish;for(int n=1;n<route.Count;n++)storyValid&=graph.CanTravel(route[n-1],route[n]);}Check(storyValid,"All fifteen story routes reach named destinations through legal edges");
            Session.SelectLevel(10);Session.Begin();foreach(var c in Session.Cars)if(c!=Session.Player)c.gameObject.SetActive(false);
            Session.Attacks.Launch(CyberAttack.SatelliteLoss);yield return null;
            Check(!Session.Comms.SatelliteAvailable&&Session.Comms.Connected,"Satellite loss uses a nearby trusted roadside reference");
            Session.Player.Recover(new Vector3(-180,Session.World.GroundHeight(new Vector3(-180,0,-180))+.2f,-180),Quaternion.identity);yield return null;
            Check(!Session.Comms.InRange&&!Session.Comms.Connected,"DSRC contact ends outside short-range coverage");
            Session.Defense.Begin(9);Session.Defense.Choose(Session.Defense.EvidenceAnswer);
            Check(!Session.Defense.Choose(Session.Defense.EvidenceAnswer)&&Session.Defense.Stage==1,"RSU handshake cannot complete outside coverage");
            Session.Player.Recover(Session.World.Network.Nodes[0],Quaternion.Euler(0,90,0));yield return new WaitForSeconds(.1f);
            Debug.Log("HANDOFF stage="+Session.Defense.Stage+" connected="+Session.Comms.Connected+" active="+Session.Attacks.Has(CyberAttack.SatelliteLoss));
            Session.Defense.Choose(Session.Defense.EvidenceAnswer);Check(Session.Defense.Choose(Session.Defense.EvidenceAnswer),"RSU fallback completes after returning to coverage");
            Session.Attacks.Launch(CyberAttack.DestinationInjection);Session.Defense.Begin(11);float before=Session.Elapsed;
            bool rejected=!Session.Defense.Choose((Session.Defense.EvidenceAnswer+1)%3);Debug.Log("DIAG_WRONG rejected="+rejected+" active="+Session.Attacks.Has(CyberAttack.DestinationInjection)+" elapsed="+Session.Elapsed+" before="+before+" state="+Session.State);
            Check(rejected&&Session.Attacks.Has(CyberAttack.DestinationInjection)&&Session.Elapsed>=before+3.99f,"Incorrect diagnostic keeps attack active and applies penalty");
            Check(!Session.Attacks.Defend(11),"Single defense call cannot bypass three diagnostic steps");
            Check(Solve(11),"Validating saved route restores trusted dispatch");
            var sensor=Session.Player.GetComponent<CarSensors>();Vector3 sensorWallPosition=Session.Player.transform.position+Session.Player.transform.forward*12+Vector3.up*1.5f;var sensorWall=Session.World.Box("Sensor test obstacle",sensorWallPosition,new Vector3(4,3,4),Session.World.TrafficPaint);yield return new WaitForSeconds(.2f);
            Check(sensor.ActualRange<12&&sensor.CameraRange<12,"Car sensors measure a visible physical obstacle");sensorWall.SetActive(false);
            Session.Attacks.Launch(CyberAttack.SensorAttack);Check(sensor.ReportedRange==2,"Sensor injection replaces range report with phantom obstacle");Session.Attacks.Launch(CyberAttack.MusicInjection);Session.Attacks.Launch(CyberAttack.Ransomware);
            Check(Session.Attacks.ActiveCount>=3,"Sensor, media and ransomware attacks coexist");
            Session.Defense.Begin(3);yield return Capture("smoke-diagnostic.png");
            Session.SelectLevel(5);Session.Begin();var g=Session.World.Network;int a=g.Columns,b=a+1;
            Check(g.CanTravel(a,b)&&!g.CanTravel(b,a)&&g.Route(b,a).Count>2,"One-way routes force legal alternate routing");
            var signals=Session.World.GetComponent<TrafficSignals>();Check(signals.Red(0,Vector3.right,14)&&!signals.Red(0,Vector3.right,2),"City traffic signal phases control crossing priority");
            var traffic=Session.Cars.Find(c=>c.GetComponent<TrafficAgent>()&&!c.GetComponent<TrafficAgent>().Hostile);traffic.Body.position=new Vector3(-70,20,-70);yield return new WaitForSeconds(.3f);g.ClosestRoad(traffic.transform.position,out _,out float roadDistance);
            Debug.Log("TRAFFIC_RECOVERY body="+traffic.Body.position+" rendered="+traffic.transform.position+" roadDistance="+roadDistance);
            Check(roadDistance<g.RoadWidth,"AI containment returns escaped cars to visible roads");
            Session.Prepare(14,3,1,true,attacker:true);Session.Begin();Session.LaunchLab(10);Check(Session.AttackerMode&&Session.AttackBudget==10&&Session.Attacks.Has(CyberAttack.MusicInjection),"Attacker lab spends limited budget to affect AI courier");
            Session.Prepare(0,0,0,false,attacker:true);Session.Begin();foreach(var c in Session.Cars)if(c!=Session.Player)c.gameObject.SetActive(false);foreach(var c in Session.Citizens)c.gameObject.SetActive(false);
            float courierEnd=Time.time+150;Time.timeScale=3;while(Session.Running&&Time.time<courierEnd)yield return null;Time.timeScale=1;
            Debug.Log("LAB_COURIER route="+Session.RouteIndex+" position="+Session.Player.transform.position+" state="+Session.State);
            Check(Session.State==GameState.Debrief&&!Session.Won&&Session.EndReason=="Lab courier resisted and delivered","Unattacked lab courier physically completes its delivery route");
            Session.Prepare(7,3,1,true);Session.Begin();foreach(var c in Session.Cars)if(c!=Session.Player)c.gameObject.SetActive(false);foreach(var c in Session.Citizens)c.gameObject.SetActive(false);
            Session.enabled=false;var landscape=Session.World.GetComponent<NaturalLandscape>();Vector3 slopeStart=Vector3.zero;bool found=false;
            for(float x=60;x<Session.World.Network.Size-40&&!found;x+=23)for(float z=70;z<Session.World.Network.Size-40&&!found;z+=29){float y=landscape.Height(x,z);Vector3 candidate=new Vector3(x,y,z);Session.World.Network.ClosestRoad(candidate,out _,out float distance);float grade=(landscape.Height(x+4,z)-y)/4;if(distance>25&&grade>.12f&&grade<.32f){slopeStart=candidate;found=true;}}
            Check(found,"Mountain map contains navigable off-road slopes");Session.Player.Recover(slopeStart+Vector3.up*.2f,Quaternion.Euler(0,90,0));Session.Player.Throttle=.45f;float pitch=0;float slopeEnd=Time.time+4;while(Time.time<slopeEnd){pitch=Mathf.Max(pitch,Mathf.Abs(Vector3.Dot(Session.Player.transform.forward,Vector3.up)));yield return null;}
            Check(Vector3.Distance(slopeStart,Session.Player.transform.position)>5&&pitch>.025f,"Suspension physically follows a mountain slope off road");yield return Capture("smoke-offroad.png");Session.enabled=true;
            Vector3 fork=Session.World.Network.Nodes[Session.World.Network.Columns-1];Session.Player.Recover(fork+Vector3.right*26,Quaternion.Euler(0,90,0));Session.Player.Throttle=1;Session.Attacks.Launch(CyberAttack.Spoofing);
            float end=Time.time+8,lowest=fork.y;while(Session.Running&&Time.time<end){lowest=Mathf.Min(lowest,Session.Player.transform.position.y);yield return null;}
            Check(!Session.Running&&!Session.Won&&lowest<fork.y-10,"Driving beyond broken bridge physically falls and ends mission");
            yield return Capture("smoke-bridge-fall.png");
            Session.SelectLevel(0);Session.Begin();foreach(var c in Session.Cars)if(c!=Session.Player)c.gameObject.SetActive(false);Session.Player.Throttle=-1;yield return new WaitForSeconds(2);
            Check(Session.Player.Speed< -4,"Car backs up smoothly from rest");
            Check(Session.Player.Body.constraints==RigidbodyConstraints.None,"Pitch and roll remain available for 3D off-road motion");
            Session.Player.Throttle=0;Session.Player.Brake=true;Session.Player.Recover(Session.World.Network.Nodes[0]+Vector3.up*1.8f,Quaternion.Euler(0,90,180));yield return new WaitForSeconds(10);
            Check(Session.Player.Upright&&Session.Recoveries>0,"Overturned stationary car automatically recovers to its last checkpoint");
        }
        IEnumerator TrafficProof(){
            int oldVsync=QualitySettings.vSyncCount,oldRate=Application.targetFrameRate;QualitySettings.vSyncCount=0;
            foreach(int fps in new[]{30,120}){
                Application.targetFrameRate=fps;Session.SelectLevel(5);Session.Begin();yield return new WaitForSeconds(.2f);
                var graph=Session.World.Network;var traffic=Session.Cars.Find(c=>c.GetComponent<TrafficAgent>()&&!c.GetComponent<TrafficAgent>().Hostile);
                foreach(var position in new[]{new Vector3(-70,20,-70),new Vector3(graph.Size+70,20,graph.Size+70),new Vector3(graph.Size/2,-20,graph.Size/2)}){
                    traffic.Body.position=position;traffic.Body.linearVelocity=Vector3.zero;
                    Debug.Log("TRAFFIC_TELEPORT targetFPS="+fps+" physicsRenderGap="+Vector3.Distance(traffic.Body.position,traffic.transform.position));
                    for(int step=0;step<12;step++)yield return new WaitForFixedUpdate();yield return null;yield return null;
                    graph.ClosestRoad(traffic.Body.position,out _,out float physical);graph.ClosestRoad(traffic.transform.position,out _,out float rendered);
                    Check(physical<graph.RoadWidth&&rendered<graph.RoadWidth,"AI road recovery at target "+fps+" fps from "+position);
                }
            }
            QualitySettings.vSyncCount=oldVsync;Application.targetFrameRate=oldRate;
        }
        IEnumerator BridgeProof()
        {
            // Drive a curved, elevated coastal bridge using physics rather than teleporting.
            Session.Prepare(7,3,2,true);Session.Begin();Session.enabled=false;
            foreach(var c in Session.Citizens)c.gameObject.SetActive(false);
            foreach(var c in Session.Cars)if(c!=Session.Player)c.gameObject.SetActive(false);
            int bridgeStart=Session.World.Network.Columns/2-1;var bridge=Session.World.Network.Path(bridgeStart,bridgeStart+1);
            Session.Player.Recover(bridge[0],Quaternion.LookRotation(bridge[1]-bridge[0]));
            int point=1;float deadline=Time.time+25;float highest=0;
            while(point<bridge.Length&&Time.time<deadline)
            {
                Vector3 delta=Session.Player.transform.InverseTransformPoint(bridge[point]);float angle=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;
                Session.Player.Steer=Mathf.Clamp(angle/27,-1,1);Session.Player.Throttle=1;Session.Player.Brake=Session.Player.Speed>12;
                highest=Mathf.Max(highest,Session.Player.transform.position.y);
                if(Vector3.Distance(Session.Player.transform.position,bridge[point])<5)point++;
                yield return null;
            }
            Debug.Log("BRIDGE_DIAGNOSTIC point="+point+" position="+Session.Player.transform.position+" highest="+highest+" speed="+Session.Player.Speed+" health="+Session.Player.Health);
            yield return new WaitForEndOfFrame();var bridgeShot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Application.dataPath,"..","bridge-probe.png"),bridgeShot.EncodeToPNG());Destroy(bridgeShot);
            Check(point==bridge.Length&&highest>2,"Car physically traverses a curved elevated beach-cliff bridge");Session.enabled=true;
        }
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame();var shot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Application.dataPath,"..",name),shot.EncodeToPNG());Destroy(shot);}
        void AuditWorld(int map){
            int missing=0,invisible=0,blocked=0;string problem="";
            foreach(var r in Session.World.GetComponentsInChildren<Renderer>())if(r.enabled)foreach(var m in r.sharedMaterials)if(!m||!m.shader||!m.shader.isSupported){missing++;problem=r.name;}
            foreach(var c in Session.World.GetComponentsInChildren<Collider>())if(c.enabled&&!c.isTrigger){var r=c.GetComponent<Renderer>();if(!r||!r.enabled){invisible++;problem=c.name;}}
            var graph=Session.World.Network;
            foreach(var edge in graph.Edges){var path=graph.Path(edge.x,edge.y);for(int i=4;i<path.Length-4;i+=3){var side=Vector3.Cross(Vector3.up,(path[i+1]-path[i-1]).normalized).normalized;
                foreach(int lane in new[]{-1,0,1}){Vector3 p=path[i]+side*lane*graph.RoadWidth*.28f;float highest=float.MinValue;string hitName="";
                    foreach(var hit in Physics.RaycastAll(p+Vector3.up*3,Vector3.down,6,~0,QueryTriggerInteraction.Ignore)){if(hit.rigidbody!=null)continue;if(hit.point.y>highest){highest=hit.point.y;hitName=hit.collider.name;}}
                    if(highest>p.y+.22f||highest<p.y-.22f){blocked++;problem=hitName+" at "+p+" surface "+highest;}
                }
            }}
            Check(missing==0,"Map "+map+" has supported visible materials: "+problem);
            Check(invisible==0,"Map "+map+" has no invisible static collision objects: "+problem);
            Check(blocked==0,"Map "+map+" sampled driving lanes are clear: "+problem);
        }
    }
}
