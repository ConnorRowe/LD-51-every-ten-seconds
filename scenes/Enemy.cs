using Godot;

namespace TenSecs
{
    public class Enemy : KinematicBody2D, IPlayerHittable
    {
        private static PackedScene grenadeScene = GD.Load<PackedScene>("res://scenes/PaintGrenade.tscn");

        [Signal]
        public delegate void Dying();

        private float[] healthHueShifts = new float[5] { .385f, 0f, .793f, .349f, .582f };

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
        private ShaderMaterial shaderMaterial = null;
        private Shaker shaker;
        private Timer grenadeTimer;

        private int currentHealth = 1;
        public int CurrentHealth
        {
            get { return currentHealth; }
            set
            {
                if (currentHealth != value)
                {
                    currentHealth = value;

                    if (shaderMaterial != null)
                        shaderMaterial.SetShaderParam("hueshift_amount", healthHueShifts[Mathf.Clamp(currentHealth - 1, 0, healthHueShifts.Length - 1)]);
                }
            }
        }

        public bool CanAttack { get; set; } = true;

        public override void _Ready()
        {
            base._Ready();

            sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            shaderMaterial = (ShaderMaterial)sprite.Material;
            shaderMaterial.SetShaderParam("hueshift_amount", healthHueShifts[currentHealth - 1]);
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

            if (velocity.Length() > GetMaxSpeed())
            {
                velocity = velocity.Normalized() * GetMaxSpeed();
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

        public void ThrowGrenades(float interval)
        {
            grenadeTimer = new Timer();
            grenadeTimer.WaitTime = interval;
            AddChild(grenadeTimer);
            grenadeTimer.Connect("timeout", this, nameof(ThrowGrenade));
            grenadeTimer.Start();
        }

        private void ThrowGrenade()
        {
            var grenade = grenadeScene.Instance<PaintGrenade>();
            Arena.INSTANCE.AddChild(grenade);
            grenade.Position = Position;
            grenade.ZIndex = ZIndex;
            SFX.ShootGrenade();
        }

        private float GetMaxSpeed()
        {
            return maxSpeed + (maxSpeed * .5f * (currentHealth - 1));
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

        void IPlayerHittable.PlayerHit()
        {
            Hit(0);
        }

        public void Hit(int damage)
        {
            SceneTreeTween hitTween = GetTree().CreateTween();
            hitTween.TweenProperty(shaderMaterial, "shader_param/flash_lerp", 1.25f, .06f);
            var ret = hitTween.TweenProperty(shaderMaterial, "shader_param/flash_lerp", 0f, .1f);
            ret.SetDelay(.06f);
            hitTween.Play();
            shaker.Shake(.8f);
            TakeDamage(damage);
        }

        private void TakeDamage(int damage)
        {
            CurrentHealth -= damage;

            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                SFX.EnemyHit();
            }
        }

        private void Die()
        {
            EmitSignal(nameof(Dying));

            Arena.MakeEnemyDeathParticles(GlobalPosition);

            SFX.EnemyDeath();

            QueueFree();
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