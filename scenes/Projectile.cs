using Godot;

namespace TenSecs
{
    public class Projectile : KinematicBody2D
    {
        public Vector2 Velocity { get; set; } = Vector2.Zero;

        public override void _Ready()
        {
            base._Ready();

            GetNode("EnemyDetectionArea").Connect("area_entered", this, nameof(AreaEntered));
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            MoveAndCollide(Velocity);
        }

        private void AreaEntered(Area2D area)
        {
            if (area.Owner is Enemy enemy)
            {
                enemy.Hit(1);
                enemy.AddExternalImpulse(GlobalPosition.DirectionTo(enemy.GlobalPosition) * 40f);
            }

            Die();
        }

        private void Die()
        {
            QueueFree();
        }
    }
}