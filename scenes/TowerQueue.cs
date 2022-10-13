using Godot;
using System;

namespace TenSecs
{
    public class TowerQueue : Node2D
    {
        private static PackedScene towerPreviewScene = GD.Load<PackedScene>("res://scenes/TowerPreview.tscn");
        private TowerType[] queue = new TowerType[3] { null, null, null };
        private Sprite[] queueIcons = new Sprite[3];
        private Shaker[] shakers = new Shaker[3];
        private int count = 0;
        private TowerPreview towerPreview = null;
        private bool first = true;

        public override void _Ready()
        {
            queueIcons[0] = GetNode<Sprite>("CurrentSlot/Frame/Icon");
            queueIcons[1] = GetNode<Sprite>("BacklogSlotA/Frame/Icon");
            queueIcons[2] = GetNode<Sprite>("BacklogSlotB/Frame/Icon");
            shakers[0] = GetNode<Shaker>("Shaker1");
            shakers[1] = GetNode<Shaker>("Shaker2");
            shakers[2] = GetNode<Shaker>("Shaker3");
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt.IsActionReleased("place_tower"))
            {
                PlaceTower(GetGlobalMousePosition());
            }
        }

        public void EveryTenSeconds()
        {
            if (count < 3)
            {
                if (first)
                {
                    queue[0] = TowerTypes.StumpShooter;
                    first = false;
                }
                else
                    queue[count] = TowerTypes.AllTowerTypes[Arena.RNG.RandiRange(0, TowerTypes.AllTowerTypes.Count - 1)];
                shakers[count].Shake(1);
                count++;
            }

            UpdateIcons();
        }

        private void UpdateIcons()
        {
            for (int i = 0; i < 3; i++)
            {
                var tt = queue[i];
                if (tt != null)
                    queueIcons[i].Texture = tt.IconTexture;
                else
                    queueIcons[i].Texture = null;
            }
        }

        public void PlaceTower(Vector2 position)
        {
            if (count > 0)
            {
                var tt = queue[0];

                if (towerPreview == null)
                {
                    towerPreview = towerPreviewScene.Instance<TowerPreview>();
                    AddChild(towerPreview);
                    towerPreview.Initialise(tt.ExclusionRadius, tt.PreviewTexture, tt.ExclusionInvert);
                    GD.Print(tt.ExclusionInvert);
                }
                else
                {
                    if (!towerPreview.CanPlace)
                        return;

                    var tower = tt.Scene.Instance<Node2D>();
                    GetParent().AddChild(tower);
                    tower.Position = position;

                    queue[0] = queue[1];
                    queue[1] = queue[2];
                    queue[2] = null;

                    count--;
                    shakers[count].Shake(1);

                    towerPreview.QueueFree();
                    towerPreview = null;

                    Arena.MakeSmokeParticles(position);
                }
            }

            UpdateIcons();
        }
    }
}