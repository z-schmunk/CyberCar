using System;
using UnityEngine;
namespace CyberCar {
 // A staged, local diagnostic; no real commands or files are executed.
 public sealed class DefenseChallenge {
 readonly GameSession session;readonly System.Random random;AttackRecord selected;bool authorized;
 public int Attack=>selected==null?-1:(int)selected.Type;public int Stage=>selected==null?0:selected.DiagnosticStage;
 public string Evidence=>selected==null?"":selected.Evidence;
 public string[] Choices=>selected==null?System.Array.Empty<string>():selected.Choices;public bool Open=>selected!=null;
 public int Mistakes{get;private set;}
 public DefenseChallenge(GameSession s,int seed){session=s;random=new System.Random(seed^871);}
 public bool CanCommit(int index)=>authorized&&index==Attack;
 public void Begin(int attack){var active=session.Attacks.Records.Find(r=>r.Active&&(int)r.Type==attack);if(active==null){session.Notify("Select an active threat to inspect its evidence.");return;}selected=active;if(string.IsNullOrEmpty(selected.Evidence))Present();}
 public void Close(){selected=null;}
 public bool Choose(int choice){
 if(!Open||choice<0||choice>2)return false;
 if(!selected.Active){Close();return false;}
 if(choice!=selected.Answer){Mistakes++;session.ElapsedPenalty(4);session.Notify("The evidence does not support that action. Recheck identity, integrity and freshness; +4 seconds.",true);return false;}
 if((Attack==8||Attack==9||Attack==1)&&Stage==1&&!session.Comms.InRange){session.Notify("Drive within 115 m of a roadside unit to perform this handoff.",true);return false;}
 if((Attack==1||Attack==9)&&Stage==1&&!session.Comms.Connected){session.Notify("Restore the RSU link before relying on its location reference.",true);return false;}
 if(Stage<2){selected.DiagnosticStage++;Present();return true;}
 bool success;
 authorized=true;try{success=session.Attacks.Defend(Attack);}finally{authorized=false;}
 if(success){selected.DiagnosticStage=3;Close();}else session.Notify("Recovery is cooling down. Keep this verified evidence and retry shortly.",true);
 return success;
 }
 void Present(){
 string good,bad,other;
 if(Stage==0){
 string[] evidence={"Dispatch signature differs from the received route.","Remote queue: 99% full. Cached map: healthy.","Steering commands originate at the entertainment gateway.","Console files are encrypted; a ransom note asks for payment.","Three convoy senders reuse one certificate identifier.","Last accepted sequence 812; incoming brake command 807.","Update publisher signature: INVALID.","Remote credentials issued an unauthorized lights-off command.","RSU session authentication fails; radio signal is strong.","GNSS signal integrity failed; wheel odometry is healthy.","Playlist contains an executable field; a duck loop replaced music.","History entry includes destination=SERVICE_EXIT; dispatch differs.","Range feed says obstacle 2 m; camera and bumper sensor show clear."};
 selected.Evidence=(Attack==12?session.Player.GetComponent<CarSensors>().Readout:evidence[Attack])+"\nSTEP 1 / CONTAIN";
 string[] actions={"Reject the unverified route","Limit flood traffic; retain cached roads","Isolate the entertainment-to-control gateway","Disconnect the infected console","Reject duplicate vehicle identities","Reject sequence 807","Quarantine the unsigned update","Revoke remote lighting access","Revoke the failed RSU session","Mark GNSS as untrusted","Quarantine the injected playlist","Reject executable fields in saved history","Quarantine the inconsistent range feed"};
 good=actions[Attack];bad="Trust the newest message automatically";other="Encrypt the suspect data and execute it";
 }else if(Stage==1){
 int nonce=random.Next(100,999);string destination=session.World.Network.Names[session.World.Network.Finish];
 if(Attack==3){selected.Evidence="OFFLINE BACKUP / signed manifest hash: "+nonce+"\nSTEP 2 / VERIFY BACKUP INTEGRITY";good="Restore isolated backup with hash "+nonce;bad="Restore online copy with hash "+(nonce+1);other="Pay for an unverified decryptor";}
 else if(Attack==8||Attack==9||Attack==1){selected.Evidence="RSU "+session.Comms.NearestName+" / challenge nonce "+nonce+"\nSTEP 2 / AUTHENTICATE WITHIN 115 m";good="Verify certificate and fresh nonce "+nonce;bad="Accept expired nonce "+(nonce-1);other="Trust the strongest signal without identity";}
 else if(Attack==0||Attack==11){selected.Evidence="SIGNED DISPATCH / "+destination+"\nSTEP 2 / CONFIRM DESTINATION";good="Use dispatch: "+destination;bad="Use history: Abandoned service exit";other="Accept a destination from the attacker";}
 else{selected.Evidence="TRUSTED REFERENCE / sequence "+nonce+"; signature valid\nSTEP 2 / CROSS-CHECK";good="Accept valid signature and fresh sequence "+nonce;bad="Accept valid signature with old sequence "+(nonce-4);other="Accept fresh sequence with invalid signature";}
 }else{
 selected.Evidence="STEP 3 / RECOVER SAFELY";
 if(Attack==3){int key=random.Next(10,99);int plain=random.Next(65,91),cipher=plain^key;
 selected.Evidence="ENCRYPTED BACKUP / byte "+cipher+"; protected key "+key+"\nXOR decoding: "+cipher+" XOR "+key+" = "+plain+". Signed manifest expects "+plain+". (Toy cipher.)";
 good="Unlock with protected key "+key+" and verify byte "+plain;bad="Use unknown key "+(key+1)+"; decoded byte "+(cipher^(key+1));other="Delete the only clean backup";}
 else {good=new[]{"Restore authenticated dispatch route","Use the authenticated RSU and cached map","Retain local driver control","Restore verified offline backup","Resume only authenticated platoon messages","Enforce increasing command sequence numbers","Boot the verified recovery image","Restore trusted local lighting schedule","Resume authenticated short-range communication","Use RSU updates plus odometry between units","Reload verified audio with data-only playlist","Load the allowlisted dispatch destination","Use camera and bumper sensor; retain manual control"}[Attack];bad="Reconnect every suspect source immediately";other="Disable all integrity checks";}
 }
 selected.Answer=random.Next(3);Choices[selected.Answer]=good;Choices[(selected.Answer+1)%3]=bad;Choices[(selected.Answer+2)%3]=other;
 }
 // Exercised by the runtime harness through the same public selection path.
 public int EvidenceAnswer=>selected==null?-1:selected.Answer;
 }
}
