using System;
using System.Collections.Generic;

namespace Beatrate.Core
{
	public class BeGenericObjectPool<T> where T : new()
	{
		private readonly Stack<T> objects = new Stack<T>();
		private readonly Action<T> resetCallback;

		public BeGenericObjectPool(Action<T> resetCallback)
		{
			this.resetCallback = resetCallback;
		}

		public T Get()
		{
			if(objects.Count == 0)
			{
				return new T();
			}

			return objects.Pop();
		}

		public void Return(T item)
		{
			resetCallback(item);
			objects.Push(item);
		}
	}

	public class BeListPool<T>
	{
		private static BeGenericObjectPool<List<T>> pool = new BeGenericObjectPool<List<T>>(item => item.Clear());

		public static List<T> Get() => pool.Get();
		public static List<T> Get(int capacity)
		{
			var list = pool.Get();
			if(list.Capacity < capacity)
			{
				list.Capacity = capacity;
			}

			return list;
		}
		public static void Return(List<T> item) => pool.Return(item);
	}

	public class BeQueuePool<T>
	{
		private static BeGenericObjectPool<Queue<T>> pool = new BeGenericObjectPool<Queue<T>>(item => item.Clear());

		public static Queue<T> Get() => pool.Get();
		public static void Return(Queue<T> item) => pool.Return(item);
	}

	public class BeDictionaryPool<T, U>
	{
		private static BeGenericObjectPool<Dictionary<T, U>> pool = new BeGenericObjectPool<Dictionary<T, U>> (item => item.Clear());

		public static Dictionary<T, U> Get() => pool.Get();
		public static void Return(Dictionary<T, U> item) => pool.Return(item);
	}

	public class BeHashSetPool<T>
	{
		private static BeGenericObjectPool<HashSet<T>> pool = new BeGenericObjectPool<HashSet<T>>(item => item.Clear());

		public static HashSet<T> Get() => pool.Get();
		public static void Return(HashSet<T> item) => pool.Return(item);
	}

	public class BeStackPool<T>
	{
		private static BeGenericObjectPool<Stack<T>> pool = new BeGenericObjectPool<Stack<T>>(item => item.Clear());

		public static Stack<T> Get() => pool.Get();
		public static void Return(Stack<T> item) => pool.Return(item);
	}
}