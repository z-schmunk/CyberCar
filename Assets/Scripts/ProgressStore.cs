using UnityEngine;

namespace CyberCar
{
    public static class ProgressStore
    {
        public const string Prefix="CyberCarGame.v1.";
        public static bool Testing;
        public static int Unlocked => Mathf.Clamp(PlayerPrefs.GetInt(Prefix+"unlocked",0),0,6);
        public static readonly string[] Achievements={"First delivery","Spoof-proof","Signal survivor","Bus guardian","Backup hero","Identity detective","Clean getaway","Off the grid","Campaign complete"};
        public static bool Has(int id)=> PlayerPrefs.GetInt(Prefix+"badge."+id,0)==1;
        public static bool Award(int id)
        {
            if(Testing||Has(id))return false;
            PlayerPrefs.SetInt(Prefix+"badge."+id,1);PlayerPrefs.Save();return true;
        }
        public static void Complete(int level)
        {
            if(Testing)return;
            PlayerPrefs.SetInt(Prefix+"unlocked",Mathf.Max(Unlocked,Mathf.Min(6,level+1)));PlayerPrefs.Save();
        }
    }
}
