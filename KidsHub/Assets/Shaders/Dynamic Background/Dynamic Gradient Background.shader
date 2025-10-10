// Shader "Custom/Dynamic Gradient Background"
// {
//     Properties
//     {
//         [Header(Color Cycle 1 (e.g., Day))]
//         _Color1Top ("Top Color 1", Color) = (0.5, 0.7, 1.0, 1.0) // Light Sky Blue
//         _Color1Bottom ("Bottom Color 1", Color) = (0.0, 0.3, 0.6, 1.0) // Deep Blue/Horizon

//         [Header(Color Cycle 2 (e.g., Sunset))]
//         _Color2Top ("Top Color 2", Color) = (1.0, 0.8, 0.5, 1.0) // Warm Orange/Yellow
//         _Color2Bottom ("Bottom Color 2", Color) = (0.5, 0.1, 0.5, 1.0) // Deep Purple

//         [Header(Color Cycle 3 (e.g., Night))]
//         _Color3Top ("Top Color 3", Color) = (0.1, 0.1, 0.3, 1.0) // Dark Blue/Night
//         _Color3Bottom ("Bottom Color 3", Color) = (0.0, 0.0, 0.1, 1.0) // Black

//         [Header(Time Settings)]
//         [Range(0.01, 1.0)] _Speed ("Cycle Speed (Segments/Second)", Float) = 0.1 
//         // 0.1 means 10 seconds per transition (30 seconds for the full C1->C2->C3->C1 loop)
//     }

//     SubShader
//     {
//         // Suitable tags for a simple background plane/quad
//         Tags { "RenderType"="Opaque" "Queue"="Background" }
//         LOD 100

//         Pass
//         {
//             // Turn off depth testing and backface culling since we only care about the color
//             ZWrite Off
//             Cull Off
//             Fog { Mode Off }

//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
            
//             #include "UnityCG.cginc"

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float4 vertex : SV_POSITION;
//                 float2 uv : TEXCOORD0; // UV coordinates (0.0 at bottom, 1.0 at top)
//             };

//             // Properties
//             fixed4 _Color1Top;
//             fixed4 _Color1Bottom;
//             fixed4 _Color2Top;
//             fixed4 _Color2Bottom;
//             fixed4 _Color3Top;
//             fixed4 _Color3Bottom;
//             float _Speed;
            
//             // Basic vertex shader 
//             v2f vert (appdata v)
//             {
//                 v2f o;
//                 // Transform vertex position from object space to clip space
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 // Pass UV coordinates directly to the fragment shader
//                 o.uv = v.uv; 
//                 return o;
//             }
            
//             fixed4 frag (v2f i) : SV_Target
//             {
//                 // 1. Calculate a wrapped time factor in the range [0, 3]
//                 // Each unit (1.0) represents a full transition segment (e.g., C1->C2)
//                 float t_wrapped = fmod(_Time.y * _Speed, 3.0); 

//                 fixed4 currentTopColor;
//                 fixed4 currentBottomColor;
//                 float localFactor;

//                 // --- Determine which two color sets to blend between ---
                
//                 // Segment 1: Color 1 -> Color 2 (t_wrapped is 0.0 to 1.0)
//                 if (t_wrapped < 1.0) {
//                     localFactor = t_wrapped;
//                     currentTopColor = lerp(_Color1Top, _Color2Top, localFactor);
//                     currentBottomColor = lerp(_Color1Bottom, _Color2Bottom, localFactor);
//                 } 
//                 // Segment 2: Color 2 -> Color 3 (t_wrapped is 1.0 to 2.0)
//                 else if (t_wrapped < 2.0) {
//                     localFactor = t_wrapped - 1.0;
//                     currentTopColor = lerp(_Color2Top, _Color3Top, localFactor);
//                     currentBottomColor = lerp(_Color2Bottom, _Color3Bottom, localFactor);
//                 } 
//                 // Segment 3: Color 3 -> Color 1 (t_wrapped is 2.0 to 3.0)
//                 else {
//                     localFactor = t_wrapped - 2.0;
//                     currentTopColor = lerp(_Color3Top, _Color1Top, localFactor);
//                     currentBottomColor = lerp(_Color3Bottom, _Color1Bottom, localFactor);
//                 }
                
