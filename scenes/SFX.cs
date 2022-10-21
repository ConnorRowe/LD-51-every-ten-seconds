using Godot;

namespace TenSecs
{
    public class SFX : Node
    {
        public static SFX INSTANCE { get; set; }

        private AudioStreamPlayer uiClick;
        private AudioStreamPlayer placeTower;
        private AudioStreamPlayer[] enemyDeaths;
        private AudioStreamPlayer[] enemyHits;
        private AudioStreamPlayer[] towerShoots;
        private AudioStreamPlayer shootGrenade;
        private AudioStreamPlayer grenadeLand;
        private AudioStreamPlayer explosion;
        private AudioStreamPlayer crystalHit;
        private AudioStreamPlayer heal;
        private AudioStreamPlayer shroom;
        private AudioStreamPlayer crystalSmash;

        public override void _Ready()
        {
            base._Ready();

            uiClick = GetNode<AudioStreamPlayer>("UIClick");
            placeTower = GetNode<AudioStreamPlayer>("PlaceTower");
            enemyDeaths = new AudioStreamPlayer[3] { GetNode<AudioStreamPlayer>("EnemyDeath1"), GetNode<AudioStreamPlayer>("EnemyDeath2"), GetNode<AudioStreamPlayer>("EnemyDeath3") };
            enemyHits = new AudioStreamPlayer[3] { GetNode<AudioStreamPlayer>("EnemyHit1"), GetNode<AudioStreamPlayer>("EnemyHit2"), GetNode<AudioStreamPlayer>("EnemyHit3") };
            shootGrenade = GetNode<AudioStreamPlayer>("ShootGrenade");
            grenadeLand = GetNode<AudioStreamPlayer>("GrenadeLand");
            towerShoots = new AudioStreamPlayer[3] { GetNode<AudioStreamPlayer>("TowerShoot1"), GetNode<AudioStreamPlayer>("TowerShoot2"), GetNode<AudioStreamPlayer>("TowerShoot3") };
            explosion = GetNode<AudioStreamPlayer>("Explosion");
            crystalHit = GetNode<AudioStreamPlayer>("CrystalHit");
            heal = GetNode<AudioStreamPlayer>("Heal");
            shroom = GetNode<AudioStreamPlayer>("Shroom");
            crystalSmash = GetNode<AudioStreamPlayer>("CrystalSmash");

            INSTANCE = this;
        }

        public static void UIClick() { INSTANCE.uiClick.Play(); }
        public static void PlaceTower() { INSTANCE.placeTower.Play(); }
        public static void EnemyDeath() { PlayFromArray(INSTANCE.enemyDeaths); }
        public static void EnemyHit() { PlayFromArray(INSTANCE.enemyHits); }
        public static void ShootGrenade() { INSTANCE.shootGrenade.Play(); }
        public static void GrenadeLand() { INSTANCE.grenadeLand.Play(); }
        public static void TowerShoot() { PlayFromArray(INSTANCE.towerShoots); }
        public static void Explosion()
        {
            if (!INSTANCE.explosion.Playing || INSTANCE.explosion.GetPlaybackPosition() >= .47f)
                INSTANCE.explosion.Play();
        }
        public static void CrystalHit() { INSTANCE.crystalHit.Play(); }
        public static void Heal() { INSTANCE.heal.Play(); }
        public static void Shroom() { INSTANCE.shroom.Play(); }
        public static void CrystalSmash() { INSTANCE.crystalSmash.Play(); }

        private static void PlayFromArray(AudioStreamPlayer[] players)
        {
            bool success = false;
            int nearestIdx = 0;
            float nearest = -1;
            for (int i = 0; i < players.Length; i++)
            {
                var ed = players[i];
                if (!ed.Playing)
                {
                    ed.Play();
                    success = true;
                    break;
                }
                else
                {
                    float playbackPos = ed.GetPlaybackPosition();
                    if (playbackPos > nearest)
                    {
                        nearest = playbackPos;
                        nearestIdx = i;
                    }
                }
            }

            if (!success)
                players[nearestIdx].Play(0);
        }
    }
}