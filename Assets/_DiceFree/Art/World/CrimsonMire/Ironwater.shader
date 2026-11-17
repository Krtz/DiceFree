Shader "DiceFree/Ironwater"
{
    Properties
    {
        _DeepColor ("Peat water", Color) = (.072,.065,.073,1)
        _ShallowColor ("Iron-rich shallows", Color) = (.38,.21,.12,1)
        _SkyColor ("Reflected sky", Color) = (.29,.40,.42,1)
        _GlintColor ("Light on the surface", Color) = (.83,.63,.44,1)
        _Pond ("Center and radius", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "Ironwater"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _SkyColor;
                half4 _GlintColor;
                float4 _Pond;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.positionWS.xz;
                float2 local = (p - _Pond.xy) / _Pond.zw;
                float edge = length(local);
                float time = _Time.y;
                float bend = sin(p.x * .42 + p.y * .56 + time * .42);
                float warp = sin(p.x * .87 - p.y * .72 + bend * .84);
                float a = sin(p.x * 2.2 + p.y * 2.72 + bend * 1.05 + warp * .66 - time * 1.17);
                float b = sin(p.x * -3.4 + p.y * 3.8 + warp * 1.9 - time * .82);
                float c = sin(p.x * .8 - p.y * .88 - time * .45);
                float depth = smoothstep(.35,.99,edge + .038 * c + .018 * b);
                float bank = smoothstep(.81,.96,edge + .028 * a);
                float3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 normal = normalize(half3(a*.12+b*.046,1,c*.085+a*.058));
                half reflectivity = pow(1-saturate(dot(view,normal)),2.6);
                Light sun = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 sky = SampleSH(half3(0,1,0));
                half3 daylight = sun.color * lerp(.42,1,sun.shadowAttenuation) * saturate(sun.direction.y);
                half glints = smoothstep(.88,1,a) * smoothstep(.10,.88,b) * smoothstep(-.22,.87,c);
                half shoreFoam = (1-smoothstep(.026,.087,abs(edge - .93 + .026*c))) * smoothstep(.1,1,b) * .19;
                half3 water = lerp(_DeepColor.rgb,_ShallowColor.rgb,depth) * (.31+sky*.52+daylight*.66);
                water += _SkyColor.rgb * (reflectivity*.85 + (.07+.07*c)*(1-bank)) * (.20+sky*.65+daylight*.24);
                water += _GlintColor.rgb * (glints * (.065 + .080 * reflectivity) + shoreFoam) * (.18+sky*.28+daylight*.58);
                return half4(water,1);
            }
            ENDHLSL
        }
    }
}
