using Godot;

namespace TenSecs
{
    public class FloatingText : Node2D
    {
        private static PackedScene packedScene = GD.Load<PackedScene>("res://scenes/FloatingText.tscn");
        private float initalDuration;
        private float duration;

        public override void _Process(float delta)
        {
            Position = new Vector2(Position.x, Position.y - (10 * delta));
            duration -= delta;

            Modulate = new Color(Modulate, duration / initalDuration);

            if (duration <= 0f)
                QueueFree();
        }

        public static FloatingText SpawnText(string bbcodeText, Vector2 pos, float duration = 4f)
        {
            FloatingText newText = packedScene.Instance<FloatingText>();
            newText.initalDuration = newText.duration = duration;
            newText.GetNode<RichTextLabel>("RichTextLabel").BbcodeText += bbcodeText;
            Arena.INSTANCE.UI.AddChild(newText);
            newText.GlobalPosition = pos;
            return newText;
        }
    }
}