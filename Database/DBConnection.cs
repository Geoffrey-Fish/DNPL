using System.Collections.ObjectModel;
using System.IO;

using CsvHelper;

using DNPL.Model;

namespace DNPL.Database;

public class DBConnection {

	private string filePath;

	public DBConnection(string filePath) {
		this.filePath = filePath;
	}
	//For the Graph in Observable style
	#region Getters
	public ObservableCollection<DrinkEntry> GetDrinkEntries() {
		if(!File.Exists(filePath)) return new ObservableCollection<DrinkEntry>();
		using var reader = new StreamReader(filePath);
		using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture);
		var records = csv.GetRecords<DrinkEntry>();
		return new ObservableCollection<DrinkEntry>(records);
	}

	public ObservableCollection<AccountEntry> GetAccountEntries() {
		if(!File.Exists(filePath)) return new ObservableCollection<AccountEntry>();

		using var reader = new StreamReader(filePath);
		using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture);
		var records = csv.GetRecords<AccountEntry>();
		return new ObservableCollection<AccountEntry>(records);
	}
	public AccountEntry GetCurrentAccountEntry() {
		var entries = GetAccountEntries();
		return entries.LastOrDefault();
	}
	#endregion

	#region setters
	public void SetDrinkEntry(DrinkEntry entry) {
		using var reader = new StreamReader(filePath);
		using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture);
		List<DrinkEntry> records = csv.GetRecords<DrinkEntry>().ToList();
		var existingEntry = records.FirstOrDefault(x => x.Name == entry.Name);
		if(existingEntry.Name == entry.Name) {
			existingEntry.Price = entry.Price;
			existingEntry.Date = entry.Date;
		} else {
			records.Add(new DrinkEntry { Name = entry.Name, Price = entry.Price, Date = entry.Date });
		}
		SaveDrinkEntries(records);
	}

	public void SetAccountEntry(AccountEntry entry) {
		using var reader = new StreamReader(filePath);
		using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture);
		List<AccountEntry> records = csv.GetRecords<AccountEntry>().ToList();
		records.Add(new AccountEntry { Before = entry.Before, Amount = entry.Amount, After = entry.After, Type = entry.Type, Date = entry.Date });
		SaveAccountEntries(records);
	}

	#endregion
	#region Data Save
	public void SaveDrinkEntries(List<DrinkEntry> entries) {
		using var writer = new StreamWriter(filePath);
		using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture);
		csv.WriteRecords(entries);
	}
	public void SaveAccountEntries(List<AccountEntry> entries) {
		using var writer = new StreamWriter(filePath);
		using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture);
		csv.WriteRecords(entries);
	}
	#endregion

}