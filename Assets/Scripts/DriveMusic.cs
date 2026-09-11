using UnityEngine;
namespace CyberCar {
 public sealed class DriveMusic:MonoBehaviour {
 public GameSession Session;AudioSource music;
 void Start(){music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/CoastalDrive");music.loop=true;music.volume=ProgressStore.MusicVolume*.4f;if(music.clip)music.Play();}
 void Update(){if(!music)return;float volume=ProgressStore.MusicVolume*(Session.Running?1:.45f);music.volume=Mathf.Lerp(music.volume,volume,Time.unscaledDeltaTime*3);if(Input.GetKeyDown(KeyCode.M)&&Session.InputEnabled)ProgressStore.MusicVolume=ProgressStore.MusicVolume>0?0:.22f;}
 }
}