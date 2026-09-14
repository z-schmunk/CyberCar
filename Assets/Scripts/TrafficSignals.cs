using System.Collections.Generic;
using UnityEngine;
namespace CyberCar {
 public sealed class TrafficSignals:MonoBehaviour {
 WorldBuilder world;readonly List<Renderer> lenses=new List<Renderer>();readonly List<int> nodes=new List<int>();readonly List<bool> axes=new List<bool>();Material red,green;
 public bool Red(int node,Vector3 direction,float time){if(world.Network.Map!=0)return false;bool horizontal=Mathf.Abs(direction.x)>Mathf.Abs(direction.z);float phase=(time+node*1.7f)%26;return horizontal?phase>=11:phase<13||phase>=24;}
 public bool MustStop(Vector3 p,Vector3 direction,float time){if(world.Network.Map!=0)return false;int n=world.Network.Nearest(p);Vector3 d=world.Network.Nodes[n]-p;float ahead=Vector3.Dot(d,direction);return ahead>11&&ahead<27&&Vector3.Cross(d,direction).magnitude<12&&Red(n,direction,time);}
 public void Build(WorldBuilder owner){world=owner;red=world.Mat("Stop signal",Color.red,0,true);green=world.Mat("Go signal",Color.green,0,true);var steel=world.Mat("Signal pole",new Color(.15f,.17f,.18f),.5f);
 if(world.Network.Map!=0)return;
 for(int n=0;n<world.Network.Nodes.Count;n++){Vector3 center=world.Network.Nodes[n];foreach(int axis in new[]{0,1}){Vector3 p=center+new Vector3(axis==0?-12:12,0,axis==0?12:-12);float ground=world.GroundHeight(p);world.Box("Traffic signal mast",new Vector3(p.x,(ground+5)/2,p.z),new Vector3(.2f,5-ground,.2f),steel);world.Box("Signal housing",p+Vector3.up*4.4f,new Vector3(.65f,1.3f,.5f),steel);var lens=world.Box("Traffic signal lens",p+new Vector3(0,4.4f,-.3f),new Vector3(.35f,.65f,.08f),red,false);lenses.Add(lens.GetComponent<Renderer>());nodes.Add(n);axes.Add(axis==0);}}
 foreach(var e in world.Network.Edges)if(world.Network.OneWay(e.x,e.y)){var path=world.Network.Path(e.x,e.y);Vector3 mid=path[12];world.Sign(mid+Vector3.right*12,"ONE WAY "+(world.Network.CanTravel(e.x,e.y)?">>>":"<<<"),green);}
 world.Sign(world.Network.Nodes[0]+new Vector3(20,0,15),"SPEED LIMIT 25 MPH",steel);
 }
 void Update(){for(int i=0;i<lenses.Count;i++)lenses[i].sharedMaterial=Red(nodes[i],axes[i]?Vector3.right:Vector3.forward,Time.time)?red:green;}
 }
}
