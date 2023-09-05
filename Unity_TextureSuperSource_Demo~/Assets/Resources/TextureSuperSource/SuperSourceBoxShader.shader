Shader "Unlit/CameraSwitcherFader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _InputMedia ("_InputMedia", 2D) = "white" {}
        _BoxSize("_BoxSize",Vector) = (0,0,0,0)
        _BoxScale("_BoxScale",Vector) = (1,1,0,0)
        _CropMode("_CropMode",int) = 0
//        _MultiplyColorA("MultiplyColorA",Color) = (1,1,1,1)
//        _WigglerValueB("WigglerB", Vector) = (0,0,0,0)
//        _ClipSizeB("_WigglerRangeB",Vector) = (0,0,0,0)
//        _MultiplyColorB("MultiplyColorB",Color) = (1,1,1,1)
//        _BlendA("Blend A", int) = 0
//        _BlendB("Blend B", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD4;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD4;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D_float _MainTex;
            float4 _MainTex_ST;
            
            float2 _BoxSize;
            float2 _BoxScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {


                // float scale = 1.-max(_BoxSize.x,_ClipSize.y);
                float2 pivot_uv = float2(0.5, 0.5); 
                float2 r = (i.uv - pivot_uv) * float2(1./_BoxScale.x, 1./_BoxScale.y);
                float2 uv = r+pivot_uv;
                uv = i.uv;


                float4  color = tex2D(_MainTex, uv);
                // float4  colB = tex2D(_TextureB, uv_b+_WigglerValueB);
                
                 // float4 col = lerp(colA,colB,_CrossFade);

                
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return color;
            }
            ENDCG
        }
    }
}
