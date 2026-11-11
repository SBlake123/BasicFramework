Shader "Game Juice/Shockwave"
{
    Properties
    {
        _Color ("Ring Color", Color) = (0.86,0.95,1,0.75)
        _Progress ("Progress", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Progress;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(appdata_base input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.texcoord.xy;
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float progress = saturate(_Progress);
                float fade = 1.0 - progress;
                float radius = lerp(0.08, 0.98, 1.0 - fade * fade);
                float distanceToRing = abs(length(input.uv * 2.0 - 1.0) - radius);
                float width = lerp(0.065, 0.018, progress);
                float ring = 1.0 - smoothstep(width * 0.25, width, distanceToRing);
                return fixed4(_Color.rgb, _Color.a * ring * fade);
            }
            ENDCG
        }
    }
}
