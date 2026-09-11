using UnityEngine;
namespace CyberCar {
 public sealed class ChaseCamera:MonoBehaviour {
 public GameSession Session;bool initialized;Transform previousCar;Camera view;
 readonly RaycastHit[] hits=new RaycastHit[24];
 void Awake(){view=GetComponent<Camera>();view.nearClipPlane=.15f;}
 Vector3 AvoidObstacles(Vector3 anchor,Vector3 desired,Rigidbody player){
 Vector3 delta=desired-anchor;float distance=delta.magnitude;if(distance<.01f)return desired;
 int count=Physics.SphereCastNonAlloc(anchor,.3f,delta/distance,hits,distance,~0,QueryTriggerInteraction.Ignore);
 float safe=distance;
 for(int i=0;i<count;i++)if(hits[i].rigidbody!=player)safe=Mathf.Min(safe,Mathf.Max(.5f,hits[i].distance-.15f));
 return anchor+delta/distance*safe;
 }
 void LateUpdate(){
 if(Session==null||Session.Player==null)return;
 var player=Session.Player;var car=player.transform;if(previousCar!=car){previousCar=car;initialized=false;}
 Vector3 anchor=car.position+Vector3.up*1.4f;
 Vector3 wanted=car.position+car.rotation*new Vector3(0,4.7f,-10.2f);
 if(Session.State==GameState.Menu)wanted=car.position+new Vector3(-9,5,10);
 wanted=AvoidObstacles(anchor,wanted,player.Body);
 Vector3 smoothed=initialized?Vector3.Lerp(transform.position,wanted,1-Mathf.Exp(-6*Time.unscaledDeltaTime)):wanted;
 transform.position=AvoidObstacles(anchor,smoothed,player.Body);initialized=true;
 transform.LookAt(car.position+car.forward*3+Vector3.up);
 float fov=62+(Session.State==GameState.Driving?Mathf.Clamp01(Mathf.Abs(player.Speed)/29)*6:0);
 view.fieldOfView=Mathf.Lerp(view.fieldOfView,fov,1-Mathf.Exp(-3*Time.unscaledDeltaTime));
 }
 }
}
