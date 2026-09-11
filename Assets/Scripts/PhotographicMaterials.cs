using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public sealed class PhotographicMaterials:MonoBehaviour {
 readonly Dictionary<string,Material> cache=new Dictionary<string,Material>();
 Material lampBulb,lampGlass;
 public void SetLampPower(bool enabled){if(lampBulb)lampBulb.SetColor("_EmissionColor",enabled?new Color(1,.65f,.28f)*2:Color.black);}
 public Material Surface(string asset,float scale=4,bool world=true){
 string key=asset+"/"+scale+"/"+world;if(cache.TryGetValue(key,out var m))return m;
 string prefix="Photographic/"+asset;bool model=asset=="rock_face_01"||asset.StartsWith("pine_sapling_medium")||asset=="street_lamp_01";
 m=new Material(Resources.Load<Shader>("PhotoSurface")){name="Photographed "+asset,enableInstancing=true};
 m.SetTexture("_MainTex",Required(prefix+(model?"_diff_1k":"_diffuse")));
 m.SetTexture("_NormalTex",Required(prefix+(model?"_nor_gl_1k":"_normal")));
 m.SetTexture("_RoughTex",Required(prefix+(model?"_arm_1k":"_rough")));
 m.SetFloat("_Scale",scale);m.SetFloat("_Triplanar",world?1:0);m.SetFloat("_NormalStrength",asset=="asphalt_02"?.35f:.7f);
 m.SetFloat("_PackedARM",model?1:0);
 cache[key]=m;return m;
 }
 Texture2D Required(string path){var t=Resources.Load<Texture2D>(path);if(!t)throw new System.InvalidOperationException("Missing photographed asset "+path);return t;}
 public GameObject Prop(string name,Vector3 position,float height,float yaw=0){
 var source=Resources.Load<GameObject>("Photographic/Models/"+name);if(!source)throw new System.InvalidOperationException("Missing prop "+name);
 var o=Instantiate(source,transform);o.name=name;o.transform.localRotation=Quaternion.Euler(0,yaw,0)*o.transform.localRotation;
 var renderers=o.GetComponentsInChildren<Renderer>();Bounds b=new Bounds();bool first=true;
 foreach(var r in renderers){if(first){b=r.bounds;first=false;}else b.Encapsulate(r.bounds);}
 float scale=height/Mathf.Max(.1f,b.size.y);o.transform.localScale*=scale;o.transform.position=position;
 foreach(var r in renderers){var old=r.sharedMaterials;bool foliage=false;for(int i=0;i<old.Length;i++){string mat=old[i]?old[i].name:"";foliage|=mat.Contains("twig");string asset=name=="RockFace"?"rock_face_01":name=="StreetLamp"?"street_lamp_01":mat.Contains("twig")?"pine_sapling_medium_twig":"pine_sapling_medium_bark";old[i]=Surface(asset,1,false);
 if(name=="StreetLamp"&&mat.Contains("bulb")){if(!lampBulb){lampBulb=new Material(Shader.Find("Standard")){color=new Color(.3f,.27f,.2f)};lampBulb.EnableKeyword("_EMISSION");}old[i]=lampBulb;}
 if(name=="StreetLamp"&&mat.Contains("glass")){if(!lampGlass){lampGlass=new Material(Shader.Find("Standard")){color=new Color(.7f,.8f,.9f,.1f),renderQueue=3000};lampGlass.SetFloat("_Mode",3);lampGlass.SetInt("_SrcBlend",1);lampGlass.SetInt("_DstBlend",10);lampGlass.SetInt("_ZWrite",0);lampGlass.EnableKeyword("_ALPHAPREMULTIPLY_ON");}old[i]=lampGlass;}
 }r.sharedMaterials=old;var filter=r.GetComponent<MeshFilter>();if(filter&&!foliage)r.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;}
 return o;
 }
 void OnDestroy(){foreach(var m in cache.Values)if(m)Destroy(m);if(lampBulb)Destroy(lampBulb);if(lampGlass)Destroy(lampGlass);}
 }
}
