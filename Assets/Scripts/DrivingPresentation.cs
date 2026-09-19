using UnityEngine;
namespace CyberCar {
 // Built-in pipeline image effect. IMGUI is drawn afterwards, keeping navigation and text sharp.
 [RequireComponent(typeof(Camera))]
 public sealed class DrivingPresentation:MonoBehaviour {
 public GameSession Session;
 Material material;
 public bool Applied{get;private set;}
 public int BloomWidth{get;private set;}public int BloomHeight{get;private set;}
 static readonly int Bloom=Shader.PropertyToID("_BloomTex"),Direction=Shader.PropertyToID("_Direction"),Strength=Shader.PropertyToID("_BloomStrength"),Exposure=Shader.PropertyToID("_Exposure");
 void Awake(){GetComponent<Camera>().allowHDR=true;var shader=Resources.Load<Shader>("DrivingPresentation");if(shader&&shader.isSupported)material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};}
 void OnRenderImage(RenderTexture source,RenderTexture destination){
 if(!material){Graphics.Blit(source,destination);return;}
 int width=Mathf.Max(1,Mathf.Min(640,source.width/4)),height=Mathf.Max(1,Mathf.RoundToInt(width*(float)source.height/source.width));
 BloomWidth=width;BloomHeight=height;
 var format=SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)?RenderTextureFormat.ARGBHalf:RenderTextureFormat.Default;
 var a=RenderTexture.GetTemporary(width,height,0,format,RenderTextureReadWrite.Linear);var b=RenderTexture.GetTemporary(width,height,0,format,RenderTextureReadWrite.Linear);
 try{
 a.filterMode=b.filterMode=FilterMode.Bilinear;
 Graphics.Blit(source,a,material,0);
 material.SetVector(Direction,new Vector4(1f/width,0,0,0));Graphics.Blit(a,b,material,1);
 material.SetVector(Direction,new Vector4(0,1f/height,0,0));Graphics.Blit(b,a,material,1);
 material.SetTexture(Bloom,a);material.SetFloat(Strength,Session&&Session.Night?.13f:.045f);material.SetFloat(Exposure,Session&&Session.Night?1.12f:1.05f);
 Graphics.Blit(source,destination,material,2);Applied=true;
 }finally{material.SetTexture(Bloom,null);RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);}
 }
 void OnDestroy(){if(material)Destroy(material);}
 }
}
