using UnityEngine;
namespace CyberCar {
 public sealed class PedestrianAgent:MonoBehaviour {
 GameSession session;CrossingSite site;float offset,immuneUntil;Transform leftLeg,rightLeg,leftArm,rightArm;
 public bool IsCrossing{get;private set;}
 public void Initialize(GameSession game,CrossingSite crossing,int index){
 session=game;site=crossing;offset=index*.65f;
 Vector3 side=Vector3.Cross(Vector3.up,site.Forward);transform.position=site.Center+side*(site.Width/2+2);
 var body=gameObject.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
 var collider=gameObject.AddComponent<CapsuleCollider>();collider.center=Vector3.up*.9f;collider.height=1.8f;collider.radius=.3f;collider.isTrigger=true;
 var clothing=session.World.Mat("Citizen jacket "+index,Color.HSVToRGB((index*.173f)%1,.45f,.52f));
 var skin=session.World.Mat("Skin "+index,Color.Lerp(new Color(.32f,.19f,.12f),new Color(.72f,.51f,.37f),(index%4)/3f));
 var trousers=session.World.Mat("Trousers",new Color(.09f,.12f,.15f));
 Part("Torso",new Vector3(0,1.12f,0),new Vector3(.43f,.55f,.24f),clothing,PrimitiveType.Capsule);
 Part("Head",new Vector3(0,1.65f,0),Vector3.one*.29f,skin,PrimitiveType.Sphere);
 leftLeg=Part("Left leg",new Vector3(-.12f,.42f,0),new Vector3(.14f,.42f,.16f),trousers,PrimitiveType.Capsule);
 rightLeg=Part("Right leg",new Vector3(.12f,.42f,0),new Vector3(.14f,.42f,.16f),trousers,PrimitiveType.Capsule);
 leftArm=Part("Left arm",new Vector3(-.28f,1.06f,0),new Vector3(.13f,.33f,.14f),clothing,PrimitiveType.Capsule);
 rightArm=Part("Right arm",new Vector3(.28f,1.06f,0),new Vector3(.13f,.33f,.14f),clothing,PrimitiveType.Capsule);
 }
 Transform Part(string name,Vector3 p,Vector3 scale,Material mat,PrimitiveType shape){var o=GameObject.CreatePrimitive(shape);o.name=name;o.transform.SetParent(transform,false);o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;Destroy(o.GetComponent<Collider>());return o.transform;}
 void Update(){
 if(session==null||!session.Running)return;float cycle=(session.Elapsed+site.Phase+offset)%30;Vector3 side=Vector3.Cross(Vector3.up,site.Forward);float span=site.Width/2+2;
 IsCrossing=cycle>=5&&cycle<19&&session.Elapsed>immuneUntil;
 float t=IsCrossing?(cycle-5)/14:cycle>=19?1:0;transform.position=site.Center+side*Mathf.Lerp(-span,span,t)+site.Forward*offset;transform.rotation=Quaternion.LookRotation(side);
 float stride=IsCrossing?Mathf.Sin(session.Elapsed*8)*24:0;leftLeg.localRotation=Quaternion.Euler(stride,0,0);rightLeg.localRotation=Quaternion.Euler(-stride,0,0);leftArm.localRotation=Quaternion.Euler(-stride,0,0);rightArm.localRotation=Quaternion.Euler(stride,0,0);
 }
 void OnTriggerEnter(Collider other){if(session==null||!session.Running||session.Elapsed<immuneUntil)return;var vehicle=other.GetComponentInParent<VehicleController>();if(vehicle==null||!vehicle.IsPlayer||Mathf.Abs(vehicle.Speed)<2)return;immuneUntil=session.Elapsed+8;IsCrossing=false;session.OnCivilianStrike();}
 }
}