using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents
{
    class Edge
    {
        public Node source;
        public Node dest;

        public Edge(Node source, Node dest)
        {
            this.source = source;
            this.dest = dest;
        }

        /******************/
        /*      GETs      */
        /******************/

        public Node get_source()
        {
            return this.source;
        }

        public Node get_dest()
        {
            return this.dest;
        }

        /******************/
        /*      SETs      */
        /******************/

        public void set_source(Node source)
        {
            this.source = source;
        }

        public void set_dest(Node dest)
        {
            this.dest = dest;
        }
    }
}
