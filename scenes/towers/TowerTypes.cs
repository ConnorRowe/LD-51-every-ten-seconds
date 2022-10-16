using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public class TowerType
    {
        public string Name { get; private set; }
        public PackedScene Scene { get; private set; }
        public Texture IconTexture { get; private set; }
        public Texture PreviewTexture { get; private set; }
        public float ExclusionRadius { get; private set; }
        public bool ExclusionInvert { get; private set; }
        public int Price { get; private set; }
        public int Index { get; set; }

        public TowerType()
        {
            
        }

        public TowerType(string name, string scenePath, string iconTexturePath, string previewTexturePath, float exclusionRadius, int price, bool exclusionInvert = false)
        {
            Name = name;
            Scene = GD.Load<PackedScene>(scenePath);
            IconTexture = GD.Load<Texture>(iconTexturePath);
            PreviewTexture = GD.Load<Texture>(previewTexturePath);
            ExclusionRadius = exclusionRadius;
            ExclusionInvert = exclusionInvert;
            Price = price;
        }

        public Godot.Collections.Array ToSignalArray()
        {
            return new Godot.Collections.Array() { Index };
        }
    }

    public static class TowerTypes
    {
        public static List<TowerType> AllTowerTypes { get; } = new List<TowerType>();
        public static TowerType StumpShooter { get; } = RegisterTowerType(new TowerType("Stump Shooter", "res://scenes/towers/StumpShooter.tscn", "res://textures/icon_stumpshooter.png", "res://textures/stump.png", 16, 80));
        public static TowerType BombFruit { get; } = RegisterTowerType(new TowerType("Bombfruit Patch", "res://scenes/towers/BombFruitTower.tscn", "res://textures/icon_bombfruit.png", "res://textures/preview_bombfruit.png", 16, 50));
        public static TowerType Upgrader { get; } = RegisterTowerType(new TowerType("Tower Upgrader", "res://scenes/towers/TowerUpgrader.tscn", "res://textures/icon_upgrader.png", "res://textures/upgrade_tower_text.png", 32, 40, exclusionInvert: true));

        private static TowerType RegisterTowerType(TowerType towerType)
        {
            AllTowerTypes.Add(towerType);
            towerType.Index = AllTowerTypes.Count - 1;
            return towerType;
        }

        public static TowerType FromIndex(int index)
        {
            return AllTowerTypes[index];
        }
    }
}