Shader "Unlit/Instancing"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        
        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow
        #pragma instancing_options procedural:setup

		struct Input
		{
			float2 uv_MainTex;
		};
		struct BoidData
		{
			float3 velocity; 
			float3 position;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<BoidData> _BoidDataBuffer;
		#endif
		sampler2D _MainTex; 

		half   _Glossiness; 
		half   _Metallic;   
		fixed4 _Color;      

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

		void vert(inout appdata_full v)
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

			BoidData boidData = _BoidDataBuffer[unity_InstanceID];

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

			v.vertex = mul(object2world, v.vertex);
			v.normal = normalize(mul(object2world, v.normal));
#endif
		}

		void setup()
		{
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
            
        ENDCG
    }
    
}
