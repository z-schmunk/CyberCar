using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    public enum CyberAttack { Spoofing,DenialOfService,ControlInjection,Ransomware,Sybil }
    public sealed class AttackRecord
    {
        public CyberAttack Type;
        public float Remaining;
        public bool Active=true;
        public bool Defended;
        public int Context;
    }
    public sealed class AttackDirector
    {
        public static readonly string[] Names={"GPS SPOOFING","SIGNAL FLOOD","CONTROL INJECTION","RANSOMWARE","SYBIL CONVOY"};
        public static readonly string[] Symptoms={"Route signature mismatch. Destination moved.","Navigation channel saturated. GPS unavailable.","Untrusted CAN commands. Steering drifting.","Vehicle console encrypted. Power restricted.","Duplicate V2X identities coordinating hostile cars."};
        public static readonly string[] Defenses={"Verify signed route","Switch to offline map","Isolate control bus","Restore clean backup","Validate V2X identities"};
        public static readonly string[] Lessons={
            "Spoofing falsifies a trusted signal. Compare authenticated navigation with independent road signs.",
            "Denial of service overwhelms availability. A cached offline map preserves navigation.",
            "Injected CAN commands alter vehicle control. Segmentation isolates the compromised controller.",
            "Ransomware denies access to systems. A known-good offline backup restores operation.",
            "Sybil attacks forge multiple identities. Authenticate senders and reject duplicate vehicle identities."};
        public readonly List<AttackRecord> Records=new List<AttackRecord>();
        public readonly float[] Cooldowns=new float[5];
        // Online contextual bandit: learns which attack succeeds at low/high vehicle speed.
        readonly float[,] values=new float[2,5];
        readonly int[,] trials=new int[2,5];
        readonly GameSession session;
        readonly System.Random rng;
        float nextAttack=10;
        int opening;
        public float TrustedTrafficUntil;
        public AttackDirector(GameSession game,int seed=182){session=game;rng=new System.Random(seed);}
        public bool Has(CyberAttack type)=>Records.Exists(r=>r.Active&&r.Type==type);
        public int ActiveCount=>Records.FindAll(r=>r.Active).Count;
        public void Tick(float dt)
        {
            for(int i=0;i<5;i++)Cooldowns[i]=Mathf.Max(0,Cooldowns[i]-dt);
            foreach(var r in Records)
            {
                if(!r.Active)continue;r.Remaining-=dt;
                if(r.Remaining<=0){r.Active=false;Learn(r,1);session.Notify(Names[(int)r.Type]+" expired uncontained",true);}
            }
            nextAttack-=dt;
            int allowed=session.Freeplay?5:Mathf.Min(5,session.Level);
            if(allowed==0||nextAttack>0||ActiveCount>=(session.Difficulty==2?2:1))return;
            int pick;
            if(opening<allowed)pick=allowed-1-opening++;
            else
            {
                int context=session.Player.Speed>14?1:0;pick=rng.Next(allowed);
                if(rng.NextDouble()>.25)
                {
                    float best=-1;
                    for(int i=0;i<allowed;i++)
                    {float score=values[context,i]+.35f/Mathf.Sqrt(1+trials[context,i]);if(!Has((CyberAttack)i)&&score>best){best=score;pick=i;}}
                }
            }
            if(!Has((CyberAttack)pick))Launch((CyberAttack)pick);
            nextAttack=session.Difficulty==0?25:session.Difficulty==1?19:13;
        }
        public void Launch(CyberAttack type)
        {
            if(Has(type))return;
            Records.Add(new AttackRecord{Type=type,Remaining=session.Difficulty==0?30:session.Difficulty==1?25:21,Context=session.Player.Speed>14?1:0});
            session.Notify(Symptoms[(int)type],true);
        }
        public bool Defend(int index)
        {
            if(index<0||index>4||Cooldowns[index]>0)return false;
            var record=Records.Find(r=>r.Active&&(int)r.Type==index);
            Cooldowns[index]=record==null?3:8;
            if(record==null){session.Notify("No matching threat. Diagnostic cooling down.");return false;}
            record.Active=false;record.Defended=true;Learn(record,0);
            if(index==4)TrustedTrafficUntil=session.Elapsed+12;
            session.Award(index+1);session.Notify(Names[index]+" contained");return true;
        }
        void Learn(AttackRecord r,float reward)
        {
            int i=(int)r.Type;int n=++trials[r.Context,i];values[r.Context,i]+=(reward-values[r.Context,i])/n;
        }
    }
}
