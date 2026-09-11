using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 [RequireComponent(typeof(VehicleController))] public sealed class TrafficAgent:MonoBehaviour {
 public GameSession Session;public bool Hostile;VehicleController car;List<Vector3> points=new List<Vector3>();int waypoint,serial;float stuck,reverseUntil;System.Random rng;
 public void Initialize(GameSession session,bool hostile,int index){Session=session;Hostile=hostile;serial=index;rng=new System.Random(index*73+session.RouteSeed);car=GetComponent<VehicleController>();Plan();}
 void Plan(){var graph=Session.World.Network;int from=graph.Nearest(transform.position);bool pursuit=Hostile&&Session.Elapsed>Session.Attacks.TrustedTrafficUntil;int target=pursuit?graph.Nearest(Session.Player.transform.position):rng.Next(3)!=0?graph.Hotspots[rng.Next(graph.Hotspots.Count)]:rng.Next(graph.Nodes.Count);if(target==from)target=(from+1)%graph.Nodes.Count;points=graph.TravelPath(graph.Route(from,target));waypoint=0;while(waypoint<points.Count-1&&Vector3.Distance(points[waypoint],transform.position)<12)waypoint++;}
 void Update(){
 if(car==null||!Session.Running)return;
 if(waypoint>=points.Count)Plan();if(points.Count==0)return;
 bool pursuit=Hostile&&Session.Elapsed>Session.Attacks.TrustedTrafficUntil;Vector3 aim=points[Mathf.Min(waypoint,points.Count-1)];
 if(waypoint>0){Vector3 tangent=(aim-points[waypoint-1]).normalized;aim+=Vector3.Cross(Vector3.up,tangent)*2.7f;}
 if(Vector3.Distance(transform.position,aim)<6)waypoint++;
 if(pursuit&&Vector3.Distance(transform.position,Session.Player.transform.position)<23)aim=Session.Player.transform.position+Session.Player.Body.linearVelocity*.2f;
 Vector3 relative=transform.InverseTransformPoint(aim);float angle=Mathf.Atan2(relative.x,relative.z)*Mathf.Rad2Deg;
 car.Steer=Mathf.Clamp(angle/30,-1,1);car.MaxSpeed=pursuit?(Session.Attacks.Has(CyberAttack.Sybil)?31:22+Session.Difficulty*2):15+serial%5;car.Throttle=1;car.Brake=Mathf.Abs(angle)>35&&Mathf.Abs(car.Speed)>10;
 bool yield=false;
 foreach(var person in Session.Citizens)if(person&&person.IsCrossing){Vector3 p=transform.InverseTransformPoint(person.transform.position);if(p.z>0&&p.z<17&&Mathf.Abs(p.x)<4)yield=true;}
 if(Physics.Raycast(transform.position+Vector3.up,transform.forward,out var hit,Mathf.Max(9,car.Speed*.9f))&&hit.rigidbody!=null&&hit.rigidbody!=car.Body){if(!pursuit||hit.rigidbody.GetComponent<VehicleController>()!=Session.Player)yield=true;}
 // A replay campaign makes ordinary vehicles repeat stale emergency-stop commands.
 if(!Hostile&&Session.Attacks.Has(CyberAttack.Replay)&&Session.InHotspot)yield=true;
 if(yield){car.Brake=true;car.Throttle=0;stuck=0;return;}
 if(Mathf.Abs(car.Speed)<.6f)stuck+=Time.deltaTime;else stuck=0;
 if(stuck>2){reverseUntil=Session.Elapsed+1.4f;stuck=0;}
 if(Session.Elapsed<reverseUntil){car.Throttle=-.6f;car.Steer=-car.Steer;car.Brake=false;}
 if(transform.position.y<-12){int n=Session.World.Network.Nearest(transform.position);car.Recover(Session.World.Network.Nodes[n],Quaternion.identity);Plan();}
 }
 }
}