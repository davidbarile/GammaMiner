Shader "Custom/Sprites/Outline (Outer)"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _SpriteScale ("SpriteScale", Range(-1, 10.0)) = 1.0
        _SpriteScale2 ("SpriteScale2", Range(-1, 10.0)) = 1.0
        _Color("Tint", Color) = (1,1,1,1)
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vertexFunc
                #pragma fragment fragmentFunc
                #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_TexelSize;    // This is filled out by inspector, it gets size of
            float _SpriteScale;
            float _SpriteScale2;

            v2f vertexFunc(appdata_base v) {
                v.vertex.xyz *= _SpriteScale;
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            // fixed4 fragmentFunc(v2f IN) : COLOR // all caps cause its not just a name but a proper name
            // {
            //     float2 scaledCoord = IN.uv * _SpriteScale2;
            //     half4 c = tex2D(_MainTex, scaledCoord);    // Color at the current pixel
            //     //half4 c = tex2D(_MainTex, IN.uv);    // Color at the current pixel
            //     c.rgb *= c.a;                        // Make the RGB value equal to the alpha value
            //     half4 outlineC = _Color;            // Color of outline that we decided to be (1,1,1,1)

            //     fixed upAlpha = tex2D(_MainTex, IN.uv + fixed2(0, _MainTex_TexelSize.y)).a;
            //     fixed downAlpha = tex2D(_MainTex, IN.uv - fixed2(0, _MainTex_TexelSize.y)).a;
            //     fixed rightAlpha = tex2D(_MainTex, IN.uv + fixed2(_MainTex_TexelSize.x, 0)).a;
            //     fixed leftAlpha = tex2D(_MainTex, IN.uv - fixed2(_MainTex_TexelSize.x, 0)).a;

            //     fixed totalAlpha = min(ceil(upAlpha + downAlpha + rightAlpha + leftAlpha), 1);    // Only outer will be 0.  instead of 1, maybe c.a?
            //     outlineC.rgba *= totalAlpha;
            //     return lerp(outlineC, c, ceil(totalAlpha * c.a));
            // }

			fixed4 fragmentFunc(v2f IN) : COLOR // all caps cause its not just a name but a proper name
            {
                float2 scaledCoord = (IN.uv -0.5) * _SpriteScale + 0.5;
                half4 c = tex2D(_MainTex, scaledCoord);     // Color at the current pixel in the texture
                c.rgb *= c.a;                               // Multiply the RGB value by the alpha value, darkens half transparent pixels
                half4 outlineC = _Color;                    // Color of outline that we decided to be (1,1,1,1)

                half outlineWidth = _SpriteScale - 1;
               
                fixed upAlpha = tex2D(_MainTex, IN.uv + fixed2(0, _MainTex_TexelSize.y * outlineWidth)).a;
                fixed downAlpha = tex2D(_MainTex, IN.uv - fixed2(0, _MainTex_TexelSize.y * outlineWidth)).a;
                fixed rightAlpha = tex2D(_MainTex, IN.uv + fixed2(_MainTex_TexelSize.x * outlineWidth, 0)).a;
                fixed leftAlpha = tex2D(_MainTex, IN.uv - fixed2(_MainTex_TexelSize.x * outlineWidth, 0)).a;

                // mask for pixels outside original sprite range
                fixed factor = outlineWidth * 1/_SpriteScale * 0.5;
                fixed outsideUV = 1;
                outsideUV *= step(IN.uv.x, 1 - factor);
                outsideUV *= step(factor, IN.uv.x);
                outsideUV *= step(IN.uv.y, 1 - factor);
                outsideUV *= step(factor, IN.uv.y);
               
                fixed totalAlpha = min(ceil(upAlpha + downAlpha + rightAlpha + leftAlpha), 1); // Only outer will be 0.  instead of 1, maybe c.a?
                outlineC *= totalAlpha;
                half4 finalColor = lerp(outlineC, c, ceil(totalAlpha * outsideUV * c.a));

                return finalColor;
            }

        ENDCG
        }
    }
}