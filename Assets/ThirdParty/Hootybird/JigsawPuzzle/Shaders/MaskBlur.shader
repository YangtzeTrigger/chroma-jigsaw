Shader "JigsawPuzzle/Blur" {
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

    _BlurX ("X Blur", Range(0.0, 0.5)) = 0.001
    _BlurY ("Y Blur", Range(0.0, 0.5)) = 0.001

    _Focus ("Focus", Range(0.0, 1.0)) = 0
    _Distribution ("Distribution", Range(0.0, 1.0)) = 0.18
    _Iterations ("Iterations", Integer) = 5
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed _BlurX;
            fixed _BlurY;
       
            fixed _Focus;
            fixed _Distribution;
            int _Iterations;

            float4 tex2Dblur(float2 position, float2 offset)
            {
                const float2 blur_offset = position.xy + float2(_BlurX, _BlurY).xy * offset * (1 - _Focus);
                return tex2D(_MainTex, blur_offset);
            }
 
            float calculateWeight(float distance)
            {
                return lerp(1, _Distribution, distance);
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {                
                const int2 iterations = int2(_Iterations, _Iterations);
                const float centralPixelWeight = 1;
 
                float4 color_sum = float4(0,0,0,0);
                float weight_sum = 0;
 
                // Add central pixel
                color_sum += tex2Dblur(i.texcoord, float2(0, 0)) * centralPixelWeight;
                weight_sum += centralPixelWeight;
 
                // Add central column
                for (int horizontal = 1; horizontal < iterations.x; ++horizontal)
                {
                    const float offset = (float)horizontal / iterations.x;
                    const float weight = calculateWeight(offset);
                   
                    color_sum += tex2Dblur(i.texcoord, float2(offset, 0)) * weight;
                    color_sum += tex2Dblur(i.texcoord, float2(-offset, 0)) * weight;
                    weight_sum += weight * 2;
                }
 
                // Add central row
                for (int vertical = 1; vertical < iterations.y; ++vertical)
                {
                    const float offset = (float)vertical / iterations.y;
                    const float weight = calculateWeight(offset);
                   
                    color_sum += tex2Dblur(i.texcoord, float2(0, offset)) * weight;
                    color_sum += tex2Dblur(i.texcoord, float2(0, -offset)) * weight;
                    weight_sum += weight * 2;
                }
 
                // Add quads
                for (int x = 1; x < iterations.x; ++x)
                {
                    for (int y = 1; y < iterations.y; ++y)
                    {
                        float2 offset = float2((float)x / iterations.x, (float)y / iterations.y);
                        const float offsetLength = length(offset);
                        const float weight = calculateWeight(offsetLength);
                       
                        color_sum += tex2Dblur(i.texcoord, float2(offset.x, offset.y)) * weight;
                        color_sum += tex2Dblur(i.texcoord, float2(-offset.x, offset.y)) * weight;
                        color_sum += tex2Dblur(i.texcoord, float2(-offset.x, -offset.y)) * weight;
                        color_sum += tex2Dblur(i.texcoord, float2(offset.x, -offset.y)) * weight;
                        weight_sum += weight * 4;
                    }
                }
 
                float4 final_color = color_sum / weight_sum;

                UNITY_APPLY_FOG(i.fogCoord, final_color);
                return final_color;
            }
        ENDCG
    }
}

}
