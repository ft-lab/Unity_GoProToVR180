//----------------------------------------------------------------.
// GoProの魚眼からVR180のequirectangularへの変換を行う.
//----------------------------------------------------------------.
Shader "Hidden/GoProToVR180/fishEyeToVR180" {
    Properties {
    }

    SubShader {
        Cull Off
        ZWrite On
        Fog { Mode Off }
		Tags { "RenderType"="Opaque" "Queue"="geometry-100" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0

            #define UNITY_PI2 (UNITY_PI * 2.0)

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // 写真のテクスチャ.
            sampler2D _LeftTex;
            sampler2D _RightTex;

            int _AngleH;                 // 写真の水平視野角度（度数）.
            int _AngleV;                 // 写真の垂直視野角度（度数）.
            float4 _BackgroundColor;     // 背景色.

            v2f vert (appdata_img v) {
                v2f o = (v2f)0;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
                return o;
            }

             /*
             * UVの魚眼レンズ補正.
             */
            float2 convFishEyeToEquirectangular (float2 uv) {
                float2 uv2 = uv;

				// FishEyeからequirectangularの変換.
				// reference : http://paulbourke.net/dome/fish2/
				float theta = UNITY_PI2 * (uv2.x - 0.5);
				float phi   = UNITY_PI * (uv2.y - 0.5);
				float sinP = sin(phi);
				float cosP = cos(phi);
				float sinT = sin(theta);
				float cosT = cos(theta);
				float3 vDir = float3(cosP * sinT, cosP * cosT, sinP);

				theta = atan2(vDir.z, vDir.x);
				phi   = atan2(sqrt(vDir.x * vDir.x + vDir.z * vDir.z), vDir.y);
				float r = phi / UNITY_PI; 

				uv2.x = 0.5 + r * cos(theta);
				uv2.y = 0.5 + r * sin(theta);

                return uv2;
            }

            float4 frag(v2f i) : SV_TARGET {
                float2 uv = i.uv;
                float4 col = _BackgroundColor;

                // 魚眼レンズ補正.
                uv = convFishEyeToEquirectangular(uv);

                float centerU = 0.5;
                float centerV = 0.5;
                float mx = 180.0 / _AngleH;
                float my = 180.0 / _AngleV;
                uv.x = (uv.x - centerU) * mx + centerU;
                uv.y = (uv.y - centerV) * my + centerV;
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return col;

                if (unity_StereoEyeIndex == 0) {
                    col = tex2D(_LeftTex, uv);
                } else {
                    col = tex2D(_RightTex, uv);
                }
                return col;
            }

            ENDCG        
        }
    }
}