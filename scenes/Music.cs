using Godot;

namespace TenSecs
{
    public class Music : AudioStreamPlayer
    {
        private static Music INSTANCE;
        private static AudioStreamOGGVorbis menuMusic = GD.Load<AudioStreamOGGVorbis>("res://audio/ld51_mainmenu_trim.ogg");
        private static AudioStreamOGGVorbis mainTheme = GD.Load<AudioStreamOGGVorbis>("res://audio/ld51_theme_trim.ogg");

        public override void _Ready()
        {
            INSTANCE = this;
        }

        public static void PlayMenuMusic()
        {
            if (INSTANCE.Stream != menuMusic)
            {
                INSTANCE.Stream = menuMusic;
                INSTANCE.Play();
            }
        }

        public static void PlayThemeMusic()
        {
            if (INSTANCE.Stream != mainTheme)
            {
                INSTANCE.Stream = mainTheme;
                INSTANCE.Play();
            }
        }

        public static void StopMusic()
        {
            INSTANCE.Stop();
        }
    }
}