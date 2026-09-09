using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    [RequireComponent(typeof(VehicleController))]
    public sealed class TrafficAgent:MonoBehaviour
    {
        public GameSession Session;
        public bool Hostile;
        VehicleController car;
        List<int> route=new List<int>();
        int waypoint;
        float repath,stuck;
        Vector3 last;
        public void Initialize(GameSession session,bool hostile,int index)
        {
            Session=session;Hostile=hostile;car=GetComponent<VehicleController>();last=transform.position;
            route=session.World.Network.Route(session.World.Network.Nearest(transform.position),(index*7+11)%session.World.Network.Nodes.Count);waypoint=1;
        }
        void Update()
        {
            if(car==null||!Session.Running)return;
            bool pursuit=Hostile&&Session.Elapsed>Session.Attacks.TrustedTrafficUntil;
            float distance=Vector3.Distance(transform.position,Session.Player.transform.position);
            repath-=Time.deltaTime;
            if(repath<0||waypoint>=route.Count)
            {
                repath=2;
                int from=Session.World.Network.Nearest(transform.position);
                int target=pursuit?Session.World.Network.Nearest(Session.Player.transform.position):(from+7)%Session.World.Network.Nodes.Count;
                route=Session.World.Network.Route(from,target);waypoint=0;
                if(route.Count>1&&Vector3.Distance(transform.position,Session.World.Network.Nodes[from])<13)waypoint=1;
            }
            Vector3 aim=waypoint<route.Count?Session.World.Network.Nodes[route[waypoint]]:Session.Player.transform.position;
            if(Vector3.Distance(transform.position,aim)<8)waypoint++;
            if(pursuit&&distance<28)aim=Session.Player.transform.position+Session.Player.Body.linearVelocity*.3f;
            Vector3 relative=transform.InverseTransformPoint(aim);
            float angle=Mathf.Atan2(relative.x,relative.z)*Mathf.Rad2Deg;
            car.Steer=Mathf.Clamp(angle/32,-1,1);
            car.MaxSpeed=pursuit?(Session.Attacks.Has(CyberAttack.Sybil)?31:23+Session.Difficulty*2):12;
            car.Throttle=1;car.Brake=Mathf.Abs(angle)>55&&Mathf.Abs(car.Speed)>9;
            if(!pursuit&&Physics.Raycast(transform.position+Vector3.up,transform.forward,out var hit,8)&&hit.rigidbody!=car.Body){car.Brake=true;car.Throttle=0;}
            if(Vector3.Distance(last,transform.position)<.03f)stuck+=Time.deltaTime;else stuck=0;
            last=transform.position;
            if(stuck>1.5f){car.Throttle=-1;car.Steer=-car.Steer;car.Brake=false;}
            if(stuck>5||transform.position.y<-10)
            {
                int n=Session.World.Network.Nearest(transform.position);car.Recover(Session.World.Network.Nodes[n]+Vector3.right*3,Quaternion.identity);stuck=0;
            }
        }
    }
}
