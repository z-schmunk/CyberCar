using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public sealed class VehicleFeedback:MonoBehaviour {
 VehicleController car;Material brakeMaterial,headlampMaterial,smokeMaterial;AudioSource tires;AudioClip tireClip;
 readonly List<ParticleSystem> smoke=new List<ParticleSystem>();bool audioPaused;
 readonly List<Light> headlights=new List<Light>();
 public bool BrakeLightsActive{get;private set;}public bool DriftFeedbackActive{get;private set;}
 public void Initialize(VehicleController owner) {
 car=owner;
 // Explicit resource shader retains emission in players without runtime Standard variants.
 var lampShader=Resources.Load<Shader>("VehicleLamp");
 brakeMaterial=new Material(lampShader){name="Responsive rear lamps"};
 headlampMaterial=new Material(lampShader){name="Independent running lamps",color=new Color(.86f,.93f,1)};
 foreach(var r in GetComponentsInChildren<Renderer>()){
 var materials=r.sharedMaterials;
 for(int i=0;i<materials.Length;i++)if(materials[i]){
 if(materials[i].name.Contains("Taillight")||r.name.StartsWith("Taillight")||r.name=="Rear light bar")materials[i]=brakeMaterial;
 else if(materials[i].name.Contains("Headlight"))materials[i]=headlampMaterial;
 }r.sharedMaterials=materials;
 }
 if(!car.IsPlayer)return;
 for(int i=0;i<2;i++){var beam=new GameObject("Independent headlight");beam.transform.SetParent(transform,false);beam.transform.localPosition=new Vector3(i==0?-.72f:.72f,.92f,2.4f);beam.transform.localRotation=Quaternion.Euler(7,0,0);var light=beam.AddComponent<Light>();light.type=LightType.Spot;light.range=85;light.spotAngle=55;light.innerSpotAngle=30;light.intensity=1.25f;light.color=new Color(.86f,.93f,1);light.shadows=LightShadows.Soft;light.shadowBias=.02f;headlights.Add(light);}
 smokeMaterial=new Material(Resources.Load<Shader>("TireSmoke"));
 for(int i=0;i<2;i++){
 var o=new GameObject("Rear tire smoke");o.transform.SetParent(transform,false);o.transform.localPosition=new Vector3(i==0?-.88f:.88f,.28f,-1.45f);
 var p=o.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
 var main=p.main;main.loop=true;main.duration=2;main.maxParticles=55;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startLifetime=new ParticleSystem.MinMaxCurve(.6f,1.3f);main.startSpeed=.65f;main.startSize=new ParticleSystem.MinMaxCurve(.25f,.65f);main.startColor=new Color(.66f,.67f,.68f,.3f);main.gravityModifier=-.08f;
 var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.13f;
 var emission=p.emission;emission.rateOverTime=0;
 var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(1,1.8f)));
 var color=p.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.75f,0),new GradientAlphaKey(0,1)});color.color=gradient;
 p.GetComponent<ParticleSystemRenderer>().sharedMaterial=smokeMaterial;p.Play();smoke.Add(p);
 }
 float[] samples=new float[22050];var rng=new System.Random(703);float previous=0;
 for(int i=0;i<samples.Length;i++){float n=(float)rng.NextDouble()*2-1;float hiss=n-previous;previous=n;samples[i]=hiss*.14f+Mathf.Sin(i*2*Mathf.PI*1730/22050f)*.17f+Mathf.Sin(i*2*Mathf.PI*2110/22050f)*.09f;}
 tireClip=AudioClip.Create("Original tire friction",samples.Length,1,22050,false);tireClip.SetData(samples,0);
 tires=gameObject.AddComponent<AudioSource>();tires.clip=tireClip;tires.loop=true;tires.volume=0;tires.Play();
 }
 void LateUpdate(){
 if(!car)return;bool driving=car.Driving&&car.Session!=null&&car.Session.Running;
 bool night=car.Session!=null&&car.Session.Night;
 foreach(var headlight in headlights)headlight.enabled=night;
 BrakeLightsActive=driving&&(car.Brake||(car.Throttle<0&&car.Speed>1)||(car.IsPlayer&&car.Session.ReplayBraking));
 brakeMaterial.color=BrakeLightsActive?new Color(1,.025f,.012f):night?new Color(.7f,.02f,.012f):new Color(.25f,.018f,.012f);
 brakeMaterial.SetColor("_EmissionColor",new Color(1,.015f,.006f)*(BrakeLightsActive?3.2f:night?1.1f:.16f));
 headlampMaterial.SetColor("_EmissionColor",new Color(.86f,.93f,1)*(night?2.5f:.25f));
 DriftFeedbackActive=driving&&car.IsDrifting;
 foreach(var p in smoke){var e=p.emission;e.rateOverTime=DriftFeedbackActive?26:0;}
 if(tires){bool pause=car.Session.Paused;if(pause!=audioPaused){if(pause)tires.Pause();else tires.UnPause();audioPaused=pause;}tires.volume=Mathf.MoveTowards(tires.volume,DriftFeedbackActive?.11f:0,Time.unscaledDeltaTime*.7f);tires.pitch=.85f+Mathf.Clamp01(Mathf.Abs(car.Speed)/30)*.3f;}
 }
 void OnDisable(){if(tires)tires.Stop();}
 void OnDestroy(){if(brakeMaterial)Destroy(brakeMaterial);if(headlampMaterial)Destroy(headlampMaterial);if(smokeMaterial)Destroy(smokeMaterial);if(tireClip)Destroy(tireClip);}
 }
}
