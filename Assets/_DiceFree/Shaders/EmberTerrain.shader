Shader "DiceFree/Ember Terrain"
{
    Properties { _BaseColor("Stone tint",Color)=(.35,.28,.22,1) _Lava("Molten surface",Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            float4 _BaseColor;float _Lava;
            struct Attributes { float4 positionOS:POSITION;float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1; };
            Varyings Vert(Attributes input){Varyings o;o.world=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(input.normalOS);return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=(input.world.xz-70000)*1.15;
                float2 cell=floor(p),f=frac(p);float closest=8,second=8;
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){float2 g=float2(x,y),q=g+float2(Hash(cell+g),Hash(cell+g+71))-f;float d=dot(q,q);if(d<closest){second=closest;closest=d;}else second=min(second,d);}
                float cracks=1-smoothstep(.015,.08,second-closest);
                float grain=Hash(floor(p*20));float3 color;
                if(_Lava>.5)
                {
                    float pulse=.78+.22*sin(_Time.y*1.7+p.x*.8+p.y*.4);
                    float crust=smoothstep(.12,.55,closest);
                    color=lerp(float3(.07,.023,.016),float3(.9,.10,.006),crust*.6);
                    color+=cracks*float3(2.6,.55,.035)*pulse+float3(.18,.018,0)*grain;
                }
                else
                {
                    float2 brick=p*.8;brick.x+=fmod(floor(brick.y),2)*.5;
                    float2 edge=min(frac(brick),1-frac(brick));float grout=1-smoothstep(.015,.04,min(edge.x,edge.y));
                    color=_BaseColor.rgb*(.80+.22*grain)*(1-grout*.45)*(1-cracks*.12);
                    Light light=GetMainLight();color*=SampleSH(normalize(input.normal))+.65*light.color*saturate(dot(normalize(input.normal),light.direction))+.25;
                }
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
