using Godot;

namespace TenSecs
{
    public class PaintGrenade : Node2D
    {
        private Vector2 dir = new Vector2(Arena.RNG.RandfRange(-1, 1), Arena.RNG.RandfRange(-1, 1)).Normalized();
        private Sprite sprite;

        public override void _Ready()
        {
            sprite = GetNode<Sprite>("Sprite");
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Throw");

            GetTree().CreateTimer(1f).Connect("timeout", this, nameof(Explode));
        }

        public override void _Process(float delta)
        {
            Position += dir * 40f * delta;
            sprite.ZIndex = (int)sprite.GlobalPosition.y;
        }

        private void Explode()
        {
            RobotPaintTexture.DrawSplat(GlobalPosition);

            SetProcess(false);

            sprite.SelfModulate = Colors.Transparent;
            GetNode<Particles2D>("Sprite/Particles2D").Emitting = false;
            GetNode<Sprite>("Shadow").Visible = false;

            GetTree().CreateTimer(.6f).Connect("timeout", this, "queue_free");

            SFX.GrenadeLand();
        }
    }
}