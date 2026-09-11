using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public enum CyberAttack {Spoofing,DenialOfService,ControlInjection,Ransomware,Sybil,Replay,Firmware,Blackout}
 public sealed class AttackRecord {public CyberAttack Type;public float Remaining;public bool Active=true,Defended;public int Context;}
 public sealed class AttackDirector {
 public const int Count=8;
 public static readonly string[] Keys={"Q","E","R","F","G","T","Y","U"};
 public static readonly KeyCode[] KeyCodes={KeyCode.Q,KeyCode.E,KeyCode.R,KeyCode.F,KeyCode.G,KeyCode.T,KeyCode.Y,KeyCode.U};
 public static readonly string[] Names={"GPS SPOOFING","SIGNAL FLOOD","CONTROL INJECTION","RANSOMWARE","SYBIL CONVOY","REPLAY ATTACK","MALICIOUS FIRMWARE","LIGHTING TAKEOVER"};
 public static readonly string[] Symptoms={"Route signature mismatch. Destination moved.","Navigation channel saturated. GPS unavailable.","Untrusted CAN commands. Steering drifting.","Vehicle console encrypted. Power restricted.","Duplicate V2X identities coordinate hostile cars.","Old brake commands replayed. Traffic stops in bursts.","Unsigned update altered steering and engine behavior.","Streetlight controller compromised. Road lights disabled."};
 public static readonly string[] Defenses={"Verify signed route","Use offline navigation","Isolate control bus","Restore clean backup","Authenticate V2X identities","Reject stale sequences","Roll back signed firmware","Restore local lighting control"};
 public static readonly string[] Lessons={
 "Compare the received destination with an authenticated route and independent signs. Reject a modified route before following it.",
 "Switch to a locally cached map so delivery can continue while the navigation channel is unavailable.",
 "Disconnect the untrusted controller from safety-critical commands, preserving local driver control.",
 "Restore a known-good offline backup to recover availability without trusting the encrypted system.",
 "Authenticate vehicle certificates and reject duplicate identities, disrupting the forged convoy.",
 "Check timestamps, nonces and increasing sequence numbers; reject previously accepted commands.",
 "Reject the unsigned update and restore trusted signed firmware to return steering and power to normal.",
 "Revoke remote control, reject unauthorized switching commands and restore a trusted local lighting schedule."};
 public static readonly string[] Explanations={
 "ATTACK: A counterfeit navigation signal or altered route is presented as trustworthy. In this game the GPS points to a closed road or missing bridge.\n\nDEFENSE: The verifier compares route data to an authenticated reference and independent road signs. This restores the intended gates.\n\nREALITY: A signed route alone cannot prove that a raw satellite position is correct. Real systems also cross-check sensors and signal integrity.",
 "ATTACK: Excess requests exhaust a navigation service or its communication channel. The GPS display disappears here, but the road remains driveable.\n\nDEFENSE: An offline map removes dependence on that channel. It preserves availability while you drive.\n\nREALITY: Offline fallback does not stop the flood at its source. Rate limits, filtering and redundant services are additional defenses.",
 "ATTACK: A compromised controller injects unauthorized CAN messages. Steering drift represents loss of command integrity.\n\nDEFENSE: Bus isolation prevents the suspect controller's messages from reaching the steering system; manual input remains available.\n\nREALITY: CAN isolation requires a suitable gateway and a safe degraded mode. It is not a universal software switch for existing cars.",
 "ATTACK: Malware encrypts required data and blocks access. Restricted engine power represents a vehicle in degraded operation.\n\nDEFENSE: Restore a verified, offline backup and isolate the compromised environment.\n\nREALITY: Backups must be tested and separated from the infected system. Restoring files alone does not remove the original entry point.",
 "ATTACK: One adversary impersonates many vehicles, giving fabricated reports the appearance of a crowd. Hostile cars coordinate at interchanges.\n\nDEFENSE: Validate certificates and reject duplicate or untrusted sender identities. Enemy coordination temporarily stops.\n\nREALITY: Authentication must be paired with certificate revocation, plausibility checks and privacy-aware identity management.",
 "ATTACK: An attacker records a valid command and sends it again later. Braking pulses and stopped traffic show why authenticity alone is insufficient.\n\nDEFENSE: Enforce message freshness with timestamps, unpredictable nonces and monotonic sequence numbers. Old commands are rejected.\n\nREALITY: Clock synchronization and replay-window design matter. A signed message can still be stale.",
 "ATTACK: A forged over-the-air update changes vehicle behavior. Inverted steering and reduced power represent compromised firmware.\n\nDEFENSE: Verify the publisher's signature, quarantine the bad update and roll back to a trusted signed image.\n\nREALITY: Secure boot and protected update keys support this process. A valid signature identifies the publisher and integrity, not freedom from all bugs.",
 "ATTACK: A compromised infrastructure controller sends unauthorized switch-off commands to networked streetlights. The darkness reduces visibility while hostile traffic approaches.\n\nDEFENSE: Isolate remote control, revoke the compromised credentials and restore a verified local lighting schedule. Your independent headlights remain available.\n\nREALITY: Authentication, authorization and network segmentation limit access. Local fallback improves resilience; restoring lights alone does not remove the attacker from every system."
 };
 public readonly List<AttackRecord> Records=new List<AttackRecord>();public readonly float[] Cooldowns=new float[Count];
 readonly float[,] values=new float[2,Count];readonly int[,] trials=new int[2,Count];readonly GameSession session;readonly System.Random rng;
 float nextAttack=10;int opening;public float TrustedTrafficUntil;
 public AttackDirector(GameSession game,int seed=182){session=game;rng=new System.Random(seed);}
 public bool Has(CyberAttack type)=>Records.Exists(r=>r.Active&&r.Type==type);
 public int ActiveCount{get{int count=0;foreach(var r in Records)if(r.Active)count++;return count;}}
 public bool AllDefended{get{for(int i=0;i<(session.Night?Count:Count-1);i++)if(!Records.Exists(r=>(int)r.Type==i&&r.Defended))return false;return true;}}
 public void Tick(float dt){
 for(int i=0;i<Count;i++)Cooldowns[i]=Mathf.Max(0,Cooldowns[i]-dt);
 foreach(var r in Records){if(!r.Active)continue;r.Remaining-=dt;if(r.Remaining<=0){r.Active=false;Learn(r,1);session.Notify(Names[(int)r.Type]+" expired uncontained",true);}}
 nextAttack-=dt*(session.InHotspot?1.45f:1);
 int allowed=session.Freeplay?(session.Night?Count:Count-1):Mathf.Min(Count,session.Level),cap=session.Difficulty==3?3:session.Difficulty>=2?2:1;
 if(allowed==0||nextAttack>0||ActiveCount>=cap)return;
 int pick;if(opening<allowed)pick=allowed-1-opening++;else{int context=session.Player.Speed>14?1:0;pick=rng.Next(allowed);if(rng.NextDouble()>.25){float best=-1;for(int i=0;i<allowed;i++){float score=values[context,i]+.35f/Mathf.Sqrt(1+trials[context,i]);if(!Has((CyberAttack)i)&&score>best){best=score;pick=i;}}}}
 if(!Has((CyberAttack)pick))Launch((CyberAttack)pick);nextAttack=session.Difficulty==0?25:session.Difficulty==1?20:session.Difficulty==2?16:12;
 }
 public void Launch(CyberAttack type){if(Has(type))return;Records.Add(new AttackRecord{Type=type,Remaining=session.Difficulty==0?35:session.Difficulty==1?29:session.Difficulty==2?25:22,Context=session.Player.Speed>14?1:0});session.Notify(Symptoms[(int)type],true);}
 public bool Defend(int index){
 if(index<0||index>=Count||Cooldowns[index]>0)return false;var r=Records.Find(a=>a.Active&&(int)a.Type==index);Cooldowns[index]=r==null?3:8;
 if(r==null){session.Notify("No matching threat. Diagnostic cooling down.");return false;}
 r.Active=false;r.Defended=true;Learn(r,0);if(index==4)TrustedTrafficUntil=session.Elapsed+16;
 session.Award(index<5?index+1:index==5?9:index==6?10:15);session.LastLesson=index;session.Notify(Names[index]+" contained. "+Lessons[index]);return true;
 }
 void Learn(AttackRecord r,float reward){int i=(int)r.Type,n=++trials[r.Context,i];values[r.Context,i]+=(reward-values[r.Context,i])/n;}
 }
}