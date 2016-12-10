using System;
namespace GeometryFriendsAgents
{
	class MyEdge
	{

		private MyNode src, dst;

		public MyEdge(MyNode _src, MyNode _dst)
		{
			this.src = _src;
			this.dst = _dst;
		}

		public MyNode getSrc()
		{
			return this.src;
		}

		public MyNode getDst() 
		{
			return this.dst;
		}

		public void setSrc(MyNode _src)
		{
			this.src = _src;
		}

		public void setDst(MyNode _dst)
		{
			this.dst = _dst;
		}
	}
}
