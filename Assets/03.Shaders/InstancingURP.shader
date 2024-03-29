Shader "Unlit/InstancingURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0
            struct BoidData
            {
                float3 velocity;
                float3 position;
            };
            StructuredBuffer<BoidData> _BoidDataBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 diff : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _ObjectScale;


            float4x4 eulerAnglesToRotationMatrix(float3 angles)
            {
                float ch = cos(angles.y); float sh = sin(angles.y); // heading
                float ca = cos(angles.z); float sa = sin(angles.z); // attitude
                float cb = cos(angles.x); float sb = sin(angles.x); // bank

                return float4x4(
                    ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
                    cb * sa, cb * ca, -sb, 0,
                    -sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
                    0, 0, 0, 1
                    );
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_TRANSFER_FOG(o,o.vertex);

                BoidData boidData = _BoidDataBuffer[v.instanceID];

                float3 pos = boidData.position.xyz;
                float3 scl = _ObjectScale;

                float4x4 object2world = (float4x4)0;
                object2world._11_22_33_44 = float4(scl.xyz, 1.0);
                float rotY =
                    atan2(boidData.velocity.x, boidData.velocity.z);
                float rotX =
                    -asin(boidData.velocity.y / (length(boidData.velocity.xyz) + 1e-8));
                float4x4 rotMatrix = eulerAnglesToRotationMatrix(float3(rotX, rotY, 0));
                object2world = mul(rotMatrix, object2world);
                object2world._14_24_34 += pos.xyz;

                o.vertex = UnityObjectToClipPos(mul(object2world, v.vertex));
                o.normal = UnityObjectToWorldNormal(v.normal);

				float nl = max(0, dot(o.normal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                col *= i.diff;
                return col;
            }
            ENDCG
        }
    }
}
