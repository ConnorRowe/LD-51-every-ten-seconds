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
        public TowerPreview TowerPreview { get; set; } = null;
        private TowerType previewedType = null;
        private Label moneyLabel;
        private Shaker acornShaker = null;

        private int money = 100;
        public int Money
        {
            get { return money; }
            set
            {
                if (money != value)
                {
                    int diff = value - money;

                    FloatingText.SpawnText(string.Format("[color=#{0}]{1}{2}", diff > 0 ? "9cdb43" : "b4202a", diff > 0 ? "+" : "", diff.ToString()), new Vector2(339, 172), 1);

                    money = value;
                    moneyLabel.Text = money.ToString();

                    acornShaker?.Shake(1);
                }
            }
        }

        public override void _Ready()
        {
            tooltipPanel = GetNode<Panel>("TowerTooltip");
            tooltipName = GetNode<Label>("TowerTooltip/VBoxContainer/TowerName");
            tooltipDesc = GetNode<Label>("TowerTooltip/VBoxContainer/Desc");
            moneyLabel = GetNode<Label>("Money");
            acornShaker = GetNode<Shaker>("Money/AcornShaker");

            RegisterShopButton(GetNode("SlotsBG/StumpShooter"), TowerTypes.StumpShooter);
            RegisterShopButton(GetNode("SlotsBG/BombFruit"), TowerTypes.BombFruit);
            RegisterShopButton(GetNode("SlotsBG/Shroom"), TowerTypes.Shroom);
            RegisterShopButton(GetNode("SlotsBG/HealFlower"), TowerTypes.HealFlower);
            RegisterShopButton(GetNode("UpgradeButton"), TowerTypes.Upgrader);

            GetParent().GetParent().Connect(nameof(Arena.EveryTenSeconds), this, nameof(EveryTenSeconds));


            tooltipPanel.RectScale = Vector2.Down;

        }

        private void EveryTenSeconds()
        {
            Money += 50;
        }


        public override void _UnhandledInput(InputEvent evt)
        {
            base._UnhandledInput(evt);

            if (evt.IsActionReleased("attack") && TowerPreview != null)
            {
                PlaceTower(GetGlobalMousePosition(), null);

                GetTree().SetInputAsHandled();
            }
            else if (evt.IsActionReleased("cancel_place_tower") && TowerPreview != null)
            {
                TowerPreview.QueueFree();
                TowerPreview = null;

                Arena.HideTowerPlacementLabel();
            }
        }

        private void RegisterShopButton(Node button, TowerType towerType)
        {
            button.Connect("mouse_entered", this, nameof(ShopBtnMouseEntered), towerType.ToSignalArray());
            button.Connect("mouse_exited", this, nameof(MouseExited));
            button.Connect("pressed", this, nameof(ShopBtnPressed), towerType.ToSignalArray());

            if (towerType != TowerTypes.Upgrader)
                button.GetNode<Label>("Price").Text = towerType.Price.ToString();
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
            if (TowerPreview == null)
            {
                TowerPreview = towerPreviewScene.Instance<TowerPreview>();
                AddChild(TowerPreview);
                TowerPreview.Initialise(towerType.ExclusionRadius, towerType.PreviewTexture, towerType.ExclusionInvert);
                GD.Print(towerType.ExclusionInvert);
                previewedType = towerType;
            }
            else
            {
                if (!TowerPreview.CanPlace)
                    return;

                var tower = previewedType.Scene.Instance<BaseTower>();
                Arena.INSTANCE.AddChild(tower);
                tower.Position = position;
                tower.TowerName = previewedType.Name;
                tower.TowerType = previewedType;
                tower.ZIndex = (int)position.y;
                if (previewedType != TowerTypes.Upgrader)
                {
                    tower.Initialise(position);
                    Arena.MakeSmokeParticles(position);
                }
                else
                {
                    ((TowerUpgrader)tower).Shop = this;
                    ((TowerUpgrader)tower).Upgrade(TowerPreview.lastUpgradeTowerSeen);
                    Arena.MakeUpgradeParticles(position);
                }

                TowerPreview.QueueFree();
                TowerPreview = null;

                SFX.PlaceTower();


                Arena.HideTowerPlacementLabel();

                Money -= previewedType.Price;
            }

        }

        private void ShopBtnMouseEntered(int towerIndex)
        {
            var tt = TowerTypes.FromIndex(towerIndex);
            tooltipName.Text = tt.Name;
            tooltipDesc.Text = tt.Description;

            SFX.UIClick();

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
    }
}