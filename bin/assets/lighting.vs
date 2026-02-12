#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;
out vec3 fragPosition;

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;
uniform float time;

void main()
{
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    fragNormal = normalize(vec3(matNormal * vec4(vertexNormal, 1.0)));
    
    vec3 pos = vertexPosition;
    
    // Viento en hojas/hierba (Canal azul entre 0.3 y 0.5)
    if (vertexColor.b > 0.3 && vertexColor.b < 0.5) { 
        float sway = sin(time * 2.5 + pos.x * 0.8 + pos.z * 0.6) * 0.065;
        float sway2 = sin(time * 1.1 + pos.x * 1.3) * 0.035;
        pos.x += sway + sway2;
        pos.z += sway * 0.5;
    }
    
    // AGUA - Ondas Gerstner
    if (vertexColor.b > 0.9) {
        vec3 worldPos = (matModel * vec4(vertexPosition, 1.0)).xyz;
        
        // CORRECCIÓN: Usamos 'worldXZ' en lugar de 'xz'
        vec2 worldXZ = vec2(worldPos.x, worldPos.z);
        
        float height = 0.0;
        float dx = 0.0;
        float dz = 0.0;
        
        // Wave 1
        vec2 dir1 = normalize(vec2(0.8, 0.6));
        float freq1 = 0.42; float amp1 = 0.075; float speed1 = 0.9; float steep1 = 0.65;
        float phase1 = dot(dir1, worldXZ) * freq1 + time * speed1;
        float c1 = cos(phase1);
        height += amp1 * sin(phase1);
        dx += dir1.x * steep1 * amp1 * c1;
        dz += dir1.z * steep1 * amp1 * c1;
        
        // Wave 2
        vec2 dir2 = normalize(vec2(-0.7, 0.7));
        float freq2 = 0.85;
        float amp2 = 0.045; float speed2 = 1.7; float steep2 = 0.9;
        float phase2 = dot(dir2, worldXZ) * freq2 + time * speed2;
        float c2 = cos(phase2);
        height += amp2 * sin(phase2);
        dx += dir2.x * steep2 * amp2 * c2;
        dz += dir2.z * steep2 * amp2 * c2;
        
        // Wave 3 (ripples)
        vec2 dir3 = normalize(vec2(0.9, -0.4));
        float freq3 = 2.4;
        float amp3 = 0.022; float speed3 = 3.1; float steep3 = 1.1;
        float phase3 = dot(dir3, worldXZ) * freq3 + time * speed3;
        float c3 = cos(phase3);
        height += amp3 * sin(phase3);
        dx += dir3.x * steep3 * amp3 * c3;
        dz += dir3.z * steep3 * amp3 * c3;
        
        pos.x += dx;
        pos.z += dz;
        pos.y += height;
    }
    
    fragPosition = vec3(matModel * vec4(pos, 1.0));
    gl_Position = mvp * vec4(pos, 1.0);
}