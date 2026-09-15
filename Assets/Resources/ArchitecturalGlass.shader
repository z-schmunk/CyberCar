Shader "CyberCar/ArchitecturalGlass" {
 Properties { _Color("Frame",Color)=(.27,.29,.30,1) }
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 fixed4 _Color;float _CyberNight;
 struct Input {float3 worldPos;float3 worldNormal;};
 float hash(float2 p){return frac(sin(dot(p,float2(12.9898,78.233)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 n=abs(IN.worldNormal);float2 uv=float2(n.x>n.z?IN.worldPos.z:IN.worldPos.x,IN.worldPos.y);float2 cell=uv/float2(2.65,3.2),f=frac(cell);
 float frame=step(f.x,.035)+step(.965,f.x)+step(f.y,.045)+step(.89,f.y);frame=saturate(frame);float roof=step(.6,n.y);frame=max(frame,roof);
 float variation=hash(floor(cell)+floor(IN.worldPos.xz/100)*7);
 float blinds=step(.65,variation)*step(.60,f.y);float3 glass=lerp(float3(.026,.06,.079),float3(.10,.155,.175),variation);
 glass=lerp(glass,float3(.18,.19,.175),blinds*.6);
 o.Albedo=lerp(glass,_Color.rgb*(.85+.15*variation),frame);o.Metallic=lerp(.42,.15,frame);o.Smoothness=lerp(.91,.32,frame);
 float lit=step(.82,variation)*(1-frame)*_CyberNight;o.Emission=float3(.85,.59,.29)*lit*.55;o.Occlusion=lerp(.72,1,frame);o.Alpha=1;
 }
 ENDCG
 } Fallback "Standard"
}
