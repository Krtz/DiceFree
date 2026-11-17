Shader "DiceFree/Woodland Water"
{
    Properties
    {
        _DeepColor ("Deep water", Color) = (.025,.18,.20,1)
        _ShallowColor ("Shallows", Color) = (.16,.48,.39,1)
        _GlintColor ("Surface glints", Color) = (.58,.85,.76,1)
        _Flow ("Flow along and across surface", Vector) = (.75,.08,0,0)
        _Pond ("Pond center and radii", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "WaterForward"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
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
                half4 _GlintColor;
                float4 _Flow;
                float4 _Pond;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float crossStream : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.crossStream = input.uv.y;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 positionXZ = input.positionWS.xz;
                float2 moving = positionXZ - _Time.y * _Flow.xy;
                float broad = sin(dot(moving, float2(.92, 1.75)) + .78 * sin(positionXZ.x * .64 - positionXZ.y * .52));
                float fine = sin(dot(moving, float2(2.35, -3.14)) + .61 * sin(positionXZ.y * 1.23));
                float drift = sin(dot(positionXZ, float2(.48, .67)) + broad * .44);

                float pondEdge = length((positionXZ - _Pond.xy) / max(_Pond.zw, .05));
                float streamEdge = abs(input.crossStream * 2 - 1);
                float edge = lerp(streamEdge, pondEdge, step(.05, _Pond.z));
                edge += .028 * broad + .016 * fine;

                float depth = smoothstep(.26, 1.02, edge + .07 * drift);
                float patch = smoothstep(-.30, .86, drift);
                float ripple = smoothstep(.83, 1, broad) * smoothstep(-.5, .75, fine) * patch;
                float rim = smoothstep(.78, .96, edge) * (.28 + .72 * patch);
                float3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float reflection = pow(1 - saturate(view.y), 2.4);

                Light sun = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 day = saturate(sun.color) * lerp(.28, 1, saturate(sun.direction.y));
                half shade = lerp(.46, 1, sun.shadowAttenuation);
                half3 tint = lerp(_DeepColor.rgb, _ShallowColor.rgb, depth) * (.95 + .06 * broad + .065 * fine);
                half3 glint = _GlintColor.rgb * (.15 * ripple + .08 * rim + .14 * reflection);
                return half4(tint * (.36 + day * shade * .75) + glint * (.39 + day * shade * .73), 1);
            }
            ENDHLSL
        }
    }
}
