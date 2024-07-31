using System.Collections.ObjectModel;
using System.Windows.Input;

using DNPL.Core;
using DNPL.Database;
using DNPL.Model;

namespace DNPL.ViewModel;
public class DrinkButtonsViewModel:ObservableObject {

	#region Commands
	public ICommand DrinkButtonPushedCommand { get; set; }
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

	#region Properties PriceTags
	private string _SpeziPrice { get; set; }
	public string SpeziPrice { get { return _SpeziPrice; } set { _SpeziPrice=value; OnPropertyChanged(nameof(SpeziPrice)); } }
	private string _EisteePrice { get; set; }
	public string EisteePrice { get { return _EisteePrice; } set { _EisteePrice=value; OnPropertyChanged(nameof(EisteePrice)); } }
	private string _AlmdudlerPrice { get; set; }
	public string AlmdudlerPrice { get { return _AlmdudlerPrice; } set { _AlmdudlerPrice=value; OnPropertyChanged(nameof(AlmdudlerPrice)); } }
	private string _ColaPrice { get; set; }
	public string ColaPrice { get { return _ColaPrice; } set { _ColaPrice=value; OnPropertyChanged(nameof(ColaPrice)); } }
	#endregion
	#region Propertys Counters
	//Counters for Visibility enhancement
	private int _SpeziCounter { get; set; }
	public int SpeziCounter { get { return _SpeziCounter; } set { _SpeziCounter=value; OnPropertyChanged(nameof(SpeziCounter)); } }
	private int _EisteeCounter { get; set; }
	public int EisteeCounter { get { return _EisteeCounter; } set { _EisteeCounter=value; OnPropertyChanged(nameof(EisteeCounter)); } }
	private int _AlmdudlerCounter { get; set; }
	public int AlmdudlerCounter { get { return _AlmdudlerCounter; } set { _AlmdudlerCounter=value; OnPropertyChanged(nameof(AlmdudlerCounter)); } }
	private int _ColaCounter { get; set; }
	public int ColaCounter { get { return _ColaCounter; } set { _ColaCounter=value; OnPropertyChanged(nameof(ColaCounter)); } }
	#endregion

	public DrinkButtonsViewModel() {
		//set up the databasecontrols
		AccountBalanceDB=DBConnection.GetInstance(accountBalancePath);
		DrinkEntriesDB=DBConnection.GetInstance(drinkEntriesPath);
		AccountEntry=AccountBalanceDB.GetCurrentAccountEntry();
		DrinkEntries=DrinkEntriesDB.GetDrinkEntries();
		//set up the commands
		DrinkButtonPushedCommand=new RelayCommand(execute: ExecuteDrinkButtonPushedCommand,canExecute: _ => true);
		//set up the price info
		SpeziPrice=GetPrice("Spezi").ToString()+" €";
		EisteePrice=GetPrice("Eistee").ToString()+" €";
		AlmdudlerPrice=GetPrice("Almdudler").ToString()+" €";
		ColaPrice=GetPrice("Cola").ToString()+" €";
		EventAggregator.Instance.Subscribe("DrinkPriceChanged",UpdatePriceInfo);
	}

	private void UpdatePriceInfo(object payload) {
		Dictionary<string,decimal> price = (Dictionary<string,decimal>)payload;
		switch(price.Keys.First()) {
			case "Spezi":
				SpeziPrice = price.Values.First().ToString()+" €";
				break;
			case "Eistee":
				EisteePrice = price.Values.First().ToString()+" €";
				break;
			case "Almdudler":
				AlmdudlerPrice = price.Values.First().ToString()+" €";
				break;
			case "Cola":
				ColaPrice = price.Values.First().ToString()+" €";
				break;
		}

	}
		
	private void ExecuteDrinkButtonPushedCommand(object drink) {
		if(drink is string) {
			AccountEntry=AccountBalanceDB.GetCurrentAccountEntry(); //get updated AccountEntry,else you get wrong values
			decimal amount = GetPrice(drink as string);
			decimal before = AccountEntry.After;
			AccountEntry entry = new AccountEntry {
				Before=before
				,
				Amount=-amount
				,
				After=before-amount
				,
				Type=(drink as string)+" purchased"
				,
				Date=DateTime.Now
			};
			AccountBalanceDB.SetAccountEntry(entry);
			switch(drink.ToString()) {
				case "Spezi":
					SpeziCounter++;
					break;
				case "Eistee":
					EisteeCounter++;
					break;
				case "Almdudler":
					AlmdudlerCounter++;
					break;
				case "Cola":
					ColaCounter++;
					break;
			}
		}
	}
	//Little getaround to fetch the price of the drink
	private decimal GetPrice(string drink) {
		return DrinkEntries.FirstOrDefault(x => x.Name==drink).Price;
	}
}
