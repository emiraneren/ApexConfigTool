using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ApexConfigTool.Models;

namespace ApexConfigTool.Services;

/// <summary>
/// Kullanıcı profillerini (%LOCALAPPDATA%\ApexConfigTool\Profiles altında JSON olarak) yönetir.
/// </summary>
public class ProfileService
{
    private readonly string profilesDirectory;

    public string ProfilesDirectory => profilesDirectory;

    public ProfileService()
    {
        profilesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ApexConfigTool",
            "Profiles");

        Directory.CreateDirectory(profilesDirectory);
    }

    public ProfileData SaveProfile(string name, string videoConfigContent, bool videoConfigReadOnly, string? autoexecContent)
    {
        string cleanName = SanitizeFileName(name.Trim());
        if (string.IsNullOrWhiteSpace(cleanName))
            throw new Exception("Geçersiz profil adı.");

        var profile = new ProfileData
        {
            Name = name.Trim(),
            CreatedAt = DateTime.Now,
            VideoConfigContent = videoConfigContent,
            VideoConfigReadOnly = videoConfigReadOnly,
            AutoexecContent = autoexecContent
        };

        string path = Path.Combine(profilesDirectory, cleanName + ".json");
        string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(path, json);
        profile.FilePath = path;

        return profile;
    }

    public List<ProfileData> GetProfiles()
    {
        Directory.CreateDirectory(profilesDirectory);

        var profiles = new List<ProfileData>();

        foreach (string file in Directory.GetFiles(profilesDirectory, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                ProfileData? profile = JsonSerializer.Deserialize<ProfileData>(json);

                if (profile == null)
                    continue;

                profile.FilePath = file;
                profiles.Add(profile);
            }
            catch
            {
                // Bozuk bir profil dosyası tüm uygulamayı çökertmemeli, yalnızca atlanır.
            }
        }

        return profiles.OrderByDescending(profile => profile.CreatedAt).ToList();
    }

    public void DeleteProfile(ProfileData profile)
    {
        if (string.IsNullOrWhiteSpace(profile.FilePath))
            return;

        string root = Path.GetFullPath(profilesDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        string target = Path.GetFullPath(profile.FilePath);

        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Geçersiz profil yolu.");

        if (File.Exists(target))
            File.Delete(target);
    }

    private string SanitizeFileName(string name)
    {
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            name = name.Replace(invalidCharacter, '_');

        return name.Trim();
    }
}
