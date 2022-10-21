using Godot;

namespace TenSecs
{
    public class MyButton : Button
    {
        public override void _Ready()
        {
            Connect("mouse_entered", SFX.INSTANCE, nameof(SFX.UIClick));
        }
    }
}