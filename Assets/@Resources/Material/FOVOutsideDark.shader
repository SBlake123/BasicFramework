Shader "Custom/FOVOutsideDark"
{
    Properties
    {
        _Color("Color", Color) = (0, 0, 0, 0.7)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
        }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata value)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(value.vertex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}