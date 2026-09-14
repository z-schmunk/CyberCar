using UnityEngine;
namespace CyberCar {
 public sealed class DriveMusic:MonoBehaviour {
 public GameSession Session;AudioSource music,duck;AudioClip quack;
 void Start(){music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/CoastalDrive");music.loop=true;music.volume=ProgressStore.MusicVolume*.4f;if(music.clip)music.Play();duck=gameObject.AddComponent<AudioSource>();duck.loop=true;
 int rate=22050;float[] wave=new float[rate*2];for(int i=0;i<wave.Length;i++){float t=i/(float)rate,beat=t%.6f,envelope=beat<.24f?Mathf.Sin(beat/.24f*Mathf.PI):0;float phase=2*Mathf.PI*(440*t-110*beat*beat);wave[i]=envelope*(Mathf.Sin(phase)+.4f*Mathf.Sin(phase*3))*.2f;}
 quack=AudioClip.Create("Injected duck loop - original synthesis",wave.Length,1,rate,false);quack.SetData(wave,0);duck.clip=quack;duck.Play();}
 void Update(){if(!music)return;bool injected=Session.Running&&Session.Attacks.Has(CyberAttack.MusicInjection);float volume=ProgressStore.MusicVolume*(injected?0:Session.Running?1:.45f);duck.volume=injected?ProgressStore.MusicVolume:0;music.volume=Mathf.Lerp(music.volume,volume,Time.unscaledDeltaTime*3);if(Input.GetKeyDown(KeyCode.M)&&Session.InputEnabled)ProgressStore.MusicVolume=ProgressStore.MusicVolume>0?0:.22f;}
 void OnDestroy(){if(quack)Destroy(quack);}
 }
}