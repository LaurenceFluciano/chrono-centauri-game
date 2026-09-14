shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float travel_intensity : hint_range(0.0, 1.0) = 0.0;
uniform float distortion_intensity : hint_range(0.0, 1.0) = 0.0;
uniform float spark_alpha : hint_range(0.0, 1.0) = 0.0;
uniform float direction : hint_range(-1.0, 1.0) = -1.0;
uniform vec4 star_color : source_color = vec4(0.898, 0.906, 0.878, 1.0); // #E5E7E0 convertido para RGB

void fragment() {
    vec2 uv = SCREEN_UV;

    vec2 distorted_uv = uv;
    distorted_uv.x += sin(distorted_uv.y * 20.0 + TIME * 10.0) * 0.05 * distortion_intensity;

    float offset_amount = direction * travel_intensity * 0.15;
    vec3 screen_color = vec3(0.0);
    int samples = 5;
    
    for(int s = 0; s < 5; s++) {
        float t = float(s) / float(samples - 1);
        vec2 stepped_uv = distorted_uv + vec2(offset_amount * t, 0.0);
        screen_color += texture(SCREEN_TEXTURE, stepped_uv).rgb;
    }
    screen_color /= float(samples);


    float movement = TIME * direction * 2.0; 
    float stars = 0.0; 

    for (float i = 0.0; i < 3.0; i += 1.0) { 
        float scale = 24.0 + i * 1.5; 

        vec2 star_uv = uv; 
        star_uv.x += movement * (0.5 + i * 0.15); 
        
        vec2 grid = star_uv * scale; 
        vec2 cell = floor(grid); 
        vec2 local = fract(grid); 

        float random = fract(
            sin(dot(cell, vec2(12.9898, 78.233))) * 
            43758.5453
        ); 

        float star = step(0.96, random); 

        float vertical = 1.0 - smoothstep(0.0, 0.1, abs(local.y - 0.5));

        float horizontal; 
        if (direction < 0.0) { 
            horizontal = 1.0 - smoothstep(0.2, 0.8, local.x); 
        } 
        else { 
            horizontal = smoothstep(0.2, 0.4, local.x); 
        } 
        
        star *= vertical; 
        star *= horizontal; 

        stars += star; 
    } 

    stars *= spark_alpha;

    vec3 deep_space_color = vec3(0.012, 0.02, 0.067); // #030511
    vec3 base_color = mix(screen_color, deep_space_color, spark_alpha);
    
    // #E5E7E0
    vec3 final_color = mix(base_color, star_color.rgb, stars * star_color.a);

    COLOR = vec4(final_color, 1.0);
}