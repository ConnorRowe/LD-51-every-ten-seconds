using Godot;

public class SmokeParticles : Particles2D
{
    public override void _Ready()
    {
        Emitting = true;

        var timer = GetTree().CreateTimer(Lifetime, false);
        timer.Connect("timeout", this, "queue_free");
    }
}
