Shader "Unlit/Building"
{
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Scale ("Scale", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

    CGPROGRAM
    #pragma surface surf Lambert

    sampler2D _MainTex;
    fixed4 _Color;
    float _Scale;

    struct Input {
        float2 uv_MainTex;
        float3 worldPos;
        float3 worldNormal;
    };

    void surf (Input IN, inout SurfaceOutput o) {
        float3 absN = abs(IN.worldNormal);
        float3 albedoX = tex2D(_MainTex, IN.worldPos.zy * _Scale) * _Color * absN.x;
        float3 albedoY = tex2D(_MainTex, IN.worldPos.xz * _Scale) * _Color * absN.y;
        float3 albedoZ = tex2D(_MainTex, IN.worldPos.xy * _Scale) * _Color * absN.z;
        o.Albedo = albedoX + albedoY + albedoZ;
        o.Alpha = 1.0;
    }
    ENDCG
    }

    Fallback "Legacy Shaders/VertexLit"
}
