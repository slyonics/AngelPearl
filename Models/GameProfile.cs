using AngelPearl.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngelPearl.Models
{
    public static partial class GameProfile
    {
		public static readonly string SETTINGS_DIRECTORY = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "AppData\\Local") + "\\" + CrossPlatformGame.GAME_NAME;
		public const string SAVE_FOLDER = "\\Save";

		public static int SaveSlot { get; set; }

        public static SaveProfile CurrentSave;

        private static ManualResetEvent autosaveEvent = new ManualResetEvent(true);

        public static void NewState()
        {
            //SaveSlot = -1;
            CurrentSave = new SaveProfile();

            CurrentSave.Money.Value = 800;

			HeroModel heroModel = new HeroModel(MuseRecord.PILOTS.First(x => x.Name == "Proxy"));
			CurrentSave.Party.Add(heroModel);

			heroModel = new HeroModel(MuseRecord.PILOTS.First(x => x.Name == "Aika"));
			CurrentSave.Party.Add(heroModel);

			heroModel = new HeroModel(MuseRecord.PILOTS.First(x => x.Name == "Rem"));
			CurrentSave.Party.Add(heroModel);

			heroModel = new HeroModel(MuseRecord.PILOTS.First(x => x.Name == "Becca"));
			CurrentSave.Party.Add(heroModel);
		}

		public static void LoadState(int slot)
		{
			SaveSlot = slot;

			FileInfo fileInfo = new FileInfo($"{SETTINGS_DIRECTORY}{SAVE_FOLDER}/Save{SaveSlot}.sav");
			using (FileStream fileStream = fileInfo.OpenRead())
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					CurrentSave = new SaveProfile(reader);
				}
			}
		}

		public static void SaveState()
		{
			string directory = SETTINGS_DIRECTORY + SAVE_FOLDER;
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using (FileStream fileStream = File.Open($"{directory}\\Save{SaveSlot}.sav", FileMode.OpenOrCreate))
			{
				using (BinaryWriter writer = new BinaryWriter(fileStream))
				{
					CurrentSave.WriteToFile(writer);
					fileStream.Flush();
				}
			}
		}

		public static bool SaveAvailable()
		{
			string directory = SETTINGS_DIRECTORY + SAVE_FOLDER;
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
				return false;
			}

			var saveFiles = Directory.EnumerateFiles(directory);
			return saveFiles.Any();
		}

		public static void SetSaveData<T>(string name, T value)
        {
            if (CurrentSave.SaveData.ContainsKey(name)) CurrentSave.SaveData[name] = value;
            else CurrentSave.SaveData.Add(name, value);
        }

        public static T GetSaveData<T>(string name)
        {
            if (CurrentSave.SaveData.ContainsKey(name)) return (T)CurrentSave.SaveData[name];

            T newValue = default(T);
            CurrentSave.SaveData.Add(name, newValue);
            return newValue;
        }
	}
}
