Shader "Custom/WX_Template"
{
    Properties
    {
        // 基础属性
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        
        // 光照属性
        _Specular ("Specular", Range(0,1)) = 0.5
        _Gloss ("Gloss", Range(0.1, 256)) = 32
        
        // 开关属性
        [Toggle] _UseFog ("Enable Fog", Float) = 1
        [Toggle] _UseLightmap ("Enable Lightmap", Float) = 0
    }
    
    SubShader
    {
        Tags { 
            "RenderType" = "Opaque" 
            "LightMode" = "ForwardBase"
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile __ _USEFOG_ON
            #pragma multi_compile __ _USELIGHTMAP_ON
            
            // 必须的include
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            // 输入结构
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                #ifdef _USELIGHTMAP_ON
                float2 uv1 : TEXCOORD1;
                #endif
            };
            
            // 输出结构
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                #ifdef _USELIGHTMAP_ON
                float2 lightmapUV : TEXCOORD3;
                #endif
                #ifdef _USEFOG_ON
                UNITY_FOG_COORDS(4)
                #endif
                LIGHTING_COORDS(5, 6)
            };
            
            // 属性变量
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Specular;
            float _Gloss;
            
            // 顶点着色器
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 光照贴图UV
                #ifdef _USELIGHTMAP_ON
                o.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif
                
                // 雾效
                #ifdef _USEFOG_ON
                UNITY_TRANSFER_FOG(o, o.pos);
                #endif
                
                // 阴影
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                
                return o;
            }
            
            // 片元着色器
            fixed4 frag(v2f i) : SV_Target
            {
                // 纹理采样
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * _Color;
                
                // 光照计算
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                
                // 漫反射
                float ndotl = max(0, dot(normal, lightDir));
                float3 diffuse = ndotl * _LightColor0.rgb * col.rgb;
                
                // 高光
                float3 halfDir = normalize(lightDir + viewDir);
                float ndoth = max(0, dot(normal, halfDir));
                float specular = pow(ndoth, _Gloss) * _Specular;
                
                // 环境光
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * col.rgb * 0.5;
                
                // 阴影
                float attenuation = LIGHT_ATTENUATION(i);
                
                // 组合
                col.rgb = (diffuse + ambient) * attenuation + specular;
                
                // 光照贴图
                #ifdef _USELIGHTMAP_ON
                fixed3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
                col.rgb *= lightmap;
                #endif
                
                // 雾效
                #ifdef _USEFOG_ON
                UNITY_APPLY_FOG(i.fogCoord, col);
                #endif
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Mobile/VertexLit"
    CustomEditor "WXShaderGUI"
}