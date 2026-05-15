Shader "UI/HeroOutlineGlow" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1, 0.84, 0.38, 1)
        _OutlineWidth ("Outline Width (px)", Range(0.5, 8)) = 3.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True"
               "RenderType"="Transparent" "PreviewType"="Plane"
               "CanUseSpriteAtlas"="True" }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float  _OutlineWidth;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv     = IN.uv;
                OUT.color  = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.uv);
                float selfAlpha = c.a;

                // 8 方向 alpha dilation：检查周围像素是否有不透明内容
                float2 step = _MainTex_TexelSize.xy * _OutlineWidth;
                float maxNeighbor = 0;
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2( step.x,  0     )).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2(-step.x,  0     )).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2( 0,       step.y)).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2( 0,      -step.y)).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2( step.x,  step.y)).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2(-step.x,  step.y)).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2( step.x, -step.y)).a);
                maxNeighbor = max(maxNeighbor, tex2D(_MainTex, IN.uv + float2(-step.x, -step.y)).a);

                // 轮廓区域 = 自身透明 但 邻域有不透明像素
                float isEdge = (1.0 - selfAlpha) * maxNeighbor;

                fixed4 outlineCol = _OutlineColor * IN.color;
                outlineCol.a = isEdge * _OutlineColor.a * IN.color.a;
                return outlineCol;
            }
            ENDCG
        }
    }
}
