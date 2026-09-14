using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public sealed class RoadsideNetwork {
 public const float Range=115;readonly GameSession session;public readonly List<Vector3> Units=new List<Vector3>();
 public bool SatelliteFallback;public bool InRange=>NearestDistance<=Range;
 public float NearestDistance{get;private set;}public string NearestName{get;private set;}="none";
 public bool Connected=>InRange&&!session.Attacks.Has(CyberAttack.RsuLoss);
 public bool SatelliteAvailable=>!SatelliteFallback&&!session.Attacks.Has(CyberAttack.SatelliteLoss);
 public Vector3 EstimatedPosition{get;private set;}public float Uncertainty{get;private set;}
 public string Source=>SatelliteAvailable?"GPS":Connected?"RSU / DSRC":"ODOMETRY / ESTIMATED";
 public RoadsideNetwork(GameSession game){session=game;var world=game.World;var steel=world.Mat("RSU mast",new Color(.25f,.28f,.3f),.5f);var blue=world.Mat("RSU status",new Color(.06f,.6f,.9f),0,true);
 for(int n=0;n<world.Network.Nodes.Count;n+=2){var node=world.Network.Nodes[n];Vector3 pos=node+new Vector3(-world.Network.RoadWidth/2-4,0,-world.Network.RoadWidth/2-4);float ground=world.GroundHeight(pos);pos.y=ground;Units.Add(pos);
 world.Box("RSU footing",pos+Vector3.up*.25f,new Vector3(.9f,.5f,.9f),steel);
 world.Box("DSRC roadside mast",pos+Vector3.up*2.75f,new Vector3(.18f,5.5f,.18f),steel);
 world.Box("DSRC transceiver",pos+Vector3.up*5,new Vector3(.7f,.65f,.3f),blue);
 world.Sign(pos+new Vector3(1,0,0),"RSU "+Units.Count.ToString("00"),blue);
 }
 }
 public void Tick(float dt){if(!session.Player)return;Vector3 position=session.Player.Body.position;NearestDistance=float.MaxValue;for(int i=0;i<Units.Count;i++){float d=Vector3.Distance(position,Units[i]);if(d<NearestDistance){NearestDistance=d;NearestName=(i+1).ToString("00");}}
 if(SatelliteAvailable||Connected){EstimatedPosition=position;Uncertainty=0;}else{EstimatedPosition+=session.Player.transform.forward*session.Player.Speed*dt+Vector3.right*.08f*dt;Uncertainty+=dt*.35f;}
 }
 }
}
