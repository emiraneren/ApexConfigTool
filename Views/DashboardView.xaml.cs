using System.Windows.Controls;
using ApexConfigTool.Services;

namespace ApexConfigTool.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    public void UpdateData(string configPath, int width, int height, bool readOnly, bool apexRunning, string launcher, string fov, string fps)
    {
        var loc = LocalizationService.Instance;

        ConfigPathText.Text = configPath;
        ResolutionText.Text = $"{width} × {height}";
        ReadOnlyText.Text = readOnly ? loc.T("DashProtected") : loc.T("DashWritable");
        ApexStateText.Text = apexRunning ? loc.T("DashRunning") : loc.T("DashOff");
        LauncherText.Text = string.IsNullOrWhiteSpace(launcher) ? loc.T("CommonUnknown") : launcher;
        FovText.Text = string.IsNullOrWhiteSpace(fov) ? loc.T("CommonDefault") : fov;
        FpsText.Text = string.IsNullOrWhiteSpace(fps) ? loc.T("CommonDefault") : fps;
    }

    /// <summary>
    /// Yalnızca "Apex çalışıyor mu" kartını günceller. Periyodik canlı kontrol için
    /// kullanılır — diğer alanları (çözünürlük, FOV vb.) sıfırlamadan, hafif bir güncelleme.
    /// </summary>
    public void SetApexRunning(bool apexRunning)
    {
        ApexStateText.Text = apexRunning ? LocalizationService.Instance.T("DashRunning") : LocalizationService.Instance.T("DashOff");
    }
}
