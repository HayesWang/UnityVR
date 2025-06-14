Shader "Custom/Outline" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (1,0.8,0,1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.015
        [Toggle] _ScaleWithDistance ("Scale With Distance", Float) = 1
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        
        // 只渲染描边
        Pass {
            Name "OUTLINE"
            
            Cull Front // 只渲染背面
            ZWrite On
            ZTest Less // 只在通过深度测试时渲染
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _ScaleWithDistance;
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            v2f vert(appdata v) {
                v2f o;
                
                // 将法线转换到视图空间
                float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                // 将顶点转换到裁剪空间
                float4 posCS = UnityObjectToClipPos(v.vertex);
                
                // 根据法线和深度来调整描边宽度
                float3 normalCS = normalize(mul((float3x3)UNITY_MATRIX_P, normalVS));
                
                // 使用屏幕空间偏移确保描边宽度在屏幕上保持一致
                float2 offset = normalize(normalCS.xy) * _OutlineWidth;
                
                // 如果启用了距离缩放，使描边宽度与相机距离成比例
                if (_ScaleWithDistance > 0.5) {
                    // 物体中心到相机的距离
                    float distance = length(UnityObjectToViewPos(float4(0,0,0,1)).xyz);
                    offset *= distance * 0.05; // 根据距离调整偏移量
                }
                
                // 为小物体设定最小描边宽度
                float minOffset = 0.003;
                offset = max(offset, float2(minOffset, minOffset));
                
                // 应用偏移
                posCS.xy += offset * posCS.w;
                
                o.pos = posCS;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}