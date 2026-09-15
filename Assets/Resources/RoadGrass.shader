Shader "CyberCar/RoadGrass" {
 SubShader { Tags {"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input {float4 color:COLOR;};
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb;o.Smoothness=.07;o.Alpha=1;}
 ENDCG
 } Fallback "Standard"
}
