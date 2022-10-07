using Godot;

namespace TenSecs
{
    public class Enemy : KinematicBody2D
    {
        protected Vector2 inputDir = Vector2.Zero;
        [Export]
        protected float acceleration = 20f;
        [Export]
        protected float maxSpeed = 200f;
        [Export]
        protected float movementDampening = 3f;
        protected Vector2 velocity = Vector2.Zero;
        protected Vector2 externalVelocity = Vector2.Zero;
        private bool drawFlip = false;
        private float dirSineOffset = Arena.RNG.RandfRange(-Mathf.Tau, Mathf.Tau);
        private AnimatedSprite sprite;
        private bool isFacingRight = false;
        private ShaderMaterial shaderMaterial;
        private Shaker shaker;

        public bool CanAttack { get; set; } = true;

        public override void _Ready()
        {
            base._Ready();

            sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            shaderMaterial = (ShaderMaterial)sprite.Material;
            shaker = GetNode<Shaker>("Shaker");
        }

        public override void _Process(float delta)
        {
            if (drawFlip)
            {
                if (Arena.RNG.Randf() > .95f)
                    RobotPaintTexture.DrawPlus(Position);
                else
                    RobotPaintTexture.DrawPixel(Position);
            }

            drawFlip = !drawFlip;

            dirSineOffset += delta * Mathf.Tau * Arena.RNG.Randf();
        }

        public override void _PhysicsProcess(float delta)
        {
            inputDir = GetInputDir().Normalized();

            velocity -= (velocity * movementDampening * delta);

            velocity += inputDir * acceleration * delta;

            externalVelocity -= (externalVelocity * 10f * delta);

            if (velocity.Length() > maxSpeed)
            {
                velocity = velocity.Normalized() * maxSpeed;
            }

            MoveAndSlide(velocity + externalVelocity);

            // Check if facing dir has changed
            if (!Mathf.IsEqualApprox(velocity.x, 0f))
            {
                if (velocity.x > 0f && !isFacingRight)
                {
                    FlipDirection();
                }
                else if (velocity.x < 0f && isFacingRight)
                {
                    FlipDirection();
                }
            }

            ZAsRelative = false;
            ZIndex = (int)GlobalPosition.y;
        }

        private void FlipDirection()
        {
            sprite.FlipH = !sprite.FlipH;
            isFacingRight = !isFacingRight;
        }

        protected Vector2 GetInputDir()
        {
            return GlobalPosition.DirectionTo(new Vector2(180, 90)).Rotated(Mathf.Sin(dirSineOffset));
        }

        public void AddExternalImpulse(Vector2 impulse)
        {
            externalVelocity += impulse;
        }

        public void Hit()
        {
            SceneTreeTween hitTween = GetTree().CreateTween();
            hitTween.TweenProperty(shaderMaterial, "shader_param/flash_lerp", 1.25f, .06f);
            var ret = hitTween.TweenProperty(shaderMaterial, "shader_param/flash_lerp", 0f, .1f);
            ret.SetDelay(.06f);
            hitTween.Play();
            shaker.Shake(.8f);
        }

        public void JustAttacked()
        {
            CanAttack = false;
            var tmr = GetTree().CreateTimer(1f);
            tmr.Connect("timeout", this, nameof(ResetCanAttack));
        }

        private void ResetCanAttack()
        {
            CanAttack = true;
        }

    }
}