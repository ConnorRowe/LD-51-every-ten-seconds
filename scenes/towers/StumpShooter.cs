using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public class StumpShooter : BaseTower
    {
        private const int enemySearchDepth = 4;
        private const float Tau = Mathf.Tau;
        private const float Pi = Mathf.Pi;
        private const float HalfPi = Mathf.Pi / 2f;
        private const float QuarterPi = Mathf.Pi / 4f;

        private static PackedScene pelletScene = GD.Load<PackedScene>("res://scenes/StumpPellet.tscn");

        private float currentShootAngle = 0f;
        private float goalShootAngle;
        private Sprite shooter;
        private ShaderMaterial shooterMat;
        private Vector2 ellipse = new Vector2(9, 5);
        private Vector2 shooterOffset = new Vector2(0, -3);

        public override void _Ready()
        {
            base._Ready();

            shooter = GetNode<Sprite>("Stump/ShooterAnchor/Shooter");
            shooterMat = (ShaderMaterial)shooter.Material;

            GetNode("CheckEnemiesTimer").Connect("timeout", this, nameof(AimNearestEnemy));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            currentShootAngle = Mathf.Wrap(Mathf.LerpAngle(currentShootAngle, goalShootAngle, delta * 4), 0f, Tau);

            shooter.Position = shooterOffset + (new Vector2(Mathf.Cos(currentShootAngle), Mathf.Sin(currentShootAngle)) * ellipse);
            shooter.Rotation = currentShootAngle - HalfPi;
            shooter.ZIndex = (currentShootAngle <= Pi ? 0 : -1);

            int frame = 0;
            if (currentShootAngle > QuarterPi && currentShootAngle <= HalfPi + QuarterPi)
                frame = 0;
            else if (currentShootAngle > HalfPi + QuarterPi && currentShootAngle <= Pi + QuarterPi)
                frame = 1;
            else if (currentShootAngle > Pi + QuarterPi && currentShootAngle <= Tau - QuarterPi)
                frame = 2;
            else
                frame = 3;

            shooterMat.SetShaderParam("frame", frame);
        }

        private void AimNearestEnemy()
        {
            // Aim at nearest enemy
            float smallestDist = float.MaxValue;
            int nearestIdx = -1;
            for (int i = 0; i < Mathf.Min(detectedEnemies.Count, enemySearchDepth); i++)
            {
                float dist = GlobalPosition.DistanceSquaredTo(detectedEnemies[i].GlobalPosition);
                if (dist < smallestDist)
                {
                    smallestDist = dist;
                    nearestIdx = i;
                }
            }

            // If found enemy
            if (nearestIdx >= 0)
            {
                var dir = GlobalPosition.DirectionTo(detectedEnemies[nearestIdx].GlobalPosition);
                goalShootAngle = Mathf.Wrap(dir.Angle(), 0f, Tau);
            }
        }

        protected override void Attack()
        {
            if (detectedEnemies.Count > 0)
            {
                Projectile pellet = pelletScene.Instance<Projectile>();
                var dir = new Vector2(Mathf.Cos(currentShootAngle), Mathf.Sin(currentShootAngle));
                Arena.INSTANCE.AddChild(pellet);
                pellet.Position = GlobalPosition + shooterOffset + (dir * ellipse);
                pellet.Velocity = dir * 2f;
                shaker.Shake(1);
            }
        }

    }
}