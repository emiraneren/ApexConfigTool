using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ApexConfigTool.Models;
using ApexConfigTool.Services;

namespace ApexConfigTool.Views;

public partial class AutoexecView : UserControl
{
    private const string Keep = "__KEEP__";

    private ApexInstallation? installation;
    private AutoexecService? autoexecService;
    private readonly ProcessService processService = new();

    // Sayfa yüklendiğinde diskten okunan değerler; "Uygula" sırasında hangi
    // alanların gerçekten değiştiğini anlamak için saklanır.
    private string loadedFov = Keep;
    private string loadedAbilityFov = Keep;
    private string loadedRawInput = Keep;
    private string loadedAcceleration = Keep;
    private string loadedFilmGrain = Keep;
    private string loadedAdsDof = Keep;
    private string loadedViewShake = Keep;
    private string loadedModelDecals = Keep;
    private string loadedCheapWater = Keep;
    private string loadedFps = "180";
    private string loadedLobbyFps = "60";
    private string loadedGeneralSensitivity = "";
    private string loadedPerScopeToggle = Keep;
    private string loadedFootstepRange = "";
    private string loadedColorblindMode = Keep;

    // mouse_zoomed_sensitivity_scalar_0..7: iç nişangah/1x, 2x, 3x, 4x, 6x, 8x, 10x, Seer pasifi (bu sırayla).
    private TextBox[] scopeSensitivityBoxes = Array.Empty<TextBox>();
    private readonly string[] scopeSensitivityCommands =
    {
        "mouse_zoomed_sensitivity_scalar_0",
        "mouse_zoomed_sensitivity_scalar_1",
        "mouse_zoomed_sensitivity_scalar_2",
        "mouse_zoomed_sensitivity_scalar_3",
        "mouse_zoomed_sensitivity_scalar_4",
        "mouse_zoomed_sensitivity_scalar_5",
        "mouse_zoomed_sensitivity_scalar_6",
        "mouse_zoomed_sensitivity_scalar_7",
    };
    private string[] loadedScopeSensitivity = new string[8];

    public event Action<string>? StatusChanged;

    public AutoexecView()
    {
        InitializeComponent();

        scopeSensitivityBoxes = new[]
        {
            ScopeIronSensBox, Scope2xSensBox, Scope3xSensBox, Scope4xSensBox,
            Scope6xSensBox, Scope8xSensBox, Scope10xSensBox, ScopeSeerSensBox
        };

        ResetCombos();
    }

    private void ResetCombos()
    {
        FovCombo.SelectedIndex = 0;
        AbilityFovCombo.SelectedIndex = 0;
        RawInputCombo.SelectedIndex = 0;
        MouseAccelerationCombo.SelectedIndex = 0;
        FilmGrainCombo.SelectedIndex = 0;
        AdsDofCombo.SelectedIndex = 0;
        ViewShakeCombo.SelectedIndex = 0;
        ModelDecalsCombo.SelectedIndex = 0;
        CheapWaterCombo.SelectedIndex = 0;
        PerScopeSensitivityCombo.SelectedIndex = 0;
        ColorblindModeCombo.SelectedIndex = 0;
    }

    public void LoadInstallation(ApexInstallation apexInstallation)
    {
        installation = apexInstallation;
        LauncherText.Text = apexInstallation.Launcher;
        GamePathText.Text = apexInstallation.GameFound ? apexInstallation.GameDirectory : LocalizationService.Instance.T("CommonNotDetected");
        AutoexecPathText.Text = apexInstallation.GameFound ? apexInstallation.AutoexecPath : LocalizationService.Instance.T("AutoUnavailable");

        if (!apexInstallation.GameFound)
        {
            autoexecService = null;
            AutoexecStateText.Text = LocalizationService.Instance.T("AutoUnavailable");
            LaunchOptionsBox.Text = "";
            RawEditor.Text = "";
            return;
        }

        autoexecService = new AutoexecService(apexInstallation.AutoexecPath);

        LaunchOptionsBox.Text = apexInstallation.Launcher switch
        {
            "Steam" => "+exec autoexec",
            "EA App" => "-exec autoexec",
            _ => "exec autoexec"
        };

        ReloadFromDisk();
    }

