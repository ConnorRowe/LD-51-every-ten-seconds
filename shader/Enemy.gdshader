shader_type canvas_item;

const vec3 flash_colour = vec3(0.99607, 0.95294, 0.75294);
uniform sampler2D hueshift_mask_tex;
uniform float flash_lerp = 0.0;
uniform float hueshift_amount = 0.0;

// From https://gist.github.com/mairod/a75e7b44f68110e1576d77419d608786
vec2 rotate2(vec2 v, float fi) {
	return v * mat2(vec2(cos(fi), -sin(fi)), vec2(sin(fi), cos(fi)));
}

// YIQ color rotation/hue shift
vec3 hueShiftYIQ(vec3 rgb, float hs) {
	float rotAngle = hs*-6.28318530718;
	const mat3 rgb2yiq = mat3(vec3(0.299, 0.596, 0.211),
	vec3(0.587, -0.274, -0.523),
	vec3(0.114, -0.322, 0.312));
	const mat3 yiq2rgb = mat3(vec3(1, 1, 1),
	vec3(0.956, -0.272, -1.106),
	vec3(0.621, -0.647, 1.703));
	vec3 yiq = rgb2yiq * rgb;
	yiq.yz = rotate2(yiq.yz, rotAngle);
	return yiq2rgb * yiq;
}

void fragment()
{
	float hueshift_mask = texture(hueshift_mask_tex, UV).r;
	COLOR = texture(TEXTURE, UV);
	COLOR.rgb = hueShiftYIQ(COLOR.rgb, hueshift_mask * hueshift_amount);
	COLOR.rgb = mix(COLOR.rgb, flash_colour, flash_lerp);
}