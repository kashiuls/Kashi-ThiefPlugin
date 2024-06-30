using Rocket.API;

public class KashiThiefConfiguration : IRocketPluginConfiguration
{
    public string Prefix { get; set; }
    public string PrefixColor { get; set; }
    public string MessageColor { get; set; }
    public string LogoUrl { get; set; }  // bazen calısyo
    public string ItemIdentifiedMessage { get; set; }
    public string TheftSuccessMessage { get; set; }
    public string TheftCancelledMessage { get; set; }
    public string NoTheftMessage { get; set; }
    public string TargetAlertMessage { get; set; }
    public string ThiefAlertMessage { get; set; }
    public string CooldownMessage { get; set; }
    public string AlertPrefix { get; set; }  // calsyo
    public float Radius { get; set; }
    public float TheftCooldown { get; set; }

    public void LoadDefaults()
    {
        Prefix = "[Hırsız Lideri]";
        PrefixColor = "orange"; // calısmıyo
        MessageColor = "yellow"; // calısmıyo
        LogoUrl = "https://i.hizliresim.com/3zbd3mt.jpg";  // bazı mesajlarda calısıyo anlamadım amk
        ItemIdentifiedMessage = "Eşya: {0}";
        TheftSuccessMessage = "Eşyayı başarıyla çaldın.";
        TheftCancelledMessage = "Çok uzağa gittin. Soygun iptal edildi.";
        NoTheftMessage = "Hiçbir eşya çalmadın.";
        TargetAlertMessage = "Bir hırsız eşyanı çaldı, dikkatli ol!";
        ThiefAlertMessage = "Eşya sahibinin dikkatini çektin, kaç!";
        CooldownMessage = "Soygun denemesi yapabilmek için biraz beklemelisin.";
        AlertPrefix = "[6. His]";
        Radius = 3.0f;
        TheftCooldown = 1800.0f; // 30 dakika
    }
}