    public void ReloadFromDisk()
    {
        if (autoexecService == null)
            return;

        bool exists = autoexecService.Exists();

        AutoexecStateText.Text = exists ? LocalizationService.Instance.T("AutoFound") : LocalizationService.Instance.T("AutoNotCreated");
        AutoexecStateText.Foreground = exists
            ? System.Windows.Media.Brushes.LightGreen
            : System.Windows.Media.Brushes.Khaki;

        RawEditor.Text = autoexecService.ReadOrEmpty();

        LoadFov();
        LoadComboCommand(AbilityFovCombo, "fov_disableAbilityScaling");
        LoadTextCommand(FpsCapBox, "fps_max", "180");
        LoadTextCommand(LobbyFpsCapBox, "lobby_max_fps", "60");
        LoadComboCommand(RawInputCombo, "m_rawinput");
        SelectComboValue(MouseAccelerationCombo, autoexecService.GetCommandValue("m_acceleration"));
        LoadComboCommand(FilmGrainCombo, "noise_filter_scale");
        LoadComboCommand(AdsDofCombo, "hud_setting_adsDof");
        LoadComboCommand(ViewShakeCombo, "sprint_view_shake_style");
        LoadComboCommand(ModelDecalsCombo, "r_drawmodeldecals");
        LoadComboCommand(CheapWaterCombo, "r_forcecheapwater");

        LoadTextCommand(GeneralSensitivityBox, "mouse_sensitivity", "");
        LoadComboCommand(PerScopeSensitivityCombo, "mouse_use_per_scope_sensitivity_scalars");
        for (int i = 0; i < scopeSensitivityBoxes.Length; i++)
            LoadTextCommand(scopeSensitivityBoxes[i], scopeSensitivityCommands[i], "");

        LoadTextCommand(FootstepRangeBox, "cl_footstep_event_max_dist", "");
        LoadComboCommand(ColorblindModeCombo, "colorblind_mode");

        CaptureLoadedState();
    }

    private void CaptureLoadedState()
    {
        loadedFov = GetComboValue(FovCombo);
        loadedAbilityFov = GetComboValue(AbilityFovCombo);
        loadedRawInput = GetComboValue(RawInputCombo);
        loadedAcceleration = GetComboValue(MouseAccelerationCombo);
        loadedFilmGrain = GetComboValue(FilmGrainCombo);
        loadedAdsDof = GetComboValue(AdsDofCombo);
        loadedViewShake = GetComboValue(ViewShakeCombo);
        loadedModelDecals = GetComboValue(ModelDecalsCombo);
        loadedCheapWater = GetComboValue(CheapWaterCombo);
        loadedFps = FpsCapBox.Text.Trim();
        loadedLobbyFps = LobbyFpsCapBox.Text.Trim();

        loadedGeneralSensitivity = GeneralSensitivityBox.Text.Trim();
        loadedPerScopeToggle = GetComboValue(PerScopeSensitivityCombo);

        for (int i = 0; i < scopeSensitivityBoxes.Length; i++)
            loadedScopeSensitivity[i] = scopeSensitivityBoxes[i].Text.Trim();

        loadedFootstepRange = FootstepRangeBox.Text.Trim();
        loadedColorblindMode = GetComboValue(ColorblindModeCombo);
    }

    private void LoadFov()
    {
        if (autoexecService == null)
            return;

        string value = autoexecService.GetCommandValue("cl_fovScale");
        if (string.IsNullOrWhiteSpace(value))
        {
            FovCombo.SelectedIndex = 0;
            return;
        }

        foreach (ComboBoxItem item in FovCombo.Items)
        {
            if (AreFovValuesClose(item.Tag?.ToString() ?? "", value))
            {
                FovCombo.SelectedItem = item;
                return;
            }
        }

        FovCombo.SelectedIndex = 0;
    }

