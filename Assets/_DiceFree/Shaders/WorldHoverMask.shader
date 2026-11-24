Shader "DiceFree/World Hover Mask"
{
    Properties
    {
        _SourceMap ("Source opacity", 2D) = "white" {}
        _UseAlphaClip ("Use opacity", Float) = 0
        _Cutoff ("Opacity cutoff", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-52" "RenderType"="Transparent" }
        Pass
        {
            Name "Hover silhouette mask"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite Off
            ZTest LEqual
            ColorMask 0
            Stencil { Ref 1 ReadMask 1 WriteMask 1 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_SourceMap); SAMPLER(sampler_SourceMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _SourceMap_ST;
            float _UseAlphaClip;
            float _Cutoff;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.uv=TRANSFORM_TEX(input.uv,_SourceMap);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                if(_UseAlphaClip>0.5) clip(SAMPLE_TEXTURE2D(_SourceMap,sampler_SourceMap,input.uv).a-_Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}
