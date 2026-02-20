#version 330

// Bloom Extract: Extracts bright pixels from the scene
in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0; // Scene texture
uniform float threshold;     // Brightness threshold (default ~0.8)

void main() {
    vec4 color = texture(texture0, fragTexCoord);
    
    // Use max component for brightness detection
    float brightness = max(max(color.r, color.g), color.b);
    
    if (brightness > threshold) {
        // Soft extraction - only truly bright elements
        float excess = (brightness - threshold) / (1.001 - threshold);
        finalColor = vec4(color.rgb * excess * 1.5, 1.0);
    } else {
        finalColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}
