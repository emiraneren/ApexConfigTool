using System;
using System.Windows;
using System.Windows.Controls;
using ApexConfigTool.Services;

namespace ApexConfigTool.Views;

public partial class PerformanceView : UserControl
{
    /// <summary>Seçilen önayarın dahili kimliği (Competitive / MaximumFps / Balanced / Quality).</summary>
    public event Action<string>? PresetRequested;

    public PerformanceView()
    {
        InitializeComponent();
    }

    private void SelectPreset(string presetId, string nameKey, string descriptionKey)
    {
        var loc = LocalizationService.Instance;

        SelectedPresetText.Text = loc.T(nameKey);
        PresetDescriptionText.Text = loc.T(descriptionKey);

        PresetRequested?.Invoke(presetId);
    }

    private void CompetitiveButton_Click(object sender, RoutedEventArgs e)
        => SelectPreset("Competitive", "PerfCompetitive", "PerfCompetitiveDesc");

    private void MaximumFpsButton_Click(object sender, RoutedEventArgs e)
        => SelectPreset("MaximumFps", "PerfMaxFps", "PerfMaxFpsDesc");

    private void BalancedButton_Click(object sender, RoutedEventArgs e)
        => SelectPreset("Balanced", "PerfBalanced", "PerfBalancedDesc");

    private void QualityButton_Click(object sender, RoutedEventArgs e)
        => SelectPreset("Quality", "PerfQuality", "PerfQualityDesc");
}
