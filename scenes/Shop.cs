using Godot;
using System;

namespace TenSecs
{
    public class Shop : Control
    {
        private static PackedScene towerPreviewScene = GD.Load<PackedScene>("res://scenes/TowerPreview.tscn");
        private SceneTreeTween tooltipTransition = null;
        private Panel tooltipPanel;
        private Label tooltipName;
        private Label tooltipDesc;
        private TowerPreview towerPreview = null;
        private TowerType previewedType = null;
        private Label moneyLabel;

        private int money = 100;
        public int Money
        {
            get { return money; }
            set
            {
                if (money != value)
                {
                    money = value;
                    moneyLabel.Text = money.ToString();
                }
            }
        }

        public override void _Ready()
        {
            tooltipPanel = GetNode<Panel>("TowerTooltip");
            tooltipName = GetNode<Label>("TowerTooltip/VBoxContainer/TowerName");
            tooltipDesc = GetNode<Label>("TowerTooltip/VBoxContainer/Desc");
            moneyLabel = GetNode<Label>("Money");

            RegisterShopButton(GetNode("SlotsBG/StumpShooter"), TowerTypes.StumpShooter);
            RegisterShopButton(GetNode("SlotsBG/BombFruit"), TowerTypes.BombFruit);

            GetParent().GetParent().Connect(nameof(Arena.EveryTenSeconds), this, nameof(EveryTenSeconds));

            var upgradeBtn = GetNode("UpgradeButton");
            upgradeBtn.Connect("mouse_entered", this, nameof(UpgradeMouseEntered));
            upgradeBtn.Connect("mouse_exited", this, nameof(MouseExited));
            upgradeBtn.Connect("pressed", this, nameof(UpgradePressed));

            tooltipPanel.RectScale = Vector2.Down;

        }

        private void EveryTenSeconds()
        {
            Money += 50;
        }


        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt.IsActionReleased("attack") && towerPreview != null)
            {
                PlaceTower(GetGlobalMousePosition(), null);

                GetTree().SetInputAsHandled();
            }
            else if (evt.IsActionReleased("cancel_place_tower") && towerPreview != null)
            {
                towerPreview.QueueFree();
                towerPreview = null;

                Arena.HideTowerPlacementLabel();
            }
        }

        private void RegisterShopButton(Node button, TowerType towerType)
        {
            button.Connect("mouse_entered", this, nameof(ShopBtnMouseEntered), towerType.ToSignalArray());
            button.Connect("mouse_exited", this, nameof(MouseExited));
            button.Connect("pressed", this, nameof(ShopBtnPressed), towerType.ToSignalArray());
        }

        private void ShopBtnPressed(int towerIndex)
        {
            var tt = TowerTypes.FromIndex(towerIndex);
            if (money >= tt.Price)
            {
                PlaceTower(GetGlobalMousePosition(), tt);

                Arena.ShowTowerPlacementLabel(tt.Name, tt.Price);
            }
        }

        public void PlaceTower(Vector2 position, TowerType towerType)
        {
            if (towerPreview == null)
            {
                towerPreview = towerPreviewScene.Instance<TowerPreview>();
                AddChild(towerPreview);
                towerPreview.Initialise(towerType.ExclusionRadius, towerType.PreviewTexture, towerType.ExclusionInvert);
                GD.Print(towerType.ExclusionInvert);
                previewedType = towerType;
            }
            else
            {
                if (!towerPreview.CanPlace)
                    return;

                var tower = previewedType.Scene.Instance<BaseTower>();
                Arena.INSTANCE.AddChild(tower);
                tower.Position = position;
                tower.TowerName = previewedType.Name;
                if(previewedType != TowerTypes.Upgrader)
                    tower.Initialise(position);

                towerPreview.QueueFree();
                towerPreview = null;

                Arena.MakeSmokeParticles(position);

                Arena.HideTowerPlacementLabel();

                Money -= previewedType.Price;
            }

        }

        private void ShopBtnMouseEntered(int towerIndex)
        {
            var tt = TowerTypes.FromIndex(towerIndex);
            tooltipName.Text = tt.Name;

            MouseEntered();
        }

        private void MouseEntered()
        {
            if (tooltipTransition != null)
            {
                tooltipTransition.Kill();
                tooltipTransition.Dispose();
            }

            tooltipTransition = GetTree().CreateTween();
            var t = tooltipTransition.TweenProperty(tooltipPanel, "rect_scale", Vector2.One, .25f);
            t.SetEase(Tween.EaseType.Out);
            t.SetTrans(Tween.TransitionType.Sine);

            tooltipTransition.Play();
        }

        private void MouseExited()
        {
            if (tooltipTransition != null)
            {
                tooltipTransition.Kill();
                tooltipTransition.Dispose();
            }

            tooltipTransition = GetTree().CreateTween();
            var t = tooltipTransition.TweenProperty(tooltipPanel, "rect_scale", Vector2.Down, .5f);
            t.SetEase(Tween.EaseType.In);
            t.SetTrans(Tween.TransitionType.Sine);

            tooltipTransition.Play();
        }

        private void UpgradeMouseEntered()
        {
            tooltipName.Text = "Upgrade Tower";
            tooltipDesc.Text = "Improves one tower. Price varies depending on the tower.";

            MouseEntered();
        }

        private void UpgradePressed()
        {
            PlaceTower(GetGlobalMousePosition(), TowerTypes.Upgrader);
        }

    }
}