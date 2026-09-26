using System;
using System.Diagnostics;
using System.IO;
using ApexConfigTool.Models;

namespace ApexConfigTool.Services;

/// <summary>
/// Apex Legends işleminin çalışıp çalışmadığını kontrol eder ve oyunu başlatır.
/// </summary>
public class ProcessService
{
    public bool IsApexRunning() => Process.GetProcessesByName("r5apex").Length > 0;

    public void LaunchApex(ApexInstallation installation)
    {
        if (IsApexRunning())
            return;

        if (installation.Launcher == "Steam")
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://rungameid/1172470",
                UseShellExecute = true
            });
            return;
        }

        if (!installation.GameFound)
            throw new Exception("Apex kurulumu tespit edilemedi.");

        string protectedExecutable = Path.Combine(installation.GameDirectory, "start_protected_game.exe");
        string r5Executable = Path.Combine(installation.GameDirectory, "r5apex.exe");

        string executable = File.Exists(protectedExecutable)
            ? protectedExecutable
            : File.Exists(r5Executable)
                ? r5Executable
                : throw new FileNotFoundException("Apex çalıştırılabilir dosyası bulunamadı.");

        Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = installation.GameDirectory,
            UseShellExecute = true
        });
    }
}
