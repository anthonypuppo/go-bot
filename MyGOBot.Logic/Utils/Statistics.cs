﻿using MyGOBot.Logic.Utils;
using POGOProtos.Networking.Responses;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MyGOBot.Logic.Utils {

	public class Statistics : BindableBase {
		private int totalExperience;
		private int totalPokemons;
		private int totalItemsRemoved;
		private int totalPokemonsTransfered;
		private int totalStardust;
		private string currentLevelInfos;
		private int currentlevel = -1;
		private string playerName;
		public DateTime initSessionDateTime = DateTime.Now;

		public int TotalExperience {
			get { return totalExperience; }
			set {
				SetProperty(ref totalExperience, value);
				OnPropertyChanged("EXPPerHour");
			}
		}

		public int TotalPokemons {
			get { return totalPokemons; }
			set {
				SetProperty(ref totalPokemons, value);
				OnPropertyChanged("PokemonPerHour");
			}
		}

		public int TotalItemsRemoved {
			get { return totalItemsRemoved; }
			set { SetProperty(ref totalItemsRemoved, value); }
		}

		public int TotalPokemonsTransfered {
			get { return totalPokemonsTransfered; }
			set { SetProperty(ref totalPokemonsTransfered, value); }
		}

		public int TotalStardust {
			get { return totalStardust; }
			set { SetProperty(ref totalStardust, value); }
		}

		public string CurrentLevelInfos {
			get { return currentLevelInfos; }
			set { SetProperty(ref currentLevelInfos, value); }
		}

		public int Currentlevel {
			get { return currentlevel; }
			set { SetProperty(ref currentlevel, value); }
		}

		public string PlayerName {
			get { return playerName; }
			set { SetProperty(ref playerName, value); }
		}

		public DateTime InitSessionDateTime {
			get { return initSessionDateTime; }
			set { SetProperty(ref initSessionDateTime, value); }
		}

		public TimeSpan Duration {
			get { return DateTime.Now - InitSessionDateTime; }
		}

		public string Runtime {
			get { return _getSessionRuntimeInTimeFormat(); }
			set { OnPropertyChanged(); }
		}

		public string EXPPerHour {
			get { return (TotalExperience / _getSessionRuntime()).ToString("N0"); }
			set { OnPropertyChanged(); }
		}

		public string PokemonPerHour {
			get { return (TotalPokemons / _getSessionRuntime()).ToString("N0"); }
			set { OnPropertyChanged(); }
		}

		public async Task<string> _getcurrentLevelInfos(Inventory inventory) {
			var stats = await inventory.GetPlayerStats();
			var output = string.Empty;
			var stat = stats?.FirstOrDefault();
			if (stat != null) {
				var ep = stat.NextLevelXp - stat.PrevLevelXp - (stat.Experience - stat.PrevLevelXp);
				var time = Math.Round(ep / (TotalExperience / _getSessionRuntime()), 2);
				var hours = 0.00;
				var minutes = 0.00;
				if (double.IsInfinity(time) == false && time > 0) {
					time = Convert.ToDouble(TimeSpan.FromHours(time).ToString("h\\.mm"), CultureInfo.InvariantCulture);
					hours = Math.Truncate(time);
					minutes = Math.Round((time - hours) * 100);
				}

				output =
					$"{stat.Level} (next level in {hours}h {minutes}m | {stat.Experience - stat.PrevLevelXp - GetXpDiff(stat.Level)}/{stat.NextLevelXp - stat.PrevLevelXp - GetXpDiff(stat.Level)} XP)";
				//output = $"{stat.Level} (LvLUp in {_hours}hours // EXP required: {_ep})";
			}
			Runtime = "";
			return output;
		}

		public double _getSessionRuntime() {
			return (DateTime.Now - InitSessionDateTime).TotalSeconds / 3600;
		}

		public string _getSessionRuntimeInTimeFormat() {
			return (DateTime.Now - InitSessionDateTime).ToString(@"dd\.hh\:mm\:ss");
		}

		public void AddExperience(int xp) {
			TotalExperience += xp;
		}

		public void AddItemsRemoved(int count) {
			TotalItemsRemoved += count;
		}

		public void GetStardust(int stardust) {
			TotalStardust = stardust;
		}

		public int GetXpDiff(int level) {
			switch (level) {
				case 1:
					return 0;
				case 2:
					return 1000;
				case 3:
					return 2000;
				case 4:
					return 3000;
				case 5:
					return 4000;
				case 6:
					return 5000;
				case 7:
					return 6000;
				case 8:
					return 7000;
				case 9:
					return 8000;
				case 10:
					return 9000;
				case 11:
					return 10000;
				case 12:
					return 10000;
				case 13:
					return 10000;
				case 14:
					return 10000;
				case 15:
					return 15000;
				case 16:
					return 20000;
				case 17:
					return 20000;
				case 18:
					return 20000;
				case 19:
					return 25000;
				case 20:
					return 25000;
				case 21:
					return 50000;
				case 22:
					return 75000;
				case 23:
					return 100000;
				case 24:
					return 125000;
				case 25:
					return 150000;
				case 26:
					return 190000;
				case 27:
					return 200000;
				case 28:
					return 250000;
				case 29:
					return 300000;
				case 30:
					return 350000;
				case 31:
					return 500000;
				case 32:
					return 500000;
				case 33:
					return 750000;
				case 34:
					return 1000000;
				case 35:
					return 1250000;
				case 36:
					return 1500000;
				case 37:
					return 2000000;
				case 38:
					return 2500000;
				case 39:
					return 1000000;
				case 40:
					return 1000000;
			}
			return 0;
		}

		public void IncreasePokemons() {
			TotalPokemons += 1;
		}

		public void IncreasePokemonsTransfered() {
			TotalPokemonsTransfered += 1;
		}

		public void SetUsername(GetPlayerResponse profile) {
			PlayerName = profile.PlayerData.Username ?? "";
		}

		public override string ToString() {
			return $"{PlayerName} - Runtime {_getSessionRuntimeInTimeFormat()} - Lvl: {CurrentLevelInfos} | EXP/H: {TotalExperience / _getSessionRuntime():0} | P/H: {TotalPokemons / _getSessionRuntime():0} | Stardust: {TotalStardust:0} | Transfered: {TotalPokemonsTransfered:0} | Items Recycled: {TotalItemsRemoved:0}";
		}

		public async void UpdateConsoleTitle(Inventory inventory) {
			try {
				CurrentLevelInfos = await _getcurrentLevelInfos(inventory);
			} catch { }
			//Logger.Write(ToString());
		}

	}

}
