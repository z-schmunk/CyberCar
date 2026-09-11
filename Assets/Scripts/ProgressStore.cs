using System;
using System.IO;
using UnityEngine;
namespace CyberCar {
 [Serializable] public sealed class DriverProfile {public int version=2,unlocked;public bool freeplay;public bool[] badges=new bool[17];public float music=.22f;}
 public static class ProgressStore {
 public const string Prefix="CyberCarGame.v1.";public static bool Testing;static DriverProfile profile;
 public static string SavePath=>Path.Combine(Application.persistentDataPath,"driver-profile-v2.json");
 public static readonly string[] Achievements={"First delivery","Spoof-proof","Signal survivor","Bus guardian","Backup hero","Identity detective","Clean getaway","Off the grid","Original campaign veteran","Replay-proof","Signed and sealed","Extreme driver","Drift master","Safe streets","Coastal champion","Lightkeeper","Night courier"};
 static DriverProfile Data {get {if(profile!=null)return profile;profile=LoadFrom(SavePath);int legacy=Mathf.Clamp(PlayerPrefs.GetInt(Prefix+"unlocked",0),0,6);profile.unlocked=Mathf.Max(profile.unlocked,legacy);profile.freeplay|=legacy>=6;for(int i=0;i<9;i++)profile.badges[i]|=PlayerPrefs.GetInt(Prefix+"badge."+i,0)==1;return profile;}}
 public static int Unlocked=>Mathf.Clamp(Data.unlocked,0,GameSession.LevelCount);
 public static bool FreeplayUnlocked=>Data.freeplay||Unlocked>=GameSession.LevelCount;
 public static float MusicVolume{get=>Data.music;set{Data.music=Mathf.Clamp01(value);Save();}}
 public static bool Has(int id)=>id>=0&&id<Data.badges.Length&&Data.badges[id];
 public static bool Award(int id){if(Testing||Has(id))return false;Data.badges[id]=true;Save();return true;}
 public static void Complete(int level){if(Testing)return;Data.unlocked=Mathf.Max(Unlocked,Mathf.Min(GameSession.LevelCount,level+1));if(Data.unlocked>=GameSession.LevelCount)Data.freeplay=true;Save();}
 public static void Save(){if(Testing)return;try{WriteTo(SavePath,Data);}catch(Exception e){Debug.LogWarning("Progress save failed: "+e.Message);}}
 public static void Reload(){profile=null;}
 public static void WriteTo(string path,DriverProfile data){Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data,true));if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}
 public static DriverProfile LoadFrom(string path) {
 foreach(string candidate in new[]{path,path+".bak"}){if(!File.Exists(candidate))continue;try{var data=JsonUtility.FromJson<DriverProfile>(File.ReadAllText(candidate));if(data==null||data.version!=2)continue;var badges=new bool[17];if(data.badges!=null)Array.Copy(data.badges,badges,Math.Min(17,data.badges.Length));data.badges=badges;data.unlocked=Mathf.Clamp(data.unlocked,0,GameSession.LevelCount);data.music=Mathf.Clamp01(data.music);return data;}catch(Exception){}}
 return new DriverProfile();
 }
 }
}