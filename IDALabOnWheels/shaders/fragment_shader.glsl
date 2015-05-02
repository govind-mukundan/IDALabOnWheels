#version 330
 
in vec4 color;
out vec4 outputColor;
varying highp vec2 fTextureCoord;
uniform sampler2D uSampler;

varying highp vec3 vLighting;

struct SimpleDirectionalLight
{
   vec3 vColor;
   vec3 vDirection;
   float fAmbientIntensity;
};

uniform SimpleDirectionalLight sunLight; 
smooth in vec3 vNormal; 

void
main()
{
    //outputColor = color;
	//gl_FragColor = texture2D(uSampler , vec2(fTextureCoord.s, 1.0 - fTextureCoord.t)) ; // * color
	vec4 vTexColor = texture2D(uSampler , vec2(fTextureCoord.s, 1.0 - fTextureCoord.t)) ; // * color

	float fDiffuseIntensity = max(0.0, dot(normalize(vNormal), -sunLight.vDirection));
   outputColor = vTexColor*vec4(sunLight.vColor*(sunLight.fAmbientIntensity+fDiffuseIntensity), 1.0);

	//gl_FragColor = texture2D(uSampler, fTextureCoord);

	//gl_FragColor = vec4(color.rgb * vLighting, color.a);
}