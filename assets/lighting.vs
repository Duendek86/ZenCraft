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
out float vVisibility; // Para la niebla en el vertice (más optimizado)

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;
uniform float time;

const float fogDensity = 0.007; // Ajusta esto: 0.005 (lejos) a 0.02 (cerca)
const float fogGradient = 1.5;

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    
    // Normales corregidas para iluminación suave
    fragNormal = normalize(vec3(matNormal * vec4(vertexNormal, 1.0)));
    
    vec3 pos = vertexPosition;
    vec3 worldPosCalculated = (matModel * vec4(vertexPosition, 1.0)).xyz;
    
    // === VIENTO EN LA HIERBA/HOJAS ===
    // Detectamos hierba si el canal azul está entre 0.3 y 0.5 (según tu código anterior)
    if (vertexColor.b > 0.3 && vertexColor.b < 0.6) { 
        // Movimiento complejo: base lenta + punta rápida
        float wind = sin(time * 1.5 + worldPosCalculated.x * 0.5 + worldPosCalculated.z * 0.5) * 0.1;
        wind += sin(time * 3.0 + worldPosCalculated.z) * 0.05; // Turbulencia
        
        // Aplicamos solo a la parte superior del bloque (si pudiéramos saberlo),
        // pero como es voxel, lo aplicamos general.
        pos.x += wind;
    }
    
    // === OLEAJE FÍSICO DEL AGUA ===
    if (vertexColor.b > 0.9) {
        float wave = sin(worldPosCalculated.x * 0.5 + time * 0.8) * 0.05;
        wave += cos(worldPosCalculated.z * 0.4 + time * 0.6) * 0.05;
        pos.y += wave;
    }
    
    fragPosition = vec3(matModel * vec4(pos, 1.0));
    vWorldPos = fragPosition;
    
    // Cálculo de niebla en vertex shader (mejor rendimiento)
    vec4 relativePos = mvp * vec4(pos, 1.0);
    float dist = length(relativePos.xyz);
    vVisibility = exp(-pow((dist * fogDensity), fogGradient));
    vVisibility = clamp(vVisibility, 0.0, 1.0);
    
    gl_Position = relativePos;
}