using UnityEngine;
using UnityEngine.Rendering;
namespace CyberCar {
 public sealed class SceneReflections:MonoBehaviour {
 GameSession session;ReflectionProbe probe;Vector3 previous;float next;int render=-1;bool night,blackout;
 public int Captures{get;private set;}
 public void Initialize(GameSession game){session=game;probe=GetComponent<ReflectionProbe>();probe.resolution=128;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.cullingMask=~(1<<8);next=Time.time+.6f;previous=Vector3.one*9999;}
 void LateUpdate(){if(!session||!session.Player||Time.time<next)return;if(render>=0&&!probe.IsFinishedRendering(render))return;
 bool changed=night!=session.Night||blackout!=session.World.Environment.Blackout;Vector3 position=session.Player.Body.position;
 if(Captures>0&&!changed&&(position-previous).sqrMagnitude<75*75)return;
 previous=position;night=session.Night;blackout=session.World.Environment.Blackout;transform.position=position+Vector3.up*4;render=probe.RenderProbe();Captures++;next=Time.time+8;
 }
 }
}
