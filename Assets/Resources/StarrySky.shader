Shader "CyberCar/StarrySky" {
 Properties { _MainTex("Photographed night sky",2D)="black"{} _Exposure("Exposure",Float)=.014 _Rotation("Rotation",Float)=40 }
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float _Exposure,_Rotation;
 struct a{float4 vertex:POSITION;};struct v{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.dir=i.vertex.xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 fixed4 frag(v i):SV_Target{
 float3 d=normalize(i.dir);float2 uv=float2(atan2(d.z,d.x)/(2*UNITY_PI)+.5+_Rotation/360,acos(clamp(d.y,-1,1))/UNITY_PI);
 float3 background=tex2D(_MainTex,float2(frac(uv.x),1-uv.y)).rgb*_Exposure;
 float2 grid=uv*float2(900,450),cell=floor(grid);float seed=hash(cell);
 float2 offset=float2(hash(cell+7.3),hash(cell+91.7))*.7+.15;
 float distance=length(frac(grid)-offset),aa=max(.02,fwidth(distance));
 float star=(1-smoothstep(.04-aa,.04+aa,distance))*step(.996,seed)*smoothstep(.02,.25,d.y);
 float3 tint=lerp(float3(.65,.78,1),float3(1,.9,.72),hash(cell+52));
 return fixed4(background+star*tint*(.8+hash(cell+19)*1.5),1);
 }
 ENDCG }
 }
}