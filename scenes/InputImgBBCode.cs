using Godot;

namespace TenSecs
{
    public static class InputImgBBCode
    {
        public static string LeftClick = MakeImgBBCode("res://textures/control_img_left_click.png");
        public static string RightClick = MakeImgBBCode("res://textures/control_img_right_click.png");

        private static string MakeImgBBCode(string imgPath)
        {
            return string.Format("[img]{0}[/img]", imgPath);
        }
    }
}