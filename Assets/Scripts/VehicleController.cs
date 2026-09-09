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
        public float MaxSpeed=29;
        public float ControlNoise;
        public float EngineLimit=1;
        public GameSession Session;
        public float LastImpact {get;private set;}
        Transform visual;
        float modelYaw;
        float hitCooldown;
        AudioSource engine,impact;
        void Awake()
        {
            Body=GetComponent<Rigidbody>();Body.mass=1200;Body.linearDamping=.12f;Body.angularDamping=4;
            Body.interpolation=RigidbodyInterpolation.Interpolate;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            Body.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
            Body.centerOfMass=new Vector3(0,.2f,0);
            var collider=GetComponent<BoxCollider>();collider.center=new Vector3(0,.8f,0);collider.size=new Vector3(2.18f,1.35f,4.6f);
            var friction=new PhysicsMaterial("Vehicle grip"){dynamicFriction=.15f,staticFriction=.15f,bounciness=.18f};collider.material=friction;
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
            }
            else
            {
                visual=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;visual.SetParent(transform,false);
                visual.localPosition=Vector3.up*.8f;visual.localScale=new Vector3(2.1f,.8f,4.5f);
                Destroy(visual.GetComponent<Collider>());visual.GetComponent<Renderer>().sharedMaterial=paint;
            }
            if(IsPlayer)
            {
                engine=gameObject.AddComponent<AudioSource>();engine.clip=MakeTone(false);engine.loop=true;engine.volume=.08f;engine.Play();
                impact=gameObject.AddComponent<AudioSource>();impact.clip=MakeTone(true);impact.volume=.45f;
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
        }
        void FixedUpdate()
        {
            if(!Driving)return;
            bool grounded=Physics.Raycast(transform.position+Vector3.up*.15f,Vector3.down,.55f);
            if(!grounded)return;
            float speed=Speed;
            Vector3 lateral=transform.right*Vector3.Dot(Body.linearVelocity,transform.right);
            Body.AddForce(-lateral*(Brake?3:8),ForceMode.Acceleration);
            if((Throttle>0&&speed<MaxSpeed*EngineLimit)||(Throttle<0&&speed>-9))Body.AddForce(transform.forward*Throttle*13*EngineLimit,ForceMode.Acceleration);
            if(Mathf.Abs(Throttle)<.01f)Body.AddForce(-transform.forward*speed*.45f,ForceMode.Acceleration);
            if(Brake)Body.AddForce(-Body.linearVelocity*3.8f,ForceMode.Acceleration);
            float steering=Steer+ControlNoise*Mathf.Sin(Time.time*3.1f);
            float turn=steering*Mathf.Sign(speed)*Mathf.Clamp01(Mathf.Abs(speed)/4)*Mathf.Lerp(85,40,Mathf.Abs(speed)/MaxSpeed);
            Body.MoveRotation(Body.rotation*Quaternion.Euler(0,turn*Time.fixedDeltaTime,0));
        }
        public void Damage(float amount)
        {
            Health=Mathf.Max(0,Health-amount);
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
            Body.position=position+Vector3.up*.08f;Body.rotation=rotation;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;
        }
    }
}
