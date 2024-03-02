using System.Windows;
using System.Windows.Controls;

namespace Pixl;

public partial class FilterNameDialog : Window
{
    public string Name { get; private set; }
    public FilterNameDialog()
    {
        InitializeComponent();
    }
    private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Name = NameField.Text; 
        Close();
    }
    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}