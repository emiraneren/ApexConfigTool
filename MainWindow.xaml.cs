using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using ApexConfigTool.Models;
using ApexConfigTool.Services;
using ApexConfigTool.Views;

namespace ApexConfigTool;

public partial class MainWindow : Window
{
    private readonly ApexDetector apexDetector = new();
    private readonly ProcessService processService = new();
    private readonly ProfileService profileService = new();
    private readonly ConfigDiffService diffService = new();
    private readonly AutoexecDiffService autoexecDiffService = new();

    private ApexInstallation? installation;
    private VideoConfigService? videoConfig;
    private BackupService? backupService;
    private AutoexecService? autoexecService;

    private readonly DashboardView dashboardView = new();
    private readonly DisplayView displayView = new();
    private readonly GraphicsView graphicsView = new();
    private readonly ShadowsView shadowsView = new();
    private readonly PerformanceView performanceView = new();
    private readonly AutoexecView autoexecView = new();
    private readonly ProfilesView profilesView = new();
    private readonly BackupsView backupsView = new();
    private readonly AdvancedView advancedView = new();

    public MainWindow()
    {
        InitializeComponent();

        performanceView.PresetRequested += PerformanceView_PresetRequested;

        autoexecView.StatusChanged += ChildView_StatusChanged;

        profilesView.StatusChanged += ChildView_StatusChanged;
        profilesView.SaveProfileRequested += ProfilesView_SaveProfileRequested;
        profilesView.LoadProfileRequested += ProfilesView_LoadProfileRequested;

        backupsView.StatusChanged += ChildView_StatusChanged;
        backupsView.BackupRestored += BackupsView_BackupRestored;

        advancedView.StatusChanged += ChildView_StatusChanged;
        advancedView.ConfigSaved += AdvancedView_ConfigSaved;

        profilesView.Configure(profileService);

        UpdateLanguageButtons();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        StatusText.Text = LocalizationService.Instance.T("StatusReady");
        UpdateProcessStatus();

        DetectConfig();
        ShowDashboard();
    }

    private void DetectConfig()
    {
        try
        {
            installation = apexDetector.Detect();
            autoexecView.LoadInstallation(installation);

            autoexecService = installation.GameFound
                ? new AutoexecService(installation.AutoexecPath)
                : null;

            if (!installation.ConfigFound)
            {
                videoConfig = null;
                backupService = null;

                backupsView.Configure(null, autoexecService);
                advancedView.Configure(null, null);

                StatusText.Text = LocalizationService.Instance.T("StatusConfigNotFound");
                UpdateProcessStatus();
                return;
            }

            videoConfig = new VideoConfigService(installation.VideoConfigPath);
            backupService = new BackupService(installation.VideoConfigPath);

            backupsView.Configure(backupService, autoexecService);
            advancedView.Configure(videoConfig, backupService);

            RefreshAll();

            StatusText.Text = installation.GameFound
                ? LocalizationService.Instance.F("StatusInstallDetected", installation.Launcher)
                : LocalizationService.Instance.T("StatusConfigLoaded");
        }
        catch (Exception ex)
        {
            StatusText.Text = LocalizationService.Instance.F("StatusDetectionError", ex.Message);
        }
    }

    private void RefreshAll()
    {
        if (videoConfig == null || installation == null)
            return;

        var resolution = videoConfig.GetResolution();
        if (resolution == null)
        {
            StatusText.Text = LocalizationService.Instance.T("StatusCouldNotReadResolution");
            return;
        }

        string config = videoConfig.Read();
        bool fullscreen = videoConfig.GetSetting(config, "setting.fullscreen") == "1";
        bool readOnly = videoConfig.IsReadOnly();
        bool apexRunning = processService.IsApexRunning();

        displayView.LoadSettings(resolution.Value.Width, resolution.Value.Height, fullscreen, readOnly);
        graphicsView.LoadSettings(videoConfig, config);
        shadowsView.LoadSettings(videoConfig, config);

        string fov = GetCurrentFov();
        string fps = autoexecService?.GetCommandValue("fps_max") ?? "";

        dashboardView.UpdateData(
            installation.VideoConfigPath,
            resolution.Value.Width,
            resolution.Value.Height,
            readOnly,
            apexRunning,
            installation.Launcher,
            fov,
            fps);

        advancedView.ReloadEverything();
        UpdateProcessStatus();
    }

    private string GetCurrentFov()
    {
        if (autoexecService == null)
            return "";

        string value = autoexecService.GetCommandValue("cl_fovScale");
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
            return "";

        if (Math.Abs(scale - 1.00) < 0.02) return "70";
        if (Math.Abs(scale - 1.27) < 0.02) return "90";
        if (Math.Abs(scale - 1.485714) < 0.02) return "104";
        if (Math.Abs(scale - 1.55) < 0.02) return "110";
        if (Math.Abs(scale - 1.70) < 0.02) return "120";

        return value;
    }

    private void UpdateProcessStatus()
    {
        var loc = LocalizationService.Instance;
        ApexProcessText.Text = processService.IsApexRunning() ? loc.T("ApexRunningPill") : loc.T("ApexOffPill");
    }

