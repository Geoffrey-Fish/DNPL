using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using DNPL.Core;
using DNPL.Database;
using DNPL.Model;

namespace DNPL.ViewModel;
public class HistoryViewModel:ObservableObject {

	#region Commands
	public ICommand AccountHistoryCommand { get; set; }
	public ICommand DrinkHistoryCommand { get; set; }
	#endregion

	#region Properties
	private string _Lbl_History { get; set; }
	public string Lbl_History { get { return _Lbl_History; } set { _Lbl_History = value; OnPropertyChanged(nameof(Lbl_History)); } }
	#endregion

	#region Database
	private readonly string accountBalancePath = PathMaker.GetAbsolutePath("AccountBalance.csv");
	private readonly string drinkEntriesPath = PathMaker.GetAbsolutePath("DrinkEntries.csv");
	private DBConnection accountBalanceDB { get; set; }
	public DBConnection AccountBalanceDB { get => accountBalanceDB; set { accountBalanceDB = value; OnPropertyChanged(nameof(AccountBalanceDB)); } }
	private DBConnection drinkEntriesDB { get; set; }
	public DBConnection DrinkEntriesDB { get => drinkEntriesDB; set { drinkEntriesDB = value; OnPropertyChanged(nameof(DrinkEntriesDB)); } }
	private ObservableCollection<AccountEntry> _AccountEntries { get; set; }
	public ObservableCollection<AccountEntry> AccountEntries { get { return _AccountEntries; } set { _AccountEntries = value; OnPropertyChanged(nameof(AccountEntries)); } }
	private ObservableCollection<DrinkEntry> _DrinkEntries { get; set; }
	public ObservableCollection<DrinkEntry> DrinkEntries { get { return _DrinkEntries; } set { _DrinkEntries = value; OnPropertyChanged(nameof(DrinkEntries)); } }
	#endregion

	#region Visibilities
	private Visibility _AccountHistoryVisibility { get; set; }
	public Visibility AccountHistoryVisibility { get { return _AccountHistoryVisibility; } set { _AccountHistoryVisibility = value; OnPropertyChanged(nameof(AccountHistoryVisibility)); } }
	private Visibility _DrinkHistoryVisibility { get; set; }
	public Visibility DrinkHistoryVisibility { get { return _DrinkHistoryVisibility; } set { _DrinkHistoryVisibility = value; OnPropertyChanged(nameof(DrinkHistoryVisibility)); } }
	#endregion

	public HistoryViewModel() {
		//set up the databasecontrols
		AccountBalanceDB = DBConnection.GetInstance(accountBalancePath);
		DrinkEntriesDB = DBConnection.GetInstance(drinkEntriesPath);
		AccountHistoryCommand = new RelayCommand(execute: _ => ExecuteAccountHistoryCommand(), canExecute: _ => true);
		DrinkHistoryCommand = new RelayCommand(execute: _ => ExecuteDrinkHistoryCommand(), canExecute: _ => true);
	}

	private void ExecuteDrinkHistoryCommand() {
		DrinkHistoryVisibility = Visibility.Visible;
		AccountHistoryVisibility = Visibility.Hidden;
		DrinkEntries = DrinkEntriesDB.GetDrinkEntries();
	}
	private void ExecuteAccountHistoryCommand() {
		AccountHistoryVisibility = Visibility.Visible;
		DrinkHistoryVisibility = Visibility.Hidden;
		AccountEntries = AccountBalanceDB.GetAccountEntries();
	}
}