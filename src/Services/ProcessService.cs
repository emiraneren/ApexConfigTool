using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using ApexConfigTool.Models;

namespace ApexConfigTool.Services;

/// <summary>Oyunu başlatma denemesinin nasıl sonuçlandığı.</summary>
public enum LaunchOutcome
{
    /// <summary>Oyun başlatma isteği gönderildi (kurulum tespit edilmişti).</summary>
    Launched,

    /// <summary>
    /// Kurulum tespit edilemedi ama Steam var: istek Steam'e iletildi. Oyun Steam'de
    /// kuruluysa açılır, değilse Steam kurulum penceresini gösterir.
    /// </summary>
    SentToSteam,

    /// <summary>Apex zaten çalışıyordu, bir şey yapılmadı.</summary>
    AlreadyRunning
}

/// <summary>
/// Apex Legends işleminin çalışıp çalışmadığını kontrol eder ve oyunu başlatır.
/// </summary>
public class ProcessService
{
    private const string SteamLaunchUri = "steam://rungameid/1172470";

    public bool IsApexRunning() => Process.GetProcessesByName("r5apex").Length > 0;

    public LaunchOutcome LaunchApex(ApexInstallation installation)
    {
        if (IsApexRunning())
            return LaunchOutcome.AlreadyRunning;

        if (installation.Launcher == "Steam")
        {
            StartSteamUri();
            return LaunchOutcome.Launched;
        }

        if (installation.GameFound)
        {
            string protectedExecutable = Path.Combine(installation.GameDirectory, "start_protected_game.exe");
            string r5Executable = Path.Combine(installation.GameDirectory, "r5apex.exe");

            string? executable = File.Exists(protectedExecutable) ? protectedExecutable
                : File.Exists(r5Executable) ? r5Executable
                : null;

            if (executable != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executable,
                    WorkingDirectory = installation.GameDirectory,
                    UseShellExecute = true
                });
                return LaunchOutcome.Launched;
            }
        }

        // Kurulum klasörü bulunamadı. Steam kuruluysa isteği yine de ona ilet:
        // oyun başka bir kütüphanedeyse Steam bulur, kurulu değilse kurulum penceresini açar.
        if (IsSteamAvailable())
        {
            StartSteamUri();
            return LaunchOutcome.SentToSteam;
        }

        throw new FileNotFoundException(
            "Apex bulunamadı ve Steam kurulu değil (EA App kurulumu için oyun klasörü tespit edilemedi).");
    }

    private static void StartSteamUri()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = SteamLaunchUri,
            UseShellExecute = true
        });
    }

    /// <summary>steam:// bağlantılarını açabilecek bir Steam kurulumu var mı?</summary>
    private static bool IsSteamAvailable()
    {
        try
        {
            using RegistryKey? handler = Registry.ClassesRoot.OpenSubKey(@"steam\shell\open\command");
            if (handler != null)
                return true;

            string? steamExe = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")?.GetValue("SteamExe")?.ToString();
            return !string.IsNullOrWhiteSpace(steamExe) && File.Exists(steamExe);
        }
        catch
        {
            return false;
        }
    }
}
