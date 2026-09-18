using System.Collections.Generic;
namespace CyberCar {
 // Stars reward delivery, road safety and verified recovery; speed never earns a star.
 public sealed class MissionRating {
 public readonly int Stars,Defended,Threats,Mistakes;
 public readonly bool Delivered,SafeDriving,SecureRecovery;
 public MissionRating(bool delivered,int crashes,int recoveries,int civilianStrikes,int mistakes,List<AttackRecord> records){
 Delivered=delivered;Mistakes=mistakes;
 foreach(var record in records){Threats++;if(record.Defended)Defended++;}
 SafeDriving=crashes==0&&recoveries==0&&civilianStrikes==0;
 SecureRecovery=Mistakes==0&&Defended==Threats;
 Stars=delivered?1+(SafeDriving?1:0)+(SecureRecovery?1:0):0;
 if(civilianStrikes>0&&Stars>1)Stars=1;
 }
 public static string Display(int stars)=>new string('*',stars)+new string('-',3-stars);
 }
}
