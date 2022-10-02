using Godot;
using System;

namespace TenSecs
{
    public partial class world : Node2D
    {
		private RobotPaintTexture robotPaintTexture;
		private Vector2 lastCursorPos;
		private SceneTreeTimer robotPaintCountTmr;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
			robotPaintTexture = GetNode<RobotPaintTexture>("RobotPaintTexture");
			lastCursorPos = GetViewport().GetMousePosition();

			CheckRobotPaint();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
			var cursorPos = GetViewport().GetMousePosition();

			if(Input.IsMouseButtonPressed(MouseButton.Left))
			{
				robotPaintTexture.DrawSplat(Vec2ToI(cursorPos));
			}

			lastCursorPos = cursorPos;
        }

		private async void CheckRobotPaint()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();

			GD.Print(await robotPaintTexture.QuantifyMetal());

			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;
			GD.Print(string.Format("Execution took {0}ms", elapsedMs));

			robotPaintCountTmr = GetTree().CreateTimer(1, false);
			robotPaintCountTmr.Timeout += this.CheckRobotPaint;
		}

		public static Vector2i Vec2ToI(Vector2 vector2)
		{
			return new Vector2i(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
		}
    }
}