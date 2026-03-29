Shader "DoMiTruth/CircleWipe"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0, 1)) = 0
        _Color ("Color", Color) = (0, 0, 0, 1)
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.15
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Progress;
            float4 _Color;
            float _Softness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Tính khoảng cách từ pixel đến tâm (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv - center;

                // Correct aspect ratio (giữ hình tròn, không bị oval)
                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x *= aspect;

                float dist = length(uv);

                // Max radius = khoảng cách từ tâm đến góc (để cover hết màn hình)
                float maxRadius = length(float2(aspect * 0.5, 0.5));

                // radius hiện tại dựa theo progress
                // Progress 0 = mở hoàn toàn (radius lớn), Progress 1 = đóng hết (radius = 0)
                float radius = (1.0 - _Progress) * maxRadius;

                // Smooth edge
                float alpha = smoothstep(radius, radius + _Softness, dist);

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
