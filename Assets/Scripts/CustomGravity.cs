using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
	public static class CustomGravity
	{
		private static List<GravitySource> sources = new();

		public static Vector3 GetGravity(Vector3 position)
		{
			Vector3 g = Vector3.zero;
			for (int i = 0; i < sources.Count; i++)
			{
				g += sources[i].GetGravity(position);
			}

			return g;
		}

		public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
		{
			Vector3 up = position.normalized;
			upAxis = Physics.gravity.y < 0.0f ? up : -up;
			return up * Physics.gravity.y;
		}

		public static Vector3 GetUpAxis(Vector3 position)
		{
			Vector3 g = Vector3.zero;
			for (int i = 0; i < sources.Count; i++)
			{
				g += sources[i].GetGravity(position);
			}

			return -g.normalized;
		}

		public static void Register(GravitySource source)
		{
			sources.Add(source);
		}

		public static void Unregister(GravitySource source)
		{
			sources.Remove(source);
		}
	}
}