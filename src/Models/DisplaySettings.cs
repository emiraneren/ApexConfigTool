namespace ApexConfigTool.Models;

/// <summary>
/// Ekran sayfasından okunan bekleyen (henüz uygulanmamış) ayarlar.
/// </summary>
public class DisplaySettings
{
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Fullscreen { get; set; }
    public bool ReadOnly { get; set; }
}
