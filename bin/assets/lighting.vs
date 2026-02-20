#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;
out vec3 fragPosition;
out vec3 vWorldPos;
out float vVisibility;

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;
uniform float time;
uniform vec3 viewPos;
uniform int uUseEntityLight;

const float fogDensity = 0.005; 
const float fogGradient = 1.5;

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    
    // Normal básica del modelo sin aplicar posición (w=0.0 equivalente a mat3)
    fragNormal = normalize(mat3(matNormal) * vertexNormal);
    
    // Posición base
    vec3 pos = vertexPosition;
    vec3 worldPosCalculated = (matModel * vec4(vertexPosition, 1.0)).xyz;

    // === VIENTO (Hierba/Hojas) ===
    if (uUseEntityLight == 0 && vertexColor.b > 0.3 && vertexColor.b < 0.6) { 
        float wind = sin(time * 2.0 + worldPosCalculated.x * 0.5 + worldPosCalculated.z * 0.5) * 0.08;
        pos.x += wind;
    }
    
    // === OLEAJE FÍSICO UNIFICADO ===
    // Aplicamos el MISMO movimiento físico suave a AMBOS tipos de agua
    // para evitar que se separen los vértices en las uniones.
    if (uUseEntityLight == 0 && vertexColor.b > 0.7) {
        // Swell (big slow ocean wave)
        float swell = sin(worldPosCalculated.x * 0.3 + time * 0.5) * 0.05;
        // Cross chop
        float chop = cos(worldPosCalculated.z * 0.5 + time * 0.8) * 0.04;
        // Small ripples
        float ripple = sin(worldPosCalculated.x * 1.5 + worldPosCalculated.z * 1.2 + time * 2.0) * 0.015;
        
        pos.y += swell + chop + ripple;
    }
    
    fragPosition = vec3(matModel * vec4(pos, 1.0));
    vWorldPos = fragPosition;
    
    // Niebla real basada en posición global (no clip-space deformado)
    vec4 relativePos = mvp * vec4(pos, 1.0);
    float dist = distance(worldPosCalculated, viewPos);
    vVisibility = exp(-pow((dist * fogDensity), fogGradient));
    vVisibility = clamp(vVisibility, 0.0, 1.0);
    
    gl_Position = relativePos;
}