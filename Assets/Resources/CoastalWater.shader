Shader "CyberCar/CoastalWater" {
 Properties {_Color("Deep water",Color)=(0.025,0.17,0.23,1)}
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard vertex:vert fullforwardshadows
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;};
 void vert(inout appdata_full v){float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;v.vertex.y+=sin(p.x*.065+_Time.y*.8)*.18+cos(p.z*.09+_Time.y*.65)*.12;}
 void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float a=sin(p.x*.43+p.y*.19+_Time.y*1.2),b=cos(p.y*.61-p.x*.17+_Time.y*.9);float fade=saturate(1-distance(_WorldSpaceCameraPos,IN.worldPos)/200);o.Normal=normalize(float3(a*.025*fade,b*.022*fade,1));o.Albedo=_Color.rgb*(.97+.03*a*b);o.Metallic=0;o.Smoothness=.68;o.Alpha=1;}
 ENDCG
 } FallBack "Standard"
}
