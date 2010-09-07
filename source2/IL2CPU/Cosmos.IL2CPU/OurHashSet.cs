﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Cosmos.IL2CPU
{
	// Contains known types and methods, both scanned and unscanned
	// We need both a HashSet and a List. HashSet for speed of checking
	// to see if we already have it. And mItems contains an indexed list
	// so we can scan it as it changes. Foreach can work on HashSet,
	// but if foreach is used while its changed, a collection changed
	// exception will occur and copy on demand for each loop has too
	// much overhead.
	// we use a custom comparer, because the default Hashcode does not work.
	// In .NET 4.0 has the DeclaringType often changed to System.Object,
	// didn't sure if hashcode changed. The situation now in .NET 4.0
	// is that the Contains method in OurHashSet checked only the
	// default Hashcode. With adding DeclaringType in the Hashcode it runs.

	public class OurHashSet<T> : IEnumerable<T>
	{
		private Dictionary<int, T> mItems = new Dictionary<int, T>();

		public bool Contains(T aItem)
		{
			if (aItem == null)
				throw new ArgumentNullException("aItem");
			return mItems.ContainsKey(GetHash(aItem));

			var methodDeclaringType = aItem.GetType().GetMethod("get_DeclaringType");
			var aItemDeclaringType = methodDeclaringType.Invoke(aItem, null);

			bool ret2 = false;
			foreach (var item in mItems)
			{
				var method = item.Value.GetType().GetMethod("get_DeclaringType");
				var itemDeclaringType = method.Invoke(item.Value, null);

				if (item.Value.ToString() == aItem.ToString()
					&& itemDeclaringType == aItemDeclaringType)
				{
					if (ret2)
						throw new Exception(" item mulitply in list!");
					ret2 = true;
				}

			}

			return ret2;
		}

		public void Add(T aItem)
		{
			if (aItem == null)
				throw new ArgumentNullException("aItem");
			mItems.Add(GetHash(aItem), aItem);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return (from item in mItems
					select item.Value).GetEnumerator();
		}

		public T GetItemInList(T aItem)
		{
			if (aItem == null)
				throw new ArgumentNullException("aItem");
			T xResult;
			if (mItems.TryGetValue(GetHash(aItem), out xResult))
				return xResult;
			else
				return aItem;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (from item in mItems
					select item.Value).GetEnumerator();
		}

		public static Type GetDeclareType(T item)
		{
			var xMethodDeclaringType = item.GetType().GetMethod("get_DeclaringType");
			var xDeclaringType = xMethodDeclaringType.Invoke(item, null) as Type;
			return xDeclaringType;
		}

		public static string GetDeclareTypeString(T item)
		{
			var xName = GetDeclareType(item);
			return xName == null ? "null" : xName.ToString();
		}

		public static int GetHash(T item)
		{
			//return item.GetHashCode();
			return (item.ToString() + GetDeclareTypeString(item)).GetHashCode();
		}
	}
}