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

const float fogDensity = 0.005; 
const float fogGradient = 1.5;

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    
    // Normal básica del modelo
    fragNormal = normalize(vec3(matNormal * vec4(vertexNormal, 1.0)));
    
    // Posición base
    vec3 pos = vertexPosition;
    vec3 worldPosCalculated = (matModel * vec4(vertexPosition, 1.0)).xyz;

    // === VIENTO (Hierba/Hojas) ===
    if (vertexColor.b > 0.3 && vertexColor.b < 0.6) { 
        float wind = sin(time * 2.0 + worldPosCalculated.x * 0.5 + worldPosCalculated.z * 0.5) * 0.08;
        pos.x += wind;
    }
    
    // === OLEAJE FÍSICO UNIFICADO ===
    // Aplicamos el MISMO movimiento físico suave a AMBOS tipos de agua
    // para evitar que se separen los vértices en las uniones.
    if (vertexColor.b > 0.7) {
        // Onda grande y lenta (mar de fondo)
        float swell = sin(worldPosCalculated.x * 0.3 + time * 0.5) * 0.03;
        // Onda pequeña cruzada
        float chop = cos(worldPosCalculated.z * 0.5 + time * 0.8) * 0.03;
        
        // Movimiento vertical suave
        pos.y += swell + chop;
    }
    
    fragPosition = vec3(matModel * vec4(pos, 1.0));
    vWorldPos = fragPosition;
    
    // Niebla
    vec4 relativePos = mvp * vec4(pos, 1.0);
    float dist = length(relativePos.xyz);
    vVisibility = exp(-pow((dist * fogDensity), fogGradient));
    vVisibility = clamp(vVisibility, 0.0, 1.0);
    
    gl_Position = relativePos;
}