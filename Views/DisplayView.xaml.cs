using System.Windows;
using System.Windows.Controls;
using ApexConfigTool.Models;

namespace ApexConfigTool.Views;

public partial class DisplayView : UserControl
{
    public DisplayView()
    {
        InitializeComponent();
    }

    private void ResolutionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomResolutionPanel == null)
            return;

        var selected = ResolutionCombo.SelectedItem as ComboBoxItem;
        string tag = selected?.Tag?.ToString() ?? "";

        CustomResolutionPanel.Visibility = tag == "custom" ? Visibility.Visible : Visibility.Collapsed;
    }

    public void LoadSettings(int width, int height, bool fullscreen, bool readOnly)
    {
        string currentTag = $"{width}x{height}";
        bool found = false;

        foreach (ComboBoxItem item in ResolutionCombo.Items)
        {
            if (item.Tag?.ToString() == currentTag)
            {
                ResolutionCombo.SelectedItem = item;
                found = true;
                break;
            }
        }

        if (!found)
        {
            foreach (ComboBoxItem item in ResolutionCombo.Items)
            {
                if (item.Tag?.ToString() == "custom")
                {
                    ResolutionCombo.SelectedItem = item;
                    break;
                }
            }

            CustomWidthBox.Text = width.ToString();
            CustomHeightBox.Text = height.ToString();
        }

        FullscreenCheck.IsChecked = fullscreen;
        ReadOnlyCheck.IsChecked = readOnly;
    }

    public DisplaySettings? GetSettings()
    {
        var selected = ResolutionCombo.SelectedItem as ComboBoxItem;
        if (selected == null)
            return null;

        string tag = selected.Tag?.ToString() ?? "";
        int width;
        int height;

        if (tag == "custom")
        {
            if (!int.TryParse(CustomWidthBox.Text, out width))
                return null;

            if (!int.TryParse(CustomHeightBox.Text, out height))
                return null;
        }
        else
        {
            string[] pieces = tag.Split('x');
            if (pieces.Length != 2)
                return null;

            if (!int.TryParse(pieces[0], out width))
                return null;

            if (!int.TryParse(pieces[1], out height))
                return null;
        }

        if (width < 640 || height < 480)
            return null;

        return new DisplaySettings
        {
            Width = width,
            Height = height,
            Fullscreen = FullscreenCheck.IsChecked == true,
            ReadOnly = ReadOnlyCheck.IsChecked == true
        };
    }
}
