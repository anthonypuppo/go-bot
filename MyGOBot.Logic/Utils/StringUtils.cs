﻿using POGOProtos.Inventory.Item;
using System.Collections.Generic;
using System.Linq;

namespace MyGOBot.Logic.Utils {

	public static class StringUtils {
		public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<ItemAward> items) {
			var enumerable = items as IList<ItemAward> ?? items.ToList();

			if (!enumerable.Any())
				return string.Empty;

			return
				enumerable.GroupBy(i => i.ItemId)
					.Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
					.Select(y => $"{y.Amount} x {y.ItemName}")
					.Aggregate((a, b) => $"{a}, {b}");
		}
	}

}
