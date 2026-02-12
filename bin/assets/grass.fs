#version 330

in vec2 fragTexCoord;
in vec4 fragColor; // r = Block Light, g = Sky Light
in vec3 fragNormal;
in vec3 fragPosition;

out vec4 finalColor;

uniform sampler2D texture0;

uniform vec3 uLightDir;
uniform vec3 uLightCol;
uniform vec3 uAmbient;
uniform vec3 viewPos;
uniform float time;

void main()
{
    // Sample texture for Alpha (Shape) only
    vec4 texelColor = texture(texture0, fragTexCoord);
    
    // Alpha Test
    if (texelColor.a < 0.5) discard;
    
    // Two-tone Gradient (User Request)
    // Map V coord (0.01 top -> 0.32 bottom) to 0..1
    float t = clamp((fragTexCoord.y - 0.01) / 0.30, 0.0, 1.0);
    
    vec3 topColor = vec3(0.5, 0.9, 0.2); // Lime/Yellow Green
    vec3 bottomColor = vec3(0.1, 0.6, 0.1); // Darker Green
    
    vec3 grassColor = mix(topColor, bottomColor, t);
    
    // --- LIGHTING (Copied from lighting.fs for consistency) ---
    
    float blockLightLevel = fragColor.r;
    float skyLightLevel = fragColor.g;
    skyLightLevel = pow(skyLightLevel, 4.0);

    vec3 lightDir = normalize(uLightDir);
    float NdotL = dot(fragNormal, lightDir);
    float sunShadowFactor = 0.7 + 0.3 * max(NdotL, 0.0); 
    
    vec3 skyBaseColor = uLightCol * sunShadowFactor;
    vec3 effectiveSkyLight = skyBaseColor + uAmbient * 0.2; 
    vec3 skyContribution = effectiveSkyLight * skyLightLevel;

    vec3 torchColor = vec3(1.0, 0.85, 0.6); 
    float blockIntensity = pow(blockLightLevel, 1.2);
    vec3 blockContribution = torchColor * blockIntensity;

    vec3 combinedLight = skyContribution + blockContribution;
    combinedLight = max(combinedLight, vec3(0.02)); // Min brightness

    vec3 finalRGB = grassColor * combinedLight;
    
    // --- FOG ---
    vec3 fogColor = mix(uAmbient, uLightCol, 0.5);
    fogColor = clamp(fogColor, 0.0, 0.9);

    float dist = gl_FragCoord.z / gl_FragCoord.w;
    float fogDensity = 0.02; 
    float fogFactor = 1.0 / exp(dist * fogDensity);
    fogFactor = clamp(fogFactor, 0.0, 1.0);

    float brightness = (fogColor.r + fogColor.g + fogColor.b) / 3.0;
    if (brightness < 0.1) fogColor *= 0.5;
    
    vec3 foggedRGB = mix(fogColor, finalRGB, fogFactor);
    
    finalColor = vec4(foggedRGB, 1.0);
}
