Shader "DiceFree/World Hover Outline"
{
    Properties
    {
        _OutlineColor ("Outline", Color) = (1,0.2,0.15,1)
        _OutlineWidth ("Width in pixels", Float) = 2
        _SourceMap ("Source opacity", 2D) = "white" {}
        _UseAlphaClip ("Use opacity", Float) = 0
        _Cutoff ("Opacity cutoff", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-50" "RenderType"="Transparent" }
        Pass
        {
            Name "Hover silhouette"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil { Ref 1 ReadMask 1 WriteMask 0 Comp NotEqual Pass Keep }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_SourceMap); SAMPLER(sampler_SourceMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _OutlineColor;
            float4 _SourceMap_ST;
            float _OutlineWidth;
            float _UseAlphaClip;
            float _Cutoff;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                float3 normalVS=TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
                float2 direction=normalVS.xy/max(length(normalVS.xy),0.001);
                output.positionCS.xy+=direction*(_OutlineWidth*2/_ScreenParams.xy)*output.positionCS.w;
                output.uv=TRANSFORM_TEX(input.uv,_SourceMap);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                if(_UseAlphaClip>0.5) clip(SAMPLE_TEXTURE2D(_SourceMap,sampler_SourceMap,input.uv).a-_Cutoff);
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
