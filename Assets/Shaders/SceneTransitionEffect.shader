Shader "Custom/SceneTransitionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0, 1)) = 0
        _CenterX ("Center X", Float) = 0.5
        _CenterY ("Center Y", Float) = 0.5
        _Radius ("Radius", Range(0, 2)) = 0.1
        _Smoothness ("Edge Smoothness", Range(0, 0.1)) = 0.01
        _Color ("Transition Color", Color) = (0, 0, 0, 1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _Progress;
            float _CenterX;
            float _CenterY;
            float _Radius;
            float _Smoothness;
            float4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算UV坐标与中心点的距离
                float2 center = float2(_CenterX, _CenterY);
                float dist = distance(i.uv, center);
                
                // 调整半径以实现扩散效果
                float currentRadius = _Progress * (1.0 + _Radius);
                
                // 计算边缘平滑过渡
                float edge = smoothstep(currentRadius - _Smoothness, currentRadius + _Smoothness, dist);
                
                // 混合原图与过渡颜色
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 finalColor = lerp(col, _Color, edge);
                
                // 调整最终颜色的透明度，以实现完全消失效果
                finalColor.a = _Progress < 0.01 ? 0.0 : edge;
                
                return finalColor;
            }
            ENDCG
        }
    }
}