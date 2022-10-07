shader_type canvas_item;

uniform float maxFrames;
uniform float frame = 0;

void vertex() {
	VERTEX /= vec2(maxFrames, 1.0);
}

void fragment() {
	vec2 uv = UV;
	uv.x /= maxFrames;
	uv.x += frame / maxFrames;
	COLOR = texture(TEXTURE, uv);
}
