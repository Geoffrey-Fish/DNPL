namespace DNPL.Model;
public struct AccountEntry {

	public decimal Before { get; set; }
	public decimal Amount { get; set; } //Amount
	public decimal After { get; set; }
	public string Type { get; set; } //BalanceChangeType
	public DateTime Date { get; set; }

}
