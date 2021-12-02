// VRChat MatCapLit shader, based on Unity's Mobile/Diffuse. Copyright (c) 2019 VRChat.

// Simple MatCapLit shader.
// -fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "VRChat/Mobile/MatCap Lit"
{
    Properties
    {    
        _MainTex("Texture", 2D) = "white" {}
        _MatCap ("MatCap (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #pragma skip_variants SHADOWS_SHADOWMASK SHADOWS_SCREEN SHADOWS_DEPTH SHADOWS_CUBE

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {    
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 color : TEXCOORD2;
                float4 indirect : TEXCOORD3;
                float4 direct : TEXCOORD4;
                float2 matcapUV : TEXCOORD5;
                SHADOW_COORDS(7)
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_DECLARE_TEX2D(_MainTex); 
            half4 _MainTex_ST;

            UNITY_DECLARE_TEX2D(_MatCap);

            float2 matcapSample(float3 viewDirection, float3 normalDirection)
            {
                half3 worldUp = float3(0,1,0);
                half3 worldViewUp = normalize(worldUp - viewDirection * dot(viewDirection, worldUp));
                half3 worldViewRight = normalize(cross(viewDirection, worldViewUp));
                half2 matcapUV = half2(dot(worldViewRight, normalDirection), dot(worldViewUp, normalDirection)) * 0.5 + 0.5;
                return matcapUV;
            }
  
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                
                half3 indirectDiffuse = ShadeSH9(float4(0, 0, 0, 1)); // We don't care about anything other than the color from GI, so only feed in 0,0,0, rather than the normal
                half4 lightCol = _LightColor0;
                 
                //If we don't have a directional light or realtime light in the scene, we can derive light color from a slightly modified indirect color.
                int lightEnv = int(any(_WorldSpaceLightPos0.xyz));
                if(lightEnv != 1)
                    lightCol = indirectDiffuse.xyzz * 0.2; 
            
                float4 lighting = lightCol; 
                
                o.color = v.color;
                o.direct = lighting;
                o.indirect = indirectDiffuse.xyzz;
                
                float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
                worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
                o.matcapUV = matcapSample(normalize(_WorldSpaceCameraPos - o.worldPos), UnityObjectToWorldNormal(v.normal)); //worldNorm.xy * 0.5 + 0.5; 
                
                TRANSFER_SHADOW(o);
                return o;
            }
            
            float4 frag (VertexOutput i, float facing : VFACE) : SV_Target
            {
                UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
            
                float4 albedo = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                float4 mc = UNITY_SAMPLE_TEX2D(_MatCap, i.matcapUV);
                half4 final = (albedo * i.color * mc) * (i.direct * attenuation + i.indirect);
                
                return float4(final.rgb, 1);
            }
            ENDCG
        }
    }

    Fallback "VRChat/Mobile/Diffuse"
}
