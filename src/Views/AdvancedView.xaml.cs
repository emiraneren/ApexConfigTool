using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApexConfigTool.Models;
using ApexConfigTool.Services;

namespace ApexConfigTool.Views;

public partial class AdvancedView : UserControl
{
    private VideoConfigService? videoConfig;
    private BackupService? backupService;
    private readonly ProcessService processService = new();
    private List<ConfigEntry> allSettings = new();

    public event Action<string>? StatusChanged;
    public event Action? ConfigSaved;

    public AdvancedView()
    {
        InitializeComponent();
    }

    public void Configure(VideoConfigService? videoService, BackupService? backups)
    {
        videoConfig = videoService;
        backupService = backups;
        ReloadEverything();
    }

    public void ReloadEverything()
    {
        if (videoConfig == null || !videoConfig.Exists())
        {
            RawEditor.Text = "";
            SettingsGrid.ItemsSource = null;
            SettingCountText.Text = LocalizationService.Instance.F("AdvSettingsCount", 0);
            return;
        }

        string content = videoConfig.Read();
        RawEditor.Text = content;
        allSettings = videoConfig.GetAllSettings(content);

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        string search = SearchBox.Text.Trim();

        List<ConfigEntry> result = string.IsNullOrWhiteSpace(search)
            ? allSettings
            : allSettings
                .Where(x =>
                    x.Setting.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.Value.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        SettingsGrid.ItemsSource = result;
        SettingCountText.Text = LocalizationService.Instance.F("AdvSettingsCount", result.Count);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        ReloadEverything();
        StatusChanged?.Invoke(LocalizationService.Instance.T("AdvStatusRefreshed"));
    }

    private void ReloadRawButton_Click(object sender, RoutedEventArgs e)
    {
        ReloadEverything();
        StatusChanged?.Invoke(LocalizationService.Instance.T("AdvStatusReloadedFromDisk"));
    }

    private void SaveRawButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (videoConfig == null || backupService == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("AdvStatusConfigNotLoaded"));
                return;
            }

            if (processService.IsApexRunning())
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("AdvStatusCloseApexBeforeSave"));
                return;
            }

            var result = MessageBox.Show(
                LocalizationService.Instance.T("AdvConfirmSaveRaw"),
                LocalizationService.Instance.T("AdvSaveRawTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            backupService.CreateBackup();
            videoConfig.RemoveReadOnly();
            videoConfig.Write(RawEditor.Text);

            ReloadEverything();
            ConfigSaved?.Invoke();

            StatusChanged?.Invoke(LocalizationService.Instance.T("AdvStatusSavedSuccessfully"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("AdvStatusSaveError", ex.Message));
        }
    }
}
