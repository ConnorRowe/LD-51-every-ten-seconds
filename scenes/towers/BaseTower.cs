using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public abstract class BaseTower : Node2D
    {
        protected List<Enemy> detectedEnemies = new List<Enemy>();
        protected Shaker shaker;
        protected Timer attackTimer;

        public override void _Ready()
        {
            base._Ready();

            ZIndex = (int)GlobalPosition.y;

            shaker = GetNode<Shaker>("Shaker");

            var enemyDetectionArea = GetNode<Area2D>("EnemyDetectionArea");
            enemyDetectionArea.Connect("area_entered", this, nameof(EnemyAreaEntered));
            enemyDetectionArea.Connect("area_exited", this, nameof(EnemyAreaExited));

            attackTimer = GetNode<Timer>("AttackTimer");
            attackTimer.Connect("timeout", this, nameof(Attack));
        }

        private void EnemyAreaEntered(Area2D area)
        {
            if (area.Owner is Enemy enemy)
            {
                detectedEnemies.Add(enemy);
                if (!enemy.IsConnected(nameof(Enemy.Dying), this, nameof(DetectedEnemyDying)))
                    enemy.Connect(nameof(Enemy.Dying), this, nameof(DetectedEnemyDying), new Godot.Collections.Array() { enemy });
            }
        }

        private void EnemyAreaExited(Area2D area)
        {
            if (area.Owner is Enemy enemy)
                detectedEnemies.Remove(enemy);
        }

        private void DetectedEnemyDying(Enemy enemy)
        {
            detectedEnemies.Remove(enemy);
        }

        protected abstract void Attack();

        public virtual void Upgrade()
        {
            attackTimer.WaitTime *= .8f;

            GD.Print("Upgraded ", Name);
        }
    }
}