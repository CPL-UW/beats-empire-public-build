Shader "Unlit/UnlitTexturedShadowCaster" {
	Properties {
		_MainTex("Texture", 2D) = "red" {}
		_ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
	}

	SubShader {
		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct vertex {
				float4 positionObject : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct fragment {
				float4 positionClip : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			fragment vert(vertex v) {
				fragment f;
				f.positionClip = UnityObjectToClipPos(v.positionObject);
				f.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return f;
			}
			
			fixed4 frag(fragment f) : SV_Target {
				return tex2D(_MainTex, f.uv);
			}

			ENDCG
		}

    Pass {
      Tags { "LightMode"="ForwardBase" }
      Offset -1.0, -2.0 // make sure shadow polygons are on top of shadow receiver
      //Blend SrcAlpha OneMinusSrcAlpha

      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      // User-specified uniforms
      uniform float4 _ShadowColor;
      uniform float4x4 _World2Receiver; // transformation from // world coordinates to the coordinate system of the plane

      float4 vert(float4 position : POSITION) : SV_POSITION {
         float4x4 modelMatrix = unity_ObjectToWorld;
         float4x4 modelMatrixInverse = unity_WorldToObject;
         float4x4 viewMatrix = mul(UNITY_MATRIX_MV, modelMatrixInverse);

         float4 lightDirection = -normalize(_WorldSpaceLightPos0);

         float4 vertexInWorldSpace = mul(modelMatrix, position);
         float4 world2ReceiverRow1 = float4(_World2Receiver[1][0], _World2Receiver[1][1], _World2Receiver[1][2], _World2Receiver[1][3]);
         float distanceOfVertex = dot(world2ReceiverRow1, vertexInWorldSpace);
         float lengthOfLightDirectionInY = dot(world2ReceiverRow1, lightDirection);

         if (distanceOfVertex > 0.0) { // && lengthOfLightDirectionInY < 0.0) {
            lightDirection = lightDirection * (distanceOfVertex / (-lengthOfLightDirectionInY));
         } else {
            lightDirection = float4(0.0, 0.0, 0.0, 0.0); // don't move vertex
         }

         return mul(UNITY_MATRIX_VP, vertexInWorldSpace + lightDirection);
      }

      float4 frag(void) : COLOR {
         return _ShadowColor;
      }

      ENDCG
    } 
	}
}
