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
   vec3 Kd; // Diffuse reflectivity
   vec3 Ka; // Ambient reflectivity
};

struct SpecularlLight
{
   vec3 Ks; // Specular reflectivity
   vec3 vDirection;
   float Shininess; 
};

uniform SimpleDirectionalLight sunLight; 
uniform SpecularlLight specLight;
smooth in vec3 vNormal; 
const int levels = 3;
const float scaleFactor = 1.0/levels;

in vec3 Position;
vec3 ads( );


void
main()
{
	vec4 vTexColor = texture2D(uSampler , vec2(fTextureCoord.s, 1.0 - fTextureCoord.t)) ; // * color

	float fDiffuseIntensity = max(0.0, dot(normalize(vNormal), -sunLight.vDirection));
	float toon = 0.0;
	if(fDiffuseIntensity < 0.25) toon = 0.0;
	else if(fDiffuseIntensity < 0.5) toon = 0.25;
	else if(fDiffuseIntensity < 0.75) toon = 0.5;
	else toon = 0.75;

   //outputColor = vTexColor*vec4(sunLight.vColor*(sunLight.fAmbientIntensity + toon), 1.0);
	outputColor = vTexColor*vec4(ads(), 1.0);

	//gl_FragColor = texture2D(uSampler, fTextureCoord);

	//gl_FragColor = vec4(color.rgb * vLighting, color.a);
}


vec3 ads( )
{

	float fDiffuseIntensity = max(0.0, dot(normalize(vNormal), -sunLight.vDirection));
	vec3 r = reflect( -specLight.vDirection, vNormal );
	vec3 v = normalize(vec3(-Position));

	return( sunLight.Ka + sunLight.vColor*(sunLight.Kd * fDiffuseIntensity) + 
		specLight.Ks * pow( max( dot(r,v), 0.0 ), specLight.Shininess ));

}