using System;
using System.Collections;
namespace GeometryFriendsAgents
{
	class MyNode
	{

		private int cellID; //this is an easier way to access Cells on the GridMap

		private ArrayList edges;
		private MyNode parent;

		public MyNode()
		{

			this.edges = new ArrayList();
			this.parent = null;

		}

		public MyNode(int _cellID)
		{
			
			this.edges = new ArrayList();
			this.parent = null;
			this.cellID = _cellID;

		}

		public int getCellID() 
		{
			
			return cellID;

		}

		public ArrayList getEdges()
		{
			
			return this.edges;

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

		public void addEdge(MyNode dst)
		{
			
			this.edges.Add(new MyEdge(this, dst));

		}
	}
}
