// Adapted from https://inspirnathan.com/posts/48-shadertoy-tutorial-part-2/ and https://godotshaders.com/shader/pixelate/

shader_type canvas_item;

uniform vec4 shadow_colour : hint_color = vec4(0,0,0,.25);
uniform vec2 shadow_size = vec2(18, 6);
uniform float pixel_size = 0.5;
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
	ivec2 size = ivec2(int(shadow_size.x),int( shadow_size.y));
	
	int xRes = size.x;
	int yRes = size.y;
	
	float xFactor = (float(xRes) * scale) / float(pixel_size);
	float yFactor = (float(yRes) * scale) / float(pixel_size);
	
	float grid_uv_x = round(UV.x * xFactor) / xFactor;
	float grid_uv_y = round(UV.y * yFactor) / yFactor;
	vec2 grid_uv = vec2(grid_uv_x, grid_uv_y);
	COLOR = sdfCircle(grid_uv - .5, .49);
}