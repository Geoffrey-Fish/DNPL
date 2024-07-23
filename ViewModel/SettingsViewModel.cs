using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

using DNPL.Core;
using DNPL.Database;
using DNPL.Model;

namespace DNPL.ViewModel;
public class SettingsViewModel:ObservableObject {

	#region Commands
	public ICommand LoadBalanceCommand { get; set; }
	public ICommand SaveLoadBalanceCommand { get; set; }
	public ICommand ChangePriceCommand { get; set; }
	public ICommand SaveChangePriceCommand { get; set; }
	#endregion

	#region Database
	private readonly string accountBalancePath = PathMaker.GetAbsolutePath("AccountBalance.csv");
	private readonly string drinkEntriesPath = PathMaker.GetAbsolutePath("DrinkEntries.csv");
	private DBConnection accountBalanceDB { get; set; }
	public DBConnection AccountBalanceDB { get => accountBalanceDB; set { accountBalanceDB=value; OnPropertyChanged(nameof(AccountBalanceDB)); } }
	private DBConnection drinkEntriesDB { get; set; }
	public DBConnection DrinkEntriesDB { get => drinkEntriesDB; set { drinkEntriesDB=value; OnPropertyChanged(nameof(DrinkEntriesDB)); } }
	private ObservableCollection<DrinkEntry> _DrinkEntries { get; set; }
	public ObservableCollection<DrinkEntry> DrinkEntries { get { return _DrinkEntries; } set { _DrinkEntries=value; OnPropertyChanged(nameof(DrinkEntries)); } }
	private AccountEntry _AccountEntry { get; set; }
	public AccountEntry AccountEntry { get { return _AccountEntry; } set { _AccountEntry=value; OnPropertyChanged(nameof(AccountEntry)); } }
	#endregion

	#region Properties
	private string _Tbx_LoadBalance { get; set; }
	public string Tbx_LoadBalance { get { return _Tbx_LoadBalance; } set { _Tbx_LoadBalance=value; OnPropertyChanged(nameof(Tbx_LoadBalance)); } }
	private string _Tbx_MatePrice { get; set; }
	public string Tbx_MatePrice { get { return _Tbx_MatePrice; } set { _Tbx_MatePrice=value; OnPropertyChanged(nameof(Tbx_MatePrice)); } }
	private string _Tbx_EisteePrice { get; set; }
	public string Tbx_EisteePrice { get { return _Tbx_EisteePrice; } set { _Tbx_EisteePrice=value; OnPropertyChanged(nameof(Tbx_EisteePrice)); } }
	private string _Tbx_AlmdudlerPrice { get; set; }
	public string Tbx_AlmdudlerPrice { get { return _Tbx_AlmdudlerPrice; } set { _Tbx_AlmdudlerPrice=value; OnPropertyChanged(nameof(Tbx_AlmdudlerPrice)); } }
	private string _Tbx_ColaPrice { get; set; }
	public string Tbx_ColaPrice { get { return _Tbx_ColaPrice; } set { _Tbx_ColaPrice=value; OnPropertyChanged(nameof(Tbx_ColaPrice)); } }

	private bool _PriceChangeInProgress = false;
	public bool PriceChangeInProgress { get { return _PriceChangeInProgress; } set { _PriceChangeInProgress=value; OnPropertyChanged(nameof(PriceChangeInProgress)); } }
	#endregion

	#region Visibility
	private Visibility _LoadBalanceVisibility { get; set; }
	public Visibility LoadBalanceVisibility { get { return _LoadBalanceVisibility; } set { _LoadBalanceVisibility=value; OnPropertyChanged(nameof(LoadBalanceVisibility)); } }
	private Visibility _MatePriceVisibility { get; set; }
	public Visibility MatePriceVisibility { get { return _MatePriceVisibility; } set { _MatePriceVisibility=value; OnPropertyChanged(nameof(MatePriceVisibility)); } }
	private Visibility _EisteePriceVisibility { get; set; }
	public Visibility EisteePriceVisibility { get { return _EisteePriceVisibility; } set { _EisteePriceVisibility=value; OnPropertyChanged(nameof(EisteePriceVisibility)); } }
	private Visibility _AlmdudlerPriceVisibility { get; set; }
	public Visibility AlmdudlerPriceVisibility { get { return _AlmdudlerPriceVisibility; } set { _AlmdudlerPriceVisibility=value; OnPropertyChanged(nameof(AlmdudlerPriceVisibility)); } }
	private Visibility _ColaPriceVisibility { get; set; }
	public Visibility ColaPriceVisibility { get { return _ColaPriceVisibility; } set { _ColaPriceVisibility=value; OnPropertyChanged(nameof(ColaPriceVisibility)); } }
	#endregion

