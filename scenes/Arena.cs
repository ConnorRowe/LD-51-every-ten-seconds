using Godot;

namespace TenSecs
{
    public class Arena : Node2D
    {
        private static Arena instance;
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();
        static Arena()
        {
            RNG.Randomize();
        }

        private RobotPaintTexture robotPaintTexture;
        private Vector2 lastCursorPos;
        private SceneTreeTimer robotPaintCountTmr;
        private ProgressBar progressBar;
        private bool quantifyMetal = true;
		private Label fpsLabel;
		private Godot.Semaphore paintSemaphore = new Semaphore();


        public override void _Ready()
        {
            Arena.instance = this;

            robotPaintTexture = GetNode<RobotPaintTexture>("RobotPaintTexture");
            progressBar = GetNode<ProgressBar>("ProgressBar");
			fpsLabel = GetNode<Label>("FPSLabel");

            var thread = new Godot.Thread();
            thread.Start(this, nameof(CheckRobotPaint), priority: Godot.Thread.Priority.Low);

			GetNode<Timer>("Timer").Connect("timeout", this, nameof(PaintTimeout));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

			fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
        }

		private void PaintTimeout()
		{
			paintSemaphore.Post();
		}

        private void CheckRobotPaint()
        {
			while(quantifyMetal)
			{
				paintSemaphore.Wait();
				robotPaintTexture.QuantifyMetal();
			}            
        }

        private void PaintAmountChanged(float paintAmount)
        {
            progressBar.Value = paintAmount;
        }

    }
}