using System.Collections.ObjectModel;
using System.Windows.Input;

using DNPL.Core;
using DNPL.Database;
using DNPL.Model;

namespace DNPL.ViewModel;
public class DrinkButtonsViewModel:ObservableObject {
	public ICommand DrinkButtonPushedCommand { get; set; }

	#region private static members
	private readonly string accountBalancePath = PathMaker.GetAbsolutePath("AccountBalance.csv");
	private readonly string drinkEntriesPath = PathMaker.GetAbsolutePath("DrinkEntries.csv");
	#endregion

	#region Database
	private DBConnection accountBalanceDB { get; set; }
	public DBConnection AccountBalanceDB { get => accountBalanceDB; set { accountBalanceDB = value; OnPropertyChanged(nameof(AccountBalanceDB)); } }
	private DBConnection drinkEntriesDB { get; set; }
	public DBConnection DrinkEntriesDB { get => drinkEntriesDB; set { drinkEntriesDB = value; OnPropertyChanged(nameof(DrinkEntriesDB)); } }
	private ObservableCollection<DrinkEntry> _DrinkEntries { get; set; }
	public ObservableCollection<DrinkEntry> DrinkEntries { get { return _DrinkEntries; } set { _DrinkEntries = value; OnPropertyChanged(nameof(DrinkEntries)); } }
	private AccountEntry _AccountEntry { get; set; }
	public AccountEntry AccountEntry { get { return _AccountEntry; } set { _AccountEntry = value; OnPropertyChanged(nameof(AccountEntry)); } }
	#endregion

	#region Properties
	private string _MatePrice { get; set; }
	public string MatePrice { get { return _MatePrice; } set { _MatePrice = value; OnPropertyChanged(nameof(MatePrice)); } }
	private string _EisteePrice { get; set; }
	public string EisteePrice { get { return _EisteePrice; } set { _EisteePrice = value; OnPropertyChanged(nameof(EisteePrice)); } }
	private string _AlmdudlerPrice { get; set; }
	public string AlmdudlerPrice { get { return _AlmdudlerPrice; } set { _AlmdudlerPrice = value; OnPropertyChanged(nameof(AlmdudlerPrice)); } }
	private string _ColaPrice { get; set; }
	public string ColaPrice { get { return _ColaPrice; } set { _ColaPrice = value; OnPropertyChanged(nameof(ColaPrice)); } }
	public int MateCounter { get; set; }
	public int EisteeCounter { get; set; }
	public int AlmdudlerCounter { get; set; }
	public int ColaCounter { get; set; }
	#endregion




	public DrinkButtonsViewModel() {
		AccountBalanceDB = new(accountBalancePath);
		DrinkEntriesDB = new(drinkEntriesPath);
		AccountEntry = AccountBalanceDB.GetCurrentAccountEntry();
		DrinkEntries = DrinkEntriesDB.GetDrinkEntries();
		DrinkButtonPushedCommand = new RelayCommand(execute: ExecuteDrinkButtonPushedCommand, canExecute: _ => true);
		MatePrice = GetPrice("Mate").ToString() + " €";
		EisteePrice = GetPrice("Eistee").ToString() + " €";
		AlmdudlerPrice = GetPrice("Almdudler").ToString() + " €";
		ColaPrice = GetPrice("Cola").ToString() + " €";
	}
	private void ExecuteDrinkButtonPushedCommand(object drink) {
		if(drink is string) {
			AccountEntry entry = new AccountEntry {
				Before = AccountEntry.After
				, Amount = GetPrice(drink as string)
				, After = AccountEntry.After - AccountEntry.Amount
				, Type = drink as string + " purchased"
				, Date = DateTime.Now
			};
			AccountBalanceDB.SetAccountEntry(AccountEntry);
		}
	}
	//Little getaround to fetch the price of the drink
	private double GetPrice(string drink) {
		return DrinkEntries.FirstOrDefault(x => x.Name == drink).Price;
	}
}