    private string? BuildPendingVideoConfig()
    {
        if (videoConfig == null)
        {
            StatusText.Text = LocalizationService.Instance.T("StatusApexConfigNotLoaded");
            return null;
        }

        DisplaySettings? settings = displayView.GetSettings();
        if (settings == null)
        {
            StatusText.Text = LocalizationService.Instance.T("StatusInvalidDisplayConfig");
            return null;
        }

        string config = videoConfig.Read();
        config = videoConfig.SetResolution(config, settings.Width, settings.Height);
        config = videoConfig.SetFullscreen(config, settings.Fullscreen);
        config = shadowsView.ApplyToConfig(videoConfig, config);
        config = graphicsView.ApplyToConfig(videoConfig, config);

        return config;
    }

    private bool ShowPreviewAndApply(bool preferLaunch)
    {
        var loc = LocalizationService.Instance;

        if (videoConfig == null || backupService == null)
        {
            StatusText.Text = loc.T("StatusApexConfigNotFound");
            return false;
        }

        if (processService.IsApexRunning())
        {
            StatusText.Text = loc.T("StatusCloseApexBeforeApply");
            return false;
        }

        string? pendingVideo = BuildPendingVideoConfig();
        if (pendingVideo == null)
            return false;

        string currentVideo = videoConfig.Read();
        List<ConfigChange> videoChanges = diffService.Compare(videoConfig, currentVideo, pendingVideo);

        if (!autoexecView.TryBuildPendingContent(out string currentAutoexec, out string pendingAutoexec, out string autoexecError))
        {
            StatusText.Text = autoexecError;
            return false;
        }

        List<ConfigChange> autoexecChanges = autoexecService != null
            ? autoexecDiffService.Compare(autoexecService, currentAutoexec, pendingAutoexec)
            : new List<ConfigChange>();

        var allChanges = new List<ConfigChange>();
        allChanges.AddRange(videoChanges);
        allChanges.AddRange(autoexecChanges);

        if (allChanges.Count == 0)
        {
            if (preferLaunch)
            {
                LaunchApex();
                return true;
            }

            StatusText.Text = loc.T("StatusNoChangesPending");
            return false;
        }

        var preview = new PreviewWindow(allChanges) { Owner = this };
        bool? result = preview.ShowDialog();

        if (result != true || !preview.ApplyChanges)
        {
            StatusText.Text = loc.T("StatusApplyCancelled");
            return false;
        }

        bool launchAfter = preferLaunch || preview.LaunchAfterApply;

        WritePendingChanges(pendingVideo, videoChanges.Count > 0, pendingAutoexec, autoexecChanges.Count > 0);

        if (launchAfter)
            LaunchApex();

        return true;
    }

    private void WritePendingChanges(string pendingVideo, bool videoChanged, string pendingAutoexec, bool autoexecChanged)
    {
        if (videoConfig == null || backupService == null)
            return;

        DisplaySettings? settings = displayView.GetSettings();
        if (settings == null)
            return;

        if (videoChanged)
        {
            backupService.CreateBackup();
            videoConfig.RemoveReadOnly();
            videoConfig.Write(pendingVideo);

            if (settings.ReadOnly)
                videoConfig.MakeReadOnly();
            else
                videoConfig.RemoveReadOnly();
        }

        if (autoexecChanged && autoexecService != null)
            autoexecService.WriteAll(pendingAutoexec);

        if (installation != null)
            autoexecView.LoadInstallation(installation);

        RefreshAll();
        backupsView.RefreshLists();

        int changedFiles = (videoChanged ? 1 : 0) + (autoexecChanged ? 1 : 0);
        var loc = LocalizationService.Instance;

        StatusText.Text = changedFiles == 2
            ? loc.T("StatusVideoAndAutoexecApplied")
            : videoChanged
                ? loc.T("StatusVideoApplied")
                : loc.T("StatusAutoexecApplied");
    }

    private void LaunchApex()
    {
        var loc = LocalizationService.Instance;

        try
        {
            if (installation == null)
            {
                StatusText.Text = loc.T("StatusInstallationUnavailable");
                return;
            }

            processService.LaunchApex(installation);
            StatusText.Text = loc.T("StatusLaunching");
            UpdateProcessStatus();
        }
        catch (Exception ex)
        {
            StatusText.Text = loc.F("StatusLaunchError", ex.Message);
        }
    }

