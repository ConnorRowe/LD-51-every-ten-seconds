using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public class TowerType
    {
        public string Name { get; set; }
        public PackedScene Scene { get; set; }
        public Texture IconTexture { get; set; }
        public Texture PreviewTexture { get; set; }
        public float ExclusionRadius { get; set; }
        public bool ExclusionInvert { get; set; } = false;
        public int Price { get; set; } = 0;
        public int Index { get; set; }
        public string Description { get; set; }

        public TowerType(string name, string scenePath, string iconTexturePath, string previewTexturePath)
        {
            Name = name;
            Scene = GD.Load<PackedScene>(scenePath);
            IconTexture = GD.Load<Texture>(iconTexturePath);
            PreviewTexture = GD.Load<Texture>(previewTexturePath);
        }

        public Godot.Collections.Array ToSignalArray()
        {
            return new Godot.Collections.Array() { Index };
        }
    }

    public static class TowerTypes
    {
        public static List<TowerType> AllTowerTypes { get; } = new List<TowerType>();
        public static TowerType StumpShooter { get; } = RegisterTowerType(new TowerType("Stump Shooter", "res://scenes/towers/StumpShooter.tscn", "res://textures/icon_stumpshooter.png", "res://textures/stump.png") { ExclusionRadius = 16, Price = 100, Description = "Shoots seeds at nearby robits." });
        public static TowerType BombFruit { get; } = RegisterTowerType(new TowerType("Bombfruit Patch", "res://scenes/towers/BombFruitTower.tscn", "res://textures/icon_bombfruit.png", "res://textures/preview_bombfruit.png") { ExclusionRadius = 16, Price = 100, Description = "Grows a fruit which when ripe, explodes when you hit it!" });
        public static TowerType Shroom { get; } = RegisterTowerType(new TowerType("Repelling Waxcap", "res://scenes/towers/RepelShroom.tscn", "res://textures/icon_shroom.png", "res://textures/shroom.png") { ExclusionRadius = 16, Price = 60, Description = "A fungus with the unique ability to push away robits." });
        public static TowerType HealFlower { get; } = RegisterTowerType(new TowerType("Mend-bloom", "res://scenes/towers/HealFlower.tscn", "res://textures/icon_flower.png", "res://textures/preview_flower.png") { ExclusionRadius = 8, Price = 80, Description = "A magical flower that summons crystallised sunlight, healing the forest and the crystal." });
        public static TowerType Upgrader { get; } = RegisterTowerType(new TowerType("Upgrade Tower", "res://scenes/towers/TowerUpgrader.tscn", "res://textures/icon_upgrader.png", "res://textures/upgrade_tower_text.png") { ExclusionRadius = 6, ExclusionInvert = true, Description = "Improves one tower. Price varies depending on the tower." });

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