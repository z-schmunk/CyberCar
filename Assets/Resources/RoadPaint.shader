Shader "CyberCar/RoadPaint" {
 Properties { _Color("Paint",Color)=(.72,.70,.62,1) _Cutoff("Wear",Range(0,1))=.24 }
 SubShader { Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;};
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){float wear=hash(floor(IN.worldPos.xz*35));float grain=hash(floor(IN.worldPos.xz*8));o.Albedo=_Color.rgb*(.88+grain*.12);o.Smoothness=.17;o.Metallic=0;o.Alpha=lerp(.18,1,smoothstep(.07,.2,wear));}
 ENDCG
 } FallBack "Transparent/Cutout/Diffuse"
}
