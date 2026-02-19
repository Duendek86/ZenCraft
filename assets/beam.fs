#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 worldPos;
out vec4 finalColor;

uniform float time;

// Simple 3D noise generator
float hash(vec3 p) {
    p = fract(p * 0.3183099 + .1);
    p *= 17.0;
    return fract(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float noise(vec3 x) {
    vec3 i = floor(x);
    vec3 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash(i + vec3(0,0,0)), hash(i + vec3(1,0,0)), f.x),
                   mix(hash(i + vec3(0,1,0)), hash(i + vec3(1,1,0)), f.x), f.y),
               mix(mix(hash(i + vec3(0,0,1)), hash(i + vec3(1,0,1)), f.x),
                   mix(hash(i + vec3(0,1,1)), hash(i + vec3(1,1,1)), f.x), f.y), f.z);
}

void main()
{
    // Fast flowing energy in Y direction (adjust to flow along the beam)
    // The beam is a cylinder, so its length is somewhat arbitrary to worldPos, 
    // but moving noise uniformly across worldspace looks great.
    vec3 flowPos = worldPos * 4.0;
    flowPos.y += time * -15.0; 
    flowPos.x += time * 2.0;

    float n1 = noise(flowPos);
    float n2 = noise(flowPos * 2.5 + vec3(time * 5.0, 0.0, time * 2.0));
    
    // Core energy stream
    float energy = pow(n1 * 0.6 + n2 * 0.4, 1.5) * 2.5;
    
    // Oscillating pulse
    float pulse = sin(worldPos.y * 5.0 - time * 20.0) * 0.5 + 0.5;
    energy *= 0.5 + 0.5 * pulse;
    
    // Base color from vertex (used for outer/core distinction)
    vec3 baseCol = fragColor.rgb;
    
    vec3 finalGlow = baseCol + vec3(0.5, 0.8, 1.0) * energy;
    
    // Add white hot core where energy is highest
    if(energy > 1.2) {
        finalGlow = mix(finalGlow, vec3(1.0, 1.0, 1.0), (energy - 1.2) * 2.0);
    }
    
    // Soft transparent edges based on energy
    float alpha = clamp(fragColor.a * (0.3 + energy * 0.8), 0.0, 1.0);
    
    finalColor = vec4(finalGlow, alpha);
}