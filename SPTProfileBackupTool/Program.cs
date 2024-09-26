using System.IO.Compression;
using System.Timers;

namespace SPTProfileBackupTool
{
	internal class Program
	{
		private static readonly System.Timers.Timer DailyTimer = new();
		private static readonly System.Timers.Timer HourlyTimer = new();

		private static string runningDir;
		private static string userDir;
		private static string profilesDir;
		private static string hourBackupDir1;
		private static string hourBackupDir2;
		private static string dailyBackupDir;

		static void Main(string[] args)
		{
			Console.Title = "SPTProfileBackupTool";
			CheckAndSetDirectories();

#if DEBUG
			DailyTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
#else
			DailyTimer.Interval = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(55)).Ticks;
#endif
			DailyTimer.Elapsed += DailyTimer_Elapsed;

#if true
			HourlyTimer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
#else
			HourlyTimer.Interval = TimeSpan.FromHours(1).Ticks;
#endif

			HourlyTimer.Elapsed += HourlyTimer_Elapsed;

			DailyTimer.Start();
			HourlyTimer.Start();

			Console.WriteLine("SPTProfileBackupTool started!");
			Console.WriteLine("Backups will be taken every hour and save 2 hours of backups, and then a big backup will be taken every 24 hours!");
			Console.WriteLine("Press ESC to close the application");

			while (!Console.KeyAvailable)
			{
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.Escape:
						{
							Environment.Exit(0);
							break;
						}
					default:
						break;
				}
			}
		}

		private static void CheckAndSetDirectories()
		{
			runningDir = Path.GetDirectoryName(AppContext.BaseDirectory);
			if (!Path.Exists(runningDir + @"\SPT.Server.exe"))
			{
				LogError("Unable to find 'SPT.Server.exe', make sure you extracted the executable to your SPT installation folder!");
				Console.ReadKey();
				Environment.Exit(1);
			}

			userDir = Path.Combine(runningDir + @"\user");
			if (!Path.Exists(userDir))
			{
				LogError("Unable to find the 'user' directory, make sure you extracted the executable to your SPT installation folder!");
				Console.ReadKey();
				Environment.Exit(1);
			}

			profilesDir = Path.Combine(userDir + @"\profiles");
			if (!Path.Exists(profilesDir))
			{
				LogError("Unable to find the 'profiles' directory, make sure you extracted the executable to your SPT installation folder!");
				Console.ReadKey();
				Environment.Exit(1);
			}

			hourBackupDir1 = Path.Combine(userDir + @"\profilesBak");
			hourBackupDir2 = Path.Combine(userDir + @"\profilesbakOld");
			dailyBackupDir = Path.Combine(userDir + @"\dailyBackups");
		}

		private static void HourlyTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!Path.Exists(hourBackupDir1))
			{
				Directory.CreateDirectory(hourBackupDir1);
			}

			if (!Path.Exists(hourBackupDir2))
			{
				Directory.CreateDirectory(hourBackupDir2);
			}

			string[] currentOldBackups = Directory.GetFiles(hourBackupDir2);
			if (currentOldBackups.Length > 0)
			{
				foreach (string oldBackup in currentOldBackups)
				{
					File.Delete(oldBackup);
				}
			}

			string[] currentBackupFiles = Directory.GetFiles(hourBackupDir1);
			if (currentBackupFiles.Length > 0)
			{
				foreach (string backupFile in currentBackupFiles)
				{
					string fileName = Path.GetFileName(backupFile);
					File.Move(backupFile, hourBackupDir2 + $@"\{fileName}");
				}
			}

			int amountOfFiles = Directory.GetFiles(profilesDir).Length;
			Console.WriteLine($"Creating hourly backup of {amountOfFiles} profiles.");
			ZipFile.CreateFromDirectory(profilesDir, hourBackupDir1 + @"\profiles.zip");
		}

		private static void DailyTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!Path.Exists(dailyBackupDir))
			{
				Directory.CreateDirectory(dailyBackupDir);
			}

			int amountOfFiles = Directory.GetFiles(profilesDir).Length;
			string date = DateTime.Now.ToString("yyyy-MM-dd");
			string filename = date + " profiles backup";
			Console.WriteLine($"Creating daily backup of {amountOfFiles} profiles.");
			ZipFile.CreateFromDirectory(profilesDir, dailyBackupDir + $@"\{filename}.zip");
		}

		private static void LogError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
		}
	}
}
