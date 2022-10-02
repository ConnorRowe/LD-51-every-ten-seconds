using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TenSecs
{
    public partial class RobotPaintTexture : Sprite2D
    {
        public static Image.Format ImageFormat { get; } = Image.Format.L8;

        private static Vector2i[] plusPixels = new Vector2i[5] { Vector2i.Up, Vector2i.Left, Vector2i.Zero, Vector2i.Right, Vector2i.Down };
        private static Image splatImg = GD.Load<Texture2D>("res://textures/splat.png").GetImage();
        private static Image splatMask = GD.Load<Texture2D>("res://textures/splat.png").GetImage();
        static RobotPaintTexture()
        {
            splatImg.Convert(ImageFormat);
            splatMask.Convert(Image.Format.La8);
        }
        private ImageTexture drawImgTex;
        private Image img;
        private List<Vector2i> pixelQueue = new List<Vector2i>();
        private List<Vector2i> splatQueue = new List<Vector2i>();


        [Export]
        public Vector2 ImageSize { get; set; } = new Vector2(360, 180);

        private static Color colour = Colors.White;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ResetImage();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);

            foreach (Vector2i pixel in pixelQueue)
            {
                if (pixel.x < ImageSize.x && pixel.y < ImageSize.y && pixel.x >= 0 && pixel.y >= 0)
                {
                    img.SetPixelv(pixel, colour);
                }
            }
            pixelQueue.Clear();

            foreach (Vector2i dst in splatQueue)
            {
                img.BlitRectMask(splatImg, splatMask, new Rect2i(0, 0, 24, 12), dst);
            }
            splatQueue.Clear();


            drawImgTex = ImageTexture.CreateFromImage(img);
            Texture = drawImgTex;
        }

        public void ResetImage()
        {
            img = new Image();
            img.Create(Mathf.RoundToInt(ImageSize.x), Mathf.RoundToInt(ImageSize.y), false, ImageFormat);

            drawImgTex = ImageTexture.CreateFromImage(img);
            Texture = drawImgTex;
        }

        private List<Vector2i> SweepPixels(Vector2 start, Vector2 end, int maxPoints)
        {
            var diff_X = end.x - start.x;
            var diff_Y = end.y - start.y;

            var interval_X = diff_X / (maxPoints + 1);
            var interval_Y = diff_Y / (maxPoints + 1);

            List<Vector2i> pixels = new List<Vector2i>();
            for (int i = 1; i <= maxPoints; i++)
            {
                pixels.Add(world.Vec2ToI(new Vector2(start.x + interval_X * i, end.y + interval_Y * i)));
            }

            return pixels;
        }

        public void DrawSweepedPixels(Vector2i start, Vector2i end, int maxPoints)
        {
            pixelQueue.AddRange(SweepPixels(start, end, maxPoints));
        }

        public override void _ExitTree()
        {
            drawImgTex.Dispose();
            img.Dispose();

            base._ExitTree();
        }

        public void DrawPixel(Vector2i pixel)
        {
            pixelQueue.Add(pixel);
        }

        public void DrawPlus(Vector2i origin)
        {
            Vector2i[] pixels = new Vector2i[5];
            for (int i = 0; i < 5; i++)
            {
                pixels[i] = plusPixels[i] + origin;
            }

            pixelQueue.AddRange(pixels);
        }

        public void DrawSplat(Vector2i origin)
        {
            splatQueue.Add(origin + new Vector2i(-12, -6));
        }

        public async Task<int> QuantifyMetal()
        {
            int count = 0;
            await Task.Run(() =>
            {
                for (int x = 0; x < ImageSize.x; x++)
                {
                    for (int y = 0; y < ImageSize.y; y++)
                    {
                        if (img.GetPixel(x, y).r > 0)
                        {
                            count++;
                        }
                    }
                }
            }
            );
            return count;
        }
    }
}