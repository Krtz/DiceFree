Shader "DiceFree/Ground Aura Sigil"
{
    Properties
    {
        _TintA ("Primary glow",Color) = (0.12,0.62,1,1)
        _TintB ("Rune highlights",Color) = (0.64,0.94,1,1)
        _Style ("Aura motif",Float) = 0
        _Opacity ("Visibility",Float) = 1
        _Phase ("Animation",Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-60" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha One
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _TintA,_TintB;
            float _Style,_Opacity,_Phase;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;output.positionCS=TransformObjectToHClip(input.positionOS.xyz);output.uv=input.uv;return output;
            }
            float2 Rotate(float2 p,float a) { float s=sin(a),c=cos(a);return float2(c*p.x-s*p.y,s*p.x+c*p.y); }
            float Stroke(float distance,float width)
            {
                float aa=max(fwidth(distance),.002);
                float core=1-smoothstep(width,width+aa,abs(distance));
                return core+exp(-abs(distance)*45)*.18;
            }
            float Segment(float2 p,float2 a,float2 b)
            {
                float2 ab=b-a;return length(p-a-ab*saturate(dot(p-a,ab)/max(.0001,dot(ab,ab))));
            }
            float Polygon(float2 p,float sides,float radius)
            {
                float a=atan2(p.y,p.x)+PI,sector=TWO_PI/sides;
                return cos(floor(.5+a/sector)*sector-a)*length(p)-radius;
            }
            float Rune(float2 p)
            {
                float diamond=abs(p.x)+abs(p.y)*.72-.055;
                return max(Stroke(diamond,.012),Stroke(Segment(p,float2(0,-.09),float2(0,.09)),.010));
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=input.uv*2-1;float r=length(p);float t=_Phase;
                float a=atan2(p.y,p.x);float primary=0,detail=0;
                float breath=.86+.14*sin(t*1.8);
                float3 color=_TintA.rgb;
                float outer=Stroke(r-.83,.012);
                // Broken arcs, a soft floor glow and luminous glyphs at readable scale.
                float arc=smoothstep(-.25,.1,sin(a*3+t*.7));
                primary=outer*arc+Stroke(r-.72,.012)*.42;
                primary+=exp(-r*r*7)*.055;
                float2 orbit=Rotate(p,t*.10);
                float sector=round(atan2(orbit.y,orbit.x)/(TWO_PI/6))*(TWO_PI/6);
                float2 glyph=Rotate(orbit,-sector)-float2(.72,0);
                detail=Rune(glyph)*.85;
                if(_Style<.5) // Mana: braided crescents and a four-point star.
                {
                    float wave=sin(a*4-t*.7)*.035;
                    primary+=Stroke(r-(.53+wave),.018)*.8;
                    primary+=Stroke(r-(.53-wave),.012)*.45;
                    float2 q=Rotate(p,-t*.12);
                    detail+=Stroke(abs(q.x)+abs(q.y)-.25,.013)*.7;
                    detail+=Stroke(Segment(q,float2(-.33,0),float2(.33,0)),.010)*.45;
                    detail+=Stroke(Segment(q,float2(0,-.33),float2(0,.33)),.010)*.45;
                }
                else if(_Style<1.5) // Elemental Weaving: six schools in an interlocking seal.
                {
                    float school=floor(frac((a+PI)/TWO_PI+t*.018)*6);
                    color=school<1?float3(1,.30,.08):school<2?float3(.22,.8,1):school<3?float3(.70,.38,1):
                        school<4?float3(1,.75,.22):school<5?float3(.25,1,.42):float3(.78,.88,1);
                    float2 q=Rotate(p,-t*.10);
                    primary+=Stroke(Polygon(q,3,.27),.018)*.85;
                    primary+=Stroke(Polygon(-q,3,.27),.018)*.85;
                    primary+=Stroke(r-.58,.012)*smoothstep(.20,.45,cos(a*6+t*.25));
                }
                else if(_Style<2.5) // Hold the Line: shield, cross and fortified hexagon.
                {
                    primary+=Stroke(Polygon(p,6,.57),.018)*.8;
                    float shield=min(Segment(p,float2(-.24,.26),float2(.24,.26)),
                        min(Segment(p,float2(-.24,.26),float2(-.21,-.05)),Segment(p,float2(.24,.26),float2(.21,-.05))));
                    shield=min(shield,min(Segment(p,float2(-.21,-.05),float2(0,-.33)),Segment(p,float2(.21,-.05),float2(0,-.33))));
                    detail+=Stroke(shield,.022);
                    detail+=Stroke(Segment(p,float2(0,-.17),float2(0,.17)),.016)*.9;
                    detail+=Stroke(Segment(p,float2(-.12,.035),float2(.12,.035)),.016)*.9;
                }
                else if(_Style<3.5) // Speed Presence: silver chevrons and sweeping psychic trails.
                {
                    float2 q=Rotate(p,t*.3);
                    float angle=round(atan2(q.y,q.x)/(TWO_PI/3))*(TWO_PI/3);
                    q=Rotate(q,-angle)-float2(.40,0);
                    detail+=Stroke(min(Segment(q,float2(-.08,-.12),float2(.08,0)),Segment(q,float2(.08,0),float2(-.08,.12))),.022);
                    primary+=Stroke(r-.49,.020)*pow(saturate(sin(a*3+t*1.5)),2);
                    primary+=Stroke(r-.61,.012)*pow(saturate(cos(a*3+t*1.5)),3)*.65;
                }
                else if(_Style<4.5) // Empowered Ice Aegis: branching snowflake.
                {
                    float angle=round(a/(TWO_PI/6))*(TWO_PI/6);float2 q=Rotate(p,-angle);
                    float snow=Segment(q,float2(.03,0),float2(.49,0));
                    snow=min(snow,min(Segment(q,float2(.29,0),float2(.41,.13)),Segment(q,float2(.29,0),float2(.41,-.13))));
                    detail+=Stroke(snow,.018);
                    primary+=Stroke(Polygon(Rotate(p,t*.08),6,.57),.013)*.8;
                }
                else if(_Style<5.5) // Radiation: three toxic blades around an unstable core.
                {
                    float spin=a+t*.33;
                    float blade=smoothstep(.25,.37,r)*(1-smoothstep(.55,.59,r))*smoothstep(.25,.4,cos(spin*3));
                    primary+=blade*.65+Stroke(r-.16,.021);
                    detail+=Stroke(r-.61,.014)*smoothstep(-.1,.15,sin(spin*9));
                }
                else if(_Style<6.5) // Marksman: arrowheads around a sighting ring.
                {
                    float angle=round(a/(TWO_PI/4))*(TWO_PI/4);float2 q=Rotate(p,-angle);
                    detail+=Stroke(Segment(q,float2(.20,0),float2(.58,0)),.014);
                    detail+=Stroke(min(Segment(q,float2(.40,-.11),float2(.58,0)),Segment(q,float2(.40,.11),float2(.58,0))),.020);
                    primary+=Stroke(r-.35,.014)+Stroke(r-.66,.012)*(.7+.3*sin(a*8+t));
                }
                else if(_Style<7.5) // Coffee: amber bean seal and warm rising steam.
                {
                    float2 q=Rotate(p,.45);
                    primary+=Stroke(length(q*float2(1.5,1))-.34,.021);
                    detail+=Stroke(q.x+.045*sin(q.y*12),.018)*(1-smoothstep(.25,.30,abs(q.y)));
                    primary+=Stroke(Polygon(Rotate(p,t*.07),8,.62),.014);
                    detail+=Stroke(abs(p.x)-(.14+.025*sin(p.y*15-t)),.012)*smoothstep(.35,.39,p.y)*(1-smoothstep(.58,.65,p.y));
                }
                else // Infernal Presence: eight tongues of flame around an ember sun.
                {
                    float wave=sin(a*8-t*1.3);
                    primary+=Stroke(r-(.48+.08*wave),.025);
                    detail+=Stroke(r-.23,.023)+Stroke(Polygon(Rotate(p,-t*.07),8,.68),.014);
                    primary+=smoothstep(.25,.65,r)*(1-smoothstep(.70,.80,r))*pow(saturate(wave),5)*.6;
                }
                float alpha=saturate((primary+detail)*_Opacity*breath)*(1-smoothstep(.94,1,r));
                clip(alpha-.008);
                float3 tint=lerp(color,_TintB.rgb,saturate(detail/(primary+detail+.001))*.65);
                return half4(tint*1.2,alpha*.86);
            }
            ENDHLSL
        }
    }
}
