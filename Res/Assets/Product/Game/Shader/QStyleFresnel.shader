Shader "Custom/QStyleFresnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.9, 0.95, 1.0, 0.85)
        _FresnelColor ("Fresnel Color", Color) = (0.9, 0.8, 1.0, 0.8)
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 3)) = 1.0
        _FresnelSoftness ("Fresnel Softness", Range(0, 1)) = 0.5
        _Alpha ("Transparency", Range(0, 1)) = 0.85
        _MinAlpha ("Min Alpha", Range(0, 1)) = 0.3
        _RimColor ("Rim Color", Color) = (1, 1, 1, 0.5)
        _RimPower ("Rim Power", Range(0.1, 10)) = 4.0
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.5
        _GlowColor ("Glow Color", Color) = (1, 0.9, 0.8, 0.3)
        _GlowPower ("Glow Power", Range(1, 10)) = 3.0
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 0.8
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
                float3 worldPos : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _FresnelColor;
            float _FresnelPower;
            float _FresnelIntensity;
            float _FresnelSoftness;
            float _Alpha;
            float _MinAlpha;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float4 _GlowColor;
            float _GlowPower;
            float _GlowIntensity;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = worldPos;
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                
                float NdotV = dot(normal, viewDir);
                
                float fresnel = 1.0 - saturate(NdotV);
                fresnel = pow(fresnel, _FresnelPower);
                fresnel = lerp(fresnel, smoothstep(0, 1, fresnel), _FresnelSoftness);
                
                float3 fresnelColor = _FresnelColor.rgb * fresnel * _FresnelIntensity;
                
                float rim = 1.0 - saturate(NdotV);
                rim = pow(rim, _RimPower);
                float3 rimColor = _RimColor.rgb * rim * _RimIntensity;
                
                float glow = pow(fresnel, _GlowPower);
                float3 glowColor = _GlowColor.rgb * glow * _GlowIntensity;
                
                float4 texColor = tex2D(_MainTex, i.uv);
                float3 baseColor = texColor.rgb * _Color.rgb;
                
                float3 finalColor = baseColor + fresnelColor + rimColor + glowColor;
                
                float alpha = _Color.a * texColor.a;
                alpha = lerp(_MinAlpha, alpha, _Alpha);
                
                float4 result = float4(finalColor, alpha);
                
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
