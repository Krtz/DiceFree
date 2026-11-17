Shader "DiceFree/Crimson Mire Ground"
{
    Properties
    {
        _BaseMap ("Region color and opacity", 2D) = "white" {}
        _Grain ("Tiled stone and peat", 2D) = "gray" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _Relief ("Relief", Range(0,1)) = .4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-12" }
        Pass
        {
            Name "Ground"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Grain); SAMPLER(sampler_Grain);
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _Relief;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv);
                clip(base.a - .005);
                float2 ground = input.positionWS.xz;
                half4 grain = SAMPLE_TEXTURE2D(_Grain,sampler_Grain,ground * .128);
                half coarse = SAMPLE_TEXTURE2D(_Grain,sampler_Grain,
                    float2(ground.x + ground.y * .37, ground.y - ground.x * .31) * .038).r;
                half fine = SAMPLE_TEXTURE2D(_Grain,sampler_Grain,ground * .354 + .28).r;
                half3 normal = normalize(input.normalWS + half3(grain.g-.5,0,grain.b-.5) * _Relief);
                Light sun = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 light = max(SampleSH(normal),.058) + sun.color * sun.shadowAttenuation *
                    saturate(dot(normal,sun.direction)) * .76;
                half3 tint = base.rgb * _Tint.rgb * (.66 + .37 * grain.r + .31 * coarse + .16 * fine);
                tint += grain.a * lerp(half3(.018,.012,.010),half3(.061,.040,.023),coarse);
                return half4(tint * light,base.a * _Tint.a);
            }
            ENDHLSL
        }
    }
}
