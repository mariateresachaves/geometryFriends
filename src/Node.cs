using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// Node contains the information about a position in the map.
    /// </summary>
    class Node
    {
        private float x = 0;
        private float y = 0;
        private float velocity_x = 0;
        private float velocity_y = 0;
        private bool diamond = false;
        private bool falldown_circle = false;
        private bool falldown_rectangle = false;

        public Node parent;
        public Node[] children;


        /// <summary>
        /// Node class constructor.
        /// </summary>
        /// <param name="x"> x position in the grid. </param>
        /// <param name="y"> y position in the grid. </param>
        /// <param name="velocity_x"> velocity in component x needed to achieve this position. </param>
        /// <param name="velocity_y"> velocity in component y needed to achieve this position. </param>
        /// <param name="parent"> nodes parent. </param>
        /// <param name="children"> all children of the node </param>
        /// <param name="diamond"> tells if there is a diamond in this position or not. </param>
        public Node(float x, float y, float velocity_x, float velocity_y, Node parent, Node[] children, bool diamond)
        {
            this.x = x;
            this.y = y;
            this.velocity_x = velocity_x;
            this.velocity_y = velocity_y;
            this.diamond = diamond;
            this.parent = parent;
            this.children = children;
            this.diamond = diamond;
        }

        /// <summary>
        /// Override of equals operator, so that it's possible to verify if two nodes are the same.
        /// </summary>
        /// <param name="obj"> node to compare with. </param>
        public override bool Equals(object obj)
        {
            // verifies if the object is of the correct type (the type must be Node)
            if (obj == null || GetType() != obj.GetType())
                return false;

            Node n = (Node)obj;

            return (get_x() == n.get_x()) && (get_y() == n.get_y()) && (has_diamond() == n.has_diamond());
        }

        /******************/
        /*      GETs      */
        /******************/

        /// <summary>
        /// Function to get the x coordinate.
        /// </summary>
        /// <returns> Returns the x coordinate. </returns>
        public float get_x()
        {
            return this.x;
        }

        /// <summary>
        /// Function to get the y coordinate.
        /// </summary>
        /// <returns> Returns the y coordinate. </returns>
        public float get_y()
        {
            return this.y;
        }

        /// <summary>
        /// Function to get the velocity in component x.
        /// </summary>
        /// <returns> Returns the velocity in component x. </returns>
        public float get_velocity_x()
        {
            return this.velocity_x;
        }

        /// <summary>
        /// Function to get the velocity in component y.
        /// </summary>
        /// <returns> Returns the velocity in component y. </returns>
        public float get_velocity_y()
        {
            return this.velocity_y;
        }

        /// <summary>
        /// Function to know if this position has a diamond.
        /// </summary>
        /// <returns> Returns true if this position has a diamond and false otherwise. </returns>
        public bool has_diamond()
        {
            return this.diamond;
        }

        /// <summary>
        /// Function to know if in this position the circle may fall down.
        /// </summary>
        /// <returns> Returns true if in this position the circle may fall down and false otherwise. </returns>
        public bool get_falldown_circle()
        {
            return this.falldown_circle;
        }

        /// <summary>
        /// Function to know if in this position the rectangle may fall down.
        /// </summary>
        /// <returns> Returns true if in this position the rectangle may fall down and false otherwise. </returns>
        public bool get_falldown_rectangle()
        {
            return this.falldown_rectangle;
        }

        /******************/
        /*      SETs      */
        /******************/

        /// <summary>
        /// Function to set the x coordinate value.
        /// </summary>
        /// <param name="x"> x new position in the grid. </param>
        public void set_x(float x)
        {
            this.x = x;
        }

        /// <summary>
        /// Function to set the y coordinate value.
        /// </summary>
        /// <param name="y"> y new position in the grid. </param>
        public void set_y(float y)
        {
            this.y = y;
        }

        /// <summary>
        /// Function to set the velocity in component x value.
        /// </summary>
        /// <param name="x"> velocity in component x new value. </param>
        public void set_velocity_x(float x)
        {
            this.x = x;
        }

        /// <summary>
        /// Function to set the velocity in component y value.
        /// </summary>
        /// <param name="y"> velocity in component y new value. </param>
        public void set_velocity_y(float y)
        {
            this.y = y;
        }

        /// <summary>
        /// Function to set the diamond value in this position.
        /// </summary>
        /// <param name="diamond"> diamond new value. </param>
        public void set_diamond(bool diamond)
        {
            this.diamond = diamond;
        }

        /// <summary>
        /// Function to set the fall down value of the circle in this position.
        /// </summary>
        /// <param name="falldown_circle"> fall down of the circle new value. </param>
        public void set_falldown_circle(bool falldown_circle)
        {
            this.falldown_circle = falldown_circle;
        }

        /// <summary>
        /// Function to set the fall down value of the rectangle in this position.
        /// </summary>
        /// <param name="falldown_rectangle"> fall down of the rectangle new value. </param>
        public void set_falldown_rectangle(bool falldown_rectangle)
        {
            this.falldown_rectangle = falldown_rectangle;
        }

        /**********************/
        /*      METHODSs      */
        /**********************/

        /// <summary>
        /// Function to add a child node.
        /// </summary>
        /// <param name="child"> child to add. </param>
        /// <returns> Returns 0 in success and -1 otherwise. </returns>
        public int add_child(Node child)
        {
            foreach (Node child_node in this.children)
            {
                if (child_node == child)
                    return -1;
            }


            return 0;
        }
    }
}
