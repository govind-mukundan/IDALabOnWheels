#version 330
 
in vec3 vPosition;
in  vec3 vColor;
out vec4 color;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
attribute vec2 vTextureCoord;
varying highp vec2 fTextureCoord;

// Lighting
attribute vec3 vSurfaceNormal; // Surface normal of all the vertices
uniform mat4 normalMatrix; // The appropriately normalized normal matrix

varying highp vec3 vLighting;
smooth out vec3 vNormal; 
 
 out vec3 Position;

void
main()
{
    gl_Position =  projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition, 1.0);
 
    color = vec4( vColor, 1.0);
	fTextureCoord = vTextureCoord;
	//fTextureCoord = vPosition.xy * vec2(0.5) + vec2(0.5);

	   vec4 vRes = normalMatrix*vec4(vSurfaceNormal, 0.0);
   vNormal = vRes.xyz; 
   Position = vec3( viewMatrix * modelMatrix * vec4(vPosition,1.0) );


	        // Apply lighting effect
        
       /* highp vec3 ambientLight = vec3(0.6, 0.6, 0.6);
        highp vec3 directionalLightColor = vec3(0.5, 0.5, 0.75);
        highp vec3 directionalVector = vec3(0.85, 0.8, 0.75);
        
        highp vec4 transformedNormal = normalMatrix * vec4(vSurfaceNormal, 1.0);
        
        highp float directional = max(dot(transformedNormal.xyz, directionalVector), 0.0);
        vLighting = ambientLight + (directionalLightColor * directional); */

}
