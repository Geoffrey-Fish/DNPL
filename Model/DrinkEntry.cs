using CsvHelper.Configuration;

namespace DNPL.Model;
public struct DrinkEntry {
	public string Name { get; set; }
	public decimal Price { get; set; }
	public DateTime Date { get; set; }
}

public sealed class DrinkEntryMap:ClassMap<DrinkEntry> {
	public DrinkEntryMap() {
		Map(m => m.Name);
		Map(m => m.Price);
		Map(m => m.Date).TypeConverterOption.Format("yyyy-MM-dd");
	}
}
