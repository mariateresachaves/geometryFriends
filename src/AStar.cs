using System;
using System.Collections;
using System.Collections.Generic;

namespace GeometryFriendsAgents
{
	class AStar
	{

		private ArrayList openList;
		private ArrayList closedList;

		// For each node, the total cost of getting from the start node to the goal
		// by passing by that node. That value is partly known, partly heuristic.
		private Dictionary<MyNode, int> fScore;

		// For each node, the cost of getting from the start node to that node.
		private Dictionary<MyNode, int> gScore;

		// For each node, which node it can most efficiently be reached from.
		// If a node can be reached from many nodes, cameFrom will eventually contain the
		// most efficient previous step.
		private Dictionary<MyNode, MyNode> cameFrom;

		private GridMap gridMap;
		private Graph graph;

		private MyNode start, goal;

		public AStar(GridMap _gridMap, Graph _graph, MyNode _start, MyNode _goal)
		{
			
			this.gridMap = _gridMap;
			this.graph = _graph;
			this.start = _start;
			this.goal = _goal;

			openList = new ArrayList();
			closedList = new ArrayList();

			cameFrom = new Dictionary<MyNode, MyNode>();

		}

		public ArrayList getChildren(MyNode _node)
		{
			
			return _node.getChildren();

		}

		public int heuristicValue(MyNode src, MyNode dst)
		{

			//Should we use the Manhattan distance? Or the Euclidean?

			Cell cSrc = gridMap.getCellByID(src.getCellID());
			Cell cDst = gridMap.getCellByID(dst.getCellID());

			float xDiff = cSrc.getXCoord() - cDst.getXCoord();
			float yDiff = cSrc.getYCoord() - cDst.getYCoord();

			//For now it's the Manhattan distance
			return Math.Abs((int)xDiff) + Math.Abs((int)yDiff);
				
		}

		public MyNode lowestFScore(ArrayList nodes)
		{

			int cellValue;
			int minimum = -Utils.INFINITY;

			MyNode lowestNode = new MyNode();

			foreach (MyNode node in nodes)
			{

				cellValue = gridMap.getCellByID(node.getCellID()).getHeuristic();

				if (cellValue < minimum)
				{
					lowestNode = node;
					minimum = cellValue;
				}

			}

			return lowestNode;

		}

		/* 
			Set both fScores and gScores to INFINITY
			Check A* algorithm on Wikipedia for more info: https://en.wikipedia.org/wiki/A*_search_algorithm
		*/

		public void initialize() 
		{

			ArrayList nodes = graph.getNodes();

			foreach (MyNode node in nodes)
			{
				if (fScore.ContainsKey(node) || gScore.ContainsKey(node))
				{
					fScore[node] = Utils.INFINITY;
					gScore[node] = Utils.INFINITY;
				}
				else
				{
					fScore.Add(node, Utils.INFINITY);
					gScore.Add(node, Utils.INFINITY);
				}
			}
		}

		/*
			Check Fischer thesis, chapter 4 (Search), algorithm 4: A* part of selecting the node with the lowest f score and calculating new f scores to determine the route
			The algorithm is the same in Wikipedia: https://en.wikipedia.org/wiki/A*_search_algorithm
		*/

		public ArrayList search() 
		{

			MyNode current;
			int tentativeGScore = 0;

			//NOTE: start and goal node is set and start node with f and g score is added to open list
			initialize();

			openList.Add(this.start); //start node with f and g score is added to open list
			fScore[this.start] = gridMap.getCellByID(this.start.getCellID()).getHeuristic();
			gScore[this.start] = 0;

			while (openList.Count > 0) //while openList is not empty
			{

				current = lowestFScore(openList);

				if (current == goal)
					return reconstructRoute(current);

				openList.Remove(current);
				closedList.Add(current);

				ArrayList children = current.getChildren(); //children is the same as neighbors

				foreach(MyNode child in children) 
				{

					if (closedList.Contains(child)) //Ignore the neighbor which is already evaluated.
						continue;

					//What is the distanceBetween? UNCOMMENT THE FOLLOWING LINE!
					//tentativeGScore = gScore[current] + distanceBetween(current, child);

					if (!openList.Contains(child)) // Discover a new node
						openList.Add(child);
					else if (tentativeGScore >= gScore[child])
						continue;

					cameFrom[child] = current;
					gScore[child] = tentativeGScore;
					fScore[child] = gScore[child] + heuristicValue(child, goal);
				}
			}

			return null; //return failure
		}

		public ArrayList reconstructRoute(MyNode current) 
		{
			ArrayList route = new ArrayList();
			route.Add(current);

			while (cameFrom.ContainsKey(current))
			{
				current = cameFrom[current];
				route.Add(current);
			}

			return route;
		}
	}
}