//                 // 2. Create the vertical gradient
//                 // The i.uv.y (UV Y-coordinate) goes from 0 (bottom) to 1 (top),
//                 // which determines the blend factor for the vertical gradient.
//                 fixed4 finalColor = lerp(currentBottomColor, currentTopColor, i.uv.y);
                
//                 return finalColor;
//             }
//             ENDCG
//         }
//     }
// }


Shader "Custom/Dynamic Gradient Background"
{
    Properties
    {
        [Space(10)] // Color Cycle 1 - Day
        _Color1Top ("Top Color 1 (Day)", Color) = (0.5, 0.7, 1.0, 1.0) 
        _Color1Bottom ("Bottom Color 1 (Day)", Color) = (0.0, 0.3, 0.6, 1.0) 

        [Space(10)] // Color Cycle 2 - Sunset
        _Color2Top ("Top Color 2 (Sunset)", Color) = (1.0, 0.8, 0.5, 1.0) 
        _Color2Bottom ("Bottom Color 2 (Sunset)", Color) = (0.8, 0.4, 0.3, 1.0) 

        [Space(10)] // Color Cycle 3 - Twilight
        _Color3Top ("Top Color 3 (Twilight)", Color) = (0.4, 0.1, 0.6, 1.0) 
        _Color3Bottom ("Bottom Color 3 (Twilight)", Color) = (0.1, 0.0, 0.2, 1.0) 

        [Space(10)] // Color Cycle 4 - Night
        _Color4Top ("Top Color 4 (Night)", Color) = (0.1, 0.1, 0.3, 1.0) 
        _Color4Bottom ("Bottom Color 4 (Night)", Color) = (0.0, 0.0, 0.1, 1.0) 

        [Space(10)] // Color Cycle 5 - Dawn
        _Color5Top ("Top Color 5 (Dawn)", Color) = (0.9, 0.5, 0.7, 1.0) 
        _Color5Bottom ("Bottom Color 5 (Dawn)", Color) = (0.6, 0.1, 0.3, 1.0) 

        [Space(10)] // Color Cycle 6 - Cloudy Day
        _Color6Top ("Top Color 6 (Cloudy)", Color) = (0.8, 0.9, 1.0, 1.0) 
        _Color6Bottom ("Bottom Color 6 (Cloudy)", Color) = (0.5, 0.6, 0.7, 1.0) 

        [Space(10)] // Color Cycle 7 - Golden Hour
        _Color7Top ("Top Color 7 (Golden)", Color) = (1.0, 0.9, 0.7, 1.0) 
        _Color7Bottom ("Bottom Color 7 (Golden)", Color) = (1.0, 0.7, 0.2, 1.0) 

        [Space(10)] // Color Cycle 8 - Hazy Sky
        _Color8Top ("Top Color 8 (Haze)", Color) = (0.7, 0.9, 0.9, 1.0) 
        _Color8Bottom ("Bottom Color 8 (Haze)", Color) = (0.4, 0.7, 0.8, 1.0) 

        [Space(10)] // Color Cycle 9 - Stormy
        _Color9Top ("Top Color 9 (Storm)", Color) = (0.4, 0.4, 0.4, 1.0) 
        _Color9Bottom ("Bottom Color 9 (Storm)", Color) = (0.1, 0.1, 0.1, 1.0) 

        [Space(10)] // Color Cycle 10 - Clear Midday
        _Color10Top ("Top Color 10 (Midday)", Color) = (0.3, 0.8, 1.0, 1.0) 
        _Color10Bottom ("Bottom Color 10 (Midday)", Color) = (0.1, 0.5, 0.9, 1.0) 

        [Space(10)] // Time Settings
        [Range(0.01, 1.0)] _Speed ("Cycle Speed (Segments/Second)", Float) = 0.1 
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Properties (20 total colors)
            fixed4 _Color1Top, _Color1Bottom;
            fixed4 _Color2Top, _Color2Bottom;
            fixed4 _Color3Top, _Color3Bottom;
            fixed4 _Color4Top, _Color4Bottom;
            fixed4 _Color5Top, _Color5Bottom;
            fixed4 _Color6Top, _Color6Bottom;
            fixed4 _Color7Top, _Color7Bottom;
            fixed4 _Color8Top, _Color8Bottom;
            fixed4 _Color9Top, _Color9Bottom;
            fixed4 _Color10Top, _Color10Bottom;
            float _Speed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Total number of segments (color cycles)
                const float NUM_SEGMENTS = 10.0;

                // 1. Calculate a wrapped time factor in the range [0, 10]
                // Each unit (1.0) represents a full transition segment (e.g., C1->C2)
                float t_wrapped = fmod(_Time.y * _Speed, NUM_SEGMENTS); 

                // Determine the current color index (0-9) and the blend factor (0.0-1.0)
                int current_idx = (int)floor(t_wrapped);
                float localFactor = t_wrapped - current_idx;

                // Determine the next color index (wraps 9 back to 0)
                int next_idx = (current_idx + 1) % (int)NUM_SEGMENTS; // Use int cast for safety with modulo

                fixed4 currentTopColor, currentBottomColor;
                fixed4 nextTopColor, nextBottomColor;

                // --- Helper macro to select the correct color properties based on index ---
                
                // Select CURRENT colors
                if (current_idx == 0) { currentTopColor = _Color1Top; currentBottomColor = _Color1Bottom; }
                else if (current_idx == 1) { currentTopColor = _Color2Top; currentBottomColor = _Color2Bottom; }
                else if (current_idx == 2) { currentTopColor = _Color3Top; currentBottomColor = _Color3Bottom; }
                else if (current_idx == 3) { currentTopColor = _Color4Top; currentBottomColor = _Color4Bottom; }
                else if (current_idx == 4) { currentTopColor = _Color5Top; currentBottomColor = _Color5Bottom; }
                else if (current_idx == 5) { currentTopColor = _Color6Top; currentBottomColor = _Color6Bottom; }
                else if (current_idx == 6) { currentTopColor = _Color7Top; currentBottomColor = _Color7Bottom; }
                else if (current_idx == 7) { currentTopColor = _Color8Top; currentBottomColor = _Color8Bottom; }
                else if (current_idx == 8) { currentTopColor = _Color9Top; currentBottomColor = _Color9Bottom; }
                else { currentTopColor = _Color10Top; currentBottomColor = _Color10Bottom; } // current_idx == 9

                // Select NEXT colors
                if (next_idx == 0) { nextTopColor = _Color1Top; nextBottomColor = _Color1Bottom; }
                else if (next_idx == 1) { nextTopColor = _Color2Top; nextBottomColor = _Color2Bottom; }
                else if (next_idx == 2) { nextTopColor = _Color3Top; nextBottomColor = _Color3Bottom; }
                else if (next_idx == 3) { nextTopColor = _Color4Top; nextBottomColor = _Color4Bottom; }
                else if (next_idx == 4) { nextTopColor = _Color5Top; nextBottomColor = _Color5Bottom; }
                else if (next_idx == 5) { nextTopColor = _Color6Top; nextBottomColor = _Color6Bottom; }
                else if (next_idx == 6) { nextTopColor = _Color7Top; nextBottomColor = _Color7Bottom; }
                else if (next_idx == 7) { nextTopColor = _Color8Top; nextBottomColor = _Color8Bottom; }
                else if (next_idx == 8) { nextTopColor = _Color9Top; nextBottomColor = _Color9Bottom; }
                else { nextTopColor = _Color10Top; nextBottomColor = _Color10Bottom; } // next_idx == 9
                
                // --- Perform Time-based Blend ---
                // 2. Interpolate the TOP colors and BOTTOM colors based on the localFactor
                fixed4 blendedTopColor = lerp(currentTopColor, nextTopColor, localFactor);
                fixed4 blendedBottomColor = lerp(currentBottomColor, nextBottomColor, localFactor);
                
                // --- Perform Vertical Gradient Blend ---
                // 3. Create the vertical gradient using the UV Y-coordinate (i.uv.y)
                fixed4 finalColor = lerp(blendedBottomColor, blendedTopColor, i.uv.y);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
