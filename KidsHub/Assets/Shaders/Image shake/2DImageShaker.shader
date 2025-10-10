Shader "Custom/2DImageShaker"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        [Header(Shake Settings)]
        [Range(0.0, 0.5)] _ShakeMagnitude ("Shake Magnitude (Units)", Float) = 0.02
        [Range(1.0, 20.0)] _ShakeSpeed ("Shake Speed (Hz)", Float) = 8.0
        
        // Standard Sprite properties for rendering
        [MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
        [KeywordEnum(None, Alpha, Premultiply)] _AlphaMode ("Alpha Mode", Float) = 0
    }

    SubShader
    {
        // Standard Sprite blending and rendering tags
        Tags
        {
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            
            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            // Shader variables defined in Properties block
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _ShakeMagnitude;
            float _ShakeSpeed;

            // Vertex input structure
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Vertex to Fragment structure
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ** VERTEX SHADER **
            v2f vert (appdata_t v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // 1. Calculate time, scaled by user-defined speed
                float t = _Time.y * _ShakeSpeed;
                
                // 2. Generate displacement offsets using sine/cosine functions
                // We use different frequencies (t and t*1.7) for x and y 
                // to make the shake less linear and more organic.
                float xOffset = sin(t) * _ShakeMagnitude;
                float yOffset = cos(t * 1.7) * _ShakeMagnitude;

                // 3. Apply displacement to the vertex position (in object space)
                // We only modify the XY plane (for 2D image)
                v.vertex.xy += float2(xOffset, yOffset);
                
                // 4. Transform vertex from object space to clip space
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // 5. Pass UVs to the fragment shader
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap (o.vertex);
                #endif

                return o;
            }

            // ** FRAGMENT SHADER **
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // Sample the texture color
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Apply the tint color and texture alpha
                return col;
            }
            ENDCG
        }
    }
}
