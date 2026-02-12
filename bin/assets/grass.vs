#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;
out vec3 fragPosition;

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;

uniform float time;
uniform vec3 viewPos; // Camera position for distance calculation

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    fragNormal = normalize(vec3(matNormal * vec4(vertexNormal, 1.0)));
    
    vec3 pos = vertexPosition;
    
    // Wind Effect (Same as lighting.vs)
    if (vertexColor.b > 0.3 && vertexColor.b < 0.5) { 
         float sway = sin(time * 3.0 + pos.x * 0.5 + pos.z * 0.5) * 0.1;
         pos.x += sway;
         pos.z += sway * 0.5; 
    }
    
    // --- Height Fading Logic REMOVED per user request ---
    // The grass stays tall at all distances.
    
    // float dist = distance(worldPos, viewPos);
    // ... fading logic removed ...
    
    // Just apply wind
    
    fragPosition = vec3(matModel * vec4(pos, 1.0));
    gl_Position = mvp * vec4(pos, 1.0);
}
