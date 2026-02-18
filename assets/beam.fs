#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

uniform float time;

void main()
{
    // CONFIGURACIÓN
    // ---------------------
    float beamSpeed = 6.0;      // Velocidad del flujo principal
    float beamDetail = 20.0;    // Cuántas "ondas" se ven verticalmente
    float thickness = 3.5;      // Grosor visual del núcleo (más alto = más fino)
    
    // COORDENADAS
    // ---------------------
    // Centramos la coordenada X en 0.0 (ahora va de -0.5 a 0.5)
    float uvX = fragTexCoord.x - 0.5;
    float uvY = fragTexCoord.y;

    // CAPAS DE ENERGÍA (El truco del "Espectáculo")
    // ---------------------
    
    // Capa 1: El flujo base (lento y grueso)
    float wave1 = sin(uvY * beamDetail - time * beamSpeed);
    
    // Capa 2: Interferencia eléctrica (rápida y fina)
    // Se mueve en dirección contraria o diferente velocidad para crear caos
    float wave2 = sin(uvY * beamDetail * 2.5 + time * beamSpeed * 1.5);
    
    // Capa 3: Ruido lateral (hace que el rayo "vibre" de lado a lado)
    float wobble = sin(uvY * 10.0 - time * 15.0) * 0.03;
    
    // Combinamos las ondas para calcular la intensidad en este píxel
    // Si la posición X coincide con la onda, es brillante.
    float intensity = 1.0 - (abs(uvX + wobble) * thickness);
    
    // Añadimos las ondas al grosor para que varíe
    intensity += (wave1 * 0.1) + (wave2 * 0.05);
    
    // Afilamos el rayo: pow hace que los valores bajos sean 0 y los altos se disparen
    intensity = clamp(intensity, 0.0, 1.0);
    intensity = pow(intensity, 3.0); 

    // FLICKER (Parpadeo de alto voltaje)
    // ---------------------
    // Usamos una función caótica simple para que la luz no sea constante
    float flicker = 0.8 + 0.2 * sin(time * 50.0) * sin(time * 20.0);

    // COLORES DE ALTA ENERGÍA
    // ---------------------
    // Color exterior (Halo)
    vec3 outerColor = vec3(0.0, 0.2, 1.0); // Azul eléctrico
    // Color interior (Núcleo caliente)
    vec3 innerColor = vec3(0.6, 0.9, 1.0); // Cian casi blanco
    // Núcleo fundido (Blanco puro para el centro absoluto)
    vec3 coreHot = vec3(1.0, 1.0, 1.0);

    // Mezclamos: Si la intensidad es baja -> Outer, si es media -> Inner, si es alta -> White
    vec3 beamColor = mix(outerColor, innerColor, intensity);
    beamColor = mix(beamColor, coreHot, pow(intensity, 2.0)); // El centro brilla blanco

    // BORDES SUAVES (Extremos del cilindro)
    // ---------------------
    float caps = smoothstep(0.0, 0.1, uvY) * smoothstep(1.0, 0.9, uvY);

    // SALIDA FINAL
    // ---------------------
    // El alpha depende de la intensidad. El espacio vacío es transparente.
    float alpha = intensity * caps * flicker;
    
    // Multiplicamos por fragColor.a por si quieres controlar transparencia global desde C++
    // DEBUG: Force solid red to check geometry visibility
    // finalColor = vec4(1.0, 0.0, 0.0, 1.0); 
    
    // DEBUG 2: Check UVs
    // finalColor = vec4(fragTexCoord.x, fragTexCoord.y, 0.0, 1.0);

    // RESTORE ORIGINAL logic but with safety
    // if (intensity < 0.0) intensity = 0.0;
    
    // Test: Force alpha 1.0
    finalColor = vec4(beamColor * flicker, 1.0);
}