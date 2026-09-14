using UnityEngine;
namespace CyberCar {
 public sealed class CarSensors:MonoBehaviour {
 VehicleController car;Material material;readonly RaycastHit[] hits=new RaycastHit[20];float next;
 public float ActualRange{get;private set;}=80;public float CameraRange{get;private set;}=80;public float BumperRange{get;private set;}=8;
 public float ReportedRange=>car.Session.Attacks.Has(CyberAttack.SensorAttack)?2:ActualRange;
 public bool FalseEmergencyBrake=>car.Session.Attacks.Has(CyberAttack.SensorAttack)&&Mathf.Sin(car.Session.Elapsed*.8f)>.65f;
 public string Readout=>"Range "+ReportedRange.ToString("0.0")+" m / camera "+(CameraRange>=79?"clear":CameraRange.ToString("0.0")+" m")+" / bumper "+(BumperRange>=7.9f?"clear":BumperRange.ToString("0.0")+" m");
 void Awake(){car=GetComponent<VehicleController>();material=new Material(Shader.Find("Standard")){color=new Color(.1f,.2f,.24f)};
 foreach(float x in new[]{-.7f,0,.7f}){var sensor=GameObject.CreatePrimitive(PrimitiveType.Sphere);sensor.name="Bumper sensor housing";sensor.transform.SetParent(transform,false);sensor.transform.localPosition=new Vector3(x,.55f,2.27f);sensor.transform.localScale=new Vector3(.16f,.12f,.08f);sensor.GetComponent<Renderer>().sharedMaterial=material;Destroy(sensor.GetComponent<Collider>());}}
 float Measure(Vector3 origin,float range){float best=range;int count=Physics.SphereCastNonAlloc(origin,.12f,transform.forward,hits,range,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<count;i++)if(hits[i].rigidbody!=car.Body)best=Mathf.Min(best,hits[i].distance);return best;}
 void Update(){if(!car.Session||!car.Session.Running)return;next-=Time.deltaTime;if(next>0)return;next=.1f;ActualRange=Measure(transform.TransformPoint(new Vector3(0,.7f,2.6f)),80);CameraRange=Measure(transform.TransformPoint(new Vector3(0,1.3f,2.6f)),80);BumperRange=Measure(transform.TransformPoint(new Vector3(.7f,.55f,2.6f)),8);}
 void OnDestroy(){if(material)Destroy(material);}
 }
}
