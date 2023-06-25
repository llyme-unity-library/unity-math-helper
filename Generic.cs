using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CollectionHelper;
using TextHelper;
using UnityEngine;

namespace MathHelper
{
	public static class Generic
	{
		public readonly static Regex NUMBERS = new(@"\d*\.?\d+");
				
		/// <summary>
		/// Like `IEnumerable<Number>.Sum()`,
		/// but multiplies instead of adding.
		/// </summary>
		public static float Diminish
			(this IEnumerable<float> enumerable,
			float initial_value = 1f)
		{
			foreach (float value in enumerable)
				if (initial_value != 0f)
					initial_value *= value;
				else
					break;
					
			return initial_value;
		}
		
		public static bool MaxConsecutive
			(this IEnumerable<int> enumerable,
			out int value,
			int step = 1,
			int start = 1)
		{
			value = start;
			bool started = false;

			foreach (int item in enumerable)
				if (started)
				{
					int next = item;

					if (next != value + step)
						break;

					value = next;
				}
				else if (value == item)
					started = true;
				else
					break;

			return started;
		}

		public static int MaxConsecutive
			(this IEnumerable<int> enumerable,
			int step = 1,
			int start = 1,
			int @default = 1) =>
			MaxConsecutive(enumerable, out int value, step, start)
			? value
			: @default;

		public static Vector2 RandomDirection
		{
			get
			{
				float v =
					UnityEngine.Random.value *
					Mathf.PI *
					2f;
				return new(Mathf.Cos(v), Mathf.Sin(v));
			}
		}

		public static IEnumerable<IEnumerable<Vector3>> Cluster
			(Vector3[] vectors,
			float maxDistance)
		{
			Dictionary<int, HashSet<int>> clusters = new();
			// Get squared distance.
			maxDistance *= maxDistance;

			for (int x = 0; x < vectors.Length; x++)
			{
				if (!clusters.TryGetValue(x, out HashSet<int> list))
					clusters[x] = list = new() { x };

				for (int y = 0; y < vectors.Length; y++)
				{
					if (x == y || list.Contains(y))
						continue;

					bool flag0 =
						list.Any(i =>
							vectors[i]
							.SquareLength(vectors[y]) <=
							maxDistance
						);

					if (!flag0)
						continue;

					list.Add(y);
					clusters[y] = list;
				}
			}

			return
				clusters.Values
				.Distinct()
				.Select(x => x.Select(y => vectors[y]));
		}

		public static bool WeightedDistribution<T>
			(IEnumerable<T> items,
			Func<T, float> weightPredicate,
			out T result)
		{
			float max = 0f;
			Dictionary<T, float> values = new();

			foreach (T item in items)
				max += values[item] = weightPredicate(item);

			float value = UnityEngine.Random.value * max;

			foreach (KeyValuePair<T, float> item in values)
				if (value <= item.Value)
				{
					result = item.Key;
					return true;
				}
				else
					value -= item.Value;

			result = default;
			return false;
		}

		/// <summary>
		/// Check if the given number can be found in the array or
		/// is contained within any of the given range values.
		/// Example; "9, 4 ~ 7, 10"
		/// Number must be 9, any number between 4 to 7 (inclusive,) or 10.
		/// </summary>
		public static bool IsContained(float value, params string[] texts)
		{
			foreach (string text in texts)
				foreach (string range in text.Split(','))
				{
					List<float> values = new();

					// Take no more than 2 numbers.
					foreach (string number in range.Split('~').Take(2))
						if (float.TryParse(number, out float result))
							values.Add(result);

					if (values.Count == 0)
						// Empty.
						continue;

					if (values.Count == 1 &&
						value == values[0])
						// Exact.
						return true;

					if (value >= values[0] &&
						value <= values[1])
						// Range.
						return true;
				}

			return false;
		}

		public static float[] Floats(this string text)
		{
			MatchCollection matches =
				NUMBERS.Matches(text);
			float[] result = new float[matches.Count];

			for (int i = 0; i < matches.Count; i++)
				result[i] = matches[i].Value.Float();

			return result;
		}

		public static RangeInt[] ToRangeInts(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return new RangeInt[0];

			List<RangeInt> ranges = new();

			foreach (string range in text.Split(','))
			{
				List<int> values = new();

				// Take no more than 2 numbers.
				foreach (string number in range.Split('~').Take(2))
					if (int.TryParse(number, out int result))
						values.Add(result);

				switch (values.Count)
				{
					case 1:
						// Exact.
						ranges.Add(new()
						{
							start = values[0],
							length = 0
						});
						break;
					case 2:
						// Range.
						ranges.Add(new()
						{
							start = values[0],
							length = values[1] - values[0]
						});
						break;
				}
			}

			return ranges.ToArray();
		}

