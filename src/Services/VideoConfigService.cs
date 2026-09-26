using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ApexConfigTool.Models;

namespace ApexConfigTool.Services;

/// <summary>
/// videoconfig.txt dosyasını okuyup yazan ve "setting.xxx" "değer" biçimindeki
/// anahtar/değer çiftlerini regex ile düzenleyen servis.
/// </summary>
public class VideoConfigService
{
    public string ConfigPath { get; }

    public VideoConfigService(string configPath)
    {
        ConfigPath = configPath;
    }

    public bool Exists() => File.Exists(ConfigPath);

    public string Read()
    {
        if (!Exists())
            throw new FileNotFoundException("videoconfig.txt bulunamadı.");

        return File.ReadAllText(ConfigPath);
    }

    public void Write(string content)
    {
        RemoveReadOnly();
        File.WriteAllText(ConfigPath, content);
    }

    public string GetSetting(string content, string setting)
    {
        string pattern = "\"" + Regex.Escape(setting) + "\"\\s+\"([^\"]*)\"";
        Match match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : "";
    }

    public bool HasSetting(string content, string setting)
    {
        string pattern = "\"" + Regex.Escape(setting) + "\"\\s+\"[^\"]*\"";
        return Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase);
    }

    public string SetSetting(string content, string setting, string value)
    {
        string pattern = "(\"" + Regex.Escape(setting) + "\"\\s+\")([^\"]*)(\")";

        if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
        {
            return Regex.Replace(
                content,
                pattern,
                match => match.Groups[1].Value + value + match.Groups[3].Value,
                RegexOptions.IgnoreCase);
        }

        int finalBrace = content.LastIndexOf('}');
        string newSetting = $"\t\"{setting}\"\t\t\"{value}\"\r\n";

        return finalBrace >= 0
            ? content.Insert(finalBrace, newSetting)
            : content + Environment.NewLine + newSetting;
    }

    /// <summary>
    /// Ayarı yalnızca dosyada zaten mevcutsa günceller; yoksa dokunmadan bırakır.
    /// Böylece oyun sürümleri arasında farklılık gösteren anahtarlar sessizce eklenmez.
    /// </summary>
    public string SetSettingIfPresent(string content, string setting, string value)
    {
        return HasSetting(content, setting) ? SetSetting(content, setting, value) : content;
    }

    public List<ConfigEntry> GetAllSettings(string content)
    {
        var settings = new List<ConfigEntry>();

        foreach (Match match in Regex.Matches(content, "\"([^\"]+)\"\\s+\"([^\"]*)\""))
        {
            settings.Add(new ConfigEntry
            {
                Setting = match.Groups[1].Value,
                Value = match.Groups[2].Value
            });
        }

        return settings;
    }

    public (int Width, int Height)? GetResolution()
    {
        string config = Read();
        string width = GetSetting(config, "setting.defaultres");
        string height = GetSetting(config, "setting.defaultresheight");

        if (int.TryParse(width, out int w) && int.TryParse(height, out int h))
            return (w, h);

        return null;
    }

    public string SetResolution(string config, int width, int height)
    {
        config = SetSetting(config, "setting.defaultres", width.ToString());
        config = SetSetting(config, "setting.defaultresheight", height.ToString());
        return config;
    }

    public string SetFullscreen(string config, bool enabled)
    {
        return SetSetting(config, "setting.fullscreen", enabled ? "1" : "0");
    }

    public string DisableShadows(string config)
    {
        config = SetSettingIfPresent(config, "setting.shadow_enable", "0");
        config = SetSettingIfPresent(config, "setting.shadow_maxdynamic", "0");
        config = SetSettingIfPresent(config, "setting.csm_enabled", "0");
        config = SetSettingIfPresent(config, "setting.csm_coverage", "0");
        config = SetSettingIfPresent(config, "setting.csm_cascade_res", "0");
        config = SetSettingIfPresent(config, "setting.new_shadow_settings", "0");
        config = SetSettingIfPresent(config, "setting.shadow_depth_dimen_min", "0");
        config = SetSettingIfPresent(config, "setting.shadow_depth_upres_factor_max", "0");
        return config;
    }

    public bool IsReadOnly()
    {
        if (!Exists())
            return false;

        return (File.GetAttributes(ConfigPath) & FileAttributes.ReadOnly) != 0;
    }

    public void MakeReadOnly()
    {
        if (!Exists())
            return;

        File.SetAttributes(ConfigPath, File.GetAttributes(ConfigPath) | FileAttributes.ReadOnly);
    }

    public void RemoveReadOnly()
    {
        if (!Exists())
            return;

        var attributes = File.GetAttributes(ConfigPath);
        if ((attributes & FileAttributes.ReadOnly) != 0)
            File.SetAttributes(ConfigPath, attributes & ~FileAttributes.ReadOnly);
    }
}
