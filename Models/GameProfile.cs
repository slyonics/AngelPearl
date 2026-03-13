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

			HeroModel proxy = new HeroModel(MuseRecord.MUSES.First(x => x.Name == "Proxy"));
			CurrentSave.Roster.Add(proxy);

			HeroModel aika = new HeroModel(MuseRecord.MUSES.First(x => x.Name == "Aika"));
			CurrentSave.Roster.Add(aika);

			HeroModel faye = new HeroModel(MuseRecord.MUSES.First(x => x.Name == "Faye"));
			CurrentSave.Roster.Add(faye);

			HeroModel karin = new HeroModel(MuseRecord.MUSES.First(x => x.Name == "Karin"));
			CurrentSave.Roster.Add(karin);

			HeroModel mascot = new HeroModel(MuseRecord.MUSES.First(x => x.Name == "Mascot"));
			CurrentSave.Mascot.Value = mascot;


            GameProfile.CurrentSave.Party.Clear();
            foreach (var maho in GameProfile.CurrentSave.Roster.Take(3))
            {
                GameProfile.CurrentSave.Party.Add(maho.Value);
            }

            GameProfile.CurrentSave.Party.Add(GameProfile.CurrentSave.Mascot.Value);


            CurrentSave.CurrentMission = new ModelProperty<MissionRecord>(MissionRecord.MISSIONS.First(x => x.Name == "Gabriel"));
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