	public SettingsViewModel() {

		LoadBalanceVisibility=Visibility.Hidden;
		MatePriceVisibility=Visibility.Hidden;
		EisteePriceVisibility=Visibility.Hidden;
		AlmdudlerPriceVisibility=Visibility.Hidden;
		ColaPriceVisibility=Visibility.Hidden;
		AccountBalanceDB=DBConnection.GetInstance(accountBalancePath);
		AccountEntry=AccountBalanceDB.GetCurrentAccountEntry();
		DrinkEntriesDB=DBConnection.GetInstance(drinkEntriesPath);
		DrinkEntries=DrinkEntriesDB.GetDrinkEntries();
		LoadBalanceCommand=new RelayCommand(execute: _ => ExecuteLoadBalanceCommand(),canExecute: _ => true);
		SaveLoadBalanceCommand=new RelayCommand(execute: _ => ExecuteSaveLoadBalanceCommand(),canExecute: _ => true);
		ChangePriceCommand=new RelayCommand(execute: ExecuteChangePriceCommand,canExecute: _ => true);
		SaveChangePriceCommand=new RelayCommand(execute: _ => ExecuteSaveChangePriceCommand(),canExecute: _ => true);
	}


	#region BalanceCommands
	/// <summary>
	/// Make Window visible for Load Balance
	/// </summary>
	private void ExecuteLoadBalanceCommand() {
		LoadBalanceVisibility=Visibility.Visible;
	}

	/// <summary>
	/// save new Balance, hide window and reload Balance for Mainview
	/// </summary>
	private void ExecuteSaveLoadBalanceCommand() {
		AccountEntry=AccountBalanceDB.GetCurrentAccountEntry();
		decimal before = AccountEntry.After;
		decimal amount = decimal.TryParse(Tbx_LoadBalance,NumberStyles.Float,CultureInfo.CurrentCulture,out amount) ? amount : 0;
		AccountEntry entry = new AccountEntry {
			Before=before
			,
			Amount=amount
			,
			After=before+amount
			,
			Type="Balance load"
			,
			Date=DateTime.Now
		};
		AccountBalanceDB.SetAccountEntry(entry);
		AccountEntry=AccountBalanceDB.GetCurrentAccountEntry();
		LoadBalanceVisibility=Visibility.Hidden;
	}

	#endregion

	#region PriceChangeCommands
	/// <summary>
	/// Change price of drink and make Textbox visible,but only if no other windows are currently open
	/// </summary>
	/// <param name="drink">The Drink to parse from the Command</param>
	private void ExecuteChangePriceCommand(object drink) {
		if(!PriceChangeInProgress) {
			PriceChangeInProgress=true;
			if(drink is string) {
				switch(drink) {
					case "Mate":
						MatePriceVisibility=Visibility.Visible;
						break;
					case "Eistee":
						EisteePriceVisibility=Visibility.Visible;
						break;
					case "Almdudler":
						AlmdudlerPriceVisibility=Visibility.Visible;
						break;
					case "Cola":
						ColaPriceVisibility=Visibility.Visible;
						break;
				}
			}
		} else {
			MessageBox.Show("Price change in progress!");
			return;
		}
	}

	/// <summary>
	/// After one price is altered, get it and save it and close all textboxes again and set the progressflag back to false
	/// </summary>
	private void ExecuteSaveChangePriceCommand() {
		Dictionary<string,decimal> input = GetCurrentPriceChange();
		if(!input.ContainsKey("ERROR")) {
			DrinkEntry newPrice = new DrinkEntry() {
				Name=input.Keys.First()
				,
				Price=input.Values.First()
				,
				Date=DateTime.Now
			};
			DrinkEntriesDB.SetDrinkEntry(newPrice);
		} else {
			MessageBox.Show("Wrong Input from executechangepricecommand)");
		}
		MatePriceVisibility=Visibility.Hidden;
		EisteePriceVisibility=Visibility.Hidden;
		AlmdudlerPriceVisibility=Visibility.Hidden;
		ColaPriceVisibility=Visibility.Hidden;
		PriceChangeInProgress=false;
	}
	#endregion

	#region Helpermethods
	/// <summary>
	/// Check which Textbox is visible and act accordingly
	/// </summary>
	/// <returns>Dictionary of Name and Price</returns>
	private Dictionary<string,decimal> GetCurrentPriceChange() {
		if(MatePriceVisibility==Visibility.Visible) {
			return new Dictionary<string,decimal> {
				{ "Mate",decimal.TryParse(Tbx_MatePrice, NumberStyles.Float, CultureInfo.CurrentCulture, out decimal matePrice) ? matePrice : 0 }};
		} else if(EisteePriceVisibility==Visibility.Visible) {
			return new Dictionary<string,decimal> { { "Eistee",decimal.TryParse(Tbx_EisteePrice,NumberStyles.Float,CultureInfo.CurrentCulture,out decimal eisteePrice) ? eisteePrice : 0 } };
		} else if(AlmdudlerPriceVisibility==Visibility.Visible) {
			return new Dictionary<string,decimal> { { "Almdudler",decimal.TryParse(Tbx_AlmdudlerPrice,NumberStyles.Float,CultureInfo.CurrentCulture,out decimal almdudlerPrice) ? almdudlerPrice : 0 } };
		} else if(ColaPriceVisibility==Visibility.Visible) {
			return new Dictionary<string,decimal> { { "Cola",decimal.TryParse(Tbx_ColaPrice,NumberStyles.Float,CultureInfo.CurrentCulture,out decimal colaPrice) ? colaPrice : 0 } };
		} else {
			return new Dictionary<string,decimal> { { "ERROR",0 } };
		}
	}
	#endregion

}
