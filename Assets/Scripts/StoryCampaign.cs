using System.Collections.Generic;
namespace CyberCar {
 public static class StoryCampaign {
 public static readonly string[] Titles={"Courier induction","The altered dispatch","Harbor under pressure","A passenger on the bus","The locked manifest","False friends","Yesterday's emergency","The lighthouse update","City without lights","Roadside silence","Under a silent sky","The poisoned playlist","A familiar destination","Seeing double","Operation homecoming"};
 public static readonly int[] Maps={0,0,1,2,1,0,1,3,0,0,2,1,0,3,3};
 public static readonly string[] Story={
 "Join the municipal response team. Collect your credential at Training Depot, then report to dispatch. Learn braking, reversing and crosswalk safety.",
 "Dispatch needs a clean route certificate at Security Campus. A forged detour leads toward a condemned service road. Compare navigation with the signed manifest.",
 "Deliver network filters to Harbor Control through the cargo interchange. A request flood overwhelms the service. Authenticate a nearby roadside unit and keep your cached roads.",
 "Carry a gateway isolator to Summit Observatory. An entertainment controller is sending steering commands. Separate the compromised source from driver controls.",
 "Bring the harbor's offline backup to its recovery team. Encrypted files require isolation, backup integrity verification and the correct protected recovery key.",
 "Escort a delivery through the downtown one-way district. Fake vehicle identities coordinate a convoy. Inspect certificates while anticipating blocked intersections.",
 "Carry a freshness policy to Harbor Control. Old emergency messages trap traffic. Accept current sequences and reject stale instructions.",
 "Transport a signed recovery image to the lighthouse. Verify the update before crossing the cliffs. A service bridge is visibly unfinished; it cannot be driven across.",
 "Restore the city's emergency lighting schedule. Keep using your headlights while you revoke the remote controller and re-enable trusted local operation.",
 "Reach Security Campus with new RSU credentials. Roadside sessions have been severed. Drive into short-range coverage and authenticate a fresh handshake.",
 "The observatory reports regional GNSS interference. Reach it using trusted roadside references and wheel odometry between units. A map is still useful without a live satellite fix.",
 "Deliver a clean audio package to Harbor Control. A malicious playlist replaces your music with duck noises. Quarantine the payload and reload data-only, verified media.",
 "An old saved journey points to the wrong destination. Deliver the dispatch manifest to Security Campus. Reject command fields; compare the actual destination with the signed order.",
 "A phantom obstacle feed threatens the lighthouse route. Cross-check independent camera and bumper readings before returning the vehicle to safe degraded operation.",
 "Carry the recovered evidence to the lighthouse response team. Combined attacks now test your earlier lessons. Manage communications, sensors and recovery while crossing the coastal pass."
 };
 public static List<int> Route(RoadNetwork g,int level){
 if(level==0)return g.MissionRoute(1,true);
 g.Names[g.Hotspots[0]]=g.Map==1?"Cargo inspection":"Medical relay";
 g.Names[g.Hotspots[1]]="Emergency exchange";g.Names[g.Hotspots[2]]="Response staging";
 var result=new List<int>{0};int current=0;
 int[] stops=level<3?new[]{g.Hotspots[0],g.Finish}:level<9?new[]{g.Hotspots[0],g.Hotspots[1],g.Finish}:new[]{g.Hotspots[0],g.Hotspots[2],g.Hotspots[1],g.Finish};
 foreach(int stop in stops){var part=g.Route(current,stop);if(part.Count>1){part.RemoveAt(0);result.AddRange(part);}current=stop;}return result;
 }
 }
}
