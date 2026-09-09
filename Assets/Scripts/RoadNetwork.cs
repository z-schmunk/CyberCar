using System.Collections.Generic;
using UnityEngine;

namespace CyberCar
{
    public sealed class RoadNetwork
    {
        public readonly List<Vector3> Nodes = new List<Vector3>();
        public readonly List<Vector2Int> Edges = new List<Vector2Int>();
        public readonly List<string> Names = new List<string>();
        public int Map { get; private set; }
        public int Finish => Nodes.Count - 1;
        public Vector3 Hazard { get; private set; }
        public RoadNetwork(int map)
        {
            Map=map;
            int columns=map==1?4:5, rows=map==1?6:5;
            float spacing=map==2?78:64;
            for(int z=0;z<rows;z++) for(int x=0;x<columns;x++)
            {
                Nodes.Add(new Vector3(x*spacing,0,z*spacing));
                Names.Add("Junction "+(Nodes.Count).ToString("00"));
                int n=z*columns+x;
                if(x>0) Edges.Add(new Vector2Int(n-1,n));
                if(z>0 && (map!=2 || x==0 || x==columns-1 || z==2)) Edges.Add(new Vector2Int(n-columns,n));
            }
            Names[0]="Operations garage";
            Names[columns-1]=map==0?"Transit exchange":map==1?"Cargo terminal":"Ridge station";
            Names[(rows/2)*columns]="Relay station";
            Names[Finish]=map==0?"Security campus":map==1?"Harbor control":"Summit observatory";
            Hazard=Nodes[columns-1]+Vector3.right*45;
        }
        public int Nearest(Vector3 position)
        {
            int best=0; float distance=float.MaxValue;
            for(int i=0;i<Nodes.Count;i++) {float d=(Nodes[i]-position).sqrMagnitude;if(d<distance){best=i;distance=d;}}
            return best;
        }
        public List<int> Route(int start,int end)
        {
            var queue=new Queue<int>();var previous=new Dictionary<int,int>();
            queue.Enqueue(start); previous[start]=-1;
            while(queue.Count>0)
            {
                int n=queue.Dequeue();if(n==end)break;
                foreach(var edge in Edges)
                {
                    int next=edge.x==n?edge.y:edge.y==n?edge.x:-1;
                    if(next<0||previous.ContainsKey(next))continue;
                    previous[next]=n;queue.Enqueue(next);
                }
            }
            var result=new List<int>();if(!previous.ContainsKey(end))return result;
            for(int n=end;n!=-1;n=previous[n]) result.Add(n);
            result.Reverse();return result;
        }
        public List<int> MissionRoute()
        {
            int first=Map==1?3:4;
            var route=Route(0,first);var rest=Route(first,Finish);rest.RemoveAt(0);route.AddRange(rest);return route;
        }
    }
}
