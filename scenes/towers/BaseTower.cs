using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public abstract class BaseTower : Node2D
    {
        public TowerType TowerType { get; set; }
        protected List<Enemy> detectedEnemies = new List<Enemy>();
        protected Shaker shaker;
        protected Timer attackTimer;
        private Label mouseOverLabel;
        private string towerName = "";
        public string TowerName
        {
            get { return towerName; }
            set
            {
                if (towerName != value)
                {
                    towerName = value;
                    UpdateMouseoverLabel();
                }
            }
        }
        private int level = 1;
        public int Level
        {
            get { return level; }
            set
            {
                if (level != value)
                {
                    level = value;
                    UpdateMouseoverLabel();
                }
            }
        }


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

            var exclusionArea = GetNode("ExclusionArea");
            exclusionArea.Connect("mouse_entered", this, nameof(MouseEntered));
            exclusionArea.Connect("mouse_exited", this, nameof(MouseExited));
        }

        public void Initialise(Vector2 position)
        {
            mouseOverLabel = GetNode<Label>("MouseOverLabel");
            RemoveChild(mouseOverLabel);
            Arena.INSTANCE.UI.AddChild(mouseOverLabel);
            Arena.INSTANCE.UI.MoveChild(mouseOverLabel, 0);
            mouseOverLabel.RectPosition += Position;
            UpdateMouseoverLabel();
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
            level++;
            attackTimer.WaitTime *= .8f;

            GD.Print("Upgraded ", Name);
            UpdateMouseoverLabel();
        }

        private void MouseEntered()
        {
            mouseOverLabel.Visible = true;
        }

        private void MouseExited()
        {
            mouseOverLabel.Visible = false;
        }

        private void UpdateMouseoverLabel()
        {
            if (mouseOverLabel != null)
                mouseOverLabel.Text = string.Format("{0} - lvl.{1}", towerName, level);
        }

        public int GetUpgradeCost()
        {
            return TowerType.Price + Mathf.RoundToInt(TowerType.Price * ((float)Level * .5f));
        }
    }
}