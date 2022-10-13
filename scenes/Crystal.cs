using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public class Crystal : Node2D
    {
        private Sprite sprite;
        private Shaker shaker;
        private ProgressBar healthBar;
        private ShaderMaterial shaderMaterial;

        private float health = 1f;
        private int crystalFrame = 0;
        private List<Enemy> detectedEnemies = new List<Enemy>();
        private float paintAmount = 0f;

        private Particles2D hitParticles;

        public override void _Ready()
        {
            sprite = GetNode<Sprite>("CrystalAnchor/Sprite");
            shaker = GetNode<Shaker>("Shaker");
            healthBar = GetNode<ProgressBar>("HealthBar");
            shaderMaterial = (ShaderMaterial)sprite.Material;
            var enemyDetectionArea = GetNode<Area2D>("EnemyDetectionArea");
            enemyDetectionArea.Connect("area_entered", this, nameof(EnemyAreaEntered));
            enemyDetectionArea.Connect("area_exited", this, nameof(EnemyAreaExited));
            var checkPaintTimer = GetNode<Timer>("CheckPaintTimer");
            checkPaintTimer.Connect("timeout", this, nameof(CheckPaint));

            hitParticles = GetNode<Particles2D>("CrystalAnchor/Sprite/HitParticles");
        }

        public override void _Process(float delta)
        {
            sprite.ZIndex = (int)sprite.GlobalPosition.y;

            foreach (Enemy enemy in detectedEnemies)
            {
                if (enemy.CanAttack)
                {
                    Hit();
                    enemy.JustAttacked();
                    enemy.AddExternalImpulse(Position.DirectionTo(enemy.Position) * 350f);
                }
            }

            if (paintAmount >= .5f && paintAmount < .75f)
            {
                DamageHealth(1f * delta);
            }
            else if (paintAmount >= .75)
            {
                DamageHealth(3f * delta);
            }
        }

        private void Hit()
        {
            DamageHealth(.07f);
            shaker.Shake(.5f);

            var newParticles = (Particles2D)hitParticles.Duplicate((int)DuplicateFlags.Scripts);
            sprite.AddChild(newParticles);
            newParticles.Emitting = true;
            var timer = GetTree().CreateTimer(newParticles.Lifetime);
            timer.Connect("timeout", newParticles, "queue_free");
        }

        private void DamageHealth(float dmg)
        {
            health -= dmg;
            healthBar.Value = health;

            if (health <= .8f && health > .6f)
                SetCrystalFrame(1);
            else if (health <= .6f && health > .4f)
                SetCrystalFrame(2);
            else if (health <= .4f && health > .2f)
                SetCrystalFrame(3);
            else if (health <= .2f && health > .0f)
                SetCrystalFrame(4);
            else if (health <= .0f)
                SetCrystalFrame(5);
        }

        private void SetCrystalFrame(int frame)
        {
            if (crystalFrame != frame)
            {
                crystalFrame = frame;
                shaderMaterial.SetShaderParam("frame", frame);
            }
        }

        private void EnemyAreaEntered(Area2D area)
        {
            if (area.Owner is Enemy enemy)
                detectedEnemies.Add(enemy);
        }

        private void EnemyAreaExited(Area2D area)
        {
            if (area.Owner is Enemy enemy)
                detectedEnemies.Remove(enemy);
        }

        private void CheckPaint()
        {
            paintAmount = RobotPaintTexture.PaintAmount;
        }
    }
}