shader_type canvas_item;

uniform float flash_lerp = 0.0;
const vec3 flash_colour = vec3(0.99607, 0.95294, 0.75294);

void fragment()
{
	COLOR = texture(TEXTURE, UV);
	COLOR.rgb = mix(COLOR.rgb, flash_colour, flash_lerp);
}