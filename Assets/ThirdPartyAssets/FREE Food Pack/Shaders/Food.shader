// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.30 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.30;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:33005,y:32964,varname:node_3138,prsc:2|emission-7387-OUT,voffset-5379-OUT;n:type:ShaderForge.SFN_Tex2d,id:6596,x:32049,y:32678,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_6596,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Fresnel,id:4538,x:32198,y:33040,varname:node_4538,prsc:2|EXP-947-OUT;n:type:ShaderForge.SFN_Slider,id:947,x:31836,y:33083,ptovrint:False,ptlb:FresnelSize,ptin:_FresnelSize,varname:node_947,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.5,cur:0.6153846,max:5;n:type:ShaderForge.SFN_Multiply,id:1553,x:32403,y:33040,varname:node_1553,prsc:2|A-4087-RGB,B-4538-OUT,C-7929-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7929,x:32109,y:33257,ptovrint:False,ptlb:FresnelIntensity,ptin:_FresnelIntensity,varname:node_7929,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:7387,x:32620,y:32984,varname:node_7387,prsc:2|A-1553-OUT,B-6596-RGB;n:type:ShaderForge.SFN_Color,id:4087,x:32021,y:32887,ptovrint:False,ptlb:FresnelColor,ptin:_FresnelColor,varname:node_4087,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_NormalVector,id:6663,x:32491,y:33598,prsc:2,pt:False;n:type:ShaderForge.SFN_Slider,id:2515,x:31732,y:33502,ptovrint:False,ptlb:push,ptin:_push,varname:node_2515,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:0.01;n:type:ShaderForge.SFN_Multiply,id:5379,x:32763,y:33457,varname:node_5379,prsc:2|A-9666-OUT,B-6663-OUT;n:type:ShaderForge.SFN_Time,id:9990,x:31552,y:33646,varname:node_9990,prsc:2;n:type:ShaderForge.SFN_Sin,id:7895,x:31943,y:33658,varname:node_7895,prsc:2|IN-4146-OUT;n:type:ShaderForge.SFN_Multiply,id:9666,x:32470,y:33403,varname:node_9666,prsc:2|A-2515-OUT,B-8649-OUT;n:type:ShaderForge.SFN_RemapRange,id:8649,x:32131,y:33658,varname:node_8649,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-7895-OUT;n:type:ShaderForge.SFN_Multiply,id:4146,x:31759,y:33805,varname:node_4146,prsc:2|A-9990-T,B-469-OUT;n:type:ShaderForge.SFN_ValueProperty,id:469,x:31552,y:33839,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:node_469,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:6596-947-7929-4087-2515-469;pass:END;sub:END;*/

Shader "FREE Food Pack/Food" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _FresnelSize ("FresnelSize", Range(0.5, 5)) = 0.6153846
        _FresnelIntensity ("FresnelIntensity", Float ) = 1
        _FresnelColor ("FresnelColor", Color) = (0.5,0.5,0.5,1)
        _push ("push", Range(0, 0.01)) = 0
        _Speed ("Speed", Float ) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _FresnelSize;
            uniform float _FresnelIntensity;
            uniform float4 _FresnelColor;
            uniform float _push;
            uniform float _Speed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_9990 = _Time + _TimeEditor;
                v.vertex.xyz += ((_push*(sin((node_9990.g*_Speed))*0.5+0.5))*v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = ((_FresnelColor.rgb*pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelSize)*_FresnelIntensity)+_MainTex_var.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform float _push;
            uniform float _Speed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_9990 = _Time + _TimeEditor;
                v.vertex.xyz += ((_push*(sin((node_9990.g*_Speed))*0.5+0.5))*v.normal);
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
