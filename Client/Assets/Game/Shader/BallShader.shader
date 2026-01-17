Shader "Custom/MobileCartoonEmissive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _MainColor ("Main Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1.0
        _TextureBrightness ("Texture Brightness", Range(0, 3)) = 1.0
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.2
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 确保在无光照环境下渲染正确
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;
            half _EmissionIntensity;
            half _TextureBrightness;
            half _ShadowThreshold;
            half _RimIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 计算世界空间法线和视角方向
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 采样并提亮纹理，然后与主色融合（正片叠底）
                fixed4 texCol = tex2D(_MainTex, i.uv);
                texCol.rgb *= _TextureBrightness;
                fixed3 baseColor = texCol.rgb * _MainColor.rgb;

                // 2. 核心：二值化卡通阴影（硬边无渐变）
                i.worldNormal = normalize(i.worldNormal);
                // 使用法线与视角的简单点积模拟基础光照
                half ndotv = 1 - dot(i.worldNormal, normalize(i.viewDir));
                // step函数实现硬分割
                half toonLight = step(_ShadowThreshold, ndotv * 1.5);
                fixed3 diffuse = baseColor * toonLight;

                // 3. 卡通边缘光 (Rim Light)
                half rim = 1.0 - saturate(dot(i.worldNormal, normalize(i.viewDir)));
                rim = pow(rim, 2); // 固定2次方，保证宽度稳定
                fixed3 rimLight = rim * _RimIntensity * baseColor;

                // 4. 独立自发光 (不依赖任何光源)
                fixed3 emission = baseColor * _EmissionIntensity;

                // 5. 最终合成：基础色块 + 边缘光 + 自发光
                fixed3 finalColor = diffuse + rimLight + emission;

                // 应用雾效（如果场景启用）
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return fixed4(finalColor, 1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture" // 极简回退，保证兼容
}