    private bool AreFovValuesClose(string first, string second)
    {
        if (!double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out double a))
            return false;

        if (!double.TryParse(second, NumberStyles.Float, CultureInfo.InvariantCulture, out double b))
            return false;

        return Math.Abs(a - b) < 0.02;
    }

    private void LoadTextCommand(TextBox textBox, string command, string fallback)
    {
        if (autoexecService == null)
            return;

        string value = autoexecService.GetCommandValue(command);
        textBox.Text = string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private void LoadComboCommand(ComboBox combo, string command)
    {
        if (autoexecService == null)
            return;

        SelectComboValue(combo, autoexecService.GetCommandValue(command));
    }

    private void SelectComboValue(ComboBox combo, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            combo.SelectedIndex = 0;
            return;
        }

        foreach (ComboBoxItem item in combo.Items)
        {
            if (item.Tag?.ToString() == value)
            {
                combo.SelectedItem = item;
                return;
            }
        }

        combo.SelectedIndex = 0;
    }

    private string GetComboValue(ComboBox combo)
    {
        var item = combo.SelectedItem as ComboBoxItem;
        return item?.Tag?.ToString() ?? Keep;
    }

    private void ApplyComboChange(Dictionary<string, string> commands, string command, ComboBox combo, string loadedValue)
    {
        string current = GetComboValue(combo);
        if (current == loadedValue)
            return;

        if (current == Keep)
        {
            commands.Remove(command);
            return;
        }

        commands[command] = current;
    }

    /// <summary>
    /// Kutudaki metni ondalıklı bir sayı olarak doğrular; boşsa komutu kaldırır,
    /// aralık dışındaysa hata döner. Hassasiyet/menzil gibi tüm sayısal alanlar için ortak.
    /// </summary>
    private bool ApplyNumericTextChange(
        Dictionary<string, string> commands, string command, TextBox textBox,
        string loadedValue, double min, double max, out string error)
    {
        error = "";
        string current = textBox.Text.Trim();

        if (current == loadedValue)
            return true;

        if (string.IsNullOrWhiteSpace(current))
        {
            commands.Remove(command);
            return true;
        }

        if (!double.TryParse(current, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            || value < min || value > max)
        {
            error = LocalizationService.Instance.F(
                "StatusNumericRange",
                min.ToString(CultureInfo.InvariantCulture),
                max.ToString(CultureInfo.InvariantCulture));
            return false;
        }

        commands[command] = value.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    public bool TryBuildPendingContent(out string currentContent, out string pendingContent, out string error)
    {
        currentContent = "";
        pendingContent = "";
        error = "";

        if (autoexecService == null)
            return true;

        currentContent = autoexecService.ReadOrEmpty();
        var commands = autoexecService.GetManagedCommands();
        var loc = LocalizationService.Instance;

        ApplyComboChange(commands, "cl_fovScale", FovCombo, loadedFov);
        ApplyComboChange(commands, "fov_disableAbilityScaling", AbilityFovCombo, loadedAbilityFov);
        ApplyComboChange(commands, "m_rawinput", RawInputCombo, loadedRawInput);

        string currentAcceleration = GetComboValue(MouseAccelerationCombo);
        if (currentAcceleration != loadedAcceleration)
        {
            if (currentAcceleration == Keep)
            {
                commands.Remove("m_acceleration");
                commands.Remove("m_customaccel");
            }
            else
            {
                commands["m_acceleration"] = currentAcceleration;
                commands["m_customaccel"] = currentAcceleration;
            }
        }

        ApplyComboChange(commands, "noise_filter_scale", FilmGrainCombo, loadedFilmGrain);
        ApplyComboChange(commands, "hud_setting_adsDof", AdsDofCombo, loadedAdsDof);
        ApplyComboChange(commands, "sprint_view_shake_style", ViewShakeCombo, loadedViewShake);
        ApplyComboChange(commands, "r_drawmodeldecals", ModelDecalsCombo, loadedModelDecals);
        ApplyComboChange(commands, "r_forcecheapwater", CheapWaterCombo, loadedCheapWater);

        string fps = FpsCapBox.Text.Trim();
        if (fps != loadedFps)
        {
            if (string.IsNullOrWhiteSpace(fps))
            {
                commands.Remove("fps_max");
            }
            else
            {
                if (!int.TryParse(fps, out int fpsValue) || fpsValue < 30 || fpsValue > 1000)
                {
                    error = loc.T("StatusFpsRange");
                    return false;
                }

                commands["fps_max"] = fpsValue.ToString();
            }
        }

        string lobbyFps = LobbyFpsCapBox.Text.Trim();
        if (lobbyFps != loadedLobbyFps)
        {
            if (string.IsNullOrWhiteSpace(lobbyFps))
            {
                commands.Remove("lobby_max_fps");
            }
            else
            {
                if (!int.TryParse(lobbyFps, out int lobbyValue) || lobbyValue < 20 || lobbyValue > 300)
                {
                    error = loc.T("StatusLobbyFpsRange");
                    return false;
                }

                commands["lobby_max_fps"] = lobbyValue.ToString();
            }
        }

        ApplyComboChange(commands, "mouse_use_per_scope_sensitivity_scalars", PerScopeSensitivityCombo, loadedPerScopeToggle);
        ApplyComboChange(commands, "colorblind_mode", ColorblindModeCombo, loadedColorblindMode);

        if (!ApplyNumericTextChange(commands, "mouse_sensitivity", GeneralSensitivityBox, loadedGeneralSensitivity, 0.01, 100, out error))
            return false;

        for (int i = 0; i < scopeSensitivityBoxes.Length; i++)
        {
            if (!ApplyNumericTextChange(commands, scopeSensitivityCommands[i], scopeSensitivityBoxes[i], loadedScopeSensitivity[i], 0.01, 10, out error))
                return false;
        }

        if (!ApplyNumericTextChange(commands, "cl_footstep_event_max_dist", FootstepRangeBox, loadedFootstepRange, 0, 20000, out error))
            return false;

        pendingContent = autoexecService.BuildManagedContent(currentContent, commands);
        return true;
    }

    private bool CanWriteRaw()
    {
        if (autoexecService == null || installation == null || !installation.GameFound)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusNotDetected"));
            return false;
        }

        if (processService.IsApexRunning())
        {
            StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusCloseApexBeforeEdit"));
            return false;
        }

        return true;
    }

    private void SaveRawButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CanWriteRaw())
                return;

            autoexecService!.WriteAll(RawEditor.Text);
            ReloadFromDisk();

            StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusSaved"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("AutoStatusSaveError", ex.Message));
        }
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        ReloadFromDisk();
        StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusReloaded"));
    }

    private void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (autoexecService == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusUnavailable"));
                return;
            }

            string? backup = autoexecService.CreateBackup();
            if (backup == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusNoFileYet"));
                return;
            }

            StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusBackupCreated"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("AutoStatusBackupError", ex.Message));
        }
    }

    private void CopyLaunchOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LaunchOptionsBox.Text))
            return;

        Clipboard.SetText(LaunchOptionsBox.Text);
        StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusLaunchOptionsCopied"));
    }

    private void OpenCfgFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (installation == null || !installation.GameFound)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("AutoStatusNotDetected"));
                return;
            }

            Directory.CreateDirectory(installation.CfgDirectory);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + installation.CfgDirectory + "\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("StatusCouldNotOpenFolder", ex.Message));
        }
    }
}
