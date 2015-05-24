	#version 330

in vec3 vPosition;
in vec3 vColor;
in vec2 vTextureCoord;


in vec3 vSurfaceNormal; // Surface normal of all the vertices

out VS_GS_INTERFACE
{
	out vec3 position;
	out vec3 normal;
	out vec4 color;
	out vec2 tex_coord;
} vs_out;

void
main()
{

	gl_Position =  vec4(vPosition, 1.0);
	
	vs_out.color = vec4( vColor, 1.0);
	vs_out.tex_coord = vTextureCoord;
	vs_out.normal = vSurfaceNormal;
	//vec4 vRes = normalMatrix*vec4(vSurfaceNormal, 0.0);
   //vNormalPass = vRes.xyz; 
	vs_out.position = vPosition;


}
