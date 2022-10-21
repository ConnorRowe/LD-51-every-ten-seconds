using Godot;
using System;

namespace TenSecs
{
    public class TowerUpgrader : BaseTower
    {
        public Shop Shop { get; set; } = null;

        public override void _Ready()
        {
            GetNode<Particles2D>("Particles2D").Emitting = true;
        }

        protected override void Attack()
        {
            return;
        }

        public void Upgrade(BaseTower towerToUpgrade)
        {
            if (towerToUpgrade != null)
            {
                int upgradeCost = towerToUpgrade.GetUpgradeCost();
                if (upgradeCost <= Shop.Money)
                {
                    towerToUpgrade.Upgrade();
                    Shop.Money -= upgradeCost;
                    FloatingText.SpawnText(string.Format("[color=#9cdb43]{0} upgraded for {1} {2}", towerToUpgrade.TowerName, upgradeCost, InputImgBBCode.Acorn), GlobalPosition);
                }
                else
                {
                    FloatingText.SpawnText(string.Format("[shake rate=8 level=6][color=#b4202a]Too expensive! ({0} {1})[/color][/shake]", upgradeCost, InputImgBBCode.Acorn), GlobalPosition);
                }
            }
            else
                GD.PrintErr("Couldn't find a tower to upgrade, this shouldn't happen!!");

            QueueFree();

        }
    }
}