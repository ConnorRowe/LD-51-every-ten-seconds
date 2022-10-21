using Godot;

namespace TenSecs
{
    public class Player : KinematicBody2D
    {
        private static AudioStreamRandomPitch swingMiss = new AudioStreamRandomPitch() { AudioStream = GD.Load<AudioStreamSample>("res://audio/swing_miss.wav"), RandomPitch = 1.2f };
        private static AudioStreamRandomPitch swingHit = new AudioStreamRandomPitch() { AudioStream = GD.Load<AudioStreamSample>("res://audio/swing_hit.wav"), RandomPitch = 1.2f };

        private Vector2 inputDir = Vector2.Zero;
        private const float Acceleration = 250f;
        private const float MaxSpeed = 75f;
        private const float MovementDamping = 10f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 externalVelocity = Vector2.Zero;
        private ShaderMaterial shaderMaterial;
        private AnimationPlayer animationPlayer;
        private const string bounce = "Bounce1";
        private const string bounceSpin = "Bounce2";
        private Node2D bambooAnchor;
        private Sprite bamboo;
        private CollisionShape2D attackCollisionShape;
        private Physics2DShapeQueryParameters attackShapeQueryParams = new Physics2DShapeQueryParameters();
        private bool queueAttack = false;
        private bool moveUp = false;
        private bool moveDown = false;
        private bool moveLeft = false;
        private bool moveRight = false;
        private bool attackHeld = false;
        private SceneTreeTween attackTween;
        private AudioStreamPlayer bambooPlayer;

        public bool LockInput { get; set; } = false;

        public override void _Ready()
        {
            // Setup attack query params
            attackCollisionShape = GetNode<CollisionShape2D>("BambooAnchor/AttackCollisionShape");
            attackShapeQueryParams.SetShape(attackCollisionShape.Shape);
            attackShapeQueryParams.CollideWithAreas = true;
            attackShapeQueryParams.CollideWithBodies = false;
            attackShapeQueryParams.CollisionLayer = 22;

            shaderMaterial = (ShaderMaterial)GetNode<Sprite>("Carrot").Material;
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            bambooAnchor = GetNode<Node2D>("BambooAnchor");
            bamboo = GetNode<Sprite>("BambooAnchor/Bamboo");
            bambooPlayer = GetNode<AudioStreamPlayer>("BambooPlayer");

            animationPlayer.Connect("animation_finished", this, nameof(AnimationFinished));
        }

        public override void _Process(float delta)
        {
            if (LockInput)
                return;

            bambooAnchor.Position = new Vector2(0, -5) + (Position.DirectionTo(GetGlobalMousePosition()) * 7);
            bambooAnchor.Rotation = (bambooAnchor.Position.x / 7f) * .523599f;

            if (!queueAttack && attackHeld && (attackTween == null || (IsInstanceValid(attackTween) && !attackTween.IsRunning())))
            {
                attackTween = CreateTween();
                bamboo.Rotation = 0;
                attackTween.TweenProperty(bamboo, "rotation", Mathf.Tau, 0.15f);
                attackTween.TweenInterval(.25f);
                attackTween.Play();
                queueAttack = true;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (LockInput)
                return;

            externalVelocity -= (externalVelocity * MovementDamping * delta);

            // Get the input direction and handle the movement/deceleration.
            // As good practice, you should replace UI actions with custom gameplay actions.
            float x = 0;
            float y = 0;
            if (moveUp)
                y -= 1f;
            if (moveDown)
                y += 1f;
            if (moveLeft)
                x -= 1f;
            if (moveRight)
                x += 1f;

            inputDir = new Vector2(x, y);
            if (inputDir != Vector2.Zero)
            {
                velocity = inputDir * MaxSpeed;

                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                {
                    shaderMaterial.SetShaderParam("frame", velocity.x >= 0 ? 3 : 1);
                }
                else
                {
                    shaderMaterial.SetShaderParam("frame", velocity.y >= 0 ? 0 : 2);
                }

                if (!animationPlayer.IsPlaying())
                    PlayBounceAnim();
            }
            else
            {
                velocity -= (velocity * MovementDamping * delta);
            }



            MoveAndSlide(velocity + externalVelocity);

            if (queueAttack)
            {
                var spaceState = GetWorld2d().DirectSpaceState;

                attackShapeQueryParams.Transform = new Transform2D(0.0f, attackCollisionShape.GlobalPosition);
                var results = spaceState.IntersectShape(attackShapeQueryParams);

                float force = 300f / results.Count;

                foreach (var dict in results)
                {
                    if (((Godot.Collections.Dictionary)dict)["collider"] is Node node && node.Owner is IPlayerHittable hittable)
                    {
                        hittable.PlayerHit();
                        if (hittable is Enemy enemy)
                            enemy.AddExternalImpulse(Position.DirectionTo(enemy.Position) * force);
                    }
                }

                bambooPlayer.Stream = results.Count > 0 ? swingHit : swingMiss;
                bambooPlayer.Play();

                queueAttack = false;
            }

            ZAsRelative = false;
            ZIndex = (int)GlobalPosition.y;
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (LockInput)
                return;

            if (evt.IsActionPressed("move_up"))
                moveUp = true;
            if (evt.IsActionPressed("move_down"))
                moveDown = true;
            if (evt.IsActionPressed("move_left"))
                moveLeft = true;
            if (evt.IsActionPressed("move_right"))
                moveRight = true;

            if (evt.IsActionReleased("move_up"))
                moveUp = false;
            if (evt.IsActionReleased("move_down"))
                moveDown = false;
            if (evt.IsActionReleased("move_left"))
                moveLeft = false;
            if (evt.IsActionReleased("move_right"))
                moveRight = false;

            if (evt.IsActionPressed("attack"))
                attackHeld = true;
            if (evt.IsActionReleased("attack"))
                attackHeld = false;
        }

        private void AnimationFinished(string name)
        {
            if (inputDir.LengthSquared() > 0)
            {
                PlayBounceAnim();
            }
        }

        private void PlayBounceAnim()
        {
            animationPlayer.Play(Arena.RNG.Randf() > .7f ? bounceSpin : bounce);
        }


    }
}