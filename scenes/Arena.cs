using Godot;

namespace TenSecs
{
    public class Arena : Node2D
    {
        public static Arena INSTANCE { get; private set; }
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();
        private static PackedScene enemyScene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
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
        private TowerQueue towerQueue;
        private float spawnEnemyTime = 1f;
        private float currentSpawnCD = 0;
        private float spawnAngle = 0f;
        private Vector2 centrePoint = new Vector2(180, 90);


        public override void _Ready()
        {
            Arena.INSTANCE = this;

            robotPaintTexture = GetNode<RobotPaintTexture>("RobotPaintTexture");
            robotPaintTexture.Connect(nameof(RobotPaintTexture.PaintAmountChanged), this, nameof(PaintAmountChanged));
            progressBar = GetNode<TextureProgress>("ProgressBar");
            fpsLabel = GetNode<Label>("FPSLabel");
            towerQueue = GetNode<TowerQueue>("TowerQueue");
            Connect(nameof(EveryTenSeconds), towerQueue, nameof(towerQueue.EveryTenSeconds));

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
        }

        public override void _PhysicsProcess(float delta)
        {
            if (currentSpawnCD >= spawnEnemyTime)
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
        }

    }
}
