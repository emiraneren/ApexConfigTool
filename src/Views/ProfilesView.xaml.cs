using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ApexConfigTool.Models;
using ApexConfigTool.Services;

namespace ApexConfigTool.Views;

public partial class ProfilesView : UserControl
{
    private ProfileService? profileService;

    public event Action<string>? SaveProfileRequested;
    public event Action<ProfileData>? LoadProfileRequested;
    public event Action<string>? StatusChanged;

    public ProfilesView()
    {
        InitializeComponent();
    }

    public void Configure(ProfileService service)
    {
        profileService = service;
        RefreshProfiles();
    }

    public void RefreshProfiles()
    {
        if (profileService == null)
            return;

        ProfileList.ItemsSource = profileService.GetProfiles();
    }

    private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
    {
        string name = ProfileNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusChanged?.Invoke(LocalizationService.Instance.T("ProfStatusEnterName"));
            return;
        }

        SaveProfileRequested?.Invoke(name);
    }

    private void LoadProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = ProfileList.SelectedItem as ProfileData;
        if (selected == null)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.T("ProfStatusSelect"));
            return;
        }

        var result = MessageBox.Show(
            LocalizationService.Instance.F("ProfConfirmLoad", selected.Name),
            LocalizationService.Instance.T("ProfLoadTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        LoadProfileRequested?.Invoke(selected);
    }

    private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (profileService == null)
                return;

            var selected = ProfileList.SelectedItem as ProfileData;
            if (selected == null)
            {
                StatusChanged?.Invoke(LocalizationService.Instance.T("ProfStatusSelect"));
                return;
            }

            var result = MessageBox.Show(
                LocalizationService.Instance.F("ProfConfirmDelete", selected.Name),
                LocalizationService.Instance.T("ProfDeleteTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            profileService.DeleteProfile(selected);
            RefreshProfiles();

            StatusChanged?.Invoke(LocalizationService.Instance.T("ProfStatusDeleted"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("ProfStatusDeleteError", ex.Message));
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshProfiles();
        StatusChanged?.Invoke(LocalizationService.Instance.T("ProfStatusRefreshed"));
    }

    private void OpenProfilesFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (profileService == null)
                return;

            Directory.CreateDirectory(profileService.ProfilesDirectory);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + profileService.ProfilesDirectory + "\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(LocalizationService.Instance.F("StatusCouldNotOpenFolder", ex.Message));
        }
    }
}
