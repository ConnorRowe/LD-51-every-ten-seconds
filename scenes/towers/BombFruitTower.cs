using Godot;
using System;

namespace TenSecs
{
    public class BombFruitTower : BaseTower, IPlayerHittable
    {
        private static PackedScene explosionScene = GD.Load<PackedScene>("res://scenes/ExplosionParticles.tscn");
        private const int MaxGrowthLevel = 6;
        private int growthLevel = 0;
        private ShaderMaterial shaderMaterial;
        private float explodeRadius = 45f;
        private bool queueExplosion = false;
        private static Physics2DShapeQueryParameters explodeShapeQuery = new Physics2DShapeQueryParameters()
        {
            CollideWithAreas = true,
            CollideWithBodies = false,
            CollisionLayer = 2
        };

        private static CircleShape2D explodeShape = new CircleShape2D() { Radius = 45f };

        static BombFruitTower()
        {
            explodeShapeQuery.SetShape(explodeShape);
        }

        public override void _Ready()
        {
            base._Ready();

            shaderMaterial = (ShaderMaterial)GetNode<Sprite>("Sprite").Material;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (queueExplosion)
            {
                queueExplosion = false;

                var spaceState = GetWorld2d().DirectSpaceState;
                explodeShape.Radius = explodeRadius;

                explodeShapeQuery.Transform = new Transform2D(0.0f, GlobalPosition + new Vector2(0, 7));
                var results = spaceState.IntersectShape(explodeShapeQuery);

                foreach (var dict in results)
                {
                    if (((Godot.Collections.Dictionary)dict)["collider"] is Node node && node.Owner is Enemy enemy)
                    {
                        enemy.Hit(1);
                        enemy.AddExternalImpulse(Position.DirectionTo(enemy.Position) * 50f);
                    }
                }

            }
        }

        protected override void Attack()
        {
            Grow();
        }

        private void Grow()
        {
            if (growthLevel < MaxGrowthLevel)
            {
                growthLevel++;
                UpdateSprite();
            }
        }

        void IPlayerHittable.PlayerHit()
        {
            if (growthLevel == MaxGrowthLevel)
            {
                //KABOOM
                var explosion = explosionScene.Instance<SmokeParticles>();
                (explosion.ProcessMaterial as ParticlesMaterial).EmissionSphereRadius = explodeRadius;
                AddChild(explosion);
                explosion.Position = new Vector2(0, 7);
                explosion.ZAsRelative = false;
                explosion.ZIndex = (int)explosion.GlobalPosition.y;
                growthLevel = 0;
                UpdateSprite();
                queueExplosion = true;
                SFX.Explosion();
            }
        }

        private void UpdateSprite()
        {
            shaderMaterial.SetShaderParam("frame", growthLevel);
        }

        public override void Upgrade()
        {
            base.Upgrade();

            explodeRadius += 10;
        }
    }
}