using System.Windows;
using PixelFactory.Common;

namespace PixelFactory.Editor.App;

public partial class AddAssetDialog : Window
{
    public string AssetName => AssetNameBox.Text;
    public AssetType AssetType => (AssetType)AssetTypeBox.SelectedItem!;

    public AddAssetDialog()
    {
        InitializeComponent();
        AssetTypeBox.ItemsSource = Enum.GetValues<AssetType>();
        AssetTypeBox.SelectedIndex = 0;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AssetNameBox.Text))
        {
            MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }
}
