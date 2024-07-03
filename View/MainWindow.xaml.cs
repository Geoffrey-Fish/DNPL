using System.Windows;

using DNPL.ViewModel;
namespace DNPL.View;
/// <summary>
public partial class MainWindow:Window {
	private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();
	public MainWindow() {
		InitializeComponent();
	}
}