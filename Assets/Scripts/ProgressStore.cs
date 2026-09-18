using System;
using System.IO;
using UnityEngine;
namespace CyberCar {
 [Serializable] public sealed class MissionBest {public int stars;public float seconds;}
 [Serializable] public sealed class DriverProfile {public int version=4,unlocked,credits;public int[] upgrades=new int[3];public bool[] rewards=new bool[15];public bool freeplay;public bool[] badges=new bool[22];public float music=.22f;public MissionBest[] best=new MissionBest[15];}
 public static class ProgressStore {
 public const string Prefix="CyberCarGame.v1.";public static bool Testing;static DriverProfile profile;
 public static string SavePath=>Path.Combine(Application.persistentDataPath,"driver-profile-v2.json");
 public static readonly string[] Achievements={"First delivery","Spoof-proof","Signal survivor","Bus guardian","Backup hero","Identity detective","Clean getaway","Off the grid","Original campaign veteran","Replay-proof","Signed and sealed","Extreme driver","Drift master","Safe streets","Coastal champion","Lightkeeper","Night courier","Roadside rescuer","Starless navigator","Clean playlist","Manifest guardian","Sensor analyst"};
 static DriverProfile Data {get {if(profile!=null)return profile;profile=LoadFrom(SavePath);int legacy=Mathf.Clamp(PlayerPrefs.GetInt(Prefix+"unlocked",0),0,6);profile.unlocked=Mathf.Max(profile.unlocked,legacy);profile.freeplay|=legacy>=6;for(int i=0;i<9;i++)profile.badges[i]|=PlayerPrefs.GetInt(Prefix+"badge."+i,0)==1;return profile;}}
 public static int Unlocked=>Mathf.Clamp(Data.unlocked,0,GameSession.LevelCount);
 public static bool FreeplayUnlocked=>Data.freeplay||Unlocked>=GameSession.LevelCount;
 public static float MusicVolume{get=>Data.music;set{Data.music=Mathf.Clamp01(value);Save();}}
 public static bool Has(int id)=>id>=0&&id<Data.badges.Length&&Data.badges[id];
 public static bool Award(int id){if(Testing||Has(id))return false;Data.badges[id]=true;Save();return true;}
 public static int Credits=>Data.credits;
 public static MissionBest Best(int level)=>level>=0&&level<Data.best.Length?Data.best[level]:null;
 public static bool RecordMission(int level,int stars,float seconds){if(Testing)return false;bool improved=ImproveRecord(Data,level,stars,seconds);if(improved)Save();return improved;}
 // Also used for isolated save validation; never writes a profile by itself.
 public static bool ImproveRecord(DriverProfile data,int level,int stars,float seconds){
 if(level<0||level>=GameSession.LevelCount||stars<1||stars>3||float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<0)return false;
 NormalizeRecords(data);var previous=data.best[level];
 if(previous!=null&&(previous.stars>stars||previous.stars==stars&&previous.seconds<=seconds))return false;
 data.best[level]=new MissionBest{stars=stars,seconds=seconds};return true;
 }
 static void NormalizeRecords(DriverProfile data){var records=new MissionBest[GameSession.LevelCount];if(data.best!=null)Array.Copy(data.best,records,Math.Min(records.Length,data.best.Length));for(int i=0;i<records.Length;i++){var record=records[i];if(record!=null&&(record.stars<1||record.stars>3||record.seconds<0||float.IsNaN(record.seconds)||float.IsInfinity(record.seconds)))records[i]=null;}data.best=records;}
 public static int Upgrade(int i)=>i>=0&&i<3?Data.upgrades[i]:0;
 public static int UpgradeCost(int i)=>100+Upgrade(i)*100;
 public static bool Purchase(int i){if(i<0||i>=3||Upgrade(i)>=3||Credits<UpgradeCost(i)||Testing)return false;Data.credits-=UpgradeCost(i);Data.upgrades[i]++;Save();return true;}
 public static void Reward(int level,bool freeplay){if(Testing)return;if(freeplay){Data.credits+=40;}else if(!Data.rewards[level]){Data.rewards[level]=true;Data.credits+=180;}Save();}
 public static void Complete(int level){if(Testing)return;Data.unlocked=Mathf.Max(Unlocked,Mathf.Min(GameSession.LevelCount,level+1));if(Data.unlocked>=GameSession.LevelCount)Data.freeplay=true;Save();}
 public static void Save(){if(Testing)return;try{WriteTo(SavePath,Data);}catch(Exception e){Debug.LogWarning("Progress save failed: "+e.Message);}}
 public static void Reload(){profile=null;}
 public static void WriteTo(string path,DriverProfile data){Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data,true));if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}
 public static DriverProfile LoadFrom(string path) {
 foreach(string candidate in new[]{path,path+".bak"}){if(!File.Exists(candidate))continue;try{var data=JsonUtility.FromJson<DriverProfile>(File.ReadAllText(candidate));if(data==null||(data.version!=2&&data.version!=3&&data.version!=4))continue;var badges=new bool[22];if(data.badges!=null)Array.Copy(data.badges,badges,Math.Min(22,data.badges.Length));data.badges=badges;data.version=4;NormalizeRecords(data);
 var upgrades=new int[3];if(data.upgrades!=null)Array.Copy(data.upgrades,upgrades,Math.Min(3,data.upgrades.Length));for(int i=0;i<3;i++)upgrades[i]=Mathf.Clamp(upgrades[i],0,3);data.upgrades=upgrades;
 var rewards=new bool[GameSession.LevelCount];if(data.rewards!=null)Array.Copy(data.rewards,rewards,Math.Min(rewards.Length,data.rewards.Length));data.rewards=rewards;data.credits=Mathf.Max(0,data.credits);data.unlocked=Mathf.Clamp(data.unlocked,0,GameSession.LevelCount);data.music=Mathf.Clamp01(data.music);return data;}catch(Exception){}}
 return new DriverProfile();
 }
 }
}
