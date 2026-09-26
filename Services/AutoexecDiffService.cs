using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ApexConfigTool.Models;

namespace ApexConfigTool.Services;

/// <summary>
/// Eski ve yeni autoexec.cfg içeriği arasındaki komut farklarını listeler.
/// </summary>
public class AutoexecDiffService
{
    public List<ConfigChange> Compare(AutoexecService service, string oldContent, string newContent)
    {
        var oldCommands = service.ParseCommands(oldContent);
        var newCommands = service.ParseCommands(newContent);

        var commands = new HashSet<string>(oldCommands.Keys, StringComparer.OrdinalIgnoreCase);
        commands.UnionWith(newCommands.Keys);

        var changes = new List<ConfigChange>();
        var loc = LocalizationService.Instance;

        foreach (string command in commands)
        {
            string oldValue = oldCommands.TryGetValue(command, out string? oldResult) ? oldResult : loc.T("DiffDefault");
            string newValue = newCommands.TryGetValue(command, out string? newResult) ? newResult : loc.T("DiffDefault");

            if (oldValue == newValue)
                continue;

            changes.Add(new ConfigChange
            {
                Source = "Autoexec",
                Setting = GetFriendlyName(command),
                OldValue = FormatValue(command, oldValue),
                NewValue = FormatValue(command, newValue)
            });
        }

        return changes.OrderBy(x => x.Setting).ToList();
    }

    private string GetFriendlyName(string command)
    {
        var loc = LocalizationService.Instance;

        return command switch
        {
            "cl_fovScale" => loc.T("DiffFov"),
            "fov_disableAbilityScaling" => loc.T("DiffAbilityFovScaling"),
            "fps_max" => loc.T("DiffFpsLimit"),
            "lobby_max_fps" => loc.T("DiffLobbyFpsLimit"),
            "m_rawinput" => loc.T("DiffRawMouseInput"),
            "m_acceleration" => loc.T("DiffMouseAcceleration"),
            "m_customaccel" => loc.T("DiffCustomAcceleration"),
            "noise_filter_scale" => loc.T("DiffFilmGrain"),
            "hud_setting_adsDof" => loc.T("DiffAdsDof"),
            "sprint_view_shake_style" => loc.T("DiffSprintViewShake"),
            "r_drawmodeldecals" => loc.T("DiffModelDecals"),
            "r_forcecheapwater" => loc.T("DiffWaterRendering"),
            _ => command
        };
    }

    private string FormatValue(string command, string value)
    {
        var loc = LocalizationService.Instance;

        if (value == loc.T("DiffDefault"))
            return value;

        if (command == "cl_fovScale")
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
            {
                if (Math.Abs(scale - 1.00) < 0.02) return "70";
                if (Math.Abs(scale - 1.27) < 0.02) return "90";
                if (Math.Abs(scale - 1.485714) < 0.02) return "104";
                if (Math.Abs(scale - 1.55) < 0.02) return "110";
                if (Math.Abs(scale - 1.70) < 0.02) return "120";
            }

            return value;
        }

        if (command == "fov_disableAbilityScaling")
            return value == "1" ? loc.T("CommonDisabled") : loc.T("CommonDefault");

        if (command == "m_rawinput")
            return value == "1" ? loc.T("CommonOn") : loc.T("CommonOff");

        if (command == "m_acceleration")
            return value == "0" ? loc.T("CommonDisabled") : loc.T("CommonEnabled");

        return value;
    }
}
