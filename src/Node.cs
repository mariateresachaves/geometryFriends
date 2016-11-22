using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents
{
    class Node
    {
        private float x = 0;
        private float y = 0;
        private float velocity_x = 0;
        private float velocity_y = 0;
        private bool diamond = false;
        private bool falldown_circle = false;
        private bool falldown_rectangle = false;
        private Node parent;

        public Node(float x, float y, float velocity_x, float velocity_y, Node parent, bool diamond)
        {
            this.x = x;
            this.y = y;
            this.velocity_x = velocity_x;
            this.velocity_y = velocity_y;
            this.diamond = diamond;
            this.parent = parent;
            this.diamond = diamond;
        }

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

        public float get_x()
        {
            return this.x;
        }

        public float get_y()
        {
            return this.y;
        }

        public float get_velocity_x()
        {
            return this.velocity_x;
        }

        public float get_velocity_y()
        {
            return this.velocity_y;
        }

        public bool has_diamond()
        {
            return this.diamond;
        }

        public bool get_falldown_circle()
        {
            return this.falldown_circle;
        }

        public bool get_falldown_rectangle()
        {
            return this.falldown_rectangle;
        }

        public Node get_parent()
        {
            return this.parent;
        }

        /******************/
        /*      SETs      */
        /******************/

        public void set_x(float x)
        {
            this.x = x;
        }

        public void set_y(float y)
        {
            this.y = y;
        }

        public void set_velocity_x(float x)
        {
            this.x = x;
        }

        public void set_velocity_y(float y)
        {
            this.y = y;
        }

        public void set_diamond(bool diamond)
        {
            this.diamond = diamond;
        }

        public void set_falldown_circle(bool falldown_circle)
        {
            this.falldown_circle = falldown_circle;
        }

        public void set_falldown_rectangle(bool falldown_rectangle)
        {
            this.falldown_rectangle = falldown_rectangle;
        }

        public void set_parent(Node parent)
        {
            this.parent = parent;
        }
    }
}
