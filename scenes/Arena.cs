using Godot;
using System;

namespace TenSecs
{
    public class Arena : Node2D
    {
        public static Arena INSTANCE { get; private set; }
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();
        private static PackedScene enemyScene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
        private static PackedScene smokeParticlesScene = GD.Load<PackedScene>("res://scenes/SmokeParticles.tscn");
        private static PackedScene enemyParticlesScene = GD.Load<PackedScene>("res://scenes/EnemyDeathParticles.tscn");
        private static PackedScene upgradeParticlesScene = GD.Load<PackedScene>("res://scenes/TowerUpgradeParticles.tscn");
        private static Texture paintBarFillFine = GD.Load<Texture>("res://textures/robot_paint_bar_fill.png");
        private static Texture paintBarFillHurt = GD.Load<Texture>("res://textures/robot_paint_bar_fill_hurt.png");
        private static Texture paintBarFillBad = GD.Load<Texture>("res://textures/robot_paint_bar_fill_bad.png");
        private static Texture paintBarFillVeryBad = GD.Load<Texture>("res://textures/robot_paint_bar_fill_verybad.png");
        [Signal]
        public delegate void EveryTenSeconds();
        static Arena()
        {
            RNG.Randomize();
        }
        public CanvasLayer UI { get; private set; }
        public Crystal Crystal { get; set; }
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
        private Label paintHoverLabel;
        private TextureRect helpOverlay;
        private RichTextLabel helpInfo;
        private Panel towerPlacementPanel;


        public override void _Ready()
        {
            Arena.INSTANCE = this;

            robotPaintTexture = GetNode<RobotPaintTexture>("RobotPaintTexture");
            robotPaintTexture.Connect(nameof(RobotPaintTexture.PaintAmountChanged), this, nameof(PaintAmountChanged));
            progressBar = GetNode<TextureProgress>("UI/ProgressBar");
            fpsLabel = GetNode<Label>("UI/FPSLabel");
            towerPlacementPanel = GetNode<Panel>("UI/TowerTooltip");
            towerPlacementLabel = GetNode<RichTextLabel>("UI/TowerTooltip/TowerPlacementLabel");
            towerPlacementShaker = GetNode<Shaker>("UI/TowerTooltip/TowerPlacementShaker");
            timeElapsedLabel = GetNode<Label>("UI/TimeElapsedLabel");
            UI = GetNode<CanvasLayer>("UI");
            Crystal = GetNode<Crystal>("Crystal");
            Crystal.Connect(nameof(Crystal.CrystalDead), this, nameof(CrystalDead));
            helpOverlay = GetNode<TextureRect>("UI/HelpOverlay");
            helpInfo = GetNode<RichTextLabel>("UI/HelpOverlay/H");
            GetNode("UI/PausePopup").Connect(nameof(PausePopup.Quit), Crystal, nameof(Crystal.Quit));
            GetNode("GameOver/EndButton").Connect("pressed", this, nameof(EndPressed));

            var thread = new Godot.Thread();
            thread.Start(this, nameof(CheckRobotPaint), priority: Godot.Thread.Priority.Low);

            GetNode<Timer>("CheckPaintTimer").Connect("timeout", this, nameof(PaintTimeout));
            GetNode<Timer>("EveryTenSeconds").Connect("timeout", this, nameof(EveryTenSecs));

            paintHoverLabel = GetNode<Label>("UI/ProgressBar/PaintHover");
            progressBar.Connect("mouse_entered", paintHoverLabel, "set_visible", new Godot.Collections.Array(true));
            progressBar.Connect("mouse_exited", paintHoverLabel, "set_visible", new Godot.Collections.Array(false));

            Music.PlayThemeMusic();

            SetProcess(false);
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

            timeElapsedLabel.Text = DateTime.FromBinary(599266080000000000).AddSeconds(timeElapsed).ToString("mm:ss");
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

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);


            if (evt.IsActionPressed("help"))
            {
                helpOverlay.Visible = true;
                if (helpInfo.Visible)
                {
                    helpInfo.Visible = false;
                    SetProcess(true);
                    GetNode<Timer>("EveryTenSeconds").Start();
                }

            }
            if (evt.IsActionReleased("help"))
            {
                helpOverlay.Visible = false;
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

            Texture fillTex = paintBarFillFine;
            string hint = "Doin' fine";
            if (paintAmount >= .25f && paintAmount < .5f)
            { fillTex = paintBarFillHurt; hint = "Heal the forest!"; }
            else if (paintAmount >= .5f && paintAmount < .75f)
            { fillTex = paintBarFillBad; hint = "Seriously get rid of the robits' metal."; }
            else if (paintAmount >= .75f)
            { fillTex = paintBarFillVeryBad; hint = "THE FOREST IS DOOMED"; }

            if (progressBar.TextureProgress_ != fillTex)
                progressBar.TextureProgress_ = fillTex;

            paintHoverLabel.Text = string.Format("{0}% ~ {1}", Mathf.RoundToInt(paintAmount * 100).ToString(), hint);

        }

        private void SpawnEnemy(Vector2 position)
        {
            Enemy enemy = enemyScene.Instance<Enemy>();
            AddChild(enemy);
            enemy.Position = position;

            int enemyHealth = Mathf.Clamp(RNG.RandiRange(1, Mathf.RoundToInt(5f / (timeElapsed / 30))) * Mathf.RoundToInt(timeElapsed / 90), 1, 5);

            enemy.CurrentHealth = enemyHealth;

            if (enemyHealth > 1)
                enemy.ThrowGrenades(5f - (enemyHealth * .5f));

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

        public static void MakeUpgradeParticles(Vector2 position)
        {
            MakeParticles(upgradeParticlesScene, position);
        }

        public static void ShowTowerPlacementLabel(string towerName, int price, float shake = 2f, bool upgrading = false)
        {
            INSTANCE.towerPlacementPanel.Visible = true;
            INSTANCE.towerPlacementLabel.BbcodeText = string.Format("[center]{0}: {1}\nCost: {2} {3}\n{4} Place\n{5} Cancel[/center]", upgrading ? "Upgrading" : "Placing", towerName, price, InputImgBBCode.Acorn, InputImgBBCode.LeftClick, InputImgBBCode.RightClick);
            INSTANCE.towerPlacementShaker.Shake(shake);
        }

        public static void HideTowerPlacementLabel()
        {
            INSTANCE.towerPlacementPanel.Visible = false;
        }

        private void CrystalDead()
        {
            Crystal.Disconnect(nameof(Crystal.CrystalDead), this, nameof(CrystalDead));

            SetProcess(false);
            SetPhysicsProcess(false);

            Music.StopMusic();
            SFX.CrystalSmash();

            GetNode<Player>("Player").LockInput = true;

            foreach (Node node in GetChildren())
            {
                if (node is Enemy || node is BaseTower || node is Projectile || node is HealOrb || node is PaintGrenade)
                    node.QueueFree();
            }

            GetNode<AnimationPlayer>("GameOverPlayer").Play("End");

            GetNode<RichTextLabel>("GameOver").BbcodeText += timeElapsedLabel.Text;

            if (timeElapsed > SaveData.MaxTime)
            {
                SaveData.SetValue("max_time", timeElapsed);
                SaveData.SaveToDisk();
                GetNode<RichTextLabel>("GameOver").BbcodeText += "\n[rainbow][wave]New personal best![/wave][/rainbow]";
            }
        }

        private void EndPressed()
        {
            GetTree().ChangeScene("res://scenes/menus/MainMenu.tscn");
            QueueFree();
        }
    }
}
