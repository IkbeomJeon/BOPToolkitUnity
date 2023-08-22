Shader"Custom/PointCloud"
{
    Properties
    {
        _PointSize("Point Size", Range(1, 50)) = 5
        _MainColor("Main Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
            };

            float _PointSize;
            float4 _MainColor;

            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(1)]
                        void geom(point v2g vertices[1], inout PointStream<v2g> stream)
            {
                v2g o = vertices[0];
                stream.Append(o);
            }

            float4 frag(v2g i) : SV_Target
            {
                return _MainColor;
            }

            ENDCG
        }
    }
}
