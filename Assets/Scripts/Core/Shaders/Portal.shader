﻿
Shader "Custom/Portal"
{
    Properties
    {
        _InactiveColour ("Inactive Colour", Color) = (1, 1, 1, 1)
        _Dissolve("Dissove", Range(0,1)) = 0.0
        _DissolveTex ("Dissolve Texure", 2D) = "white" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
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
                float2 uv:TEXCOORD;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;                
                float4 screenPos : TEXCOORD0;
                float2 uv :TEXCOORD1;
            };
            sampler2D _MainTex;
            sampler2D _DissolveTex;
            float _Dissolve;
            float4 _InactiveColour;
            int displayMask; // set to 1 to display texture, otherwise will draw test colour

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv=v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                clip(tex2D(_DissolveTex,i.uv).r-_Dissolve);
                float2 uv = i.screenPos.xy/ i.screenPos.w;
                fixed4 portalCol = tex2D(_MainTex, uv);
                return portalCol * displayMask + _InactiveColour * (1-displayMask);
            }
            ENDCG
        }
    }
    Fallback "Standard" // for shadows
}
