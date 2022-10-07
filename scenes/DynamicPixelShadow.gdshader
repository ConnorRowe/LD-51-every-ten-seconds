// Adapted from https://inspirnathan.com/posts/48-shadertoy-tutorial-part-2/ and https://godotshaders.com/shader/pixelate/

shader_type canvas_item;

uniform vec4 shadow_colour : hint_color = vec4(0,0,0,.25);
uniform vec2 shadow_size = vec2(18, 6);
uniform float pixel_size = 38;
uniform float scale = 1.0;

vec4 sdfCircle(vec2 uv, float r) {
    float x = uv.x;
    float y = uv.y;
    
    float d = length(vec2(x, y)) - r;
    
    return d > 0. ? vec4(0) : shadow_colour;
}

void vertex()
{
	VERTEX *= shadow_size * scale;
}

void fragment()
{
	vec2 shadow_size_factor = (shadow_size / shadow_size.x)*scale;
	vec2 grid_uv = round(UV * (shadow_size_factor * pixel_size)) / (shadow_size_factor * pixel_size);
	COLOR = sdfCircle(grid_uv - .5, .49);
}