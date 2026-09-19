Shader "CyberCar/RoadFurniture" {
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input {float4 color:COLOR;};
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb;o.Metallic=.55;o.Smoothness=.28;o.Alpha=1;}
 ENDCG
 } FallBack "Standard"
}
