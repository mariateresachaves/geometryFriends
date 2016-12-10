using System;
using System.Collections;
namespace GeometryFriendsAgents
{
	class MyNode
	{
        public enum nodeType { Start, Platform, ToFall, FallDownPoint, Goal, ToDiamond };

		private int cellID; //this is an easier way to access Cells on the GridMap

		private ArrayList edges;
		private MyNode parent;

		private nodeType type;

		public MyNode()
		{
			this.edges = new ArrayList();
			this.parent = null;
		}

		public MyNode(int _cellID, nodeType _type)
		{
			this.edges = new ArrayList();
			this.parent = null;
			this.cellID = _cellID;
            this.type = _type;
		}

        /*********************/
        /*      GETTERS      */
        /*********************/

        public int getCellID() 
		{
			return cellID;
		}

		public ArrayList getEdges()
		{
			return this.edges;
		}

        public MyNode getParent()
        {
            return this.parent;
        }

        public nodeType getType()
        {
            return this.type;
        }

        public ArrayList getChildren() 
		{
			ArrayList children = new ArrayList();

			foreach (MyEdge edge in edges) 
			{
				children.Add(edge.getDst()); //dst = child, src = parent
			}

			return children;
		}

        /*********************/
        /*      SETTERS      */
        /*********************/

        public void setCellID(int _cellID)
        {
            this.cellID = _cellID;
        }

        public void setEdges(ArrayList _edges)
        {
            this.edges = _edges;
        }

        public void setParent(MyNode _parent)
        {
            this.parent = _parent;
        }

        public void setType(nodeType _type)
        {
            this.type = _type;
        }

        /**********************/
        /*      METHODSs      */
        /**********************/
        
        public void addEdge(MyNode dst)
		{
			this.edges.Add(new MyEdge(this, dst));
		}
                
	}
}
