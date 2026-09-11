Shader "CyberCar/TireSmoke" {
 SubShader {
 Tags { "Queue"="Transparent" "RenderType"="Transparent" }
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off Cull Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct a { float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR; };
 struct v { float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR; };
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
 fixed4 frag(v i):SV_Target {float2 p=i.uv*2-1;float alpha=saturate(1-dot(p,p));alpha=alpha*alpha;float cloud=.75+.25*sin(p.x*12+sin(p.y*9))*sin(p.y*11);return fixed4(i.color.rgb,i.color.a*alpha*cloud);}
 ENDCG
 }
 }
}
