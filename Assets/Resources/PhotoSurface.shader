Shader "CyberCar/PhotoSurface" {
 Properties { _MainTex("Albedo",2D)="white"{} _NormalTex("Normal",2D)="bump"{} _RoughTex("Roughness",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Scale("Meters per tile",Float)=4 _Triplanar("World mapping",Float)=1 _NormalStrength("Normal strength",Float)=.7 _PackedARM("Packed ARM",Float)=0 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex,_NormalTex,_RoughTex;fixed4 _Color;float _Scale,_Triplanar,_NormalStrength,_PackedARM;
 struct Input {float2 uv_MainTex;float3 worldPos;float3 worldNormal;INTERNAL_DATA};
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 wn=normalize(WorldNormalVector(IN,float3(0,0,1)));float3 w=pow(abs(wn),4);w/=max(.001,w.x+w.y+w.z);
 float2 x=IN.worldPos.zy/_Scale,y=IN.worldPos.xz/_Scale,z=IN.worldPos.xy/_Scale;
 fixed3 albedo;float rough;float3 normal;
 if(_Triplanar<.5){albedo=tex2D(_MainTex,IN.uv_MainTex).rgb;rough=lerp(tex2D(_RoughTex,IN.uv_MainTex).r,tex2D(_RoughTex,IN.uv_MainTex).g,_PackedARM);normal=UnpackNormal(tex2D(_NormalTex,IN.uv_MainTex));normal.xy*=_NormalStrength;o.Normal=normalize(normal);}
 else {
 albedo=tex2D(_MainTex,x).rgb*w.x+tex2D(_MainTex,y).rgb*w.y+tex2D(_MainTex,z).rgb*w.z;
 rough=tex2D(_RoughTex,x).r*w.x+tex2D(_RoughTex,y).r*w.y+tex2D(_RoughTex,z).r*w.z;
 float3 nx=UnpackNormal(tex2D(_NormalTex,x)),ny=UnpackNormal(tex2D(_NormalTex,y)),nz=UnpackNormal(tex2D(_NormalTex,z));
 float3 perturb=float3(0,nx.y,nx.x)*w.x+float3(ny.x,0,ny.y)*w.y+float3(nz.x,nz.y,0)*w.z;
 normal=normalize(wn+perturb*_NormalStrength);
 o.Normal=float3(dot(normal,WorldNormalVector(IN,float3(1,0,0))),dot(normal,WorldNormalVector(IN,float3(0,1,0))),dot(normal,WorldNormalVector(IN,float3(0,0,1))));
 }
 o.Albedo=albedo*_Color.rgb;o.Smoothness=saturate(1-rough)*.65;o.Metallic=0;o.Alpha=1;
 }
 ENDCG
 } FallBack "Standard"
}
