using System.Collections.ObjectModel;

using DNPL.Core;
using DNPL.Database;
using DNPL.Model;

namespace DNPL.ViewModel;
public class StatisticsViewModel:ObservableObject {


	#region Commands
	#endregion

	#region Database
	private readonly string accountBalancePath = PathMaker.GetAbsolutePath("AccountBalance.csv");
	private DBConnection accountBalanceDB { get; set; }
	public DBConnection AccountBalanceDB { get => accountBalanceDB; set { accountBalanceDB=value; OnPropertyChanged(nameof(AccountBalanceDB)); } }
	private ObservableCollection<AccountEntry> _AccountEntries { get; set; }
	public ObservableCollection<AccountEntry> AccountEntries { get { return _AccountEntries; } set { _AccountEntries=value; OnPropertyChanged(nameof(AccountEntry)); } }
	#endregion

	#region Properties
	private string _TopDrinkText { get; set; }
	public string TopDrinkText { get => _TopDrinkText; set { _TopDrinkText=value; OnPropertyChanged(nameof(TopDrinkText)); } }
	private string _TotalSpentText { get; set; }
	public string TotalSpentText { get => _TotalSpentText; set { _TotalSpentText=value; OnPropertyChanged(nameof(TotalSpentText)); } }
	private string _SpeziText { get; set; }
	public string SpeziText { get => _SpeziText; set { _SpeziText=value; OnPropertyChanged(nameof(SpeziText)); } }
	private string _EisteeText { get; set; }
	public string EisteeText { get => _EisteeText; set { _EisteeText=value; OnPropertyChanged(nameof(EisteeText)); } }
	private string _AlmdudlerText { get; set; }
	public string AlmdudlerText { get => _AlmdudlerText; set { _AlmdudlerText=value; OnPropertyChanged(nameof(AlmdudlerText)); } }
	private string _ColaText { get; set; }
	public string ColaText { get => _ColaText; set { _ColaText=value; OnPropertyChanged(nameof(ColaText)); } }

	#endregion
	public StatisticsViewModel() {
		//set up the databasecontrols
		AccountBalanceDB=DBConnection.GetInstance(accountBalancePath);
		AccountEntries=AccountBalanceDB.GetAccountEntries();
	}


	public void CalculateStatistics(ObservableCollection<AccountEntry> accountEntries) {
		var totalAmountPerDrink = accountEntries
			.GroupBy(de => de.Type.Substring(0,de.Type.IndexOf(" ")))
			.ToDictionary(g => g.Key,g => g.Sum(de => de.Amount));

		var totalAmountOverall = totalAmountPerDrink.Values.Sum();

		var mostUsedDrink = totalAmountPerDrink
			.OrderByDescending(kvp => kvp.Value)
			.FirstOrDefault().Key;

		var leastUsedDrink = totalAmountPerDrink
			.OrderBy(kvp => kvp.Value)
			.FirstOrDefault().Key;

		TopDrinkText=mostUsedDrink;
		TotalSpentText=totalAmountOverall.ToString()+" €";
		SpeziText=totalAmountPerDrink["Spezi"].ToString()+" €";
		EisteeText=totalAmountPerDrink["Eistee"].ToString()+" €";
		AlmdudlerText=totalAmountPerDrink["Almdudler"].ToString()+" €";
		ColaText=totalAmountPerDrink["Cola"].ToString()+" €";

	}
}


