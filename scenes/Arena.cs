using Godot;

namespace TenSecs
{
    public class Arena : Node2D
    {
        public static Arena INSTANCE { get; private set; }
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();
        private static PackedScene enemyScene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
        private static PackedScene smokeParticlesScene = GD.Load<PackedScene>("res://scenes/SmokeParticles.tscn");
        private static PackedScene enemyParticlesScene = GD.Load<PackedScene>("res://scenes/EnemyDeathParticles.tscn");
        [Signal]
        public delegate void EveryTenSeconds();
        static Arena()
        {
            RNG.Randomize();
        }

        private RobotPaintTexture robotPaintTexture;
        private Vector2 lastCursorPos;
        private TextureProgress progressBar;
        private bool quantifyMetal = true;
        private Label fpsLabel;
        private Godot.Semaphore paintSemaphore = new Semaphore();
        private float spawnEnemyTime = 2.5f;
        private float currentSpawnCD = 0;
        private float spawnAngle = 0f;
        private Vector2 centrePoint = new Vector2(180, 90);
        private float timeElapsed = 0f;
        private RichTextLabel towerPlacementLabel;
        private Shaker towerPlacementShaker;
        private Label timeElapsedLabel;


        public override void _Ready()
        {
            Arena.INSTANCE = this;

            robotPaintTexture = GetNode<RobotPaintTexture>("RobotPaintTexture");
            robotPaintTexture.Connect(nameof(RobotPaintTexture.PaintAmountChanged), this, nameof(PaintAmountChanged));
            progressBar = GetNode<TextureProgress>("UI/ProgressBar");
            fpsLabel = GetNode<Label>("UI/FPSLabel");
            towerPlacementLabel = GetNode<RichTextLabel>("UI/TowerPlacementLabel");
            towerPlacementShaker = GetNode<Shaker>("UI/TowerPlacementShaker");
            timeElapsedLabel = GetNode<Label>("UI/TimeElapsedLabel");

            var thread = new Godot.Thread();
            thread.Start(this, nameof(CheckRobotPaint), priority: Godot.Thread.Priority.Low);

            GetNode<Timer>("CheckPaintTimer").Connect("timeout", this, nameof(PaintTimeout));
            GetNode<Timer>("EveryTenSeconds").Connect("timeout", this, nameof(EveryTenSecs));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            spawnAngle += Mathf.Tau * delta * .05f;
            if (spawnAngle >= Mathf.Tau)
                spawnAngle -= Mathf.Tau;

            currentSpawnCD += delta;

            fpsLabel.Text = Engine.GetFramesPerSecond().ToString();

            timeElapsed += delta;

            timeElapsedLabel.Text = string.Format("{0:00}:{1:00}", (timeElapsed / 60) % 60, timeElapsed % 60);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (currentSpawnCD >= Mathf.Clamp(spawnEnemyTime * (1f - (timeElapsed / 500)), .25f, 2.5f))
            {
                currentSpawnCD = 0;

                var space = GetWorld2d().DirectSpaceState;
                var result = space.IntersectRay(centrePoint, centrePoint + (new Vector2(Mathf.Cos(spawnAngle), Mathf.Sin(spawnAngle)) * 210f), collisionLayer: 4);

                if (result.Count > 0)
                    SpawnEnemy((Vector2)result["position"]);
                else
                    GD.Print("Failed to spawn an enemy, this should never happen!");
            }
        }

        private void EveryTenSecs()
        {
            EmitSignal(nameof(EveryTenSeconds));
        }

        private void PaintTimeout()
        {
            paintSemaphore.Post();
        }

        private void CheckRobotPaint()
        {
            while (quantifyMetal)
            {
                paintSemaphore.Wait();
                robotPaintTexture.QuantifyMetal();
            }
        }

        private void PaintAmountChanged(float paintAmount)
        {
            progressBar.Value = paintAmount;
        }

        private void SpawnEnemy(Vector2 position)
        {
            Enemy enemy = enemyScene.Instance<Enemy>();
            AddChild(enemy);
            enemy.Position = position;

            int enemyHealth = Mathf.Clamp(RNG.RandiRange(1, Mathf.RoundToInt(5f / (timeElapsed / 30))) * Mathf.RoundToInt(timeElapsed / 90), 1, 5);

            enemy.CurrentHealth = enemyHealth;
        }

        private static void MakeParticles(PackedScene particlesScene, Vector2 position)
        {
            SmokeParticles smoke = particlesScene.Instance<SmokeParticles>();
            INSTANCE.AddChild(smoke);
            smoke.ZIndex = (int)position.y;
            smoke.GlobalPosition = position;
        }

        public static void MakeSmokeParticles(Vector2 position)
        {
            MakeParticles(smokeParticlesScene, position);
        }

        public static void MakeEnemyDeathParticles(Vector2 position)
        {
            MakeParticles(enemyParticlesScene, position);
        }

        public static void ShowTowerPlacementLabel(string towerName, int price)
        {
            INSTANCE.towerPlacementLabel.BbcodeText = string.Format("[center]Placing: {0}\nCost: {1}\n{2} Place\n{3} Cancel[/center]", towerName, price, InputImgBBCode.LeftClick, InputImgBBCode.RightClick);
            INSTANCE.towerPlacementLabel.Visible = true;
            INSTANCE.towerPlacementShaker.Shake(2);
        }

        public static void HideTowerPlacementLabel()
        {
            INSTANCE.towerPlacementLabel.Visible = false;
        }
    }
}
