using System;
using System.Collections;

namespace GeometryFriendsAgents
{
	class YHeuristicAStar
	{

		private GridMap gridMap;
		private Graph graph;

		private AStar astar;

		public YHeuristicAStar(GridMap _gridMap, Graph _graph)
		{

			this.gridMap = _gridMap;
			this.graph = _graph;
		}

		public Queue search()
		{

			MyNode startNode = new MyNode();

			ArrayList diamonds = new ArrayList();
			ArrayList queueList = new ArrayList();
			ArrayList completeList = new ArrayList();
			ArrayList nodes = graph.getNodes();

			Queue completeQueue = new Queue();

			for (int n = 0; n < nodes.Count; n++)
			{
				MyNode node = (MyNode)nodes[n];

				// if (node.isDiamond())
					// diamonds.Add(node);
			}

			while (diamonds.Count > 0)
			{

				ArrayList queueListTmp = new ArrayList();
				ArrayList queueDistanceListTmp = new ArrayList();

				/*
				 * Run A* several times for each diamond and calculate the routes and distances
				 * for each diamond and add them to the list
				*/

				for (int i = 0; i < diamonds.Count; i++)
				{

					astar = new AStar(gridMap, graph, startNode, (MyNode)diamonds[i]);
					queueListTmp.Add(astar.search());
					queueDistanceListTmp.Add(astar.getCompleteDistance());
				}

				int shortest = (int)queueDistanceListTmp[0];

				for (int i = 1; i < queueDistanceListTmp.Count; i++)
				{

					int tmpDistance = (int)queueDistanceListTmp[i];

					if (tmpDistance < shortest)
						shortest = tmpDistance;

				}

				if (shortest != int.MaxValue) //if there is a shorter route
				{

					int shortestIndex = queueDistanceListTmp.IndexOf(shortest);
					queueList.Add(queueListTmp[shortestIndex]); //add the route which has the shortest distance
					startNode = (MyNode)diamonds[shortestIndex]; //change the starting node
					diamonds.Remove(startNode);

				}
			}

			if(queueList.Count == 0 || queueList[0] == null) //if there are no routes
				return new Queue();

			completeList.AddRange((ArrayList)queueList[0]); //add to the end of the list

			for (int i = 1; i < queueList.Count; i++)
			{

				ArrayList tmp = (ArrayList)queueList[i];
				tmp.RemoveAt(0);
				completeList.AddRange(tmp);
				
			}

			for (int i = 0; i < completeList.Count; i++)
				completeQueue.Enqueue(completeList[i]);

			return completeQueue;
		}
	}
}
