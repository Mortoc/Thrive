Shader "Transparent/Aerial Perspective" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Transparency ("Trans", Range(0, 1)) = 0.5
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
float _Transparency;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 color = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = color.rgb;
	o.Alpha = _Transparency;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
