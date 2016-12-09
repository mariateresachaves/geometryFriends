using GeometryFriends.AI.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GeometryFriendsAgents
{
    /// <summary>
    /// Cell contains the information about a cell of the grid.
    /// </summary>
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

        /// <summary>
        /// Cell class constructor with no parameters.
        /// </summary>
        public Cell()
        {
        }

        /// <summary>
        /// Cell class constructor with parameters.
        /// </summary>
        /// <param name="pos_x"> x position in the grid. </param>
        /// <param name="pos_y"> y position in the grid. </param>
        public Cell(int pos_x, int pos_y)
        {
            setID(System.Threading.Interlocked.Increment(ref next_id));

            setGridPos(pos_x, pos_y);
            setCoords();
        }

        /// <summary>
        /// Function to get the cell's identification number.
        /// </summary>
        /// <returns> Returns the cell's identification number. </returns>
        public int getID()
        {
            return this.id;
        }

        /// <summary>
        /// Function to get the x position of the cell in the grid.
        /// </summary>
        /// <returns> Returns the x position of the cell in the grid. </returns>
        public int getX()
        {
            return this.grid_pos[0];
        }

        /// <summary>
        /// Function to get the y position of the cell in the grid.
        /// </summary>
        /// <returns> Returns the y position of the cell in the grid. </returns>
        public int getY()
        {
            return this.grid_pos[1];
        }

        /// <summary>
        /// Function to get the x coordinate of the cell's position in the map.
        /// </summary>
        /// <returns> Returns the x coordinate of the cell's position in the map. </returns>
        public float getXCoord()
        {
            return this.coords[0];
        }

        /// <summary>
        /// Function to get the y coordinate of the cell's position in the map.
        /// </summary>
        /// <returns> Returns the y coordinate of the cell's position in the map. </returns>
        public float getYCoord()
        {
            return this.coords[1];
        }

        /// <summary>
        /// Function to get the cell's coordinates in the map.
        /// </summary>
        /// <returns> Returns the cell's coordinates in the map. </returns>
        public float[] getCoords()
        {
            return this.coords;
        }

        /// <summary>
        /// Function to know if the cell is a top limit of the map.
        /// </summary>
        /// <returns> Returns true if the cell is a top limit of the map and false otherwise. </returns>
        public Boolean isTop()
        {
            return this.top;
        }

        /// <summary>
        /// Function to know if the cell is a bottom limit of the map.
        /// </summary>
        /// <returns> Returns true if the cell is a bottom limit of the map and false otherwise. </returns>
        public Boolean isBottom()
        {
            return this.bottom;
        }

        /// <summary>
        /// Function to know if the cell has already been visited (used for the algorithm).
        /// </summary>
        /// <returns> Returns true if the cell has already been visited and false otherwise. </returns>
        public Boolean isVisited()
        {
            return this.visited;
        }

        /// <summary>
        /// Function to know if the cell is a diamond of the map.
        /// </summary>
        /// <returns> Returns true if the cell is a diamond of the map and false otherwise. </returns>
        public Boolean isDiamond()
        {
            return this.diamond;
        }

        /// <summary>
        /// Function to know if the cell is a platform of the map.
        /// </summary>
        /// <returns> Returns true if the cell is a platform of the map and false otherwise. </returns>
        public Boolean isPlatform()
        {
            return this.platform;
        }

        /// <summary>
        /// Function to know if the cell is a fall down circle platform of the map.
        /// </summary>
        /// <returns> Returns true if the cell is a fall down circle platform of the map and false otherwise. </returns>
        public Boolean isFalldownCircle()
        {
            return this.falldown_circle;
        }

        /// <summary>
        /// Function to know if the cell is a fall down rectangle platform of the map.
        /// </summary>
        /// <returns> Returns true if the cell is a fall down rectangle platform of the map and false otherwise. </returns>
        public Boolean isFalldownRectangle()
        {
            return this.falldown_rectangle;
        }

        /// <summary>
        /// Function to get the cell's heuristic value.
        /// </summary>
        /// <returns> Returns the cell's heuristic value. </returns>
        public int getHeuristic()
        {
            return this.heuristic_value;
        }

        /*********************/
        /*      SETTERS      */
        /*********************/

        /// <summary>
        /// Function to set the cell's identification number.
        /// </summary>
        /// <param name="_id"> cell's identification number. </param>
        public void setID(int _id)
        {
            id = _id;
        }

        /// <summary>
        /// Function to set the x position of the cell in the grid.
        /// </summary>
        /// <param name="_x"> x position of the cell in the grid. </param>
        public void setX(int _x)
        {
            grid_pos[0] = _x;
        }

        /// <summary>
        /// Function to set the y position of the cell in the grid.
        /// </summary>
        /// <param name="_y"> y position of the cell in the grid. </param>
        public void setY(int _y)
        {
            this.grid_pos[1] = _y;
        }

        /// <summary>
        /// Function to set cell's position in the grid.
        /// </summary>
        /// <param name="_x"> x position of the cell in the grid. </param>
        /// <param name="_y"> y position of the cell in the grid. </param>
        public void setGridPos(int _x, int _y)
        {
            setX(_x);
            setY(_y);
        }

        /// <summary>
        /// Function to calculate and set x coordinate value of the cell in the map.
        /// </summary>
        public void setXCoord()
        {
			this.coords[0] = getX() * Utils.GRID_SIZE + Utils.TRANSLATION_X;
        }

        /// <summary>
        /// Function to calculate and set y coordinate value of the cell in the map.
        /// </summary>
        public void setYCoord()
        {
            this.coords[1] = getY() * Utils.GRID_SIZE + Utils.TRANSLATION_Y;
        }

        /// <summary>
        /// Function to set coordinates of the cell in the map.
        /// </summary>
        public void setCoords()
        {
            this.setXCoord();
            this.setYCoord();
        }

        /// <summary>
        /// Function to change the cell's diamond value, which tells whether a cell is a diamond or not.
        /// </summary>
        /// <param name="_diamond"> cell's diamond new value. </param>
        public void setDiamond(Boolean _diamond)
        {
            this.diamond = _diamond;
        }

        /// <summary>
        /// Function to change the cell's top value, which tells whether a cell is a top limit or not.
        /// </summary>
        /// <param name="_top"> cell's top new value. </param>
        public void setTop(Boolean _top)
        {
            this.top = _top;
        }

        /// <summary>
        /// Function to change the cell's bottom value, which tells whether a cell is a bottom limit or not.
        /// </summary>
        /// <param name="_bottom"> cell's bottom new value. </param>
        public void setBottom(Boolean _bottom)
        {
            this.bottom = _bottom;
        }

        /// <summary>
        /// Function to change the cell's visited value, which tells whether a cell has already been visited or not.
        /// </summary>
        /// <param name="_visited"> cell's visited new value. </param>
        public void setVisited(Boolean _visited)
        {
            this.visited = _visited;
        }

        /// <summary>
        /// Function to change the cell's platform value, which tells whether a cell is a platform or not.
        /// </summary>
        /// <param name="_platform"> cell's platform new value. </param>
        public void setPlatform(Boolean _platform)
        {
            this.platform = _platform;
        }

        /// <summary>
        /// Function to set the cell's heuristic value.
        /// </summary>
        /// <param name="_heuristic_value"> cell's new heurist value. </param>
        public void setHeuristic(int _heuristic_value)
        {
            this.heuristic_value = _heuristic_value;
        }

        // function to set the upper neighbor cell position

        /*********************/
        /*      METHODS      */
        /*********************/

        /// <summary>
        /// Function to calculate the cell's upper neighbor position.
        /// </summary>
        /// <returns> Returns the cell's upper neighbor position. </returns>
        public int[] upperCell()
        {
            if (this.isTop())
                return null;

            int x = this.getX();
            int y = this.getY() - 1;

            int[] pos = { x, y };

            return pos;
        }

        /// <summary>
        /// Function to calculate the cell's lower neighbor position.
        /// </summary>
        /// <returns> Returns the cell's lower neighbor position. </returns>
        public int[] lowerCell()
        {
            if (this.isBottom())
                return null;

            int x = this.getX();
            int y = this.getY() + 1;

            int[] pos = { x, y };

            return pos;
        }

        /// <summary>
        /// Function to calculate the cell's left neighbor position.
        /// </summary>
        /// <returns> Returns the cell's left neighbor left position. </returns>
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

        /// <summary>
        /// Function to calculate the cell's right neighbor position.
        /// </summary>
        /// <returns> Returns the cell's right neighbor right position. </returns>
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