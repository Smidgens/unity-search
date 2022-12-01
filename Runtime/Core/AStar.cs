// smidgens @ github

namespace Smidgenomics.Unity.Search
{
	using System.Collections.Generic;
	using QueueException = System.ArgumentException;

	/// <summary>
	/// A* search
	/// </summary>
	public static class AStar
	{
		/// <summary>
		/// A* path search
		/// </summary>
		/// <param name="query">Search parameters</param>
		/// <param name="queue">Custom queue implementation</param>
		/// <returns>Path</returns>
		public static Result Search
		(
			in Query query,
			ISearchQueue queue = null
		)
		{
			// shenanigans?
			if (!IsValidQuery(query)) { throw new QueueException("Invalid A* query"); }

			// use default queue implementation
			if(queue == null) { queue = new SearchQueue(); }

			// get initial weight for start -> goal
			float goalWeight = query.heuristic.EstimateDistance(query.start, query.goal);

			// traverse from start node
			queue.Enqueue(new SearchNode(query.start, 0f, goalWeight, null));

			// run search
			ISearchNode goal = Search(query, queue, query.heuristic);

			return new Result
			{
				// goal != null -> path exists
				path = goal?.TracePath()
			};
		}

		/// <summary>
		/// Validate search parameters
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		private static bool IsValidQuery(in Query query)
		{
			return query.heuristic != null;
		}

		/// <summary>
		/// Search node
		/// </summary>
		public interface ISearchNode
		{
			/// <summary>
			/// Unique key in search space
			/// </summary>
			int Key { get; }

			/// <summary>
			/// Previous step in path
			/// </summary>
			ISearchNode Parent { get; }
			
			/// <summary>
			/// 
			/// </summary>
			float TotalCost { get; }

			/// <summary>
			/// Cost of visiting node
			/// </summary>
			float VisitCost { get; }

			/// <summary>
			/// Heuristic, node -> goal
			/// </summary>
			float GoalDistanceEstimate { get; }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="parent">New parent</param>
			/// <param name="distance">Distance estimate</param>
			void SetParent(ISearchNode parent, in float distance);

			/// <summary>
			/// Get path from start key to goal
			/// </summary>
			/// <returns>Key list, root -> node</returns>
			List<int> TracePath();
		}

		/// <summary>
		/// Queue for search space
		/// </summary>
		public interface ISearchQueue
		{
			int QueueLength { get; }
			void Enqueue(ISearchNode node);
			ISearchNode Dequeue();
			void Close(int key);
			ISearchNode FindOpen(in int key);
			bool IsClosed(int key);
			bool IsQueued(int key);
		}

		/// <summary>
		/// Search result
		/// </summary>
		public ref struct Result
		{
			/// <summary>
			/// Path to goal
			/// </summary>
			public List<int> path;
		}

		/// <summary>
		/// Search query
		/// </summary>
		public ref struct Query
		{
			/// <summary>
			/// Path start
			/// </summary>
			public int start;

			/// <summary>
			/// Path goal
			/// </summary>
			public int goal;

			/// <summary>
			/// Custom heuristic
			/// </summary>
			public IAStarHeuristic heuristic;
		}

		/// <summary>
		/// Recursive search
		/// </summary>
		/// <param name="query">Search params</param>
		/// <param name="queue">Search context</param>
		/// <param name="heuristic">Heuristic provider</param>
		/// <returns>Goal node, null if no path exists</returns>
		private static ISearchNode Search
		(
			in Query query,
			in ISearchQueue queue,
			IAStarHeuristic heuristic
		)
		{
			// queue empty -> no path found
			if (queue.QueueLength == 0) { return null; }

			// select least costly node available
			ISearchNode currentNode = queue.Dequeue();

			// goal key -> path found
			if (currentNode.Key == query.goal) { return currentNode; }

			// switzerland closed to visitors
			queue.Close(currentNode.Key);

			List<(int, float)> neighbours = heuristic.GetReachableNeighbours(currentNode.Key);

			foreach (var (neighbourKey, neighbourCost) in neighbours)
			{
				// neighbour closed for visits
				if (queue.IsClosed(neighbourKey)) { continue; }

				ISearchNode neighbourNode = queue.FindOpen(neighbourKey);

				// node hasn't been visited, add to queue
				if (neighbourNode == null)
				{
					// add node to search queue, estimate initial cost
					float goalDistance = heuristic.EstimateDistance(neighbourKey, query.goal);
					neighbourNode = new SearchNode(neighbourKey, neighbourCost, goalDistance, currentNode);
					queue.Enqueue(neighbourNode);
					continue;
				}

				// current distance to neighbour is shorter than one previously computed
				float g = currentNode.GoalDistanceEstimate + neighbourCost;
				if ((g + neighbourNode.VisitCost) < neighbourNode.TotalCost)
				{
					neighbourNode.SetParent(currentNode, neighbourCost);
				}
			}

			return Search(query, queue, heuristic);
		}

		/// <summary>
		/// Search context
		/// </summary>
		private class SearchQueue : ISearchQueue
		{
			public int QueueLength => _queue.Count;
			public void Close(int key) => _closed.Add(key);

			// key is closed for visits
			public bool IsClosed(int key) => _closed.Contains(key);
			
			// queue contains key
			public bool IsQueued(int key) => FindNode(_queue, key) != null;

			// dequeue best
			public ISearchNode Dequeue()
			{
				float min = float.MaxValue;
				int index = -1;
				for (int i = 0; i < _queue.Count; i++)
				{
					float f = _queue[i].TotalCost;
					if (f < min) { index = i; min = f; }
				}
				ISearchNode r = _queue[index];
				_queue.RemoveAt(index);
				return r;
			}

			// enqueue for eval
			public void Enqueue(ISearchNode node) => _queue.Add(node);

			// Visitor queue
			private List<ISearchNode> _queue = new List<ISearchNode>();

			// Closed for visits
			private HashSet<int> _closed = new HashSet<int>();

			// index of node i in path list
			private static ISearchNode FindNode(List<ISearchNode> l, int key)
			{
				for(int i = 0; i < l.Count; i++)
				{
					if (l[i].Key == key) { return l[i]; }
				}
				return null;
			}

			public ISearchNode FindOpen(in int i) => FindNode(_queue, i);
		}

		/// <summary>
		/// Traversal context for node
		/// </summary>
		internal sealed class SearchNode : ISearchNode
		{
			// preceding node in path
			public ISearchNode Parent { get; private set; } = null;

			// key in search space
			public int Key { get; private set; }

			// total cost of going through node
			public float TotalCost => VisitCost + GoalDistanceEstimate;

			// "What is the cost of pies?"
			public float VisitCost { get; private set; } = 0f;

			// node -> goal
			public float GoalDistanceEstimate { get; private set; } = 0f;

			public SearchNode
			(
				in int node,
				in float distance,
				in float h,
				ISearchNode parent
			)
			{
				VisitCost = h;
				Key = node;
				Parent = parent;
				SetParent(parent, distance);
			}

			// re-parent
			public void SetParent(ISearchNode parent, in float distance)
			{
				Parent = parent;
				GoalDistanceEstimate = (parent != null ? parent.GoalDistanceEstimate : 0) + distance;
			}

			// get path from root node
			public List<int> TracePath()
			{
				List<int> path = new List<int>();
				ISearchNode current = this;
				while (current != null)
				{
					path.Add(current.Key);
					current = current.Parent;
				}
				path.Reverse();
				return path;
			}
		}

	}
}