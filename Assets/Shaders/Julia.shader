Shader "Juliabrot/Julia"
{
	Properties
	{
		_ColorScheme("Texture", 2D) = "white" {}

		[IntRange] _NumIterations("Number of Iterations", Range(1, 1000)) = 50
	}

	SubShader 
	{
		Tags { "RenderType" = "Opaque" }

		Pass 
		{
			// https://github.com/nothke/JobifiedTextureModifyTests/blob/7423b6187248b940b9972d5199f9d2b684f826b7/Assets/Shaders/Mandelbrot.shader
			// https://github.com/nothke/JobifiedTextureModifyTests/blob/7423b6187248b940b9972d5199f9d2b684f826b7/Assets/Scripts/MandelbrotTest.cs#L127
			CGPROGRAM
			
			#pragma target 4.5
			#pragma vertex VertexFunc
			#pragma fragment FragmentFunc

			#pragma multi_compile __ DOUBLE_PRECISION

			#include "UnityCG.cginc"

			struct AppData {
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f {
				float4 position : SV_POSITION;
				float2 uv       : TEXCOORD0;
			};

			v2f VertexFunc(AppData IN) {
				v2f OUT;

				OUT.position = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;

				return OUT;
			}
			
			float3 Hue(float H) {
			    float R = abs(H * 6 - 3) - 1;
			    float G = 2 - abs(H * 6 - 2);
			    float B = 2 - abs(H * 6 - 4);
			    return saturate(float3(R,G,B));
			}

			float4 HSVtoRGB(in float3 HSV) {
			    return float4(((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z, 1);
			}

			float4 HSVtoRGB(in float h, in float s, in float v) {
			    return float4(((Hue(h) - 1) * s + 1) * v, 1);
			}
			
			void complexPow(inout double2 v, int power) {
				double2 ab = v;

				for (int i = 1; i < power; i++) {
					ab = double2(
						ab.x * v.x - ab.y * v.y, 
						ab.x * v.y + v.x * ab.y
					);
				}
				v = ab;
			}

			int Compute(double2 start, double2 c, int threshold, int order) {
				double2 ab = start;
				
				int iter = 0;
				const float MAX_MAG_SQUARED = 4;

				while ((iter < threshold) && (ab.x * ab.x + ab.y * ab.y) <= MAX_MAG_SQUARED) {
					complexPow(ab, order);
					ab += c;

					iter++;
				}
				return iter;
			}

			int _NumIterations;

			int _Order = 2;

			StructuredBuffer<double> _PositionBounds;

			fixed4 FragmentFunc(v2f IN) : SV_Target {
				// return fixed4(HSVtoRGB(IN.uv.x, IN.uv.y, 0.7));

				double2 position = double2(_PositionBounds[0], _PositionBounds[1]);
				double2 bounds = double2(_PositionBounds[2], _PositionBounds[3]);

				double2 start = position + bounds * double2(IN.uv);
				double2 coord = double2(_PositionBounds[4], _PositionBounds[5]);

				int numIters = _NumIterations;

				int iter = Compute(start, coord, numIters, _Order);

				float4 pixelColor;
				if (iter >= _NumIterations) {
					pixelColor = float4(0, 0, 0, 1);
				} else {
					float hpre = (iter + 170.0) * (1.0 / 255.0);
					float h = fmod(hpre, 1.0);
					float s = 0.7;
					float v = 1.0;

					pixelColor = HSVtoRGB(float3(h, s, 1.0));
				}

				return fixed4(pixelColor);
			}

			ENDCG
		}
	}
}