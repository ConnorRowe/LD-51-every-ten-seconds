using Godot;
using System;

namespace TenSecs
{
    public class TowerUpgrader : Node2D
    {
        public override void _Ready()
        {
            GetNode<Particles2D>("Particles2D").Emitting = true;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            var physicsShapeQueryParams = new Physics2DShapeQueryParameters();
            physicsShapeQueryParams.CollideWithAreas = true;
            physicsShapeQueryParams.CollisionLayer = 8;
            var shape = new CircleShape2D();
            shape.Radius = 32;
            physicsShapeQueryParams.SetShape(shape);
            physicsShapeQueryParams.Transform = new Transform2D(0f, GlobalPosition);

            var spaceState = GetWorld2d().DirectSpaceState;
            var results = spaceState.IntersectShape(physicsShapeQueryParams, maxResults: 1);

            float smallestDist = float.MaxValue;
            BaseTower nearestTower = null;
            foreach (Godot.Collections.Dictionary dict in results)
            {
                if (dict["collider"] is Node node && node.Owner is BaseTower baseTower)
                {
                    float dist = GlobalPosition.DistanceSquaredTo(baseTower.GlobalPosition);
                    if (dist < smallestDist)
                    {
                        smallestDist = dist;
                        nearestTower = baseTower;
                    }
                }
            }

            // If found enemy
            if (nearestTower != null)
                nearestTower.Upgrade();
            else
                GD.Print("Couldn't find a tower to upgrade, this shouldn't happen!!");

            SetPhysicsProcess(false);
            QueueFree();
        }
    }
}