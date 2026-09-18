using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 [RequireComponent(typeof(VehicleController))] public sealed class TrafficAgent:MonoBehaviour {
 public GameSession Session;public bool Hostile;VehicleController car;List<Vector3> points=new List<Vector3>();int waypoint,serial;float stuck,reverseUntil,containTimer;public int PlatoonId=>Hostile?-1:(serial-20)/3;public bool Congested{get;private set;}System.Random rng;
 public void Initialize(GameSession session,bool hostile,int index){Session=session;Hostile=hostile;serial=index;rng=new System.Random((hostile?index:(index-20)/3)*73+session.RouteSeed);car=GetComponent<VehicleController>();Plan();}
 void Plan(){var graph=Session.World.Network;int from=graph.Nearest(car.Body.position);bool pursuit=Hostile&&Session.Elapsed>Session.Attacks.TrustedTrafficUntil;int target=pursuit?graph.Nearest(Session.Player.Body.position):rng.Next(3)!=0?graph.Hotspots[rng.Next(graph.Hotspots.Count)]:rng.Next(graph.Nodes.Count);if(!Hostile&&(serial-20)%3!=0){foreach(var other in Session.Cars){var member=other.GetComponent<TrafficAgent>();if(member!=null&&member!=this&&member.serial==20+PlatoonId*3){target=graph.Nearest(other.Body.position);break;}}}
 if(target==from)target=(from+1)%graph.Nodes.Count;points=graph.TravelPath(graph.Route(from,target,Hostile));waypoint=0;while(waypoint<points.Count-1&&Vector3.Distance(points[waypoint],car.Body.position)<12)waypoint++;}
 void FixedUpdate(){
 if(car==null||!Session.Running)return;containTimer-=Time.fixedDeltaTime;if(containTimer>0)return;containTimer=.15f;
 // Use the physical pose: the rendered transform deliberately trails it with interpolation.
 var graph=Session.World.Network;Vector3 position=car.Body.position;
 Vector3 road=graph.ClosestRoad(position,out Vector3 tangent,out float distance);float edge=graph.RoadWidth/2-1.5f;
 if(distance<=edge)return;
 if(distance>20){car.Recover(road,Quaternion.LookRotation(tangent));Plan();return;}
 Vector3 offset=Vector3.ProjectOnPlane(position-road,Vector3.up),clamped=road+Vector3.ClampMagnitude(offset,edge);clamped.y=road.y+.08f;
 car.Body.position=clamped;car.Body.linearVelocity=Vector3.Project(car.Body.linearVelocity,tangent);
 }
 void Update(){
 if(car==null||!Session.Running)return;
 if(waypoint>=points.Count)Plan();if(points.Count==0)return;
 bool pursuit=Hostile&&Session.Elapsed>Session.Attacks.TrustedTrafficUntil;Vector3 aim=points[Mathf.Min(waypoint,points.Count-1)];
 if(waypoint>0){Vector3 tangent=(aim-points[waypoint-1]).normalized;aim+=Vector3.Cross(Vector3.up,tangent)*2.7f;}
 if(Vector3.Distance(transform.position,aim)<6)waypoint++;
 if(pursuit&&Vector3.Distance(transform.position,Session.Player.transform.position)<23){Vector3 candidate=Session.Player.transform.position+Session.Player.Body.linearVelocity*.2f;Session.World.Network.ClosestRoad(candidate,out _,out float distance);if(distance<Session.World.Network.RoadWidth/2-1)aim=candidate;}
 Vector3 relative=transform.InverseTransformPoint(aim);float angle=Mathf.Atan2(relative.x,relative.z)*Mathf.Rad2Deg;
 car.Steer=Mathf.Clamp(angle/30,-1,1);car.MaxSpeed=pursuit?(Session.Attacks.Has(CyberAttack.Sybil)?31:22+Session.Difficulty*2):Session.World.Network.SpeedLimit-serial%3*.35f;car.Throttle=1;car.Brake=Mathf.Abs(angle)>35&&Mathf.Abs(car.Speed)>10;
 bool yield=Session.World.GetComponent<TrafficSignals>().MustStop(transform.position,transform.forward,Time.time)&&!pursuit;
 // Three-car groups share a route and maintain a speed-dependent gap. Congestion is physical.
 if(!Hostile&&serial%3!=2){foreach(var other in Session.Cars){if(other==car||other.IsPlayer)continue;var agent=other.GetComponent<TrafficAgent>();if(agent==null||agent.Hostile||agent.PlatoonId!=PlatoonId)continue;Vector3 local=transform.InverseTransformPoint(other.transform.position);if(local.z>0&&local.z<Mathf.Max(9,car.Speed*1.2f)&&Mathf.Abs(local.x)<3.5f)yield=true;}}

 foreach(var person in Session.Citizens)if(person&&person.IsCrossing){Vector3 p=transform.InverseTransformPoint(person.transform.position);if(p.z>0&&p.z<17&&Mathf.Abs(p.x)<4)yield=true;}
 if(Physics.Raycast(transform.position+Vector3.up,transform.forward,out var hit,Mathf.Max(9,car.Speed*.9f))&&hit.rigidbody!=null&&hit.rigidbody!=car.Body){if(!pursuit||hit.rigidbody.GetComponent<VehicleController>()!=Session.Player)yield=true;}
 // A replay campaign makes ordinary vehicles repeat stale emergency-stop commands.
 if(!Hostile&&Session.Attacks.Has(CyberAttack.Replay)&&Session.InHotspot)yield=true;
 Congested=yield;if(yield){car.Brake=true;car.Throttle=0;stuck=0;return;}
 if(Mathf.Abs(car.Speed)<.6f)stuck+=Time.deltaTime;else stuck=0;
 if(stuck>2){reverseUntil=Session.Elapsed+1.4f;stuck=0;}
 if(Session.Elapsed<reverseUntil){car.Throttle=-.6f;car.Steer=-car.Steer;car.Brake=false;}
 if(transform.position.y<-12){int n=Session.World.Network.Nearest(transform.position);car.Recover(Session.World.Network.Nodes[n],Quaternion.identity);Plan();}
 }
 }
}
