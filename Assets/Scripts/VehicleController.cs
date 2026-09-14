using UnityEngine;

namespace CyberCar
{
    [RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
    public sealed class VehicleController:MonoBehaviour
    {
        public Rigidbody Body {get;private set;}
        public float Health {get;private set;}=100;
        public float Speed=>Body==null?0:Vector3.Dot(Body.linearVelocity,transform.forward);
        public bool IsPlayer;
        public bool Driving;
        public float Throttle,Steer;
        public bool Brake;
        public bool Drift;
        public bool IsDrifting {get;private set;}
        public float DriftSeconds {get;private set;}
        public float SteeringPolarity=1;
        public float MaxSpeed=29;
        public float ControlNoise;
        public float EngineLimit=1;
        public GameSession Session;
        public float LastImpact {get;private set;}
        Transform visual;
        float modelYaw;
        float hitCooldown;
        AudioSource engine,impact;
        readonly System.Collections.Generic.List<Transform> wheelPivots=new System.Collections.Generic.List<Transform>();
        readonly System.Collections.Generic.List<bool> frontWheels=new System.Collections.Generic.List<bool>();
        TrailRenderer[] skidTrails;
        float wheelSpin,smoothSteer,smoothThrottle,gripBlend;public bool Grounded{get;private set;}public bool Upright=>Vector3.Dot(transform.up,Vector3.up)>.35f;readonly RaycastHit[] groundHits=new RaycastHit[12];
        void Awake()
        {
            Body=GetComponent<Rigidbody>();Body.mass=1200;Body.linearDamping=.12f;Body.angularDamping=4;
            Body.interpolation=RigidbodyInterpolation.Interpolate;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            Body.constraints=RigidbodyConstraints.None;
            Body.centerOfMass=new Vector3(0,.2f,0);
            var collider=GetComponent<BoxCollider>();collider.center=new Vector3(0,.8f,0);collider.size=new Vector3(2.18f,1.35f,4.6f);
            var friction=new PhysicsMaterial("Vehicle grip"){dynamicFriction=.15f,staticFriction=.15f,bounciness=.08f};collider.material=friction;
        }
        public void SetupVisual(Material paint)
        {
            var model=Resources.Load<GameObject>("Art/CyberInterceptor");
            if(model!=null)
            {
                visual=Instantiate(model,transform).transform;visual.localPosition=Vector3.zero;modelYaw=180;
                visual.localRotation=Quaternion.Euler(0,modelYaw,0);
                foreach(var r in visual.GetComponentsInChildren<Renderer>())
                {
                    var mats=r.sharedMaterials;
                    for(int i=0;i<mats.Length;i++) if(mats[i]!=null&&mats[i].name.Contains("BodyPaint"))mats[i]=paint;
                    r.sharedMaterials=mats;
                }
                var parts=visual.GetComponentsInChildren<Transform>();
                foreach(var wheel in parts)if(wheel.name.StartsWith("Wheel"))
                {
                    var pivot=new GameObject("Animated wheel").transform;pivot.SetParent(visual);pivot.position=wheel.position;pivot.localRotation=Quaternion.identity;
                    frontWheels.Add(transform.InverseTransformPoint(wheel.position).z>0);wheelPivots.Add(pivot);
                    foreach(var part in parts)if((part.name.StartsWith("Wheel")||part.name.StartsWith("Hub")||part.name.StartsWith("Alloy spoke"))&&Vector3.Distance(part.position,pivot.position)<.65f)part.SetParent(pivot,true);
                }
            }
            else
            {
                visual=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;visual.SetParent(transform,false);
                visual.localPosition=Vector3.up*.8f;visual.localScale=new Vector3(2.1f,.8f,4.5f);
                Destroy(visual.GetComponent<Collider>());visual.GetComponent<Renderer>().sharedMaterial=paint;
            }
            gameObject.AddComponent<VehicleFeedback>().Initialize(this);
            if(IsPlayer)
            {
                engine=gameObject.AddComponent<AudioSource>();engine.clip=MakeTone(false);engine.loop=true;engine.volume=.08f;engine.Play();
                impact=gameObject.AddComponent<AudioSource>();impact.clip=MakeTone(true);impact.volume=.45f;
                skidTrails=new TrailRenderer[2];
                for(int i=0;i<2;i++){var trail=new GameObject("Drift tire trace");trail.transform.SetParent(transform,false);trail.transform.localPosition=new Vector3(i==0?-.88f:.88f,.16f,-1.45f);var t=trail.AddComponent<TrailRenderer>();t.sharedMaterial=Resources.Load<Material>("Impact");t.time=5;t.startWidth=.22f;t.endWidth=.2f;t.startColor=new Color(.03f,.03f,.03f,.6f);t.endColor=new Color(.03f,.03f,.03f,0);t.minVertexDistance=.2f;t.emitting=false;skidTrails[i]=t;}
            }
        }
        static AudioClip MakeTone(bool crash)
        {
            int length=crash?11025:22050;float[] samples=new float[length];var random=new System.Random(22);
            for(int i=0;i<length;i++)samples[i]=crash?(float)(random.NextDouble()*2-1)*Mathf.Exp(-i/1500f):Mathf.Sin(i*2*Mathf.PI*70/22050f)*.5f+Mathf.Sin(i*2*Mathf.PI*140/22050f)*.15f;
            var clip=AudioClip.Create(crash?"Impact":"Motor",length,1,22050,false);clip.SetData(samples,0);return clip;
        }
        void Update()
        {
            if(engine){engine.pitch=.55f+Mathf.Abs(Speed)/19;engine.volume=Driving?.045f+Mathf.Abs(Throttle)*.04f:0;}
            if(visual)visual.localRotation=Quaternion.Euler(Throttle*-1.5f,modelYaw,-Steer*Mathf.Clamp(Speed,-15,15)*.16f);
            wheelSpin-=Speed*Time.deltaTime/.48f*Mathf.Rad2Deg;
            for(int i=0;i<wheelPivots.Count;i++)wheelPivots[i].localRotation=Quaternion.Euler(wheelSpin,frontWheels[i]?Steer*22:0,0);
            if(skidTrails!=null)foreach(var trail in skidTrails)trail.emitting=Driving&&IsDrifting;
        }
        void FixedUpdate()
        {
            if(!Driving)return;
            float dt=Time.fixedDeltaTime;Vector3 normal=Vector3.zero;int contacts=0;
            // Four sprung tire contact points support pitch, roll, grades and airborne motion.
            foreach(float x in new[]{-.82f,.82f})foreach(float z in new[]{-1.5f,1.5f}){
                Vector3 origin=transform.TransformPoint(new Vector3(x,.65f,z));
                int count=Physics.RaycastNonAlloc(origin,-transform.up,groundHits,1.15f,~0,QueryTriggerInteraction.Ignore);float nearest=float.MaxValue;RaycastHit ground=default;
                for(int i=0;i<count;i++)if(groundHits[i].rigidbody!=Body&&groundHits[i].normal.y>.25f&&groundHits[i].distance<nearest){nearest=groundHits[i].distance;ground=groundHits[i];}
                if(nearest==float.MaxValue)continue;contacts++;normal+=ground.normal;
                float compression=.82f-nearest,velocity=Vector3.Dot(Body.GetPointVelocity(origin),ground.normal);
                float spring=Mathf.Clamp(compression*72-velocity*7,-2,32);
                Body.AddForceAtPosition(ground.normal*spring*.25f,origin,ForceMode.Acceleration);
            }
            Grounded=contacts>0;if(!Grounded){IsDrifting=false;return;}
            normal.Normalize();float speed=Speed;
            Vector3 forward=Vector3.ProjectOnPlane(transform.forward,normal).normalized,right=Vector3.Cross(normal,forward);
            smoothSteer=Mathf.MoveTowards(smoothSteer,Steer,dt*(Drift?2.5f:3.5f));smoothThrottle=Mathf.MoveTowards(smoothThrottle,Throttle,dt*5);
            IsDrifting=Drift&&Mathf.Abs(speed)>8&&Mathf.Abs(smoothSteer)>.12f;
            gripBlend=Mathf.MoveTowards(gripBlend,IsDrifting?1:0,dt*(IsDrifting?2:1.8f));
            float tireUpgrade=IsPlayer?ProgressStore.Upgrade(0)*.5f:0;
            float grip=Mathf.Lerp(7.5f+tireUpgrade,1.55f,gripBlend);
            Body.AddForce(-right*Vector3.Dot(Body.linearVelocity,right)*grip,ForceMode.Acceleration);
            if(IsDrifting)DriftSeconds+=dt;
            bool opposite=smoothThrottle*speed<0&&Mathf.Abs(speed)>1;
            float acceleration=opposite?21:13;
            if((smoothThrottle>0&&speed<MaxSpeed*EngineLimit)||(smoothThrottle<0&&speed>-10))Body.AddForce(forward*smoothThrottle*acceleration*EngineLimit,ForceMode.Acceleration);
            if(Mathf.Abs(Throttle)<.01f)Body.AddForce(-forward*speed*.45f,ForceMode.Acceleration);
            if(Brake||(IsPlayer&&Session!=null&&Session.ReplayBraking))Body.AddForce(-Vector3.ProjectOnPlane(Body.linearVelocity,normal)*4.5f,ForceMode.Acceleration);
            float steering=smoothSteer*SteeringPolarity+ControlNoise*Mathf.Sin(Time.time*3.1f);
            float turn=steering*Mathf.Sign(speed)*Mathf.Clamp01(Mathf.Abs(speed)/3)*Mathf.Lerp(78,35,Mathf.Clamp01(Mathf.Abs(speed)/MaxSpeed))*(1+gripBlend*.2f);
            Body.MoveRotation(Quaternion.AngleAxis(turn*dt,normal)*Body.rotation);
            Vector3 tilt=Vector3.Cross(transform.up,normal);
            Body.AddTorque(tilt*28-Vector3.ProjectOnPlane(Body.angularVelocity,normal)*5,ForceMode.Acceleration);
        }
        public void Damage(float amount)
        {
            Health=Mathf.Max(0,Health-amount*(IsPlayer?1-ProgressStore.Upgrade(1)*.1f:1));
            if(visual)visual.localScale=new Vector3(1,1,Mathf.Lerp(.87f,1,Health/100));
        }
        void OnCollisionEnter(Collision collision)
        {
            if(!Driving||Time.time<hitCooldown)return;
            float force=collision.relativeVelocity.magnitude;
            if(force<3.5f||Mathf.Abs(collision.GetContact(0).normal.y)>.7f)return;
            hitCooldown=Time.time+.5f;LastImpact=Time.time;Damage((force-3)*1.65f);
            if(IsPlayer)
            {
                Session?.OnCrash(force);if(impact)impact.Play();
                WorldBuilder.Sparks(collision.GetContact(0).point);
            }
        }
        public void Recover(Vector3 position,Quaternion rotation)
        {
            IsDrifting=false;Drift=false;smoothSteer=0;smoothThrottle=0;gripBlend=0;if(skidTrails!=null)foreach(var trail in skidTrails)trail.Clear();
            Body.position=position+Vector3.up*.08f;Body.rotation=rotation;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;
        }
    }
}
