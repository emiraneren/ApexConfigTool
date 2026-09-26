using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ApexConfigTool.Models;
using ApexConfigTool.Services;

namespace ApexConfigTool.Views;

public partial class BackupsView : UserControl
{
    private BackupService? videoBackupService;
    private AutoexecService? autoexecService;
    private readonly ProcessService processService = new();

    public event Action<string>? StatusChanged;
    public event Action? BackupRestored;

    public BackupsView()
    {
        InitializeComponent();
    }

    public void Configure(BackupService? videoService, AutoexecService? autoexec)
    {
        videoBackupService = videoService;
        autoexecService = autoexec;
        RefreshLists();
    }

    public void RefreshLists()
    {
        VideoBackupList.ItemsSource = videoBackupService?.GetBackups();
        AutoexecBackupList.ItemsSource = autoexecService?.GetBackups();
    }

    private bool CheckApexClosed()
    {
        if (!processService.IsApexRunning())
            return true;

        StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusCloseApex"));
        return false;
    }

    private void RestoreVideoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CheckApexClosed())
                return;

            if (videoBackupService == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusVideoUnavailable"));
                return;
            }

            var selected = VideoBackupList.SelectedItem as BackupEntry;
            if (selected == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusSelectVideo"));
                return;
            }

            var result = MessageBox.Show(
                LocalizationService.Instance.T("BackupsConfirmRestore"),
                LocalizationService.Instance.T("BackupsRestoreTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            videoBackupService.RestoreBackup(selected.FullPath);
            RefreshLists();
            BackupRestored?.Invoke();

            StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusVideoRestored"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("StatusRestoreError", ex.Message));
        }
    }

    private void DeleteVideoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (videoBackupService == null)
                return;

            var selected = VideoBackupList.SelectedItem as BackupEntry;
            if (selected == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusSelectVideo"));
                return;
            }

            var result = MessageBox.Show(
                LocalizationService.Instance.T("BackupsConfirmDelete"),
                LocalizationService.Instance.T("BackupsDeleteTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            videoBackupService.DeleteBackup(selected.FullPath);
            RefreshLists();

            StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusVideoDeleted"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("BackupsStatusDeleteError", ex.Message));
        }
    }

    private void RestoreAutoexecButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CheckApexClosed())
                return;

            if (autoexecService == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusAutoexecUnavailable"));
                return;
            }

            var selected = AutoexecBackupList.SelectedItem as BackupEntry;
            if (selected == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusSelectAutoexec"));
                return;
            }

            var result = MessageBox.Show(
                LocalizationService.Instance.T("BackupsConfirmRestore"),
                LocalizationService.Instance.T("BackupsRestoreTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            autoexecService.RestoreBackup(selected.FullPath);
            RefreshLists();
            BackupRestored?.Invoke();

            StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusAutoexecRestored"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("StatusRestoreError", ex.Message));
        }
    }

    private void DeleteAutoexecButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (autoexecService == null)
                return;

            var selected = AutoexecBackupList.SelectedItem as BackupEntry;
            if (selected == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusSelectAutoexec"));
                return;
            }

            var result = MessageBox.Show(
                LocalizationService.Instance.T("BackupsConfirmDelete"),
                LocalizationService.Instance.T("BackupsDeleteTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            autoexecService.DeleteBackup(selected.FullPath);
            RefreshLists();

            StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusAutoexecDeleted"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("BackupsStatusDeleteError", ex.Message));
        }
    }

    private void OpenVideoFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (videoBackupService == null)
            return;

        OpenFolder(videoBackupService.BackupDirectory);
    }

    private void OpenAutoexecFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (autoexecService == null)
            return;

        OpenFolder(autoexecService.BackupDirectory);
    }

    private void OpenFolder(string path)
    {
        try
        {
            Directory.CreateDirectory(path);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + path + "\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("StatusCouldNotOpenFolder", ex.Message));
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshLists();
        StatusChanged?.Invoke(LocalizationService.Instance.T("BackupsStatusListRefreshed"));
    }
}
