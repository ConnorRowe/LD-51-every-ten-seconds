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

        public TowerType(string name, string scenePath, string iconTexturePath, string previewTexturePath, float exclusionRadius, bool exclusionInvert = false)
        {
            Name = name;
            Scene = GD.Load<PackedScene>(scenePath);
            IconTexture = GD.Load<Texture>(iconTexturePath);
            PreviewTexture = GD.Load<Texture>(previewTexturePath);
            ExclusionRadius = exclusionRadius;
            ExclusionInvert = exclusionInvert;
        }
    }

    public static class TowerTypes
    {
        public static List<TowerType> AllTowerTypes { get; } = new List<TowerType>();
        public static TowerType StumpShooter { get; } = RegisterTowerType(new TowerType("Stump Shooter", "res://scenes/towers/StumpShooter.tscn", "res://textures/icon_stumpshooter.png", "res://textures/stump.png", 16));
        public static TowerType Upgrader { get; } = RegisterTowerType(new TowerType("Tower Upgrader", "res://scenes/towers/TowerUpgrader.tscn", "res://textures/icon_upgrader.png", "res://textures/upgrade_tower_text.png", 32, exclusionInvert: true));

        private static TowerType RegisterTowerType(TowerType towerType)
        {
            AllTowerTypes.Add(towerType);
            return towerType;
        }
    }
}