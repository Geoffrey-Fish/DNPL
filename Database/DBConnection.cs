using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

using CsvHelper;
using CsvHelper.Configuration;

using DNPL.Model;

namespace DNPL.Database {
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
		Name = "Mate",
		Price = 0.8m,
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
				var existingEntry = records.FirstOrDefault(x => x.Name==entry.Name);

				existingEntry.Price=entry.Price;
				existingEntry.Date=entry.Date;
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
}





/*

public class DBConnection:IDisposable {

	private static readonly Dictionary<string,DBConnection> _instances = new Dictionary<string,DBConnection>();
	private static readonly object _lock = new object();
	private string filePath;

	private DBConnection(string filePath) {
		this.filePath=filePath;
	}

	/// <summary>
	/// Singleton Pattern,Checks the Dictionary if there is an entry for one of the two databases.
	/// </summary>
	public static DBConnection GetInstance(string filePath) {
		if(!_instances.ContainsKey(filePath)) {
			lock(_lock) {
				if(!_instances.ContainsKey(filePath)) {
					_instances[filePath]=new DBConnection(filePath);
				}
			}
		}
		return _instances[filePath];
	}

	//For the Graph in Observable style
	#region Getters
	/// <summary>
	/// The whole trick is like this: lock the files you are about to open. read them, let others read and write on them separatly.
	/// If you can't open the file, wait 200ms and try again.
	/// </summary>
	/// <returns>ObservableCollection Drinkentries</returns>
	public ObservableCollection<DrinkEntry> GetDrinkEntries() {
		lock(_lock) {
			if(!File.Exists(filePath)) return new ObservableCollection<DrinkEntry>();
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
					using(var reader = new StreamReader(stream))
					using(var csv = new CsvReader(reader,System.Globalization.CultureInfo.CurrentCulture)) {
						var records = csv.GetRecords<DrinkEntry>();
						return new ObservableCollection<DrinkEntry>(records);
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
			return new ObservableCollection<DrinkEntry>();
		}
	}

	/// <summary>
	/// The whole trick is like this: lock the files you are about to open. read them, let others read and write on them separatly.
	/// If you can't open the file, wait 200ms and try again.
	/// </summary>
	/// <returns>ObservableCollection AccountEntries</returns>
	public ObservableCollection<AccountEntry> GetAccountEntries() {
		lock(_lock) {
			if(!File.Exists(filePath)) return new ObservableCollection<AccountEntry>();
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
					using(var reader = new StreamReader(stream))
					using(var csv = new CsvReader(reader,new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) {
						HeaderValidated=null,
						MissingFieldFound=null
					})) {
						var records = csv.GetRecords<AccountEntry>();
						return new ObservableCollection<AccountEntry>(records);
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
			return new ObservableCollection<AccountEntry>();
		}
	}

	/// <summary>
	/// This one only wants the latest entry for showing in the main window and elsewhere
	/// </summary>
	/// <returns>The latest Accountentry</returns>
	public AccountEntry GetCurrentAccountEntry() {
		for(int i = 0;i<3;i++) {
			try {
				var entries = GetAccountEntries();
				return entries.LastOrDefault();
			} catch(IOException) {
				if(i==2) throw;
				Thread.Sleep(1000);
			}
		}
		return new AccountEntry();
	}
	#endregion

	#region setters
	/// <summary>
	/// Like the others, it is save to work on a file.
	/// read the drinks, search the given, update the data, if not, add new entry to list.
	/// </summary>
	/// <param name="entry">Drink to change</param>
	public void SetDrinkEntry(DrinkEntry entry) {
		List<DrinkEntry> records = new List<DrinkEntry>();
		lock(_lock) {
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
					using(var reader = new StreamReader(stream))
					using(var csv = new CsvReader(reader,new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) {
						HeaderValidated=null,
						MissingFieldFound=null
					})) {
						records=csv.GetRecords<DrinkEntry>().ToList();
						var existingEntry = records.FirstOrDefault(x => x.Name==entry.Name);
						if(existingEntry.Name==entry.Name) {
							existingEntry.Price=entry.Price;
							existingEntry.Date=entry.Date;
						} else {
							records.Add(new DrinkEntry { Name=entry.Name,Price=entry.Price,Date=entry.Date });
						}
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
		}
		SaveDrinkEntries(records);
	}

	/// <summary>
	/// Like the others, it is save to work on a file.
	/// Add a new entry to the account list for drinks or updates on the balance
	/// </summary>
	/// <param name="entry">Accountentry</param>
	public void SetAccountEntry(AccountEntry entry) {
		List<AccountEntry> records = new List<AccountEntry>();
		lock(_lock) {
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
					using(var reader = new StreamReader(stream))
					using(var csv = new CsvReader(reader,new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) {
						HeaderValidated=null,
						MissingFieldFound=null
					})) {
						records=csv.GetRecords<AccountEntry>().ToList();
						records.Add(entry);
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Create,FileAccess.Write,FileShare.Read))
					using(var writer = new StreamWriter(stream))
					using(var csv = new CsvWriter(writer,System.Globalization.CultureInfo.CurrentCulture)) {
						csv.WriteRecords(records);
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
		}
	}

	#endregion

	#region Data Save
	/// <summary>
	/// Way more interesting now. Take the file, write on it, others may only read on it.
	/// </summary>
	/// <param name="entries">Drinkentries</param>
	public void SaveDrinkEntries(List<DrinkEntry> entries) {
		lock(_lock) {
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Create,FileAccess.Write,FileShare.Read))
					using(var writer = new StreamWriter(stream))
					using(var csv = new CsvWriter(writer,System.Globalization.CultureInfo.CurrentCulture)) {
						csv.WriteRecords(entries);
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
		}
	}
	/// <summary>
	/// Way more interesting now. Take the file, write on it, others may only read on it.
	/// </summary>
	/// <param name="entries">Accountentries</param>
	public void SaveAccountEntries(List<AccountEntry> entries) {
		lock(_lock) {
			for(int i = 0;i<3;i++) {
				try {
					using(var stream = new FileStream(filePath,FileMode.Create,FileAccess.Write,FileShare.Read))
					using(var writer = new StreamWriter(stream))
					using(var csv = new CsvWriter(writer,System.Globalization.CultureInfo.CurrentCulture)) {
						csv.WriteRecords(entries);
					}
				} catch(IOException) {
					if(i==2) throw;
					Thread.Sleep(200);
				}
			}
		}
	}
	#endregion


	#region Disposing
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	private bool disposed = false;
	protected virtual void Dispose(bool disposing) {
		if(!disposed) {
			if(disposing) {
				// Dispose managed resources here
				// For example, close any open file streams or connections
			}

			// Dispose unmanaged resources here, if any

			disposed=true;
		}
	}
	#endregion
}*/