		/// <summary>
		/// Removes a random value and returns it.
		/// </summary>
		public static T RandomPop<T>(this List<T> list)
		{
			if (list.IsNullOrEmpty())
				return default;

			int index = UnityEngine.Random.Range(0, list.Count);
			T value = list[index];
			list.RemoveAt(index);
			return value;
		}

		public static T Random<T>(this T[] array) =>
			array.IsNullOrEmpty()
			? default
			: array[UnityEngine.Random.Range(0, array.Length)];

		public static T Random<T>(this IList<T> collection) =>
			collection.IsNullOrEmpty()
			? default
			: collection[UnityEngine.Random.Range(0, collection.Count)];

		/// <summary>
		/// Returns a random integer from one of the given ranges.
		/// Inclusive.
		/// </summary>
		public static int RandomInt(params RangeInt[] ranges)
		{
			if (ranges.Length == 0)
				return 0;

			RangeInt range = ranges.Length > 1
				? ranges[UnityEngine.Random.Range(0, ranges.Length)]
				: ranges[0];

			return UnityEngine.Random.Range(range.start, range.end + 1);
		}

		/// <summary>
		/// If the value is within the start
		/// and end of the range (inclusive.)
		/// </summary>
		public static bool Contains
			(this RangeInt range,
			int value) =>
			value >= range.start && value <= range.end;

		public static bool Contains
			(this RangeInt[] ranges,
			int value)
		{
			foreach (RangeInt range in ranges)
				if (range.Contains(value))
					return true;

			return false;
		}

		public static Vector3 Floor(this Vector3 v) =>
			new(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));

		/// <summary>
		/// Get the difference between two normal vectors.
		/// Parameters are assumed to be normal vectors.
		/// Result ranges from 0f to 4f.
		/// </summary>
		public static float NormalDifference(this Vector2 a, Vector2 b) =>
			Mathf.Pow(a.Length(b), 2f);

		/// <summary>
		/// Get direction from a to b.
		/// </summary>
		public static Vector2 Towards
			(this Vector2 a, Vector2 b) =>
			(b - a).normalized;

		public static Vector3 Towards
			(this Vector3 a, Vector3 b) =>
			(b - a).normalized;

		public static float Radians(this Vector2 v) =>
			Mathf.Atan2(v.y, v.x);

		public static float SquareMagnitude(this float x, float y) =>
			x * x + y * y;

		/// <summary>
		/// Get distance between a and b.
		/// </summary>
		public static float Length(this float a, float b) =>
			Mathf.Abs(b - a);

		/// <summary>
		/// Get distance between a and b.
		/// </summary>
		public static float Length(this Vector2 a, Vector2 b) =>
			(b - a).magnitude;

		public static float SquareLength(this Vector2 a, Vector2 b) =>
			(b - a).sqrMagnitude;

		/// <summary>
		/// Get distance between a and b.
		/// </summary>
		public static float Length(this Vector3 a, Vector3 b) =>
			(b - a).magnitude;
		
		public static float SquareLength(this Vector3 a, Vector3 b) =>
			(b - a).sqrMagnitude;

		/// <summary>
		/// Create a Vector2 with a Vector3's x and z.
		/// </summary>
		public static Vector2 XZ(this Vector3 v3) =>
			new(v3.x, v3.z);

		/// <summary>
		/// Wrap angle between 0f to 360f.
		/// </summary>
		public static float W360(this float a) =>
			a < 0f ? 360f + a % 360f : a % 360f;

		/// <summary>
		/// Radians to degrees.
		/// </summary>
		public static float R2D(this float r) =>
			r * Mathf.Rad2Deg;

		/// <summary>
		/// Degrees to radians.
		/// </summary>
		public static float D2R(this float d) =>
			d * Mathf.Deg2Rad;

		/// <summary>
		/// Rotate angle from a to b by adding a fixed amount.
		/// </summary>
		public static float RotateAngleTo
			(this float a,
			float b,
			float rate)
		{
			float next = Mathf.DeltaAngle(a, b);

			if (Mathf.Abs(next) <= rate)
				return b;

			return next > 0f ? a + rate : a - rate;
		}

		/// <summary>
		/// Get direction in X-axis.
		/// </summary>
		public static int TowardsX
			(this Vector2 a,
			Vector2 b) =>
			a.x > b.x ? -1 : 1;

		public static Quaternion LookQuaternion
			(this Vector2 direction) =>
			Quaternion.Euler(
				0f,
				0f,
				Mathf.Atan2(direction.y, direction.x)
				.R2D()
			);
	}
}
