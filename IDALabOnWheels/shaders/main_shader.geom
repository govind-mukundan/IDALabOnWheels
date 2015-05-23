#version 330

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec2 vTexCoordPass[];
in vec3 vNormalPass[];
in mat4 mMVP[];
in vec4 colorPass[];
in vec3 PositionPass[];

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
void Tesselate();


void main()
{

PassThrough();
//Tesselate();

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
color = colorPass[n];
Position = PositionPass[n];
vNormal = vNormalPass[n];
fTextureCoord = vTexCoordPass[n];
// Emit the vertex
EmitVertex();
} 
// End the primitive. This is not strictly necessary
// and is only here for illustrative purposes.
EndPrimitive();
}

void Tesselate()
{
mat4 mvp = projectionMatrix * viewMatrix * modelMatrix;
vec3 vNormalsTransformed[3];

    vec3 vMiddle = (gl_in[0].gl_Position.xyz+gl_in[1].gl_Position.xyz+gl_in[2].gl_Position.xyz)/3.0+(vNormalPass[0]+vNormalPass[1]+vNormalPass[2])*0.8;
    vec2 vTexCoordMiddle = (vTexCoordPass[0]+vTexCoordPass[1]+vTexCoordPass[2])/3.0;
    for(int i = 0; i < 3; i++)vNormalsTransformed[i] = (vec4(vNormalPass[i], 1.0) * normalMatrix).xyz;
    vec3 vNormalMiddle = (vNormalsTransformed[0]+vNormalsTransformed[1]+vNormalsTransformed[2])/3.0;
    for(int i = 0; i < 3; i++)
    {
		color = colorPass[i];

		   Position = vec3( viewMatrix * modelMatrix * vec4(PositionPass[i],1.0) );
      vec3 vPos = gl_in[i].gl_Position.xyz;
      gl_Position = mvp*vec4(vPos, 1.0);
      vNormal = (vec4(vNormalsTransformed[i], 1.0)).xyz;
      fTextureCoord = vTexCoordPass[i];
      EmitVertex();

      vPos = gl_in[(i+1)%3].gl_Position.xyz;
      gl_Position = mvp*vec4(vPos, 1.0);
      vNormal = (vec4(vNormalsTransformed[(i+1)%3], 1.0)).xyz;
      fTextureCoord = vTexCoordPass[(i+1)%3];
      EmitVertex();

      gl_Position = mvp*vec4(vMiddle, 1.0);
      vNormal = vNormalMiddle;
      fTextureCoord = vTexCoordMiddle;
      EmitVertex();

      EndPrimitive();
    }


}