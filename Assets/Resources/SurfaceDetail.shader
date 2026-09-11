Shader "CyberCar/SurfaceDetail" {
 Properties { _Color("Color",Color)=(1,1,1,1) _MainTex("Surface",2D)="white"{} _Scale("Meters per tile",Float)=4 _Smoothness("Smoothness",Range(0,1))=.2 _Metallic("Metallic",Range(0,1))=0 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex; fixed4 _Color; float _Scale,_Smoothness,_Metallic;
 struct Input {float3 worldPos;float3 worldNormal;};
 void surf(Input IN,inout SurfaceOutputStandard o) {
 float3 n=abs(IN.worldNormal);float2 uv=IN.worldPos.xz;
 if(n.x>n.y&&n.x>n.z)uv=IN.worldPos.zy;else if(n.z>n.y)uv=IN.worldPos.xy;
 fixed4 c=tex2D(_MainTex,uv/_Scale)*_Color;
 o.Albedo=c.rgb;o.Smoothness=_Smoothness;o.Metallic=_Metallic;o.Alpha=1;
 }
 ENDCG
 } FallBack "Standard"
}
