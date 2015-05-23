#version 330
 
in vec3 vPosition;
in  vec3 vColor;
out vec4 colorPass;
in vec2 vTextureCoord;
out vec2  vTexCoordPass;

in vec3 vSurfaceNormal; // Surface normal of all the vertices


smooth out vec3 vNormalPass; 
 
 out vec3 PositionPass;


void
main()
{

    gl_Position =  vec4(vPosition, 1.0);
 
    colorPass = vec4( vColor, 1.0);
	vTexCoordPass = vTextureCoord;
	vNormalPass = vSurfaceNormal;
	//vec4 vRes = normalMatrix*vec4(vSurfaceNormal, 0.0);
   //vNormalPass = vRes.xyz; 
   PositionPass = vPosition;


}
