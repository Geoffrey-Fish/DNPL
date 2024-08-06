using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

using CsvHelper;
using CsvHelper.Configuration;

using DNPL.Model;

namespace DNPL.Database;
public class DBConnection:IDisposable { 
	private static readonly Dictionary<string,DBConnection> _instances = new Dictionary<string,DBConnection>();
	private static readonly object _lock = new object();
	private readonly string _filePath;
	private bool _disposed;
	#region Create new Databases
	private List<AccountEntry> _defaultAccountEntries = new List<AccountEntry>() {
			new AccountEntry() {
			Before=0
			,
			Amount=0
			,
			After=0
			,
			Type="Balance load"
			,
			Date=DateTime.Now
			}
		};
	private List<DrinkEntry> _defaultDrinkEntries = new List<DrinkEntry>() {
		new DrinkEntry() {
		Name = "Spezi",
		Price = 0.5m,
		Date = DateTime.Now
		},
		new DrinkEntry() {
		Name = "Eistee",
		Price = 0.7m,
		Date = DateTime.Now
		},
		new DrinkEntry() {
		Name = "Almdudler",
		Price = 0.9m,
		Date = DateTime.Now
		},
		new DrinkEntry() {
		Name = "Cola",
		Price = 1.0m,
		Date = DateTime.Now
		}
		};



	#endregion
	private DBConnection(string filePath) {
		_filePath=filePath;
	}

	public static DBConnection GetInstance(string filePath) {
		if(!_instances.TryGetValue(filePath,out var instance)) {
			lock(_lock) {
				if(!_instances.TryGetValue(filePath,out instance)) {
					instance=new DBConnection(filePath);
					_instances[filePath]=instance;
				}
			}
		}
		return instance;
	}

	private CsvConfiguration GetCsvConfiguration() {
		return new CsvConfiguration(CultureInfo.CurrentCulture) {
			HeaderValidated=null,
			MissingFieldFound=null
		};
	}

	private T ReadCsvFile<T>(Func<CsvReader,T> readFunc) {
		if(!File.Exists(_filePath)) {
			if(_filePath.Contains("AccountBalance.csv")) {
				using var stream = new FileStream(_filePath,FileMode.Create,FileAccess.Write,FileShare.ReadWrite);
				using var writer = new StreamWriter(stream);
				using var csv = new CsvWriter(writer,GetCsvConfiguration());
				csv.WriteRecords(_defaultAccountEntries);
			} else if(_filePath.Contains("DrinkEntries.csv")) {
				using var stream = new FileStream(_filePath,FileMode.Create,FileAccess.Write,FileShare.ReadWrite);
				using var writer = new StreamWriter(stream);
				using var csv = new CsvWriter(writer,GetCsvConfiguration());
				csv.WriteRecords(_defaultDrinkEntries);
			}
			return ReadCsvFile(readFunc);
		}

		for(int i = 0;i<3;i++) {
			try {
				using var stream = new FileStream(_filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
				using var reader = new StreamReader(stream);
				using var csv = new CsvReader(reader,GetCsvConfiguration());
				return readFunc(csv);
			} catch(IOException) {
				if(i==2) throw;
				Thread.Sleep(200);
			}
		}

		return default;
	}

	public ObservableCollection<DrinkEntry> GetDrinkEntries() {
		return ReadCsvFile(csv => new ObservableCollection<DrinkEntry>(csv.GetRecords<DrinkEntry>()));
	}

	public ObservableCollection<AccountEntry> GetAccountEntries() {
		return ReadCsvFile(csv => new ObservableCollection<AccountEntry>(csv.GetRecords<AccountEntry>()));
	}
	public AccountEntry GetCurrentAccountEntry() {
		for(int i = 0;i<3;i++) {
			try {
				var entries = GetAccountEntries();
				if(entries.Count>0) {
					return entries[entries.Count-1];
				}
				return new AccountEntry();
			} catch(IOException) {
				if(i==2) throw;
				Thread.Sleep(1000);
			}
		}
		return new AccountEntry();
	}

	public void SetDrinkEntry(DrinkEntry entry) {
		List<DrinkEntry> records = new List<DrinkEntry>();
		lock(_lock) {
			records=ReadCsvFile(csv => csv.GetRecords<DrinkEntry>().ToList());
			int index = records.FindIndex(x => x.Name==entry.Name);
			if(index!=-1) {
				// Update existing entry
				records[index]=entry;
			} else {
				// Add new entry
				records.Add(entry);
			}
		}
		SaveEntries(records);
	}

	public void SetAccountEntry(AccountEntry entry) {
		List<AccountEntry> records;
		lock(_lock) {
			records=ReadCsvFile(csv => csv.GetRecords<AccountEntry>().ToList());
			records.Add(entry);
		}
		SaveEntries(records);
	}

	private void SaveEntries<T>(List<T> entries) {
		lock(_lock) {
			for(int i = 0;i<3;i++) {
				try {
					using var stream = new FileStream(_filePath,FileMode.Create,FileAccess.Write,FileShare.Read);
					using var writer = new StreamWriter(stream);
					using var csv = new CsvWriter(writer,CultureInfo.CurrentCulture);
					csv.WriteRecords(entries);
					return;
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
		}
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if(!_disposed) {
			if(disposing) {
				// Dispose managed resources here
			}

			// Dispose unmanaged resources here, if any

			_disposed=true;
		}
	}
}
