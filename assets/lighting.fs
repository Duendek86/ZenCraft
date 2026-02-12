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
uniform vec3 uLightCol; // Color del sol (ej: 1.0, 0.9, 0.8)
uniform vec3 uAmbient;  // Color de sombra (ej: 0.1, 0.1, 0.3)
uniform vec3 viewPos;
uniform float time;

// Función para simular micro-relieve en el agua sin texturas extra
float waveNoise(vec2 p, float t) {
    float n = sin(p.x * 2.0 + t) * 0.5 + sin(p.y * 1.5 + t * 1.2) * 0.5;
    n += sin((p.x + p.y) * 4.0 - t * 2.0) * 0.2; // Detalles finos
    return n;
}

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);
    
    // Alpha Clipping para la hierba (para que se vea definida y no con bordes raros)
    if (texelColor.a < 0.5) discard;
    
    vec3 normal = normalize(fragNormal);
    vec3 lightDir = normalize(uLightDir);
    vec3 viewDir = normalize(viewPos - fragPosition);
    
    vec3 resultColor = vec3(0.0);
    float finalAlpha = texelColor.a;

    // ==========================================
    // 1. ILUMINACIÓN AGUA (Estilo Shader Pack)
    // ==========================================
    if (fragColor.b > 0.9) {
        // Data from vertex
        float skyLight = fragColor.g; // Sun/Sky light level attenutation (0.0-1.0)
        skyLight = pow(skyLight, 4.0); // Curve for darkness

        // Generate fake normals for waves
        float w = waveNoise(vWorldPos.xz * 0.5, time * 1.5);
        vec3 waterNormal = normalize(vec3(w * 0.2, 1.0, w * 0.15)); 
        
        // Fresnel
        float fresnel = pow(1.0 - max(dot(viewDir, waterNormal), 0.0), 3.0);
        
        // Dynamic Water Colors
        // Multiply by uAmbient to darken at night
        // Boost a bit because uAmbient is very dark now (0.001)
        vec3 ambientTerm = max(uAmbient, vec3(0.005)); 
        
        vec3 deepWater = vec3(0.05, 0.15, 0.3) * ambientTerm * 4.0; 
        vec3 surfaceWater = vec3(0.0, 0.4, 0.6) * ambientTerm * 4.0;
        
        // Sky Reflection should match Sky Color (uLightCol)
        // But uLightCol is Sunlight color. During day it's white.
        // We want blue reflection. 
        // Let's Mix uLightCol with a Sky Tint.
        vec3 skyTint = vec3(0.5, 0.7, 1.0);
        vec3 skyReflection = uLightCol * skyTint * 0.5; // Dampen slightly
        
        // Mix Base
        vec3 waterBase = mix(deepWater, surfaceWater, 0.5 + w * 0.2);
        
        // Apply Sky Light Attenuation (Shadows/Caves)
        waterBase *= skyLight;
        skyReflection *= skyLight;
        
        // Mix Reflection
        resultColor = mix(waterBase, skyReflection, fresnel * 0.8);
        
        // Specular (Sun/Moon Reflection)
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float spec = pow(max(dot(waterNormal, halfwayDir), 0.0), 256.0); 
        resultColor += uLightCol * spec * 2.0 * skyLight; // Mask spec by sky light
        
        // Visibility adjustments
        finalAlpha = clamp(0.35 + fresnel * 0.45, 0.0, 1.0);
    
    } 
    // ==========================================
    // 2. ILUMINACIÓN TERRENO (Hierba, Tierra, Bloques)
    // ==========================================
    else {
        // Data unpacking from Vertex Color
        float blockLight = fragColor.r; // 0.0 - 1.0 (Torch)
        float skyLight = fragColor.g;   // 0.0 - 1.0 (Sun/Sky)

        // --- Diffuse (Sun) ---
        // Sun only lights up blocks exposed to sky (skyLight > 0)
        // We also apply a shadow ramp so low sky light = no sun
        float diff = max(dot(normal, lightDir), 0.0);
        vec3 sunLight = (diff * uLightCol) * skyLight;
        
        // --- Ambient (Hemispheric) ---
        // Use uniform uAmbient for the base color (dynamic day/night)
        // Sky ambient is uAmbient. Ground ambient is dimmer.
        float hemiMix = normal.y * 0.5 + 0.5;
        vec3 ambientSky = uAmbient;
        vec3 ambientGround = uAmbient * 0.3; // Darker ground
        vec3 ambientColor = mix(ambientGround, ambientSky, hemiMix);
        
        // Ambient is also masked by Sky Light (Deep caves are dark)
        // We add a small base value so total darkness isn't pitch black if we want (optional)
        // But for "classic" feel, 0 sky light = 0 ambient from sky.
        vec3 finalAmbient = ambientColor * skyLight;
        
        // --- Torch Light ---
        // Warm color, quadratic falloff
        float torchIntensity = pow(blockLight, 2.0);
        vec3 torchColor = vec3(1.0, 0.7, 0.4); // More orange/warm
        vec3 finalTorch = torchColor * torchIntensity * 2.5; // Boost intensity
        
        // --- Combine ---
        vec3 lighting = sunLight + finalAmbient + finalTorch;
        
        // Ensure lighting doesn't blow out
        // lighting = min(lighting, vec3(1.5)); 

        resultColor = texelColor.rgb * lighting;

        // Apply a bit of saturation adjustment for vibrancy
        float luminance = dot(resultColor, vec3(0.2126, 0.7152, 0.0722));
        resultColor = mix(vec3(luminance), resultColor, 1.4); // 1.4 = 40% more saturation
    }

    // ==========================================
    // 3. POST-PROCESADO (Niebla y Gamma)
    // ==========================================
    
    // Aplicar niebla (azul cielo pálido)
    vec3 fogColor = mix(uAmbient, uLightCol, 0.5); // Dynamic Fog Color
    if (length(fogColor) < 0.1) fogColor = vec3(0.01, 0.01, 0.02); // Night fog
    
    resultColor = mix(fogColor, resultColor, vVisibility);

    // Tone Mapping & Gamma Correction (CRUCIAL para quitar el efecto "lavado")
    // Esto convierte el color lineal a espacio de color de monitor (sRGB)
    resultColor = resultColor / (resultColor + vec3(1.0)); // Reinhard Tone Mapping simple
    resultColor = pow(resultColor, vec3(1.0/2.2));       // Gamma Correction
    
    finalColor = vec4(resultColor, finalAlpha);
}