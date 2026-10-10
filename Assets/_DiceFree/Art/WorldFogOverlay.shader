Shader "DiceFree/WorldFogOverlay"
{
    Properties
    {
        _FogTex ("Exploration Fog Alpha", 2D) = "black" {}
        _FogTint ("Fog Tint", Color) = (0.035, 0.055, 0.085, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+80" "RenderType"="Transparent" }
        Pass
        {
            Name "Ground Fog"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _FogTint;
                float4 _FogTex_ST;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half fog = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, input.uv).a;
                return half4(_FogTint.rgb, fog * _FogTint.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
