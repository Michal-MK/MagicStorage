using System;
using System.Collections.Generic;

namespace MagicStorageTwo.Extensions {
	public static class LinqExt {
		/// <summary>
		/// Apply <see cref="Action"/> to every element of the sequence.
		/// </summary>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e, Action<T> a) {
			foreach (var item in e) {
				a(item);
			}
			return e;
		}
	}
}
