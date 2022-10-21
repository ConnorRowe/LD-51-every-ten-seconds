using Godot;

namespace TenSecs
{
    public class HealOrb : Node2D
    {
        public override void _Ready()
        {
            base._Ready();

            Arena.MakeSmokeParticles(GetNode<Node2D>("AnimatedSprite").GlobalPosition);

            GetTree().CreateTimer(2).Connect("timeout", this, nameof(Heal));
        }

        private void Heal()
        {
            Arena.MakeUpgradeParticles(GlobalPosition);
            RobotPaintTexture.DrawHeal(GlobalPosition);
            Arena.INSTANCE.Crystal.HealFromOrb();
            SFX.Heal();
			QueueFree();
        }
    }
}