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
        public Queue<int> getNeighbors(Cell cell, Queue<int> neighbors)
        {
            int heuristic_value = cell.getHeuristic() + 1;

            int[] goalUpper = cell.upperCell();
            int[] goalLower = cell.lowerCell();
            int[] goalLeft = cell.leftCell();
            int[] goalRight = cell.rightCell();

            if (goalUpper != null && !this.gridMap[goalUpper[0], goalUpper[1]].notHeuristicCell())      
            {
                int id = this.gridMap[goalUpper[0], goalUpper[1]].getID();

                if(this.gridMap[goalUpper[0], goalUpper[1]].getHeuristic() > heuristic_value)
                {
                    this.gridMap[goalUpper[0], goalUpper[1]].setHeuristic(heuristic_value);
                }
                
                if (this.gridMap[goalUpper[0], goalUpper[1]].isVisited() == false)
                {
                    this.gridMap[goalUpper[0], goalUpper[1]].setVisited(true);

                    neighbors.Enqueue(id);
                }
            }

            if (goalLower != null && !this.gridMap[goalLower[0], goalLower[1]].notHeuristicCell())
            {
                int id = this.gridMap[goalLower[0], goalLower[1]].getID();

                if (this.gridMap[goalLower[0], goalLower[1]].getHeuristic() > heuristic_value)
                {
                    this.gridMap[goalLower[0], goalLower[1]].setHeuristic(heuristic_value);
                }

                if (this.gridMap[goalLower[0], goalLower[1]].isVisited() == false)
                {
                    this.gridMap[goalLower[0], goalLower[1]].setVisited(true);

                    neighbors.Enqueue(id);
                }
            }

            if (goalLeft != null && !this.gridMap[goalLeft[0], goalLeft[1]].notHeuristicCell())
            {
                int id = this.gridMap[goalLeft[0], goalLeft[1]].getID();

                if (this.gridMap[goalLeft[0], goalLeft[1]].getHeuristic() > heuristic_value)
                {
                    this.gridMap[goalLeft[0], goalLeft[1]].setHeuristic(heuristic_value);
                }

                if (this.gridMap[goalLeft[0], goalLeft[1]].isVisited() == false)
                {
                    this.gridMap[goalLeft[0], goalLeft[1]].setVisited(true);

                    neighbors.Enqueue(id);
                }
            }

            if (goalRight != null && !this.gridMap[goalRight[0], goalRight[1]].notHeuristicCell())
            {
                int id = gridMap[goalRight[0], goalRight[1]].getID();

                if (this.gridMap[goalRight[0], goalRight[1]].getHeuristic() > heuristic_value)
                {
                    this.gridMap[goalRight[0], goalRight[1]].setHeuristic(heuristic_value);
                }

                if (this.gridMap[goalRight[0], goalRight[1]].isVisited() == false)
                {
                    this.gridMap[goalRight[0], goalRight[1]].setVisited(true);

                    neighbors.Enqueue(id);
                }
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

        public Cell getCellByCoords(float coords_x, float coords_y)
        {
            float pos_x = (float) Math.Floor((coords_x - Utils.TRANSLATION_X) / Utils.GRID_SIZE);
            float pos_y = (float)Math.Floor((coords_y - Utils.TRANSLATION_Y) / Utils.GRID_SIZE);

            if(pos_x == 75)
            {
                pos_x--;
            }

            foreach (Cell c in gridMap)
            {
                if (c.getX() == pos_x && c.getY() == pos_y)
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Function to calculate the heuristic values on the grid given a certain goal.
        /// </summary>
        /// <param name="goal"> goal cell. </param>
        public void calcHeuristicValues(float coords_x, float coords_y)
        {
            // Queue initialization
            Queue<int> neighbors = new Queue<int>();

            Cell goal_1 = getCellByCoords(coords_x, coords_y);

            int[] upper_pos = goal_1.upperCell();
            Cell goal_2 = this.gridMap[upper_pos[0], upper_pos[1]];

            int[] left_2_pos = goal_2.leftCell();
            Cell goal_3 = this.gridMap[left_2_pos[0], left_2_pos[1]];

            int[] left_1_pos = goal_1.leftCell();
            Cell goal_4 = this.gridMap[left_1_pos[0], left_1_pos[1]];

            // Set goals cells as visited
            this.gridMap[goal_1.getX(), goal_1.getY()].setVisited(true);
            this.gridMap[goal_2.getX(), goal_2.getY()].setVisited(true);
            this.gridMap[goal_3.getX(), goal_3.getY()].setVisited(true);
            this.gridMap[goal_4.getX(), goal_4.getY()].setVisited(true);

            // Set goals cells heuristic initial value
            this.gridMap[goal_1.getX(), goal_1.getY()].setHeuristic(0);
            this.gridMap[goal_2.getX(), goal_2.getY()].setHeuristic(0);
            this.gridMap[goal_3.getX(), goal_3.getY()].setHeuristic(0);
            this.gridMap[goal_4.getX(), goal_4.getY()].setHeuristic(0);

            neighbors = this.getNeighbors(goal_1, neighbors);
            neighbors = this.getNeighbors(goal_2, neighbors);
            neighbors = this.getNeighbors(goal_3, neighbors);
            neighbors = this.getNeighbors(goal_4, neighbors);

            // Set neighbors cells heuristic initial value
            this.gridMap[goal_1.getX(), goal_1.getY()].setHeuristic(0);
            this.gridMap[goal_2.getX(), goal_2.getY()].setHeuristic(0);
            this.gridMap[goal_3.getX(), goal_3.getY()].setHeuristic(0);
            this.gridMap[goal_4.getX(), goal_4.getY()].setHeuristic(0);

            while (neighbors.Count != 0)
            {
                int curr_cell_id = neighbors.Dequeue();
                Cell curr_cell = this.getCellByID(curr_cell_id);

                neighbors = this.getNeighbors(curr_cell, neighbors);
            }
        }

        public void reset()
        {
            foreach (Cell c in gridMap)
            {
                if(c.getHeuristic() != -1)
                    c.reset();
            }
        }


    }
}
