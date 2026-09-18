Shader "CyberCar/VehicleLamp" {
 Properties { _Color("Lens tint", Color)=(1,1,1,1) _EmissionColor("Lamp output", Color)=(1,1,1,1) }
 SubShader {
 Tags { "RenderType"="Opaque" "Queue"="Geometry" }
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; };
 struct v2f { float4 vertex:SV_POSITION; UNITY_FOG_COORDS(0) };
 fixed4 _Color,_EmissionColor;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);UNITY_TRANSFER_FOG(o,o.vertex);return o;}
 fixed4 frag(v2f i):SV_Target{fixed4 c=fixed4(_Color.rgb*.08+_EmissionColor.rgb,1);UNITY_APPLY_FOG(i.fogCoord,c);return c;}
 ENDCG
 }
 }
}
