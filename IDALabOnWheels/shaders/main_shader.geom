#version 330

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VS_GS_INTERFACE
{
	vec3 position;
	vec3 normal;
	vec4 color;
	vec2 tex_coord;
} gs_in[];

smooth out vec3 vNormal;
smooth out vec2 fTextureCoord;
smooth out vec4 color;
out vec3 Position;

uniform float fBender;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform mat4 normalMatrix; // The appropriately normalized normal matrix

void PassThrough();


void main()
{

	PassThrough();


}


// A simple pass through geometry shader
void PassThrough()
{
	int n;
	mat4 mvp = projectionMatrix * viewMatrix * modelMatrix;
// Loop over the input vertices
	for (n = 0; n < gl_in.length(); n++)
	{
// Copy the input position to the output
		vec3 vPos = gl_in[n].gl_Position.xyz;
		gl_Position = mvp*vec4(vPos, 1.0);
		color = gs_in[n].color;
		Position = vec3( viewMatrix * modelMatrix * vec4(gs_in[n].position,1.0) );
		vec4 vRes = normalMatrix*vec4(gs_in[n].normal, 0.0);
		vNormal = vRes.xyz; 
		fTextureCoord = gs_in[n].tex_coord;
// Emit the vertex
		EmitVertex();
	} 
// End the primitive. This is not strictly necessary
// and is only here for illustrative purposes.
	EndPrimitive();
}

