using GeometryFriends.AI.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GeometryFriendsAgents
{
    public class Cell
    {
        private int id;

        private int[] grid_pos = new int[2];
        private float[] coords = new float[2];

        private Boolean top = false;
        private Boolean bottom = false;                
       
        private Boolean platform = false;
        private Boolean goal = false;

        private Boolean visited = false;

        /*
         * GETTERS
         */

        public Cell()
        {
        }

        public int getID()
        {
            return this.id;
        }

        public int getX()
        {
            return this.grid_pos[0];
        }

        public int getY()
        {
            return this.grid_pos[1];
        }

        public float getXCoord()
        {
            return this.coords[0];
        }

        public float getYCoord()
        {
            return this.coords[1];
        }

        public float[] getCoords()
        {
            return this.coords;
        }

        public Boolean isTop()
        {
            return this.top;
        }

        public Boolean isBottom()
        {
            return this.bottom;
        }

        public Boolean isVisited()
        {
            return this.visited;
        }

        public Boolean isGoal()
        {
            return this.goal;
        }

        public Boolean isPlatform()
        {
            return this.platform;
        }

        /*
         * SETTERS
         */

        public void setID(int _id)
        {
            id = _id;
        }

        public void setX(int _x)
        {
            grid_pos[0] = _x;
        }

        public void setY(int _y)
        {
            this.grid_pos[1] = _y;
        }

        public void setGridPos(int _x, int _y)
        {
            setX(_x);
            setY(_y);
        }

        public void setXCoord()
        {
            this.coords[0] = (getX() * Utils.WIDTH) / Utils.GRID_SIZE;
        }

        public void setYCoord()
        {
            this.coords[1] = (getY() * Utils.HEIGHT) / Utils.GRID_SIZE;
        }

        public void setCoords()
        {
            this.setXCoord();
            this.setYCoord();
        }

        public void setGoal(Boolean _goal)
        {
            this.goal = _goal;
        }

        public void setTop(Boolean _top)
        {
            this.top = _top;
        }

        public void setBottom(Boolean _bottom)
        {
            this.bottom = _bottom;
        }

        public void setVisited(Boolean _visited)
        {
            this.visited = _visited;
        }

        public void setPlatform(Boolean _platform)
        {
            this.platform = _platform;
        }

    }
}