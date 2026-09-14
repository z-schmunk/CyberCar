using UnityEngine;
namespace CyberCar {
 public sealed class LabCourier:MonoBehaviour {
 public GameSession Session;VehicleController car;int routeIndex=-1,point;Vector3[] path;
 void Start(){car=GetComponent<VehicleController>();}
 void Update(){if(!car||!Session.Running)return;
 if(routeIndex!=Session.RouteIndex){routeIndex=Session.RouteIndex;path=Session.World.Network.Path(Session.Route[routeIndex-1],Session.Route[routeIndex]);point=1;}
 Vector3 aim=path[Mathf.Min(point,path.Length-1)];if(Vector3.Distance(transform.position,aim)<7&&point<path.Length-1)point++;
 if(Session.Attacks.Has(CyberAttack.Spoofing)||Session.Attacks.Has(CyberAttack.DestinationInjection)){var graph=Session.World.Network;int fork=graph.Columns-1;if(Vector3.Distance(transform.position,graph.Nodes[fork])<80)aim=graph.Hazard;else{var diversion=graph.TravelPath(graph.Route(graph.Nearest(transform.position),fork));if(diversion.Count>2)aim=diversion[Mathf.Min(5,diversion.Count-1)];}}
 Vector3 relative=transform.InverseTransformPoint(aim);float angle=Mathf.Atan2(relative.x,relative.z)*Mathf.Rad2Deg;car.Steer=Mathf.Clamp(angle/30,-1,1);car.Throttle=car.Speed<12?1:0;car.Brake=Mathf.Abs(angle)>35&&car.Speed>8;
 }
 }
}
