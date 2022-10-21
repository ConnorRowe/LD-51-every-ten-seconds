using Godot;
using System.Collections.Generic;

namespace TenSecs
{
    public class RobotPaintTexture : Sprite
    {
        [Signal]
        public delegate void PaintAmountChanged(float paintAmount);

        private static RobotPaintTexture instance;
        public static Image.Format ImageFormat { get; } = Image.Format.La8;

        private static Vector2[] plusPixels = new Vector2[5] { Vector2.Up, Vector2.Left, Vector2.Zero, Vector2.Right, Vector2.Down };
        private static Image splatImg = GD.Load<Texture>("res://textures/splat.png").GetData();
        private static Image healMask = GD.Load<Texture>("res://textures/heal_mask.png").GetData();
        private static Image healImg = GD.Load<Texture>("res://textures/heal_img.png").GetData();
        static RobotPaintTexture()
        {
            splatImg.Convert(ImageFormat);
            healMask.Convert(ImageFormat);            
            healImg.Convert(ImageFormat);            
        }
        private ImageTexture drawImgTex = new ImageTexture();
        private Image img;
        private List<Vector2> pixelQueue = new List<Vector2>();
        private List<Vector2> splatQueue = new List<Vector2>();
        private List<Vector2> healQueue = new List<Vector2>();


        [Export]
        public Vector2 ImageSize { get; set; } = new Vector2(360, 180);
        public int PixelCount => (int)(ImageSize.x * ImageSize.y);

        private float paintAmount = 0;
        public static float PaintAmount
        {
            get { return instance.paintAmount; }
            set
            {
                if (instance.paintAmount != value)
                {
                    instance.paintAmount = value;
                    instance.EmitSignal(nameof(PaintAmountChanged), instance.paintAmount);
                }
            }
        }

        private static Color colour = Colors.White;

        private Godot.Mutex imgMutex = new Godot.Mutex();

        public override void _Ready()
        {
            RobotPaintTexture.instance = this;
            ResetImage();
            imgMutex.Unlock();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            imgMutex.Lock();
            img.Lock();

            foreach (Vector2 pixel in pixelQueue)
            {
                if (pixel.x < ImageSize.x && pixel.y < ImageSize.y && pixel.x >= 0 && pixel.y >= 0)
                {
                    img.SetPixelv(pixel, colour);
                }
            }

            foreach (Vector2 dst in splatQueue)
            {
                img.BlitRectMask(splatImg, splatImg, new Rect2(0, 0, 24, 12), dst);
            }

            foreach(Vector2 dst in healQueue)
            {
                img.BlitRectMask(healImg, healMask, new Rect2(0, 0, 36, 18), dst);
            }

            img.Unlock();

            imgMutex.Unlock();

            pixelQueue.Clear();
            splatQueue.Clear();

            drawImgTex.CreateFromImage(img, 1);
            Texture = drawImgTex;

        }

        public void ResetImage()
        {
            img = new Image();
            img.Create(Mathf.RoundToInt(ImageSize.x), Mathf.RoundToInt(ImageSize.y), false, ImageFormat);

            drawImgTex.CreateFromImage(img, 1);
            Texture = drawImgTex;
        }

        private static List<Vector2> SweepPixels(Vector2 start, Vector2 end, int maxPoints)
        {
            var diff_X = end.x - start.x;
            var diff_Y = end.y - start.y;

            var interval_X = diff_X / (maxPoints + 1);
            var interval_Y = diff_Y / (maxPoints + 1);

            List<Vector2> pixels = new List<Vector2>();
            for (int i = 1; i <= maxPoints; i++)
            {
                pixels.Add(new Vector2(start.x + interval_X * i, end.y + interval_Y * i));
            }

            return pixels;
        }

        public static void DrawSweepedPixels(Vector2 start, Vector2 end, int maxPoints)
        {
            instance.pixelQueue.AddRange(SweepPixels(start, end, maxPoints));
        }

        public override void _ExitTree()
        {
            drawImgTex.Dispose();
            img.Dispose();

            base._ExitTree();
        }

        public static void DrawPixel(Vector2 pixel)
        {
            instance.pixelQueue.Add(pixel);
        }

        public static void DrawPlus(Vector2 origin)
        {
            Vector2[] pixels = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                pixels[i] = plusPixels[i] + origin;
            }

            instance.pixelQueue.AddRange(pixels);
        }

        public static void DrawSplat(Vector2 origin)
        {
            instance.splatQueue.Add(origin + new Vector2(-12, -6));
        }

        public static void DrawHeal(Vector2 origin)
        {
            instance.healQueue.Add(origin + new Vector2(-18, -9));
        }

        public void QuantifyMetal()
        {
            int count = 0;

            imgMutex.Lock();
            img.Lock();

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

            img.Unlock();
            imgMutex.Unlock();

            PaintAmount = ((float)count) / ((float)PixelCount);
        }
    }
}