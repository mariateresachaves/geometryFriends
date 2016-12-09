using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// GridMap contains information about the map's grid.
    /// </summary>
    class GridMap
    {
        private Cell[,] gridMap = new Cell[Utils.COL_CELLS, Utils.ROW_CELLS];

        /// <summary>
        /// GridMap class constructor.
        /// </summary>
        public GridMap()
        {
            for(int i = 0; i < Utils.COL_CELLS; i++)
            {
                for(int j = 0; j < Utils.ROW_CELLS; j++)
                {
                    gridMap[i, j] = new Cell(i, j);

					if (j == Utils.ROW_CELLS-1)
                        gridMap[i, j].setBottom(true);

					if (j == 0)
                        gridMap[i, j].setTop(true);
                }
            }
        }

        /// <summary>
        /// Function to set all obstacles in the gridMap.
        /// </summary>
        /// <param name="obstaclesInfo"> set of obstacles on the map. </param>
        public void setCellObstacle(ObstacleRepresentation[] obstaclesInfo)
        {
            float obs_x;
            float obs_y;

            foreach (ObstacleRepresentation obstacle in obstaclesInfo)
            {
                obs_x = obstacle.X - Utils.TRANSLATION_X;
                obs_y = obstacle.Y - Utils.TRANSLATION_Y;

                float x1 = obs_x - ((float)obstacle.Width / 2);
				float y1 = obs_y - ((float)obstacle.Height / 2);

				float x2 = obs_x + ((float)obstacle.Width / 2);
				float y2 = obs_y - ((float)obstacle.Height / 2);

				float x3 = obs_x + ((float)obstacle.Width / 2);
				float y3 = obs_y + ((float)obstacle.Height / 2);

				float x4 = obs_x - ((float)obstacle.Width / 2);
				float y4 = obs_y + ((float)obstacle.Height / 2);

                splitObstacle(x1, y1, x2, y2, x3, y3, x4, y4);
            }
        }

        /// <summary>
        /// Function to set a gridMap cell as an obstacle.
        /// </summary>
        /// <param name="x1"> x coordinate of the upper left corner of the obstacle. </param>
        /// <param name="y1"> y coordinate of the upper left corner of the obstacle. </param>
        /// <param name="x2"> x coordinate of the upper right corner of the obstacle. </param>
        /// <param name="y2"> y coordinate of the upper right corner of the obstacle. </param>
        /// <param name="x3"> x coordinate of the lower right corner of the obstacle. </param>
        /// <param name="y3"> y coordinate of the lower right corner of the obstacle. </param>
        /// <param name="x4"> x coordinate of the lower left corner of the obstacle. </param>
        /// <param name="y4"> y coordinate of the lower left corner of the obstacle. </param>
        public void splitObstacle(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            float width = x2 - x1;
            float height = y3 - y2;
            
			int pos_init_x = (int)Math.Floor(x1 / Utils.GRID_SIZE);
			int pos_init_y = (int)Math.Floor(y1 / Utils.GRID_SIZE);

            int n_cols = (int) Math.Ceiling(width / Utils.GRID_SIZE);
            int n_rows = (int) Math.Ceiling(height / Utils.GRID_SIZE);
            
            for (int i = 0; i < n_cols; i++)
            {
                for(int j = 0; j < n_rows; j++)
                {
                    gridMap[i+pos_init_x, j+pos_init_y].setPlatform(true);
                }
            }
        }

        /// <summary>
        /// Function to get the grid of the map.
        /// </summary>
        /// <returns> Returns the grid of the map. </returns>
        public Cell[,] getGridMap()
        {
            return this.gridMap;
        }

        /// <summary>
        /// Function to get the neighbors of a given cell.
        /// </summary>
        /// <param name="cell"> cell of the map. </param>
        /// <param name="neighbors"> queue where the cell's neighbors are going to be stored. </param>
        /// <param name="heuristic_value"> heuristic value of the cell. </param>
        /// <returns> Returns a queue with the cell's neighbors. </returns>
        public Queue<int> getNeighbors(Cell cell, Queue<int> neighbors, int heuristic_value)
        {
            int[] goalUpper = cell.upperCell();
            int[] goalLower = cell.lowerCell();
            int[] goalLeft = cell.leftCell();
            int[] goalRight = cell.rightCell();

            if (goalUpper != null)
            {
                int id = gridMap[goalUpper[0], goalUpper[1]].getID();

                if (!gridMap[goalUpper[0], goalUpper[1]].isVisited())
                {
                    gridMap[goalUpper[0], goalUpper[1]].setHeuristic(heuristic_value);
                    gridMap[goalUpper[0], goalUpper[1]].setVisited(true);
                }

                neighbors.Enqueue(id);
            }

            if (goalLower != null)
            {
                int id = gridMap[goalLower[0], goalLower[1]].getID();

                if (!gridMap[goalLower[0], goalLower[1]].isVisited())
                {
                    gridMap[goalLower[0], goalLower[1]].setHeuristic(heuristic_value);
                    gridMap[goalLower[0], goalLower[1]].setVisited(true);
                }

                neighbors.Enqueue(id);
            }

            if (goalLeft != null)
            {
                int id = gridMap[goalLeft[0], goalLeft[1]].getID();

                if (!gridMap[goalLeft[0], goalLeft[1]].isVisited())
                {
                    gridMap[goalLeft[0], goalLeft[1]].setHeuristic(heuristic_value);
                    gridMap[goalLeft[0], goalLeft[1]].setVisited(true);
                }

                neighbors.Enqueue(id);
            }

            if (goalRight != null)
            {
                int id = gridMap[goalRight[0], goalRight[1]].getID();

                if (!gridMap[goalRight[0], goalRight[1]].isVisited())
                {
                    gridMap[goalRight[0], goalRight[1]].setHeuristic(heuristic_value);
                    gridMap[goalRight[0], goalRight[1]].setVisited(true);
                }

                neighbors.Enqueue(id);
            }

            return neighbors;
        }

        /// <summary>
        /// Function to get a cell of the map given the cell's identification number.
        /// </summary>
        /// <returns> Returns the cell in success and null otherwise. </returns>
        public Cell getCellByID(int id)
        {
            foreach(Cell cell in gridMap)
            {
                if (cell.getID() == id)
                    return cell;
            }

            return null;
        }

        /// <summary>
        /// Function to calculate the heuristic values on the grid given a certain goal.
        /// </summary>
        /// <param name="goal"> goal cell. </param>
        public void calcHeuristicValues(Cell goal)
        {
            // Queue initialization
            Queue<int> neighbors = new Queue<int>();

            neighbors = this.getNeighbors(goal, neighbors, 1);

            int heuristic_value = 2;

            while (neighbors.Count != 0)
            {
                int curr_cell_id = neighbors.Dequeue();
                Cell curr_cell = this.getCellByID(curr_cell_id);

                neighbors = this.getNeighbors(curr_cell, neighbors, heuristic_value);

                heuristic_value++;
            }
        }


    }
}
