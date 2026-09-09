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
            yield return null;Session.InputEnabled=false;Session.SelectLevel(0);Session.Begin();
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
            for(int i=0;i<5;i++)
            {
                Session.Attacks.Launch((CyberAttack)i);yield return null;
                Check(Session.Attacks.Has((CyberAttack)i),"Attack activates: "+i);
                if(i==0)Check(Vector3.Distance(Session.Target,Session.World.Network.Hazard)<.1f,"Spoofed GPS points at physical hazard");
                if(i==1)Check(!Session.World.Beacon.gameObject.activeSelf,"Signal flood removes GPS gate");
                if(i==2)Check(Session.Player.ControlNoise>0,"Injection changes steering input");
                if(i==3)Check(Session.Player.EngineLimit<1,"Ransomware restricts engine");
                Check(Session.Attacks.Defend(i)&&!Session.Attacks.Has((CyberAttack)i),"Correct defense contains attack: "+i);
                Check(!Session.Attacks.Defend(i),"Cooldown prevents repeated activation: "+i);
                yield return null;
                if(i==2)Check(Session.Player.ControlNoise==0,"Bus isolation restores steering");
                if(i==3)Check(Session.Player.EngineLimit==1,"Backup restores engine power");
            }
            Session.TogglePause();float elapsed=Session.Elapsed;yield return new WaitForSecondsRealtime(.15f);Check(Session.Elapsed==elapsed,"Pause freezes mission timer");Session.TogglePause();
            for(int map=0;map<3;map++)
            {
                Session.Prepare(5,map,2,true);Session.Begin();yield return new WaitForSeconds(.15f);
                Check(Session.Cars.Count==12,"Expert traffic and hostile cars spawn");
                yield return new WaitForSeconds(.5f);
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
                if(map==2)
                {
                    yield return new WaitForEndOfFrame();var reportShot=ScreenCapture.CaptureScreenshotAsTexture();
                    if(reportShot){File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-report.png"),reportShot.EncodeToPNG());Destroy(reportShot);}
                }
            }
            Session.SelectLevel(1);Session.Begin();Session.Player.Damage(100);yield return null;yield return null;Check(!Session.Won&&Session.State==GameState.Debrief,"Destroyed vehicle fails mission");
            Session.SelectLevel(3);Session.Begin();Session.Player.Recover(new Vector3(0,-20,0),Quaternion.identity);yield return new WaitForSeconds(.1f);Check(!Session.Won&&Session.State==GameState.Debrief,"Cliff fall fails mission");
            Session.SelectLevel(1);Session.Begin();Session.Attacks.Launch(CyberAttack.Spoofing);Session.Attacks.Tick(40);Check(Session.Attacks.Records.Exists(r=>r.Type==CyberAttack.Spoofing&&!r.Defended&&!r.Active),"Uncontained attack is recorded as missed");
            for(int level=1;level<6;level++)
            {
                Session.SelectLevel(level);Session.Begin();Session.Attacks.Tick(11);
                Check(Session.Attacks.Has((CyberAttack)(level-1)),"Mission introduces its new attack first: "+level);
            }
            Session.SelectLevel(0);Session.Begin();Time.timeScale=100;
            yield return new WaitForSeconds(301);
            Check(Session.State==GameState.Debrief&&!Session.Won&&Session.EndReason=="Delivery window expired","Mission timer expiry fails delivery");Time.timeScale=1;
            Session.Prepare(5,0,1,true);Session.Begin();Session.Attacks.Launch(CyberAttack.Spoofing);
            yield return new WaitForSeconds(.5f);
            string output=Path.Combine(Application.dataPath,"..","smoke-gameplay.png");
            yield return new WaitForEndOfFrame();
            var screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            if(screenshot){File.WriteAllBytes(output,screenshot.EncodeToPNG());Destroy(screenshot);}
            Session.Menu();yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();
            screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            if(screenshot){File.WriteAllBytes(Path.Combine(Application.dataPath,"..","smoke-menu.png"),screenshot.EncodeToPNG());Destroy(screenshot);}
            Check(runtimeErrors==0,"No runtime errors or exceptions");
            File.WriteAllText(Path.Combine(Application.dataPath,"..","smoke-results.txt"),"PASS: "+checks+" checks\nPhysics acceleration, collision, five attacks and defenses, cooldowns, pause, all map completions, destruction, cliff fall, missed attack reporting, no runtime errors.\n");
            Debug.Log("CYBERCAR_SMOKE_SUCCESS / "+checks);Application.Quit(0);
        }
    }
}
