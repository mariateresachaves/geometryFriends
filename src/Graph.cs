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

        /*********************/
        /*      SETTERS      */
        /*********************/



        /**********************/
        /*      METHODSs      */
        /**********************/

        public Boolean addNode(MyNode node)
        {
            //Should we first check if there is another node with the same cellID?

            nodes.Add(node);

            return true;
        }

    }
}
