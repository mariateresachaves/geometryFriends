using System;
using System.Collections;
using System.Collections.Generic;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// Implementation relative to the A* Algorithm.
    /// </summary>
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
        private int[,] adjacencyMatrix_actions;


        private MyNode start, goal;

		private int completeDistance;

        /// <summary>
        /// AStar class constructor.
        /// </summary>
        /// <param name="_gridMap"> grid of the map. </param>
        /// <param name="_graph"> graph of the map. </param>
        /// <param name="_start"> starting node. </param>
        /// <param name="_goal"> goal node. </param>
		public AStar(GridMap _gridMap, Graph _graph, MyNode _start, MyNode _goal)
		{
			completeDistance = int.MaxValue;

			this.gridMap = _gridMap;
			this.graph = _graph;
			this.start = _start;
			this.goal = _goal;
            this.adjacencyMatrix_actions = graph.getAdjacencyMatrix();

			openList = new ArrayList();
			closedList = new ArrayList();

            fScore = new Dictionary<MyNode, int>();
            gScore = new Dictionary<MyNode, int>();

            cameFrom = new Dictionary<MyNode, MyNode>();
		}

        /// <summary>
        /// Function to get the children of a given node.
        /// </summary>
        /// <returns> Returns the children of a given node. </returns>
		public ArrayList getChildren(MyNode _node)
		{
            ArrayList children = new ArrayList();
            ArrayList graph_nodes = graph.getNodes();

            int i = 0;

            bool notFound = true;

            while(notFound)
            {
                MyNode tmp = (MyNode)graph_nodes[i];
                if (tmp.getCellID() == _node.getCellID())
                    notFound = false;
                else
                    i++;

            }

            for (int j = 0; j < graph_nodes.Count; j++)
            {
                if (this.adjacencyMatrix_actions[i, j] != 0)
                    children.Add(graph_nodes[j]);
            }

            return children;

        }

        /// <summary>
        /// Function to calculate the heuristic value from a node to another.
        /// </summary>
        /// <returns> Returns the heuristic value from a node to another. </returns>
		public int heuristicValue(MyNode src, MyNode dst)
		{
			Cell cSrc = gridMap.getCellByID(src.getCellID());
			Cell cDst = gridMap.getCellByID(dst.getCellID());

			float xDiff = cSrc.getXCoord() - cDst.getXCoord();
			float yDiff = cSrc.getYCoord() - cDst.getYCoord();

			//Manhattan distance
			return Math.Abs((int)xDiff) + Math.Abs((int)yDiff);
		}

        /// <summary>
        /// Function to get the node with the lowerest F score value.
        /// </summary>
        /// <returns> Returns the node with the lowerest F score value. </returns>
		public MyNode lowestFScore(ArrayList nodes)
		{
			int cellValue;
			int minimum = Utils.INFINITY;

			MyNode lowestNode = new MyNode();

			foreach (MyNode node in nodes)
			{
				cellValue = gridMap.getCellByID(node.getCellID()).getHeuristic();

				if (cellValue < minimum && cellValue != -1) //-1 is a platform!!
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

        /// <summary>
        /// Function to set start and goal node.
        /// Start node with f and g score is added to the open list.
        /// </summary>
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
			Check Fischer thesis, chapter 4 (Search), algorithm 4:
            A* part of selecting the node with the lowest f score and calculating new f scores
            to determine the route.
			The algorithm is the same in Wikipedia: https://en.wikipedia.org/wiki/A*_search_algorithm
		*/

        // TODO: Vou aqui na documentação

        /// <summary>
        /// Function to ...
        /// </summary>
        /// <returns> Returns the ... </returns>
        public ArrayList search() 
		{
			MyNode current;
			int tentativeGScore = 0;
            
			initialize();

			openList.Add(this.start); //start node with f and g score is added to open list
			fScore[this.start] = gridMap.getCellByID(this.start.getCellID()).getHeuristic();
			gScore[this.start] = 0;

			while (openList.Count > 0) //while openList is not empty
			{
				current = lowestFScore(openList);

				if (current == goal)
				{
					completeDistance = fScore[current]; //this is just useful for Y-Heuristic A*
					return reconstructRoute(current);
				}

				openList.Remove(current);
				closedList.Add(current);

				ArrayList children = this.getChildren(current); //children is the same as neighbors

				foreach(MyNode child in children) 
				{

					if (closedList.Contains(child)) //Ignore the neighbor which is already evaluated.
						continue;

					//What is the distanceBetween? UNCOMMENT THE FOLLOWING LINE!
					tentativeGScore = gScore[current] + heuristicValue(current, child);

					if (!openList.Contains(child) && child.getType() != MyNode.nodeType.Tunnel) // Discover a new node
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

            route.Reverse();

            if(route != null)
            {
                for (int i = 0; i < route.Count; i++)
                {
                    MyNode node = (MyNode)route[i];
                    if(node.getType() == MyNode.nodeType.Tunnel)
                    {
                        route.RemoveRange(i, route.Count - i);
                        break;
                    }
                }
            }

            return route;
		}

		public int getCompleteDistance()
		{
			return completeDistance;
		}
	}
}
