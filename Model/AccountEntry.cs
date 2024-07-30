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
		Map(m => m.Before).Name("Before");
		Map(m => m.Amount).Name("Amount");
		Map(m => m.After).Name("After");
		Map(m => m.Type).Name("Type");
		Map(m => m.Date).Name("Date");
	}
}
