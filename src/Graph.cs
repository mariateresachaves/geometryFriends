using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents
{
    class Graph
    {
		private ArrayList nodes;
		private AStar astar;
        private int[,] adjacencyMatrix_actions;

        private String agent;

        private GridMap gridMap;

        public Graph() 
		{
			nodes = new ArrayList();
		}

		public Graph(GridMap _gridMap)
		{
			nodes = new ArrayList();
			this.gridMap = _gridMap;
		}

        /*********************/
        /*      GETTERS      */
        /*********************/

        public ArrayList getNodes()
		{
			return this.nodes;
		}

		public MyNode getNodeByCellID(int cellID)
		{
            foreach (MyNode node in nodes) 
			{
                if (node.getCellID() == cellID)
					return node;
			}

			return null;
		}

        public MyNode getStart()
        {
            MyNode start = null;

            foreach(MyNode node in this.nodes)
            {
                if (node.getType() == MyNode.nodeType.Start)
                    start = node;
            }

            return start;
        }

        public int[,] getAdjacencyMatrix()
        {
            return this.adjacencyMatrix_actions;
        }

        /*********************/
        /*      SETTERS      */
        /*********************/

        public void setGridMap(GridMap _gridMap)
        {
            this.gridMap = _gridMap;
        }

        public void setAdjacencyMatrix(int[,] _adjacencyMatrix_actions)
        {
            this.adjacencyMatrix_actions = _adjacencyMatrix_actions;
        }

        public void setAgent(String _agent)
        {
            this.agent = _agent;
        }

        /**********************/
        /*      METHODSs      */
        /**********************/

        public ArrayList search(MyNode _start, MyNode _goal)
        {
            this.astar = new AStar(this.gridMap, this, _start, _goal);
            ArrayList path = this.astar.search();

            //ArrayList path = new ArrayList();

            return path;
        }

        public Boolean addNode(MyNode node)
        {
            //Should we first check if there is another node with the same cellID?

            nodes.Add(node);

            return true;
        }

    }
}
