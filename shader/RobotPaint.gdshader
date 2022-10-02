shader_type canvas_item;

uniform vec4 highlight_colour:source_color;
uniform sampler2D metal_floor_tex: repeat_enable, filter_nearest;

void fragment() {
	COLOR = texture(TEXTURE, UV);
	if(COLOR.r == 0.0)
	{
		discard;
	}
	else
	{
		COLOR = texture(metal_floor_tex, fract(UV * vec2(20, 10))) * vec4(COLOR.r);
	}
	
	vec2 uv_adjust = 1.0f / vec2(textureSize(TEXTURE, 0));
	bool up_empty = texture(TEXTURE, UV + vec2(0, -uv_adjust.y)).r == 0.0f;
	if(up_empty)
	{
		COLOR = highlight_colour;
	}
}
