Shader "Custom/VerticalLineScroll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _LineWidth ("Line Width", Range(0.001, 0.2)) = 0.02
        _ScrollSpeed ("Scroll Speed", Range(0.1, 5)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _LineColor;
            float _LineWidth;
            float _ScrollSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample base image
                fixed4 col = tex2D(_MainTex, i.uv);

                // Create moving vertical line
                float time = _Time.y * _ScrollSpeed;
                float linePos = frac(time); // scroll loop (0–1)

                // Compare UV.x to line position
                float dist = abs(i.uv.x - linePos);
                float lineMask = smoothstep(_LineWidth, 0.0, dist);

                // Mix base + line
                col.rgb = lerp(col.rgb, _LineColor.rgb, lineMask * _LineColor.a);

                return col;
            }
            ENDCG
        }
    }
}
