using Godot;

namespace TenSecs
{
    public class RepelShroom : BaseTower
    {
        private Particles2D particles;

        public override void _Ready()
        {
            base._Ready();

            particles = GetNode<Particles2D>("Particles2D");
        }

        protected override void Attack()
        {
            if (detectedEnemies.Count > 0)
            {
                foreach (Enemy enemy in detectedEnemies)
                {
                    enemy.AddExternalImpulse(Position.DirectionTo(enemy.Position) * (50 + (Level * 25)));
                    shaker.Shake(1);
                }

                if (!particles.Emitting)
                    particles.Emitting = true;
                else
                    particles.Restart();

                SFX.Shroom();
            }
        }
    }
}