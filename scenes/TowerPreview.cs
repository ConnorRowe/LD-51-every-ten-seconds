using Godot;
using System;

namespace TenSecs
{
    public class TowerPreview : Node2D
    {
        private static readonly Color canPlaceColour = new Color("400087ff");
        private static readonly Color canNotPlaceColour = new Color("40ff0000");

        private Sprite towerPreview;
        private ShaderMaterial exclusionAreaMat;
        private CircleShape2D exclusionShape = new CircleShape2D();
        private Physics2DShapeQueryParameters physicsShapeQueryParams = new Physics2DShapeQueryParameters();

        private bool canPlace = false;
        public bool CanPlace
        {
            get { return canPlace; }
            set
            {
                if (canPlace != value)
                {
                    canPlace = value;
                    exclusionAreaMat.SetShaderParam("shadow_colour", canPlace ? canPlaceColour : canNotPlaceColour);
                }
            }
        }

        public bool Invert { get; set; }

        public override void _Ready()
        {
            towerPreview = GetNode<Sprite>("TowerPreview");
            exclusionAreaMat = (ShaderMaterial)GetNode<Sprite>("ExclusionAreaPreview").Material;
            physicsShapeQueryParams.SetShape(exclusionShape);
            physicsShapeQueryParams.CollisionLayer = 8;
        }

        public void Initialise(float exclusionRadius, Texture towerPreviewTex, bool invert)
        {
            exclusionShape.Radius = exclusionRadius;
            exclusionAreaMat.SetShaderParam("shadow_size", new Vector2(exclusionRadius, exclusionRadius));
            towerPreview.Texture = towerPreviewTex;
            Invert = invert;
        }

        public override void _PhysicsProcess(float delta)
        {
            GlobalPosition = GetGlobalMousePosition();
            physicsShapeQueryParams.Transform = new Transform2D(0f, GlobalPosition);

            var spaceState = GetWorld2d().DirectSpaceState;
            var results = spaceState.IntersectShape(physicsShapeQueryParams, maxResults: 1);

            CanPlace = results.Count == 0 ^ Invert;
        }
    }
}