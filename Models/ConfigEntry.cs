namespace ApexConfigTool.Models;

/// <summary>
/// videoconfig.txt içindeki tek bir anahtar/değer çifti.
/// </summary>
public class ConfigEntry
{
    public string Setting { get; set; } = "";
    public string Value { get; set; } = "";
}
