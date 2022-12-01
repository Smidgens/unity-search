// smidgens @ github

namespace Smidgenomics.Unity.Search
{
	/// <summary>
	/// Heuristics on 2D grid
	/// </summary>
	public static class Heuristic2D
	{
		/// <summary>
		/// Delta x + delta y
		///		abs(x2-x1) + abs(y2-y1)
		/// </summary>
		/// <param name="x1">Start x</param>
		/// <param name="y1">Start y</param>
		/// <param name="x2">End x</param>
		/// <param name="y2">End y</param>
		/// <returns></returns>
		public static float Manhattan(float x1, float y1, float x2, float y2)
		{
			return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
		}

		/// <summary>
		/// Straight distance from A to B
		///		sqrt((x2 - x1)^2 + (y2-y1)^2)
		/// </summary>
		/// <param name="x1">Start x</param>
		/// <param name="y1">Start y</param>
		/// <param name="x2">End x</param>
		/// <param name="y2">End y</param>
		/// <returns>Distance between points</returns>
		public static float Straight(float x1, float y1, float x2, float y2)
		{
			return Math.Sqrt(Math.Pow2(x2 - x1) + Math.Pow2(y2 - y1));
		}

		/// <summary>
		/// Maaaaaaath Daaaaaaamon
		/// </summary>
		private static class Math
		{
			public static float Sqrt(in float v) => (float)System.Math.Sqrt(v);
			public static float Abs(in float v) => System.Math.Abs(v);
			public static float Pow2(in float v) => v * v;
		}
	}
}