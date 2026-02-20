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

uniform vec3 uCrystalPoints[16];
uniform int uCrystalCount;

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
    for (int i = 0; i < 4; ++i) { // 4 octaves for more detail
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
        // WATER - VIVID DRAMATIC RENDERING
        // ==========================================
        
        bool isFlowing = (fragColor.b < 0.85);
        vec3 waterNormal = getWaterNormal(vWorldPos, time, isFlowing);
        
        // --- ANIMATED CAUSTICS ---
        vec2 cuv = vWorldPos.xz;
        float c1 = fbm(cuv * 0.6 + time * vec2(0.15, 0.1));
        float c2 = fbm(cuv * 0.9 - time * vec2(0.1, 0.18) + 3.7);
        // Sharp caustic lines
        float caustic = pow(abs(sin(c1 * 6.28) * sin(c2 * 6.28)), 0.3);
        
        // --- FOAM ---
        float foam = 0.0;
        if (isFlowing) {
            foam = smoothstep(0.3, 0.55, waterNormal.y);
        }
        
        // --- MUCH BRIGHTER BASE COLORS ---
        vec3 colDeep    = vec3(0.02, 0.12, 0.25);   // Rich deep blue
        vec3 colShallow = vec3(0.08, 0.45, 0.55);   // Vivid teal
        if (isFlowing) {
            colDeep    = vec3(0.05, 0.18, 0.30);
            colShallow = vec3(0.15, 0.55, 0.60);
        }
        
        float df = waterNormal.y * 0.5 + 0.5;
        vec3 albedo = mix(colDeep, colShallow, df);
        
        // Strong caustic overlay - bright teal lines on surface
        albedo += vec3(0.15, 0.5, 0.55) * caustic * 0.6;
        
        // Foam
        if (isFlowing) albedo = mix(albedo, vec3(0.9, 0.95, 1.0), foam * 0.6);

        // --- SPECULAR ---
        vec3 halfDir = normalize(lightDir + viewDir);
        float specHard  = pow(max(dot(waterNormal, halfDir), 0.0), 256.0);
        float specSoft  = pow(max(dot(waterNormal, halfDir), 0.0), 32.0);
        float specBroad = pow(max(dot(waterNormal, halfDir), 0.0), 8.0);
        
        vec3 specColor = uLightCol * specHard * 5.0
                       + uLightCol * specSoft * 0.8
                       + uLightCol * specBroad * 0.2;

        // --- FRESNEL ---
        float fresnel = pow(1.0 - max(dot(viewDir, waterNormal), 0.0), 3.0);
        vec3 skyReflect = uLightCol * vec3(0.6, 0.8, 1.0);
        
        // Final water color - ambient provides minimum brightness
        vec3 waterFinal = albedo * max(uAmbient * 1.5, vec3(0.15));
        
        // Sky reflection via Fresnel
        waterFinal = mix(waterFinal, skyReflect, fresnel * 0.5);
        
        // Sun sparkle
        waterFinal += specColor;
        
        // Crystal glow on water
        for(int i=0; i<uCrystalCount; i++) {
             float dist = distance(vWorldPos, uCrystalPoints[i]);
             if (dist < 12.0) {
                 float intensity = pow(1.0 - (dist / 12.0), 2.0) * 0.8;
                 waterFinal += vec3(0.1, 0.9, 0.4) * intensity;
             }
        }
        
        // Sky light - but keep minimum brightness so water is never black
        float skyLight = pow(fragColor.g, 1.5); // Less aggressive darkening
        waterFinal *= max(skyLight, 0.25);

        resultColor = waterFinal;
        finalAlpha = clamp(0.55 + fresnel * 0.4 + foam * 0.15, 0.0, 0.88);
        
    } else {
        // ==========================================
        // ILUMINACIÓN TERRENO ESTÁNDAR (Simplificada)
        // ==========================================
        float blockLight = (uUseEntityLight > 0) ? uEntityLight.x : fragColor.r;
        float skyLight = (uUseEntityLight > 0) ? uEntityLight.y : fragColor.g;

        // Difusa
        float diff = max(dot(normal, lightDir), 0.0);
        vec3 sunLight = (diff * uLightCol) * skyLight;
        
        // Ambiental
        vec3 ambientColor = uAmbient * (normal.y * 0.5 + 0.5); // Hemisférica
        
        // Luz de antorcha
        vec3 torchColor = vec3(1.0, 0.7, 0.4) * pow(blockLight, 2.0) * 2.0;
        
        // --- Crystal Dynamic Glow ---
        vec3 crystalGlow = vec3(0.0);
        for(int i=0; i<uCrystalCount; i++) {
             float dist = distance(vWorldPos, uCrystalPoints[i]);
             if (dist < 8.0) {
                 float intensity = pow(1.0 - (dist / 8.0), 2.0) * 0.8;
                 crystalGlow += vec3(0.1, 1.0, 0.3) * intensity;
             }
        }
        
        vec3 lighting = sunLight + (ambientColor * skyLight) + torchColor + crystalGlow;
        resultColor = texelColor.rgb * lighting;
        
        // Añadir emisión (brillo propio) sin verse afectado por las luces
        resultColor += texelColor.rgb * uEmission;
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