Shader "Custom/VolumetricDustWind"
{
    Properties
    {
        _Color ("Dust Color", Color) = (0.8,0.7,0.6,1)
        _Density ("Density", Range(0,2)) = 0.6
        _StepSize ("Ray Step Size", Range(0.01,0.2)) = 0.05
        _WindSpeed ("Wind Speed", Float) = 1
        _NoiseScale ("Noise Scale", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _Color;
            float _Density;
            float _StepSize;
            float _WindSpeed;
            float _NoiseScale;

            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(
                        lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                        lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x),
                        f.y),
                    lerp(
                        lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                        lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x),
                        f.y),
                    f.z);
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                o.viewDir = worldPos - GetCameraPositionWS();
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 rayDir = normalize(i.viewDir);
                float3 rayPos = i.worldPos;

                float alpha = 0;

                for (int s = 0; s < 32; s++)
                {
                    float3 p = rayPos * _NoiseScale;
                    p.x += _Time.y * _WindSpeed;

                    float d = noise(p);
                    alpha += d * _Density * _StepSize;

                    rayPos += rayDir * _StepSize;
                }

                alpha = saturate(alpha);
                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
