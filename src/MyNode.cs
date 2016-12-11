using System;
using System.Collections;

namespace GeometryFriendsAgents
{
	public class MyNode
	{
        public enum nodeType { Start, Platform, ToFall, FallDownPoint, Goal, ToDiamond };

		private Cell cell; //this is an easier way to access Cells on the GridMap

		private ArrayList edges;
		private MyNode parent;

		private nodeType type;

		public MyNode()
		{
			this.edges = new ArrayList();
			this.parent = null;
		}

		public MyNode(Cell _cell, nodeType _type)
		{
			this.edges = new ArrayList();
			this.parent = null;
			this.cell = _cell;
            this.type = _type;
		}

        /*********************/
        /*      GETTERS      */
        /*********************/

        public Cell getCell() 
		{
			return cell;
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

        public int getCellID()
        {
            return this.cell.getID();
        }

        public float getCellXCoord()
        {
            return this.cell.getXCoord();
        }

        public float getCellYCoord()
        {
            return this.cell.getYCoord();
        }

        public bool isDiamond()
        {
            return this.cell.isDiamond();
        }

        /*********************/
        /*      SETTERS      */
        /*********************/

        public void setCell(Cell _cell)
        {
            this.cell = _cell;
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
