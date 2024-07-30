using CsvHelper.Configuration;

namespace DNPL.Model;
public struct AccountEntry {

	public decimal Before { get; set; }
	public decimal Amount { get; set; } //Amount
	public decimal After { get; set; }
	public string Type { get; set; } //BalanceChangeType
	public DateTime Date { get; set; }

}

public sealed class AccountEntryMap:ClassMap<AccountEntry> {
	public AccountEntryMap() {
		Map(m => m.Before);
		Map(m => m.Amount);
		Map(m => m.After);
		Map(m => m.Type);
		Map(m => m.Date).TypeConverterOption.Format("yyyy-MM-dd");
	}
}
