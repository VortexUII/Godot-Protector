shader_type canvas_item;

uniform float speed = 1.5;
uniform float intensity = 0.6;
uniform vec4 base_color : source_color = vec4(0.1, 0.8, 0.3, 1.0);

void fragment() {
    vec2 uv = UV;

    // Create a smooth wave animation
    float wave = sin((uv.y + TIME * speed) * 6.0) * 0.5 + 0.5;

    // Add radial glow from center
    float dist = distance(uv, vec2(0.5));
    float glow = smoothstep(0.6, 0.2, dist);

    // Combine effects
    float effect = mix(wave, glow, 0.5) * intensity;

    vec3 green = base_color.rgb + vec3(0.0, effect, 0.0);

    COLOR = vec4(green, base_color.a);
}
