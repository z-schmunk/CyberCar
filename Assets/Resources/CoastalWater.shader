Shader "CyberCar/CoastalWater" {
 Properties {_Color("Deep water",Color)=(0.018,0.105,0.15,1) _CoastZ("Coast offset",Float)=-70 _WaterLevel("Sea height",Float)=-7}
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard vertex:vert fullforwardshadows
 #pragma target 3.0
 fixed4 _Color;float _CoastZ,_WaterLevel;
 struct Input {float3 worldPos;};
 void vert(inout appdata_full v){float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;v.vertex.y+=sin(p.x*.042+p.z*.02+_Time.y*.8)*.20+cos(p.z*.073-p.x*.016+_Time.y*.57)*.12;}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float2 p=IN.worldPos.xz;float t=_Time.y;
 float dx=cos(p.x*.42+p.y*.22+t*1.3)*.055+cos(p.x*1.1-p.y*.7+t*1.8)*.018;
 float dz=sin(p.y*.39-p.x*.15+t*1.1)*.055+sin(p.y*1.3+p.x*.4+t*2)*.016;
 float attenuation=saturate(1-distance(_WorldSpaceCameraPos,IN.worldPos)/700);
 o.Normal=normalize(float3(dx*attenuation,dz*attenuation,1));
 float shore=_CoastZ+sin(p.x*.012)*5;float shallow=exp(-abs(p.y-shore)*.05);
 float wave=sin((p.y-shore)*.55-t*1.65+sin(p.x*.16)*.5);
 float foam=shallow*smoothstep(.75,.97,wave)*.48;
 o.Albedo=lerp(_Color.rgb,float3(.07,.25,.24),shallow*.72);o.Albedo=lerp(o.Albedo,float3(.64,.73,.70),foam);
 o.Smoothness=lerp(.84,.36,foam);o.Metallic=0;o.Alpha=1;
 }
 ENDCG
 } FallBack "Standard"
}
