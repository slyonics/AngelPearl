namespace AngelPearl.Main
{
    public enum GameFont
    {
        Console,
        Dialogue,
        Interface,
        LargeConsole,

        None = -1
    }

    public enum GameView
    {
        Conversation_ConversationView,
        Conversation_FastConversationView,
        Conversation_NameView,
        Conversation_SelectionView,
        Crawler_BattleView,
        Crawler_CommandView,
        Crawler_MapView,
        Crawler_TargetView,
        Title_TitleView,

        None = -1
    }

    public enum GameSound
    {
        Back,
        BattleStart,
        Blip,
        Bonk,
        Chest,
        Confirm,
        Cure,
        Cursor,
        Drop,
        Encounter,
        EnemyDeath,
        Error,
        Eruption,
        Fire,
        Fireball,
        Freeze,
        GetItem,
        Heal,
        Ice,
        LevelUp,
        Miss,
        Pickup,
        Ready,
        Savepoint,
        Screech,
        Selection,
        Slash,
        Talk,
        Thunder,

        None = -1
    }

    public enum GameMusic
    {
        VoxTest,
        TheGrappler,

        None = -1
    }

    public enum GameData
    {
        ConversationData,
        EncounterData,
        EnemyData,
        HeroData,
        ItemData,
        ShopData,
        TownData,

        None = -1
    }

    public enum GameShader
    {
        BattleEnemy,
        BattlePlayer,
        Billboard,
        ColorFade,
        Default,
        Portrait,
        Wall,

        None = -1
    }

    public enum GameSprite
    {
        Palette,
        Background_Blank,
        Background_Splash,
        Enemies_Avatar,
        Enemies_Golem,
        NPCs_Chest,
        NPCs_Coral,
        NPCs_Door,
        Particles_Bash,
        Particles_BlackHole,
        Particles_BlueHeal,
        Particles_Cure1,
        Particles_Cure2,
        Particles_DamageDigits,
        Particles_Darkness,
        Particles_Eruption,
        Particles_Exclamation,
        Particles_Fireburst,
        Particles_Freeze,
        Particles_GreenHeal,
        Particles_Heal,
        Particles_Icefall,
        Particles_Impact,
        Particles_Skull,
        Particles_Slash,
        Particles_Smoke,
        Particles_Sonic,
        Particles_Star,
        Particles_Stone,
        Particles_Thunderbolt,
        Particles_Tornado,
        Particles_Weapons,
        Portraits_Aika,
        Portraits_Proxy,
        Tiles_CrawlerTiles,
        Widgets_Gauges_HealthBar,
        Widgets_Gauges_TechGauge,
        Widgets_Gauges_TechGaugeBackground,
        Widgets_Gauges_TechGaugeBar,
        Widgets_Gauges_TechSlider,
        Widgets_Images_Enter,
        Widgets_Images_FoeMarker,
        Widgets_Images_GameLogo,
        Widgets_Images_Icons,
        Widgets_Images_MiniMap,
        Widgets_Images_Pointer,
        Widgets_Images_YouAreHere,
        Widgets_Ninepatches_BattleWindow,
        Widgets_Ninepatches_Blank,
        Widgets_Ninepatches_Cursor,
        Widgets_Ninepatches_LabelGlow,
        Widgets_Ninepatches_MagicBackground,
        Widgets_Ninepatches_MagicForeground,
        Widgets_Ninepatches_MagicFrame,
        Widgets_Ninepatches_MagicPopup,
        Widgets_Ninepatches_MagicPopupSelected,
        Widgets_Ninepatches_MagicWindow,
        Widgets_Ninepatches_OrnateFrame,
        Widgets_Ninepatches_OrnatePanel,
        Widgets_Ninepatches_ThinFrame,
        Widgets_Ninepatches_ThinPanel,
        Widgets_Ninepatches_ThinPanelSelected,
        Background_SealedDoor,

        None = -1
    }

    public enum GameMap
    {
        CirceLabyrinth,
        TestAngel,

        None = -1
    }

}
