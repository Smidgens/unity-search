// smidgens @ github

namespace Smidgenomics.Unity.Search
{
	using System.Collections.Generic;

	/// <summary>
	/// A* heuristics
	/// </summary>
	public interface IAStarHeuristic
	{
		/// <summary>
		/// Compute estimate distance between nodes
		/// </summary>
		/// <param name="start">Start key, search space</param>
		/// <param name="goal">Goal key, search space</param>
		/// <returns>Estimate</returns>
		float EstimateDistance(in int start, in int goal);

		/// <summary>
		/// Get reachable neighbour nodes
		/// </summary>
		/// <param name="key"></param>
		/// <returns>List of neighbour keys + visit cost</returns>
		List<(int, float)> GetReachableNeighbours(in int key);
	}
}