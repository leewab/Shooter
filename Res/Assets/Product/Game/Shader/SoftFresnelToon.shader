Shader "Custom/SimpleSoftFresnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.9, 0.95, 1.0, 0.85)
        _FresnelColor ("Fresnel Color", Color) = (0.9, 0.8, 1.0, 0.8)
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 2)) = 1.0
        _Alpha ("Transparency", Range(0, 1)) = 0.85
    }
    
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _FresnelColor;
            float _FresnelPower;
            float _FresnelIntensity;
            float _Alpha;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                // 基础纹理
                float4 texColor = tex2D(_MainTex, i.uv);
                float3 baseColor = texColor.rgb * _Color.rgb;
                
                // 菲涅尔效果
                float3 normal = normalize(i.worldNormal);
                float fresnel = 1.0 - saturate(dot(normal, normalize(i.viewDir)));
                fresnel = pow(fresnel, _FresnelPower);
                float3 fresnelColor = _FresnelColor.rgb * fresnel * _FresnelIntensity;
                
                // 最终颜色
                float3 finalColor = baseColor + fresnelColor;
                
                // 透明度
                float alpha = _Color.a * texColor.a;
                alpha = max(alpha, 0.3);  // 确保最小透明度
                
                float4 result = float4(finalColor, alpha);
                
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}