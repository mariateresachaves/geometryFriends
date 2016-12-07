using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeometryFriendsAgents
{
    class GridMap
    {
        private Cell[,] gridMap = new Cell[Utils.COL_CELLS, Utils.ROW_CELLS];
        
        public GridMap()
        {
            for(int i = 0; i < Utils.COL_CELLS; i++)
            {
                for(int j = 0; j < Utils.ROW_CELLS; j++)
                {
                    gridMap[i, j] = new Cell(i, j);

                    if (j == Utils.ROW_CELLS-1)
                        gridMap[i, j].setBottom(true);

                    else if (j == 0)
                        gridMap[i, j].setTop(true);
                }
            }
        }

        public void setCellObstacle(ObstacleRepresentation[] obstaclesInfo)
        {
            foreach(ObstacleRepresentation obstacle in obstaclesInfo)
            {
                float x1 = obstacle.X - obstacle.Width / 2;
                float y1 = obstacle.Y - obstacle.Height / 2;

                float x2 = obstacle.X + obstacle.Width / 2;
                float y2 = obstacle.Y - obstacle.Height / 2;

                float x3 = obstacle.X + obstacle.Width / 2;
                float y3 = obstacle.Y + obstacle.Height / 2;

                float x4 = obstacle.X - obstacle.Width / 2;
                float y4 = obstacle.Y + obstacle.Height / 2;

                splitObstacle(x1, y1, x2, y2, x3, y3, x4, y4);
            }
        }

        public void splitObstacle(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            float width = x2 - x1;
            float height = y3 - y2;

            Debug.WriteLine("X1 - " + x1);
            Debug.WriteLine("Y1 - " + y1);
            Debug.WriteLine("X2 - " + x2);
            Debug.WriteLine("Y2 - " + y2);
            Debug.WriteLine("X3 - " + x3);
            Debug.WriteLine("Y3 - " + y3);
            Debug.WriteLine("X4 - " + x4);
            Debug.WriteLine("Y4 - " + y4);

            int pos_init_x = (int)Math.Floor(x1 / Utils.GRID_SIZE);
            int pos_init_y = (int)Math.Floor(y1 / Utils.GRID_SIZE);

            int n_cols = (int) Math.Floor(width / Utils.GRID_SIZE);
            int n_rows = (int)Math.Floor(height / Utils.GRID_SIZE);

            for (int i = 0; i < n_cols; i++)
            {
                for(int j = 0; j < n_rows; j++)
                {
                    gridMap[i+pos_init_x, j+pos_init_y].setPlatform(true);
                }
            }
        }

        /** REMOVE **/
        public float CelltoWidth(int i)
        {
            float temp = (float)((i * 1200) / 40);
            return temp;
        }
        public float CelltoHeight(int j)
        {
            float temp = (float)((j * 720) / 40);
            return temp;
        }

        /** REMOVE **/

        public Cell[,] getGridMap()
        {
            return this.gridMap;
        }

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

        public Cell getCellByID(int id)
        {
            foreach(Cell cell in gridMap)
            {
                if (cell.getID() == id)
                    return cell;
            }

            return null;
        }

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
