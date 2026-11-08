Shader "DiceFree/Cornberg Water"
{
    Properties
    {
        _DeepColor ("Deep water", Color) = (.025,.20,.23,1)
        _ShallowColor ("Shallow water", Color) = (.19,.54,.47,1)
        _GlintColor ("Water glints", Color) = (.65,.91,.76,1)
        _Basin ("Basin center and radius", Vector) = (-2,3,1.27,0)
        _BasinMode ("Basin mode", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "WaterForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _GlintColor;
                float4 _Basin;
                float _BasinMode;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS=TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 p=input.positionWS.xz;
                float time=_Time.y;
                float streamCenter=-18+sin(p.y*.065)*2;
                float streamEdge=saturate(abs(p.x-streamCenter)/2.37);
                float basinDistance=length(p-_Basin.xy);
                float basinEdge=saturate(basinDistance/max(_Basin.z,.05));
                float edge=lerp(streamEdge,basinEdge,_BasinMode);

                float flow=p.y-time*.95;
                float broad=sin(flow*1.29+p.x*3.1+sin(flow*.75+p.x*1.8)*.9);
                float fine=sin(flow*2.75-p.x*5.3+sin(flow*.34+p.x*1.7));
                float patches=smoothstep(.18,.82,sin(flow*.74+p.x*2.15));
                float streak=smoothstep(.74,.98,broad)*patches*smoothstep(-.40,.61,fine);
                float shore=smoothstep(.75,1,edge)*(.65+.35*broad);
                float rings=smoothstep(.84,.98,sin(basinDistance*27-time*2.3))*(.48+.52*sin(p.x*2.7+p.y*3));
                float ripple=lerp(streak,rings,_BasinMode);
                float saturation=saturate(smoothstep(.16,1,edge)*.85+.13*broad+.055*fine);
                half3 color=lerp(_DeepColor.rgb,_ShallowColor.rgb,saturation);

                Light sun=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 daylight=saturate(sun.color)*(.38+.62*saturate(sun.direction.y))*lerp(.58,1,sun.shadowAttenuation);
                color *= .47+daylight*.72;
                float3 view=GetWorldSpaceNormalizeViewDir(input.positionWS);
                float reflected=smoothstep(.0,.78,1-saturate(view.y));
                color+=_GlintColor.rgb*(ripple*.18+shore*.048+reflected*.045);
                color+=_GlintColor.rgb*.038*_BasinMode;
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
