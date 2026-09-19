Shader "CyberCar/RoadSurface" {
 Properties { _MainTex("Asphalt",2D)="white"{} _NormalTex("Normal",2D)="bump"{} _RoughTex("Roughness",2D)="white"{} }
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _MainTex,_NormalTex,_RoughTex;
 struct Input {float3 worldPos;float3 worldNormal;float2 road;float fade;INTERNAL_DATA};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.road=v.texcoord1.xy;o.fade=v.color.r;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float2 uv=IN.worldPos.xz/4;
 float3 wn=normalize(WorldNormalVector(IN,float3(0,0,1)));
 float3 n=UnpackNormal(tex2D(_NormalTex,uv));float3 normal=normalize(wn+float3(n.x,0,n.y)*.3);
 o.Normal=float3(dot(normal,WorldNormalVector(IN,float3(1,0,0))),dot(normal,WorldNormalVector(IN,float3(0,1,0))),dot(normal,WorldNormalVector(IN,float3(0,0,1))));
 float wheelDistance=min(abs(abs(IN.road.x)-1.85),abs(abs(IN.road.x)-3.55));
 float tracks=(1-smoothstep(.12,.52,wheelDistance))*IN.fade;
 float2 cell=IN.road/float2(4.5,13),local=frac(cell),edge=min(local,1-local);
 float patch=step(.82,hash(floor(cell)))*smoothstep(.05,.13,min(edge.x,edge.y))*IN.fade;
 float macro=.96+.04*sin(IN.worldPos.x*.17+sin(IN.worldPos.z*.21));
 o.Albedo=tex2D(_MainTex,uv).rgb*float3(.63,.66,.69)*macro*(1-tracks*.14-patch*.17);
 o.Smoothness=clamp((1-tex2D(_RoughTex,uv).r)*.45+tracks*.07+patch*.04,.04,.28);
 o.Metallic=0;o.Occlusion=1;o.Alpha=1;
 }
 ENDCG
 } FallBack "Standard"
}
