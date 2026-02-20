#version 330

in vec2 fragTexCoord;
in vec4 fragColor; 
in vec3 fragNormal;
in vec3 fragPosition;
in vec3 vWorldPos;
in float vVisibility;

out vec4 finalColor;

uniform sampler2D texture0;
uniform vec3 uLightDir;
uniform vec3 uLightCol; // Color del sol
uniform vec3 uAmbient;  // Color ambiental
uniform vec3 viewPos;
uniform float time;
uniform vec4 colDiffuse;
uniform int uUseEntityLight;
uniform vec2 uEntityLight;
uniform float uEmission;

// Función de ruido simple para simular agua sin texturas externas
float hash(vec2 p) { return fract(1e4 * sin(17.0 * p.x + p.y * 0.1) * (0.1 + abs(sin(p.y * 13.0 + p.x)))); }

float noise(vec2 x) {
    vec2 i = floor(x);
    vec2 f = fract(x);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// Función fractal para olas más detalladas
float fbm(vec2 x) {
    float v = 0.0;
    float a = 0.5;
    vec2 shift = vec2(100.0);
    mat2 rot = mat2(cos(0.5), sin(0.5), -sin(0.5), cos(0.50));
    for (int i = 0; i < 3; ++i) { // 3 octavas
        v += a * noise(x);
        x = rot * x * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

// ==========================================
// FUNCIÓN PRINCIPAL DE SUPERFICIE DE AGUA
// ==========================================
vec3 getWaterNormal(vec3 pos, float t, bool isFlowing) {
    float scale = 0.8; // Escala de las olas
    vec2 p = pos.xz * scale;
    
    float height = 0.0;
    
    if (isFlowing) {
        // --- AGUA EN MOVIMIENTO ---
        // Se desplaza rápidamente en una dirección (simulando corriente)
        // Usamos dos capas moviéndose a velocidades distintas para que no se vea como una textura estática deslizando
        float flowSpeed = 1.5;
        vec2 flowDir = vec2(0.7, 0.5); // Dirección diagonal
        
        height += fbm(p + t * flowSpeed * flowDir) * 0.6; 
        height += fbm(p * 1.5 - t * (flowSpeed * 0.8) * vec2(flowDir.y, -flowDir.x)) * 0.4; // Turbulencia cruzada
    } else {
        // --- AGUA ESTÁTICA ---
        // Movimiento orgánico y lento, sin dirección fija
        height += fbm(p + t * 0.2) * 0.5;
        height += fbm(p * 1.3 - t * 0.15) * 0.5;
    }
    
    // Convertir el mapa de altura en normales
    // Calculamos la diferencia de altura con puntos vecinos
    vec2 eps = vec2(0.1, 0.0);
    // Nota: simplificación de derivada para rendimiento
    float h1 = fbm((p + eps.xy) + (isFlowing ? t : 0.0)); 
    float h2 = fbm((p + eps.yx) + (isFlowing ? t : 0.0));
    
    // Construir vector normal
    // Cuanto más alto el multiplicador (2.0), más "picado" el mar
    vec3 n = normalize(vec3(height - h1, 2.0, height - h2)); 
    return n;
}

void main()
{
    vec4 baseColorMod = colDiffuse;
    if (baseColorMod.a == 0.0) { baseColorMod = vec4(1.0); }
    
    vec4 texelColor = texture(texture0, fragTexCoord) * baseColorMod;
    
    // Si estamos usando luz de entidad, asumimos que fragColor es el color real del vértice
    // y lo multiplicamos por el color de la textura (que será blanco por defecto para primitivas).
    if (uUseEntityLight > 0) {
        texelColor *= fragColor;
    }
    
    if (texelColor.a < 0.5) discard;

    vec3 normal = normalize(fragNormal);
    vec3 lightDir = normalize(uLightDir);
    vec3 viewDir = normalize(viewPos - fragPosition);
    vec3 resultColor = vec3(0.0);
    float finalAlpha = texelColor.a;

    // Detectar si es agua (Vertex color Blue > 0.7), pero solo si no es una entidad
    bool isWater = (uUseEntityLight == 0) && (fragColor.b > 0.7);

    if (isWater) {
        // ==========================================
        // ILUMINACIÓN DE AGUA AVANZADA
        // ==========================================
        
        // 1. Detectar tipo de agua
        // Si el azul está entre 0.7 y 0.85 es flujo, si es > 0.9 es estática
        bool isFlowing = (fragColor.b < 0.85);
        
        // 2. Obtener Normal del Agua (Unificada pero con comportamiento distinto)
        vec3 waterNormal = getWaterNormal(vWorldPos, time, isFlowing);
        
        // Si es flujo, añadimos un poco de espuma simulada basada en la altura de la ola
        float foam = 0.0;
        if (isFlowing) {
            foam = smoothstep(0.4, 0.7, waterNormal.y); // Crestas de olas más blancas
        }
        
        // 3. Colores Base (Gradient profundo)
        // Agua profunda oscura y superficie más clara turquesa
        vec3 colDeep = vec3(0.02, 0.05, 0.15);  // Azul marino oscuro
        vec3 colShallow = vec3(0.0, 0.35, 0.5); // Turquesa
        if (isFlowing) colShallow = vec3(0.2, 0.5, 0.6); // Flujo un poco más claro/espumoso
        
        // Mezcla basada en la normal (falso efecto de profundidad)
        vec3 albedo = mix(colDeep, colShallow, waterNormal.y * 0.5 + 0.5);
        
        // Añadir espuma en el flujo
        if (isFlowing) albedo = mix(albedo, vec3(0.9, 0.95, 1.0), foam * 0.3);

        // 4. Especular (El brillo del sol - Crucial para que se vea bien)
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specStrength = 1.0;
        float shininess = 128.0; // Cuanto más alto, más pequeño y nítido el punto de luz
        float spec = pow(max(dot(waterNormal, halfwayDir), 0.0), shininess);
        vec3 specularColor = uLightCol * spec * specStrength;

        // 5. Efecto Fresnel (Reflexión angular)
        // Si miras perpendicular (abajo), ves el fondo (transparente).
        // Si miras rasante, ves el cielo (reflejo).
        float fresnel = pow(1.0 - max(dot(viewDir, waterNormal), 0.0), 4.0);
        
        // Color del cielo aproximado (dinámico según el sol)
        // Usamos uLightCol (color del sol) para que de noche sea oscuro
        vec3 skyColor = uLightCol * vec3(0.4, 0.6, 0.9); 
        
        // Mezclamos el color base del agua con el reflejo del cielo según Fresnel
        vec3 waterFinal = mix(albedo * (uAmbient + 0.05), skyColor, fresnel * 0.6);
        
        // Añadimos el brillo del sol encima
        waterFinal += specularColor;
        
        // Iluminación básica de sombras (Sky light attenuation)
        float skyLight = pow(fragColor.g, 2.0); // Canal verde es luz de cielo
        waterFinal *= max(skyLight, 0.1); // Nunca totalmente negro

        resultColor = waterFinal;
        
        // Ajuste de Alpha: Más transparente en el centro, más opaco en ángulos rasantes
        finalAlpha = clamp(0.4 + fresnel * 0.5 + foam * 0.3, 0.0, 1.0);
        
    } else {
        // ==========================================
        // ILUMINACIÓN CRISTAL (Glassy & Glowing)
        // ==========================================
        float blockLight = (uUseEntityLight > 0) ? uEntityLight.x : fragColor.r;
        float skyLight = (uUseEntityLight > 0) ? uEntityLight.y : fragColor.g;

        // Base color ignoring texture (which is black for untextured models)
        vec3 crystalBase = vec3(0.1, 0.8, 0.5);

        // Difusa
        float diff = max(dot(normal, lightDir), 0.0);
        vec3 sunLight = (diff * uLightCol) * skyLight;
        
        // Ambiental
        vec3 ambientColor = uAmbient * (normal.y * 0.5 + 0.5); 
        
        // Luz de antorcha
        vec3 torchColor = vec3(1.0, 0.7, 0.4) * pow(blockLight, 2.0) * 2.0;
        
        // Specular reflections for the glassy look
        vec3 viewDir = normalize(viewPos - fragPosition);
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specStrength = 1.5;
        float shininess = 64.0;
        float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
        vec3 specularColor = uLightCol * spec * specStrength * skyLight;
        
        // Fresnel for glowing edges
        float fresnel = pow(1.0 - max(dot(viewDir, normal), 0.0), 2.0);
        vec3 edgeGlow = vec3(0.2, 1.0, 0.5) * fresnel * 0.8;

        vec3 lighting = sunLight + (ambientColor * skyLight) + torchColor;
        
        // Emisión base permanente (self-glow brillante verde)
        vec3 selfGlow = vec3(0.1, 0.9, 0.3) * 0.8;
        
        resultColor = (crystalBase * lighting) + specularColor + edgeGlow + selfGlow;
        
        // Emisión pulsante / dependiente de recarga
        vec3 emissionColor = vec3(0.2, 1.0, 0.3); // Bright green emission
        resultColor += (crystalBase * emissionColor * uEmission * 1.5);
        
        finalAlpha = 0.85; // Fixed translucency since texelColor.a could be 0
    }
    // ==========================================
    // POST-PROCESADO
    // ==========================================
    
    // Niebla
    vec3 fogColor = mix(uAmbient, uLightCol, 0.5);
    if (length(fogColor) < 0.1) fogColor = vec3(0.01, 0.01, 0.02);
    resultColor = mix(fogColor, resultColor, vVisibility);
    
    // Clamp to avoid artifacts
    resultColor = clamp(resultColor, 0.0, 1.0);
    
    finalColor = vec4(resultColor, finalAlpha);
}