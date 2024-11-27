Shader "CustomRenderTexture/AdditiveNoPostProcessing"
{
    Properties {
        _TintColor ("Tint Color", Color) = (1, 1, 1, 0.7)  // 기본 밝기 향상
        _MainTex ("Particle Texture", 2D) = "white" {}
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha  // Softer blending
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off

        SubShader {
            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _TintColor;

                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    #ifdef SOFTPARTICLES_ON
                    float4 projPos : TEXCOORD2;
                    #endif
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                float4 _MainTex_ST;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    #ifdef SOFTPARTICLES_ON
                    o.projPos = ComputeScreenPos(o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                    #endif
                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    return o;
                }

                UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
                float _InvFade;

                fixed4 frag (v2f i) : SV_Target
                {
                    #ifdef SOFTPARTICLES_ON
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                    float partZ = i.projPos.z;
                    float fade = saturate(_InvFade * (sceneZ - partZ));
                    i.color.a *= fade;
                    #endif

                    // 기본 텍스처에서 색상을 가져옴
                    fixed4 col = i.color * _TintColor * tex2D(_MainTex, i.texcoord);

                    // 색상 채도와 밝기를 조금 더 조절하여 강화
                    col.rgb = col.rgb * 1.3;  // 추가 밝기 증가
                    col.rgb = saturate(col.rgb);  // RGB 값을 제한하여 과다 밝기 방지

                    // 알파값 보정
                    col.a = saturate(col.a * 0.85);  // 부드럽게 블렌딩

                    // 감마 보정 최소화
                    #ifdef UNITY_COLORSPACE_GAMMA
                    col.rgb = GammaToLinearSpace(col.rgb * 1.1);  // 색상을 좀 더 선명하게
                    #endif

                    // 블랙으로의 포그 블렌딩
                    UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0, 0, 0, 0));
                    return col;
                }
                ENDCG
            }
        }
    }
}