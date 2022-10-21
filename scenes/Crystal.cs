using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public class Crystal : Node2D
    {
        [Signal]
        public delegate void CrystalDead();

        private Sprite sprite;
        private Shaker shaker;
        private TextureProgress healthBar;
        private Label healthText;
        private ShaderMaterial shaderMaterial;

        private float health = 1f;
        private int crystalFrame = 0;
        private List<Enemy> detectedEnemies = new List<Enemy>();
        private float paintAmount = 0f;

        private Particles2D hitParticles;
        private bool dead = false;

        public override void _Ready()
        {
            sprite = GetNode<Sprite>("CrystalAnchor/Sprite");
            shaker = GetNode<Shaker>("Shaker");
            healthBar = GetNode<TextureProgress>("CrystalAnchor/Sprite/HealthBar");
            healthBar.Visible = false;
            healthText = healthBar.GetNode<Label>("Label");
            shaderMaterial = (ShaderMaterial)sprite.Material;

            var enemyDetectionArea = GetNode<Area2D>("EnemyDetectionArea");
            enemyDetectionArea.Connect("area_entered", this, nameof(EnemyAreaEntered));
            enemyDetectionArea.Connect("area_exited", this, nameof(EnemyAreaExited));

            GetParent().GetNode<RobotPaintTexture>("RobotPaintTexture").Connect(nameof(RobotPaintTexture.PaintAmountChanged), this, nameof(PaintAmountChanged));

            var mouseDetection = GetNode("CrystalAnchor/Sprite/MouseDetection");
            mouseDetection.Connect("mouse_entered", this, nameof(MouseEntered));
            mouseDetection.Connect("mouse_exited", this, nameof(MouseExited));

            hitParticles = GetNode<Particles2D>("CrystalAnchor/Sprite/HitParticles");
        }

        public override void _Process(float delta)
        {
            if (dead)
                return;

            sprite.ZIndex = (int)sprite.GlobalPosition.y;

            foreach (Enemy enemy in detectedEnemies)
            {
                if (enemy.CanAttack)
                {
                    Hit();
                    enemy.JustAttacked();
                    enemy.AddExternalImpulse(Position.DirectionTo(enemy.Position) * 300f);
                }
            }

            if (paintAmount >= .25f && paintAmount < .5f)
            {
                DamageHealth(.0025f * delta);
            }
            else if (paintAmount >= .5f && paintAmount < .75f)
            {
                DamageHealth(.0125f * delta);
            }
            else if (paintAmount >= .75)
            {
                DamageHealth(.05f * delta);
            }
        }

        private void Hit()
        {
            if (dead)
                return;

            DamageHealth(.07f);
            shaker.Shake(.5f);
            SFX.CrystalHit();

            var newParticles = (Particles2D)hitParticles.Duplicate((int)DuplicateFlags.Scripts);
            sprite.AddChild(newParticles);
            newParticles.Emitting = true;
            var timer = GetTree().CreateTimer(newParticles.Lifetime);
            timer.Connect("timeout", newParticles, "queue_free");
        }

        private void DamageHealth(float dmg)
        {
            if (dead)
                return;

            health -= dmg;
            health = Mathf.Clamp(health, 0, 1);
            healthBar.Value = health;
            healthText.Text = string.Format("{0}%", (int)(health * 100));

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

            if (health <= 0f)
                EmitSignal(nameof(CrystalDead));
        }

        public void HealFromOrb()
        {
            DamageHealth(-.02f);
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

        private void MouseEntered()
        {
            healthBar.Visible = true;
        }

        private void MouseExited()
        {
            healthBar.Visible = false;
        }

        private void PaintAmountChanged(float paintAmount)
        {
            this.paintAmount = paintAmount;
        }

        public void Quit()
        {
            DamageHealth(999999999999);
        }

    }
}