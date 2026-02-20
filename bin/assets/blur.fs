#version 330

// Gaussian Blur - single pass (horizontal OR vertical based on uniform)
in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 direction; // (1/w, 0) for horizontal, (0, 1/h) for vertical

void main() {
    // 9-tap Gaussian kernel weights
    float weights[5] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
    
    vec3 result = texture(texture0, fragTexCoord).rgb * weights[0];
    
    for (int i = 1; i < 5; i++) {
        vec2 offset = direction * float(i);
        result += texture(texture0, fragTexCoord + offset).rgb * weights[i];
        result += texture(texture0, fragTexCoord - offset).rgb * weights[i];
    }
    
    finalColor = vec4(result, 1.0);
}
