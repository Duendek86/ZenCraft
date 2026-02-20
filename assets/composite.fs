#version 330

// Final Composite: combines the original scene with the blurred bloom
in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;  // Original scene
uniform sampler2D texture1;  // Blurred bloom
uniform float bloomIntensity; // How strong the bloom effect is (default ~1.0)

void main() {
    vec3 scene = texture(texture0, fragTexCoord).rgb;
    vec3 bloom = texture(texture1, fragTexCoord).rgb;
    
    // Additive blend (no tone mapping - game handles its own colors)
    vec3 result = scene + bloom * bloomIntensity;
    
    // Just clamp to prevent overflow
    result = clamp(result, 0.0, 1.0);
    
    finalColor = vec4(result, 1.0);
}
