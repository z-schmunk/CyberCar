Shader "Hidden/CyberCar/DrivingPresentation" {
 Properties {_MainTex("Source",2D)="white"{} _BloomTex("Bloom",2D)="black"{}}
 SubShader {
 Cull Off ZWrite Off ZTest Always
 CGINCLUDE
 #include "UnityCG.cginc"
 sampler2D _MainTex,_BloomTex;
 float4 _MainTex_TexelSize;float2 _Direction;float _BloomStrength,_Exposure;
 float3 film(float3 x){return saturate((x*(2.51*x+.03))/(x*(2.43*x+.59)+.14));}
 half4 extract(v2f_img i):SV_Target {
 float2 step=_MainTex_TexelSize.xy;
 float3 c=(tex2D(_MainTex,i.uv+step*float2(-1,-1)).rgb+tex2D(_MainTex,i.uv+step*float2(1,-1)).rgb+tex2D(_MainTex,i.uv+step*float2(-1,1)).rgb+tex2D(_MainTex,i.uv+step*float2(1,1)).rgb)*.25;
 float brightness=max(c.r,max(c.g,c.b));float knee=saturate((brightness-.8)/.8);return half4(c*max(brightness-1.2,knee*knee*.2)/max(brightness,.001),1);
 }
 half4 blur(v2f_img i):SV_Target {
 float3 c=tex2D(_MainTex,i.uv).rgb*.227027;
 c+=(tex2D(_MainTex,i.uv+_Direction*1.384615).rgb+tex2D(_MainTex,i.uv-_Direction*1.384615).rgb)*.316216;
 c+=(tex2D(_MainTex,i.uv+_Direction*3.230769).rgb+tex2D(_MainTex,i.uv-_Direction*3.230769).rgb)*.070270;
 return half4(c,1);
 }
 half4 compose(v2f_img i):SV_Target {float3 c=max(0,tex2D(_MainTex,i.uv).rgb)*_Exposure+tex2D(_BloomTex,i.uv).rgb*_BloomStrength;return half4(lerp(c,film(c),.6),1);}
 ENDCG
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment extract
 #pragma target 3.0
 ENDCG}
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment blur
 #pragma target 3.0
 ENDCG}
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment compose
 #pragma target 3.0
 ENDCG}
 }
 Fallback Off
}
