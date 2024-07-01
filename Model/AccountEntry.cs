namespace DNPL.Model;
public struct AccountEntry {

	public double Before { get; set; }
	public double Amount { get; set; } //Amount
	public double After { get; set; }
	public string Type { get; set; } //BalanceChangeType
	public DateTime Date { get; set; }

}
