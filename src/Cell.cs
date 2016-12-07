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
        static int next_id;

        private int id;

        private int[] grid_pos = new int[2];
        private float[] coords = new float[2];

        private Boolean top = false;
        private Boolean bottom = false;                
       
        private Boolean platform = false;
        private Boolean falldown_circle = false;
        private Boolean falldown_rectangle = false;

        private Boolean diamond = false;

        private Boolean visited = false;

        private int heuristic_value = 0;

        /*********************/
        /*      GETTERS      */
        /*********************/

        public Cell()
        {
        }

        public Cell(int pos_x, int pos_y)
        {
            setID(System.Threading.Interlocked.Increment(ref next_id));

            setGridPos(pos_x, pos_y);
            setCoords();
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

        public Boolean isDiamond()
        {
            return this.diamond;
        }

        public Boolean isPlatform()
        {
            return this.platform;
        }

        public Boolean isFalldownCircle()
        {
            return this.falldown_circle;
        }
  
        public Boolean isFalldownRectangle()
        {
            return this.falldown_rectangle;
        }

        public int getHeuristic()
        {
            return this.heuristic_value;
        }

        /*********************/
        /*      SETTERS      */
        /*********************/

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

        public void setDiamond(Boolean _diamond)
        {
            this.diamond = _diamond;
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

        public void setHeuristic(int _heuristic_value)
        {
            this.heuristic_value = _heuristic_value;
        }

        /*********************/
        /*      METHODS      */
        /*********************/

        public int[] upperCell()
        {
            if (this.isTop())
                return null;

            int x = this.getX();
            int y = this.getY() - 1;

            int[] pos = { x, y };

            return pos;
        }

        public int[] lowerCell()
        {
            if (this.isBottom())
                return null;

            int x = this.getX();
            int y = this.getY() + 1;

            int[] pos = { x, y };

            return pos;
        }

        public int[] leftCell()
        {
            int x = this.getX();

            if (x == 0)
                return null;

            x--;
            int y = this.getY();

            int[] pos = { x, y };

            return pos;
        }

        public int[] rightCell()
        {
            int x = this.getX();

            if (x == Utils.COL_CELLS-1)
                return null;

            x++;
            int y = this.getY();

            int[] pos = { x, y };

            return pos;
        }
    }
}