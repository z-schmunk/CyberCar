Shader "CyberCar/LandscapeBlend" {
 Properties { _GroundTex("Ground",2D)="white"{} _GroundNormal("Ground normal",2D)="bump"{} _RockTex("Rock",2D)="white"{} _RockNormal("Rock normal",2D)="bump"{} _Scale("Rock meters",Float)=7 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _GroundTex,_GroundNormal,_RockTex,_RockNormal;float _Scale;
 struct Input {float3 worldPos;float3 worldNormal;INTERNAL_DATA};
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 n=normalize(WorldNormalVector(IN,float3(0,0,1))),w=pow(abs(n),5);w/=max(.001,w.x+w.y+w.z);
 float3 p=IN.worldPos/_Scale;float macro=noise(IN.worldPos.xz*.016),micro=noise(IN.worldPos.xz*.09);
 float exposed=smoothstep(.12,.48,1-n.y+(macro-.5)*.22);
 float3 ground=tex2D(_GroundTex,IN.worldPos.xz/6).rgb;
 float3 rock=tex2D(_RockTex,p.zy).rgb*w.x+tex2D(_RockTex,p.xz).rgb*w.y+tex2D(_RockTex,p.xy).rgb*w.z;
 ground*=lerp(float3(.80,.88,.67),float3(1.08,1.04,.88),macro);
 o.Albedo=lerp(ground,rock*float3(.93,.92,.89),exposed)*lerp(.82,1.08,micro);
 float3 nx=UnpackNormal(tex2D(_RockNormal,p.zy)),ny=UnpackNormal(tex2D(_RockNormal,p.xz)),nz=UnpackNormal(tex2D(_RockNormal,p.xy));
 float3 gn=UnpackNormal(tex2D(_GroundNormal,IN.worldPos.xz/6));
 float3 perturb=lerp(float3(gn.x,0,gn.y)*.45,(float3(0,nx.y,nx.x)*w.x+float3(ny.x,0,ny.y)*w.y+float3(nz.x,nz.y,0)*w.z)*.65,exposed);
 float3 normal=normalize(n+perturb);o.Normal=float3(dot(normal,WorldNormalVector(IN,float3(1,0,0))),dot(normal,WorldNormalVector(IN,float3(0,1,0))),dot(normal,WorldNormalVector(IN,float3(0,0,1))));
 o.Smoothness=lerp(.07,.18,exposed);o.Occlusion=lerp(.84,1,micro);o.Metallic=0;o.Alpha=1;
 }
 ENDCG
 } Fallback "Standard"
}
