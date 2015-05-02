#version 330
 
in vec4 color;
out vec4 outputColor;
varying highp vec2 fTextureCoord;
uniform sampler2D uSampler;

varying highp vec3 vLighting;

void
main()
{
    //outputColor = color;
	gl_FragColor = texture2D(uSampler , vec2(fTextureCoord.s, 1.0 - fTextureCoord.t)) ; // * color

	//gl_FragColor = texture2D(uSampler, fTextureCoord);

	//gl_FragColor = vec4(color.rgb * vLighting, color.a);
}