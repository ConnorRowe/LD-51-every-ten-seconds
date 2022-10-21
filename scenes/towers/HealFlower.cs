using Godot;

namespace TenSecs
{
    public class HealFlower : BaseTower
    {
        private PackedScene healScene = GD.Load<PackedScene>("res://scenes/HealOrb.tscn");

        public override void _Ready()
        {
            base._Ready();
        }

        protected override void Attack()
        {
            var heal = healScene.Instance<HealOrb>();
            Arena.INSTANCE.AddChild(heal);
            heal.Position = new Vector2(Arena.RNG.RandfRange(12, 350), Arena.RNG.RandfRange(9, 172));
            RobotPaintTexture.DrawHeal(GlobalPosition);
        }
    }
}