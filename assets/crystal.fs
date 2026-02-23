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

// Función obsoleta purgada
void main()
{
    vec4 baseColorMod = colDiffuse;
    if (baseColorMod.a == 0.0) { baseColorMod = vec4(1.0); }
    
    vec4 texelColor = texture(texture0, fragTexCoord) * baseColorMod;
    
    // Si estamos usando luz de entidad, asumimos que fragColor es el color real del vértice
    if (uUseEntityLight > 0) {
        texelColor *= fragColor;
    }
    
    if (texelColor.a < 0.5) discard;

    vec3 normal = normalize(fragNormal);
    vec3 lightDir = normalize(uLightDir);
    vec3 viewDir = normalize(viewPos - fragPosition);
    vec3 resultColor = vec3(0.0);
    float finalAlpha = texelColor.a;

    // ==========================================
    // ILUMINACIÓN CRISTAL (Glassy & Glowing)
    // ==========================================
    float blockLight = (uUseEntityLight > 0) ? uEntityLight.x : fragColor.r;
    float skyLight = (uUseEntityLight > 0) ? uEntityLight.y : fragColor.g;

    // Base color ignoring texture (which is black for untextured models)
    vec3 crystalBase = vec3(0.02, 0.7, 0.2);

    // Difusa
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 sunLight = (diff * uLightCol) * skyLight;
    
    // Ambiental
    vec3 ambientColor = uAmbient * (normal.y * 0.5 + 0.5); 
    
    // Luz de antorcha
    vec3 torchColor = vec3(1.0, 0.7, 0.4) * pow(blockLight, 2.0) * 2.0;
    
    // Specular reflections for the glassy look
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float specStrength = 1.5;
    float shininess = 64.0;
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
    vec3 specularColor = uLightCol * spec * specStrength * skyLight;
    
    // Fresnel for glowing edges
    float fresnel = pow(1.0 - max(dot(viewDir, normal), 0.0), 2.0);
    vec3 edgeGlow = vec3(0.0, 0.9, 0.3) * fresnel * 0.8;

    vec3 lighting = sunLight + (ambientColor * skyLight) + torchColor;
    
    // Emisión base permanente (self-glow brillante verde)
    vec3 selfGlow = vec3(0.0, 0.8, 0.1) * 0.35;
    
    resultColor = (crystalBase * lighting) + specularColor + edgeGlow + selfGlow;
    
    // Emisión pulsante / dependiente de recarga
    vec3 emissionColor = vec3(0.0, 1.0, 0.15); // Bright green emission
    resultColor += (crystalBase * emissionColor * uEmission * 1.5);
    
    finalAlpha = 0.90; // Fixed translucency since texelColor.a could be 0

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