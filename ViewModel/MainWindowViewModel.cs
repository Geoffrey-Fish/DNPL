using System.Windows;
using System.Windows.Input;

using DNPL.Core;
using DNPL.Database;
using DNPL.Model;

namespace DNPL.ViewModel;
public class MainWindowViewModel:ObservableObject {
	#region private static members
	private readonly string accountBalancePath = PathMaker.GetAbsolutePath("AccountBalance.csv");
	#endregion

	#region Database
	private DBConnection accountBalanceDB { get; set; }
	public DBConnection AccountBalanceDB { get => accountBalanceDB; set { accountBalanceDB = value; OnPropertyChanged(nameof(AccountBalanceDB)); } }
	private DBConnection drinkEntriesDB { get; set; }
	private AccountEntry accountEntry { get; set; }
	public AccountEntry AccountEntry { get => accountEntry; set { accountEntry = value; OnPropertyChanged(nameof(AccountEntry)); } }
	private List<DrinkEntry> drinkEntries { get; set; }
	#endregion

	#region Commands
	public ICommand DrinkButtonsViewCommand { get; set; }
	public ICommand SettingsViewCommand { get; set; }
	public ICommand HistoryViewCommand { get; set; }
	public ICommand ExitCommand { get; set; }
	#endregion

	#region Properties
	private string _Tbl_CurrentBalanceInfo { get; set; }
	public string Tbl_CurrentBalanceInfo { get => _Tbl_CurrentBalanceInfo; set { _Tbl_CurrentBalanceInfo = value; OnPropertyChanged(nameof(Tbl_CurrentBalanceInfo)); } }

	#endregion

	#region Viewmodels
	//represents the current view
	private object _currentView { get; set; }
	public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); } }
	//Here are the viewmodels saved for the session
	private readonly DrinkButtonsViewModel _drinkButtonsViewModel;
	private readonly SettingsViewModel _settingsViewModel;
	private readonly HistoryViewModel _historyViewModel;
	#endregion


	//Main Initializer
	public MainWindowViewModel() {
		//set up the databasecontrols
		AccountBalanceDB = new(accountBalancePath);
		AccountEntry = AccountBalanceDB.GetCurrentAccountEntry();

		//Set info about current balance
		Tbl_CurrentBalanceInfo = AccountEntry.Before.ToString();
		//set up the viewmodels
		_drinkButtonsViewModel = new();
		_settingsViewModel = new();
		_historyViewModel = new();
		//set up the commands
		DrinkButtonsViewCommand = new RelayCommand(_ => CurrentView = _drinkButtonsViewModel);
		SettingsViewCommand = new RelayCommand(_ => CurrentView = _settingsViewModel);
		HistoryViewCommand = new RelayCommand(_ => CurrentView = _historyViewModel);
		//set up the view
		CurrentView = _drinkButtonsViewModel;
		//set up Exit Button
		ExitCommand = new RelayCommand(execute: _ => Application.Current.Shutdown(), canExecute: _ => true);
	}

}