    private void ShowDashboard() => MainContent.Content = dashboardView;

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshAll();
        ShowDashboard();
    }

    private void DisplayButton_Click(object sender, RoutedEventArgs e) => MainContent.Content = displayView;

    private void GraphicsButton_Click(object sender, RoutedEventArgs e) => MainContent.Content = graphicsView;

    private void ShadowsButton_Click(object sender, RoutedEventArgs e) => MainContent.Content = shadowsView;

    private void PerformanceButton_Click(object sender, RoutedEventArgs e) => MainContent.Content = performanceView;

    private void AutoexecButton_Click(object sender, RoutedEventArgs e) => MainContent.Content = autoexecView;

    private void ProfilesButton_Click(object sender, RoutedEventArgs e)
    {
        profilesView.RefreshProfiles();
        MainContent.Content = profilesView;
    }

    private void BackupsButton_Click(object sender, RoutedEventArgs e)
    {
        backupsView.RefreshLists();
        MainContent.Content = backupsView;
    }

    private void AdvancedButton_Click(object sender, RoutedEventArgs e)
    {
        advancedView.ReloadEverything();
        MainContent.Content = advancedView;
    }

    private void ChildView_StatusChanged(string message) => StatusText.Text = message;

    private void AdvancedView_ConfigSaved()
    {
        RefreshAll();
        backupsView.RefreshLists();
    }

    private void PerformanceView_PresetRequested(string presetId)
    {
        graphicsView.ApplyPreset(presetId);
        shadowsView.ApplyPreset(presetId);

        string nameKey = presetId switch
        {
            "Competitive" => "PerfCompetitive",
            "MaximumFps" => "PerfMaxFps",
            "Balanced" => "PerfBalanced",
            "Quality" => "PerfQuality",
            _ => "PerfCustom"
        };

        StatusText.Text = LocalizationService.Instance.F("StatusPresetLoaded", LocalizationService.Instance.T(nameKey));
    }

    private void ProfilesView_SaveProfileRequested(string profileName)
    {
        var loc = LocalizationService.Instance;

        try
        {
            if (videoConfig == null)
                return;

            string? autoexecContent = autoexecService != null && autoexecService.Exists()
                ? autoexecService.ReadOrEmpty()
                : null;

            profileService.SaveProfile(profileName, videoConfig.Read(), videoConfig.IsReadOnly(), autoexecContent);

            profilesView.RefreshProfiles();
            StatusText.Text = loc.F("StatusProfileSaved", profileName);
        }
        catch (Exception ex)
        {
            StatusText.Text = loc.F("StatusProfileSaveError", ex.Message);
        }
    }

    private void ProfilesView_LoadProfileRequested(ProfileData profile)
    {
        var loc = LocalizationService.Instance;

        try
        {
            if (videoConfig == null || backupService == null)
                return;

            if (processService.IsApexRunning())
            {
                StatusText.Text = loc.T("StatusCloseApexBeforeLoadProfile");
                return;
            }

            backupService.CreateBackup();
            videoConfig.RemoveReadOnly();
            videoConfig.Write(profile.VideoConfigContent);

            if (profile.VideoConfigReadOnly)
                videoConfig.MakeReadOnly();

            if (profile.AutoexecContent != null && autoexecService != null)
                autoexecService.WriteAll(profile.AutoexecContent);

            if (installation != null)
                autoexecView.LoadInstallation(installation);

            RefreshAll();
            backupsView.RefreshLists();

            StatusText.Text = loc.F("StatusProfileLoaded", profile.Name);
        }
        catch (Exception ex)
        {
            StatusText.Text = loc.F("StatusProfileLoadError", ex.Message);
        }
    }

    private void BackupsView_BackupRestored()
    {
        if (installation != null)
            autoexecView.LoadInstallation(installation);

        RefreshAll();
    }

    private void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        var loc = LocalizationService.Instance;

        try
        {
            if (backupService == null)
                return;

            backupService.CreateBackup();

            if (autoexecService != null && autoexecService.Exists())
                autoexecService.CreateBackup();

            backupsView.RefreshLists();
            StatusText.Text = loc.T("StatusBackupCreated");
        }
        catch (Exception ex)
        {
            StatusText.Text = loc.F("StatusBackupError", ex.Message);
        }
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e) => ShowPreviewAndApply(false);

    private void ApplyLaunchButton_Click(object sender, RoutedEventArgs e) => ShowPreviewAndApply(true);

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        var loc = LocalizationService.Instance;

        try
        {
            if (backupService == null)
                return;

            if (processService.IsApexRunning())
            {
                StatusText.Text = loc.T("StatusCloseApexBeforeRestore");
                return;
            }

            bool restored = backupService.RestoreLatest();
            if (!restored)
            {
                StatusText.Text = loc.T("StatusNoBackupFound");
                return;
            }

            RefreshAll();
            backupsView.RefreshLists();

            StatusText.Text = loc.T("StatusLatestRestored");
        }
        catch (Exception ex)
        {
            StatusText.Text = loc.F("StatusRestoreError", ex.Message);
        }
    }

    private void LangTrButton_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService.Instance.Language = AppLanguage.Turkish;
        UpdateLanguageButtons();
        UpdateProcessStatus();
    }

    private void LangEnButton_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService.Instance.Language = AppLanguage.English;
        UpdateLanguageButtons();
        UpdateProcessStatus();
    }

    private void UpdateLanguageButtons()
    {
        bool isTurkish = LocalizationService.Instance.IsTurkish;

        LangTrButton.Style = isTurkish
            ? (Style)FindResource("LanguageToggleActiveButtonStyle")
            : (Style)FindResource("LanguageToggleButtonStyle");

        LangEnButton.Style = !isTurkish
            ? (Style)FindResource("LanguageToggleActiveButtonStyle")
            : (Style)FindResource("LanguageToggleButtonStyle");
    }
}
