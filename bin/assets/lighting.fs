#version 330

in vec2 fragTexCoord;
in vec4 fragColor; 
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
    vec4 texelColor = texture(texture0, fragTexCoord);
    if (texelColor.a < 0.5) discard;
    
    vec3 texColor = texelColor.rgb;
    float alpha = texelColor.a;
    
    vec3 normal = normalize(fragNormal);
    vec3 worldPos = fragPosition;
    vec3 viewDir = normalize(viewPos - worldPos);
    vec3 lightDir = normalize(uLightDir);
    
    // === AGUA ===
    if (fragColor.b > 0.9) {
        // CORRECCIÓN: Usamos 'worldXZ' en lugar de 'xz'
        vec2 worldXZ = vec2(worldPos.x, worldPos.z);
        
        float dx = 0.0;
        float dz = 0.0;
        
        vec2 dir1 = normalize(vec2(0.8, 0.6)); float freq1=0.42; float amp1=0.075; float steep1=0.65;
        float phase1 = dot(dir1, worldXZ) * freq1 + time * 0.9;
        dx += steep1 * amp1 * freq1 * cos(phase1) * dir1.x;
        dz += steep1 * amp1 * freq1 * cos(phase1) * dir1.z;
        
        vec2 dir2 = normalize(vec2(-0.7, 0.7)); float freq2=0.85; float amp2=0.045;
        float steep2=0.9;
        float phase2 = dot(dir2, worldXZ) * freq2 + time * 1.7;
        dx += steep2 * amp2 * freq2 * cos(phase2) * dir2.x;
        dz += steep2 * amp2 * freq2 * cos(phase2) * dir2.z;
        
        vec2 dir3 = normalize(vec2(0.9, -0.4)); float freq3=2.4; float amp3=0.022;
        float steep3=1.1;
        float phase3 = dot(dir3, worldXZ) * freq3 + time * 3.1;
        dx += steep3 * amp3 * freq3 * cos(phase3) * dir3.x;
        dz += steep3 * amp3 * freq3 * cos(phase3) * dir3.z;
        
        normal = normalize(vec3(-dx, 1.0, -dz));
        float fresnel = pow(1.0 - max(dot(viewDir, normal), 0.0), 3.5);
        
        vec3 waterDeep    = vec3(0.03, 0.22, 0.48);
        vec3 waterShallow = vec3(0.28, 0.80, 0.88);
        vec3 baseColor = mix(waterShallow, waterDeep, fresnel);
        
        vec3 skyReflect = mix(vec3(0.6, 0.78, 0.95), uLightCol, 0.4);
        
        // Caustics (usando worldXZ)
        float caustic = sin(worldXZ.x * 15.0 + time*4.0) * sin(worldXZ.y * 12.0 + time*3.2) * 0.6 + 0.8;
        caustic = pow(max(caustic, 0.0), 3.0) * 0.45;
        
        texColor = mix(baseColor, skyReflect, fresnel * 0.8);
        texColor += uLightCol * caustic * 0.35;
        
        vec3 halfDir = normalize(lightDir + viewDir);
        float spec = pow(max(dot(normal, halfDir), 0.0), 180.0);
        texColor += uLightCol * spec * 1.8;
        
        alpha = mix(0.45, 0.88, fresnel);
    }
    
    // Iluminación general
    float blockLightLevel = fragColor.r;
    float skyLightLevel   = pow(fragColor.g, 2.0);
    
    float NdotL = max(dot(normal, lightDir), 0.0);
    float sunShadowFactor = 0.22 + 0.78 * NdotL;
    
    vec3 skyContribution = uLightCol * sunShadowFactor * skyLightLevel;
    vec3 blockContribution = vec3(1.0, 0.8, 0.5) * pow(blockLightLevel, 1.1) * 1.1;
    
    vec3 combinedLight = max(skyContribution + blockContribution, vec3(0.04));
    vec3 finalRGB = texColor * combinedLight;
    
    // Niebla
    vec3 fogColor = mix(uAmbient, uLightCol, 0.5);
    float dist = gl_FragCoord.z / gl_FragCoord.w;
    float fogFactor = 1.0 / exp(dist * 0.018);
    fogFactor = clamp(fogFactor, 0.0, 1.0);
    finalRGB = mix(fogColor, finalRGB, fogFactor);
    
    finalColor = vec4(finalRGB, alpha);
}