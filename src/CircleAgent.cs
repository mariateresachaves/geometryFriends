using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.ActionSimulation;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A circle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class CircleAgent : AbstractCircleAgent
    {
        //agent implementation specificiation
        private bool implementedAgent;
        private string agentName = "MyCircle";

        //auxiliary variables for agent action
        private Moves currentAction;
        private List<Moves> possibleMoves;
        private long lastMoveTime;
        private Random rnd;

        //predictor of actions for the circle
        private ActionSimulator predictor = null;
        private DebugInformation[] debugInfo = null;
        private int debugCircleSize = 20;

        //debug agent predictions and history keeping
        private List<CollectibleRepresentation> caughtCollectibles;
        private List<CollectibleRepresentation> uncaughtCollectibles;
        private object remainingInfoLock = new Object();
        private List<CollectibleRepresentation> remaining;

        //Sensors Information and level state
        private CountInformation numbersInfo;
        private RectangleRepresentation rectangleInfo;
        private CircleRepresentation circleInfo;
        private ObstacleRepresentation[] obstaclesInfo;
        private ObstacleRepresentation[] rectanglePlatformsInfo;
        private ObstacleRepresentation[] circlePlatformsInfo;
        private CollectibleRepresentation[] collectiblesInfo;

        private int nCollectiblesLeft;

        private List<AgentMessage> messages;

        //Area of the game screen
        private Rectangle area;

        //Grid info
        private GridMap grid;

        //Graph info
        private Graph graph;

        //Adjacency matrix
        public int[,] adjacencyMatrix_actions;
        public int[,] adjacencyMatrix_directions;
        public float[,] adjacencyMatrix_distances;

        //Directions
        enum Direction { Right, RightDown, Down, LeftDown, Left, LeftUp, Up, RightUp };

        //Actions
        enum Actions { NoEdge, Small, Big, Jump };

        public CircleAgent()
        {
            //Change flag if agent is not to be used
            implementedAgent = true;

            //setup for action updates
            lastMoveTime = DateTime.Now.Second;
            currentAction = Moves.NO_ACTION;
            rnd = new Random();

            //prepare the possible moves  
            possibleMoves = new List<Moves>();
            possibleMoves.Add(Moves.ROLL_LEFT);
            possibleMoves.Add(Moves.ROLL_RIGHT);
            possibleMoves.Add(Moves.JUMP);
            possibleMoves.Add(Moves.GROW);

            //history keeping
            uncaughtCollectibles = new List<CollectibleRepresentation>();
            caughtCollectibles = new List<CollectibleRepresentation>();
            remaining = new List<CollectibleRepresentation>();

            //messages exchange
            messages = new List<AgentMessage>();

            Console.WriteLine("Circle Agent - " + numbersInfo.ToString());
        }

        //implements abstract circle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            numbersInfo = nI;
            nCollectiblesLeft = nI.CollectiblesCount;
            rectangleInfo = rI;
            circleInfo = cI;
            obstaclesInfo = oI;
            rectanglePlatformsInfo = rPI;
            circlePlatformsInfo = cPI;
            collectiblesInfo = colI;
            uncaughtCollectibles = new List<CollectibleRepresentation>(collectiblesInfo);
            this.area = area;

            //GridMap
            grid = new GridMap();
            SetupGrid();

            //graph
            graph = new Graph();
            SetupGraph();

            //adjacency matrix
            initAdjacencyMatrix();

            ArrayList goal_nodes = defineGoals();
            MyNode start_node = graph.getStart();
            ArrayList path = new ArrayList();

            graph.setGridMap(this.grid);
            graph.setAdjacencyMatrix(this.adjacencyMatrix_actions);
            graph.setAgent(Utils.CIRCLE_AGENT);

            StreamWriter sw_path = new StreamWriter("path.csv");

            foreach (MyNode goal in goal_nodes)
            {
                this.grid.calcHeuristicValues(goal.getCell().getXCoord(), goal.getCell().getYCoord());

                path = graph.search(start_node, goal);
                if (path != null)
                {
                    foreach (MyNode node_path in path)
                    {
                        sw_path.Write(node_path.getID() + ";");
                    }
                    sw_path.WriteLine();

                    start_node = (MyNode)path[path.Count - 1];

                }

                this.grid.reset();

            }

            sw_path.Close();

            //send a message to the rectangle informing that the circle setup is complete and show how to pass an attachment: a pen object
            messages.Add(new AgentMessage("Setup complete, testing to send an object as an attachment.", new Pen(Color.AliceBlue)));

            DebugSensorsInfo();
        }

        public ArrayList defineGoals()
        {
            ArrayList goal_nodes = new ArrayList();

            foreach (MyNode node in this.graph.getNodes())
            {
                if (node.getCell().isDiamond())
                    goal_nodes.Add(node);
            }

            return goal_nodes;
        }

        public void SetupGrid()
        {
            this.grid.setCellObstacle(rectanglePlatformsInfo);
            this.grid.setCellObstacle(obstaclesInfo);
            //NOTE: Implement setCellObstacle before calcHeuristicValues
            this.grid.calcHeuristicValues(collectiblesInfo[0].X, collectiblesInfo[0].Y);
        }

        public void addNodeFallDown(Cell toFall, MyNode.nodeType type)
        {
            Cell tmp = null;
            Cell curr = toFall;
            bool findCell = true;
            int[] pos;

            while (findCell)
            {
                pos = curr.lowerCell();
                tmp = this.grid.getGridMap()[pos[0], pos[1]];

                //Floor
                if (tmp.isBottom())
                {
                    findCell = false;
                    this.graph.addNode(new MyNode(curr, type));
                }

                else
                {
                    if (tmp.isPlatform())
                    {
                        findCell = false;
                        this.graph.addNode(new MyNode(curr, type));
                    }
                }

                curr = tmp;
            }
        }

        public void graphNodeObstacles(ObstacleRepresentation obstacle)
        {
            //Obstacle CENTER position
            float obs_x = obstacle.X;
            float obs_y = obstacle.Y;

            //Obstacle P1 - upper left
            float x1 = obs_x - ((float)obstacle.Width / 2);
            float y1 = obs_y - ((float)obstacle.Height / 2);

            //Obstacle P2 - upper right
            float x2 = obs_x + ((float)obstacle.Width / 2);
            float y2 = obs_y - ((float)obstacle.Height / 2);

            //Obstacle P3 - lower right
            float x3 = obs_x + ((float)obstacle.Width / 2);
            float y3 = obs_y + ((float)obstacle.Height / 2);

            //Obstacle P4 - lower left
            float x4 = obs_x - ((float)obstacle.Width / 2);
            float y4 = obs_y + ((float)obstacle.Height / 2);

            int[] pos;

            //Cell of position P1
            Cell c1 = this.grid.getCellByCoords(x1, y1);

            pos = c1.upperCell();
            Cell c1_upper = null;

            if (pos != null)
                c1_upper = this.grid.getGridMap()[pos[0], pos[1]];

            pos = c1_upper.leftCell();
            Cell c1_upper_left = null;

            if (pos != null)
                c1_upper_left = this.grid.getGridMap()[pos[0], pos[1]];

            pos = c1_upper.rightCell();
            Cell c1_upper_right = null;

            if (pos != null)
                c1_upper_right = this.grid.getGridMap()[pos[0], pos[1]];

            //Cell of position P2
            Cell c2 = this.grid.getCellByCoords(x2, y2);

            pos = c2.leftCell();
            Cell c2_left = null;

            if (pos != null)
                c2_left = this.grid.getGridMap()[pos[0], pos[1]];

            pos = c2.upperCell();
            Cell c2_upper = null;

            if (pos != null)
                c2_upper = this.grid.getGridMap()[pos[0], pos[1]];

            pos = c2_upper.leftCell();
            Cell c2_upper_left = null;

            if (pos != null)
                c2_upper_left = this.grid.getGridMap()[pos[0], pos[1]];

            pos = c2_upper.rightCell();
            Cell c2_upper_right = null;

            if (pos != null)
                c2_upper_right = this.grid.getGridMap()[pos[0], pos[1]];

            //Add first node

            //Platform with wall on the left side
            if (c1_upper != null && c1_upper_left == null && !c1_upper.isPlatform() && !c1_upper.isTop())
                this.graph.addNode(new MyNode(c1_upper, MyNode.nodeType.Platform));

            //Platform with falldown on the left side
            if (c1_upper != null && c1_upper_left != null && !c1_upper.isPlatform() && !c1_upper_left.isPlatform() && !c1_upper.isTop())
            {
                this.graph.addNode(new MyNode(c1_upper_left, MyNode.nodeType.ToFall));

                this.addNodeFallDown(c1_upper_left, MyNode.nodeType.FallDownPoint);
            }

            //Platform in the middle or with wall on the right side
            if (c1_upper != null && c1_upper_left != null && !c1_upper.isPlatform() && c1_upper_left.isPlatform() && !c1_upper.isTop())
                this.graph.addNode(new MyNode(c1_upper, MyNode.nodeType.Platform));

            //Case where c1 has a platform on top
            Cell c1_translate = null;
            if (c1_upper != null && c1_upper.isPlatform())
            {
                Cell tmp = new Cell();
                bool find_cell = true;
                Cell current = c1_upper;
                while (find_cell)
                {
                    pos = current.rightCell();
                    if (pos != null)
                        tmp = this.grid.getGridMap()[pos[0], pos[1]];

                    if (!tmp.isPlatform())
                    {
                        c1_translate = tmp;
                        find_cell = false;
                    }

                    //Check if end of platform
                    else if (tmp.getX() >= c2_upper_left.getX())
                        find_cell = false;

                    current = tmp;
                }

                if (c1_translate != null)
                    this.graph.addNode(new MyNode(c1_translate, MyNode.nodeType.Platform));
            }

            //Add second node

            //Falldown right
            if (c2_upper != null && c2_upper_left != null && !c2_upper.isPlatform() && !c2_upper_left.isPlatform() && !c2_upper.isTop())
            {
                this.graph.addNode(new MyNode(c2_upper, MyNode.nodeType.ToFall));

                this.addNodeFallDown(c2_upper, MyNode.nodeType.FallDownPoint);
            }

            //Has right platform
            if (c2_upper != null && c2_upper_left != null && c2_upper.isPlatform() && !c2_upper_left.isPlatform() && !c2_upper.isTop())
                this.graph.addNode(new MyNode(c2_upper_left, MyNode.nodeType.Platform));

            //Case where c2 has a platform on top
            Cell c2_translate = null;
            if (c2_upper_left != null && c2_upper_left.isPlatform())
            {
                Cell tmp = new Cell();
                bool find_cell = true;
                Cell current = c2_upper_left;
                while (find_cell)
                {
                    pos = current.leftCell();
                    if (pos != null)
                        tmp = this.grid.getGridMap()[pos[0], pos[1]];

                    if (!tmp.isPlatform())
                    {
                        c2_translate = tmp;
                        find_cell = false;
                    }

                    //Check if end of platform
                    else if (tmp.getX() <= c1.getX())
                        find_cell = false;

                    current = tmp;
                }

                Cell a = c2_translate;

                if (c2_translate != null)
                    this.graph.addNode(new MyNode(c2_translate, MyNode.nodeType.Platform));
            }

            //Cell of position P3
            Cell c3 = this.grid.getCellByCoords(x3, y3);
            Cell c3_tunnel = null;
            Cell tmp_3 = new Cell();
            bool find_cell_3 = true;
            Cell current_3 = null;
            bool emptySpace_3 = false;

            pos = c3.leftCell();
            if (pos != null)
                current_3 = this.grid.getGridMap()[pos[0], pos[1]];

            pos = current_3.lowerCell();
            float c3_lower_height = Utils.INFINITY;
            if (pos != null)
            {
                current_3 = this.grid.getGridMap()[pos[0], pos[1]];
                c3_lower_height = current_3.getYCoord();
            }


            if (current_3 != null)
            {
                while (find_cell_3)
                {
                    pos = current_3.lowerCell();
                    if (pos != null)
                        tmp_3 = this.grid.getGridMap()[pos[0], pos[1]];

                    if (emptySpace_3 && (tmp_3.isPlatform() || tmp_3.isBottom()))
                    {
                        c3_tunnel = current_3;
                        find_cell_3 = false;
                    }

                    if (!tmp_3.isPlatform() && !tmp_3.isBottom())
                        emptySpace_3 = true;


                    current_3 = tmp_3;
                }

                float height = Math.Abs(c3_lower_height - c3_tunnel.getYCoord());

                if (c3_tunnel != null && height < Utils.SMALL_RADIUS)
                {
                    MyNode node_3 = new MyNode(c3_tunnel, MyNode.nodeType.Tunnel);
                    node_3.setTunnel(height);
                    this.graph.addNode(node_3);
                }

            }

            //Cell of position P4
            Cell c4 = this.grid.getCellByCoords(x4, y4);
            Cell c4_tunnel = null;
            Cell tmp_4 = new Cell();
            bool find_cell_4 = true;
            Cell current_4 = null;
            bool emptySpace_4 = false;

            pos = c4.lowerCell();
            float c4_lower_height = Utils.INFINITY;
            if (pos != null)
            {
                current_4 = this.grid.getGridMap()[pos[0], pos[1]];
                c4_lower_height = current_4.getYCoord();
            }

            if (current_4 != null)
            {
                while (find_cell_4)
                {
                    pos = current_4.lowerCell();
                    if (pos != null)
                        tmp_4 = this.grid.getGridMap()[pos[0], pos[1]];

                    if (emptySpace_4 && (tmp_4.isPlatform() || tmp_4.isBottom()))
                    {
                        c4_tunnel = current_4;
                        find_cell_4 = false;
                    }

                    if (!tmp_4.isPlatform() && !tmp_4.isBottom())
                        emptySpace_4 = true;


                    current_4 = tmp_4;
                }

                float height = Math.Abs(c4_lower_height - c4_tunnel.getYCoord());

                if (c4_tunnel != null && height < Utils.SMALL_RADIUS)
                {
                    MyNode node_4 = new MyNode(c4_tunnel, MyNode.nodeType.Tunnel);
                    node_4.setTunnel(height);
                    this.graph.addNode(node_4);
                }

            }
        }

        public void addNodeDiamond(Cell diamond)
        {
            Cell tmp = null;
            Cell curr = diamond;
            bool findCell = true;
            int[] pos;

            while (findCell)
            {
                pos = curr.lowerCell();
                tmp = this.grid.getGridMap()[pos[0], pos[1]];

                //Floor or platform 
                if (tmp.isBottom() || tmp.isPlatform())
                {
                    findCell = false;
                    float height = curr.getYCoord() - diamond.getYCoord();

                    if (height < Utils.TRESHOLD_DIAMOND)
                        this.graph.addNode(new MyNode(curr, MyNode.nodeType.LowGoal));

                    else
                    {
                        this.graph.addNode(new MyNode(diamond, MyNode.nodeType.Goal));
                        this.addNodeFallDown(diamond, MyNode.nodeType.ToDiamond);
                    }


                }

                curr = tmp;
            }
        }

        public void SetupGraph()
        {
            foreach (ObstacleRepresentation obstacle in obstaclesInfo)
                this.graphNodeObstacles(obstacle);

            foreach (ObstacleRepresentation obstacle in rectanglePlatformsInfo)
                this.graphNodeObstacles(obstacle);


            foreach (CollectibleRepresentation diamond in collectiblesInfo)
            {
                Cell c = this.grid.getCellByCoords(diamond.X, diamond.Y);
                c.setDiamond(true);
                this.addNodeDiamond(c);
            }

            this.graph.addNode(new MyNode(grid.getCellByCoords(circleInfo.X, circleInfo.Y), MyNode.nodeType.Start));
        }

        public void initAdjacencyMatrix()
        {
            ArrayList nodes = this.graph.getNodes();
            ArrayList parameters = new ArrayList();

            this.adjacencyMatrix_actions = new int[nodes.Count, nodes.Count];
            this.adjacencyMatrix_directions = new int[nodes.Count, nodes.Count];
            this.adjacencyMatrix_distances = new float[nodes.Count, nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    // no relation between the same node
                    if (i == j)
                        continue;

                    MyNode src = (MyNode)nodes[i];
                    MyNode dest = (MyNode)nodes[j];

                    parameters = checkEdgeParameters(src, dest);

                    this.adjacencyMatrix_actions[i, j] = (int)parameters[0];
                    this.adjacencyMatrix_directions[i, j] = (int)parameters[1];
                    this.adjacencyMatrix_distances[i, j] = (float)parameters[2];
                }
            }
        }

        // src always on the left
        public bool hasHorizontalWall(Cell src, Cell dest)
        {
            Cell tmp = new Cell();
            bool find_cell = true;
            Cell current = src;
         
            int[] pos;

            while (find_cell)
            {
                pos = current.rightCell();
                if (pos != null)
                    tmp = this.grid.getGridMap()[pos[0], pos[1]];

                if (tmp.isPlatform())
                    return true;

                //Check if end of platform
                else if (tmp.getX() <= dest.getX())
                    find_cell = false;

                current = tmp;
            }

            return false;
        }

        // src always on the top
        public bool hasVerticalWall(Cell src, Cell dest)
        {
            
            Cell tmp = new Cell();
            bool find_cell = true;
            Cell current = src;

            int[] pos;

            if(dest.getXCoord() < src.getXCoord())
            {
                pos = src.leftCell();
                if (pos != null)
                    current = this.grid.getGridMap()[pos[0], pos[1]];
            }

            while (find_cell)
            {
                pos = current.lowerCell();
                if (pos != null)
                    tmp = this.grid.getGridMap()[pos[0], pos[1]];

                if (tmp.isPlatform())
                    return true;

                //Check if end of platform
                else if (tmp.getY() >= dest.getY())
                    find_cell = false;

                current = tmp;
            }

            return false;
        }

        public bool hasTunnel(MyNode src, MyNode dest)
        {
            float src_X = src.getCellXCoord();
            float src_Y = src.getCellYCoord();

            float dest_X = dest.getCellXCoord();
            float dest_Y = dest.getCellYCoord();

            if (dest.getType() == MyNode.nodeType.Tunnel)
                return true;
            foreach (MyNode node in this.graph.getNodes())
            {
                if (node.getID() == src.getID() || node.getID() == dest.getID())
                    continue;

                //horizontal tunnel
                float curr_x = node.getCellXCoord();
                float curr_y = node.getCellYCoord();

                if (curr_x >= src_X && curr_x < dest_X && curr_y >= src_Y && curr_y < dest_Y && node.getType() == MyNode.nodeType.Tunnel)
                    return true;
               else if (curr_x > src_X && curr_x <= dest_X && curr_y < src_Y && curr_y >= dest_Y && node.getType() == MyNode.nodeType.Tunnel)
                    return true;
                else if (curr_x <= src_X && curr_x > dest_X && curr_y >= src_Y && curr_y < dest_Y  && node.getType() == MyNode.nodeType.Tunnel)
                    return true;
                else if (curr_x < src_X && curr_x >= dest_X && curr_y < src_Y && curr_y >= dest_Y && node.getType() == MyNode.nodeType.Tunnel)
                    return true;

            }

            return false;
        }

        public ArrayList checkEdgeParameters(MyNode src, MyNode dest)
        {
            float deltaX = src.getCellXCoord() - dest.getCellXCoord();
            float deltaY = src.getCellYCoord() - dest.getCellYCoord();

            // no edge between the nodes
            int action = (int)Actions.NoEdge;
            int direction = -1;
            float distance = -1;

            // RIGHT
            if (deltaX < 0 && deltaY == 0)
            {
                direction = (int)Direction.Right;
                distance = Math.Abs(deltaX); //Euclidean distance
            }

            // RIGHT DOWN
            // diagonal edge forbidden for diamonds
            if (deltaX < 0 && deltaY < 0 && !(src.isDiamond() || dest.isDiamond()))
            {
                direction = (int)Direction.RightDown;
                distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)); //Euclidean distance
            }

            // DOWN
            if (deltaX == 0 && deltaY < 0)
            {
                direction = (int)Direction.Down;
                distance = Math.Abs(deltaY);
            }

            // LEFT DOWN
            // diagonal edge forbidden for diamonds
            if (deltaX > 0 && deltaY < 0 && !(src.isDiamond() || dest.isDiamond()))
            {
                direction = (int)Direction.LeftDown;
                distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)); //Euclidean distance
            }

            // LEFT
            if (deltaX > 0 && deltaY == 0)
            {
                direction = (int)Direction.Left;
                distance = deltaX;
            }

            // LEFT UP
            // diagonal edge forbidden for diamonds
            if (deltaX > 0 && deltaY > 0 && !(src.isDiamond() || dest.isDiamond()))
            {
                direction = (int)Direction.LeftUp;
                distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)); //Euclidean distance
            }

            // UP
            if (deltaX == 0 && deltaY > 0)
            {
                direction = (int)Direction.Up;
                distance = deltaY;
            }

            // RIGHT UP
            // diagonal edge forbidden for diamonds
            if (deltaX < 0 && deltaY > 0 && !(src.isDiamond() || dest.isDiamond()))
            {
                direction = (int)Direction.RightUp;
                distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)); //Euclidean distance
            }
            int a;
            // Choose action
            if (src.getCellID() == 1433 && dest.getCellID() == 117)
                a = 1;
            // DIAMOND ON AIR CAN ONLY GO TO HIS FALLDOWN POINT
            if (deltaX == 0 && src.getType() == MyNode.nodeType.Goal && dest.getType() == MyNode.nodeType.ToDiamond)
            {
                if (deltaY > 0 && !this.hasVerticalWall(dest.getCell(), src.getCell()))
                    action = (int)Actions.Small;
                else if (deltaY < 0 && !this.hasVerticalWall(src.getCell(), dest.getCell()))
                    action = (int)Actions.Small;
            }

            // GROW
            else if (dest.isDiamond() && direction == (int)Direction.Up && Math.Abs(Math.Abs(deltaY) - Utils.GRID_SIZE) <= Utils.BIG_RADIUS && Math.Abs(deltaY - Utils.GRID_SIZE) > Utils.SMALL_RADIUS && src.getType() != MyNode.nodeType.Goal)
                action = (int)Actions.Big;

            // TODO: Understand the missing 19 pixels

            // JUMP TO DIAMOND OR PLATFORM
            else if (deltaX == 0 && deltaY > Utils.SMALL_RADIUS && deltaY <= (2 * Utils.DIAMOND_TIP + 2 * Utils.SMALL_RADIUS + Utils.MAX_MORPH_UP + Utils.JUMP_HEIGHT) && src.getType() != MyNode.nodeType.Goal)
            {
                if (deltaY > 0 && !this.hasVerticalWall(dest.getCell(), src.getCell()))
                    action = (int)Actions.Jump;
                else if (deltaY < 0 && !this.hasVerticalWall(src.getCell(), dest.getCell()))
                    action = (int)Actions.Jump;
            }

            // NO SPECIAL ACTION - NORMAL RADIUS SIZE
            else if (deltaY <= Utils.GRID_SIZE && src.getType() != MyNode.nodeType.Goal) // circle can roll through 1 cell height platform
            {
                if (deltaX > 0 && !this.hasHorizontalWall(dest.getCell(), src.getCell()) && !hasTunnel(src, dest))
                    action = (int)Actions.Small;
                else if (deltaX < 0 && !this.hasHorizontalWall(src.getCell(), dest.getCell()) && !hasTunnel(src, dest))
                    action = (int)Actions.Small;

                if (deltaY > 0 && !this.hasVerticalWall(dest.getCell(), src.getCell()))
                    action = (int)Actions.Small;
                else if (deltaY < 0 && !this.hasVerticalWall(src.getCell(), dest.getCell()))
                    action = (int)Actions.Small;
            }

            ArrayList ret = new ArrayList();

            ret.Add(action);
            ret.Add(direction);
            ret.Add(distance);

            return ret;
        }

        //implements abstract circle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        /*WARNING: this method is called independently from the agent update - Update(TimeSpan elapsedGameTime) - so care should be taken when using complex 
         * structures that are modified in both (e.g. see operation on the "remaining" collection)      
         */
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            nCollectiblesLeft = nC;

            rectangleInfo = rI;
            circleInfo = cI;
            collectiblesInfo = colI;
            lock (remaining)
            {
                remaining = new List<CollectibleRepresentation>(collectiblesInfo);
            }

            DebugSensorsInfo();
            Log.LogInformation("<" + currentAction.ToString() + "> " + "Circle height - " + circleInfo.Y);
        }

        //implements abstract circle interface: provides the circle agent with a simulator to make predictions about the future level state
        public override void ActionSimulatorUpdated(ActionSimulator updatedSimulator)
        {
            predictor = updatedSimulator;
        }

        //implements abstract circle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return implementedAgent;
        }

        //implements abstract circle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return agentName;
        }

        //simple algorithm for choosing a random action for the circle agent
        private void RandomAction()
        {
            /*
             Circle Actions
             ROLL_LEFT = 1      
             ROLL_RIGHT = 2
             JUMP = 3
             GROW = 4
            */
            currentAction = possibleMoves[rnd.Next(possibleMoves.Count)];

            //send a message to the rectangle agent telling what action it chose
            messages.Add(new AgentMessage("Going to :" + currentAction));
        }

        //implements abstract circle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            return currentAction;
        }

        //implements abstract circle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {
            //Every second one new action is choosen
            if (lastMoveTime == 60)
                lastMoveTime = 0;

            if ((lastMoveTime) <= (DateTime.Now.Second) && (lastMoveTime < 60))
            {
                if (!(DateTime.Now.Second == 59))
                {
                    RandomAction();
                    lastMoveTime = lastMoveTime + 1;
                    DebugSensorsInfo();
                }
                else
                    lastMoveTime = 60;
            }

            //check if any collectible was caught
            lock (remaining)
            {
                if (remaining.Count > 0)
                {
                    List<CollectibleRepresentation> toRemove = new List<CollectibleRepresentation>();
                    foreach (CollectibleRepresentation item in uncaughtCollectibles)
                    {
                        if (!remaining.Contains(item))
                        {
                            caughtCollectibles.Add(item);
                            toRemove.Add(item);
                        }
                    }
                    foreach (CollectibleRepresentation item in toRemove)
                    {
                        uncaughtCollectibles.Remove(item);
                    }
                }
            }

            //predict what will happen to the agent given the current state and current action
            if (predictor != null) //predictions are only possible where the agents manager provided
            {
                /*
                 * 1) simulator can only be properly used when the Circle and Rectangle characters are ready, this must be ensured for smooth simulation
                 * 2) in this implementation we only wish to simulate a future state when whe have a fresh simulator instance, i.e. the generated debug information is empty
                */
                if (predictor.CharactersReady() && predictor.SimulationHistoryDebugInformation.Count == 0)
                {
                    List<CollectibleRepresentation> simCaughtCollectibles = new List<CollectibleRepresentation>();
                    //keep a local reference to the simulator so that it can be updated even whilst we are performing simulations
                    ActionSimulator toSim = predictor;

                    //prepare the desired debug information (to observe this information during the game press F1)
                    toSim.DebugInfo = true;
                    //you can also select the type of debug information generated by the simulator to circle only, rectangle only or both as it is set by default
                    //toSim.DebugInfoSelected = ActionSimulator.DebugInfoMode.Circle;

                    //setup the current circle action in the simulator
                    toSim.AddInstruction(currentAction);

                    //register collectibles that are caught during simulation
                    toSim.SimulatorCollectedEvent += delegate (Object o, CollectibleRepresentation col) { simCaughtCollectibles.Add(col); };

                    //simulate 2 seconds (predict what will happen 2 seconds ahead)
                    toSim.Update(2);

                    //prepare all the debug information to be passed to the agents manager
                    List<DebugInformation> newDebugInfo = new List<DebugInformation>();
                    //clear any previously passed debug information (information passed to the manager is cumulative unless cleared in this way)
                    newDebugInfo.Add(DebugInformationFactory.CreateClearDebugInfo());
                    //add all the simulator generated debug information about circle/rectangle predicted paths
                    newDebugInfo.AddRange(toSim.SimulationHistoryDebugInformation);
                    //create additional debug information to visualize collectibles that have been predicted to be caught by the simulator
                    foreach (CollectibleRepresentation item in simCaughtCollectibles)
                    {
                        newDebugInfo.Add(DebugInformationFactory.CreateCircleDebugInfo(new PointF(item.X - debugCircleSize / 2, item.Y - debugCircleSize / 2), debugCircleSize, GeometryFriends.XNAStub.Color.Red));
                        newDebugInfo.Add(DebugInformationFactory.CreateTextDebugInfo(new PointF(item.X, item.Y), "Predicted catch!", GeometryFriends.XNAStub.Color.White));
                    }
                    //create additional debug information to visualize collectibles that have already been caught by the agent
                    foreach (CollectibleRepresentation item in caughtCollectibles)
                    {
                        newDebugInfo.Add(DebugInformationFactory.CreateCircleDebugInfo(new PointF(item.X - debugCircleSize / 2, item.Y - debugCircleSize / 2), debugCircleSize, GeometryFriends.XNAStub.Color.GreenYellow));
                    }
                    //set all the debug information to be read by the agents manager


                    //CANCER
                    //create grid debug information 

                    Cell[,] gridMap = grid.getGridMap();

                    for (int i = 0; i < Utils.COL_CELLS; i++)
                    {
                        Console.WriteLine("[");
                        for (int j = 0; j < Utils.ROW_CELLS; j++)
                        {
                            Console.WriteLine(gridMap[i, j] + ", ");
                        }
                        Console.WriteLine("]\n");
                    }

                    for (int i = 0; i < Utils.COL_CELLS; i++)
                    {
                        for (int j = 0; j < Utils.ROW_CELLS; j++)
                        {
                            Cell c = gridMap[i, j];

                            if (c.isPlatform())
                            {
                                newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(c.getXCoord(), c.getYCoord()), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Green));
                            }

                            if (c.isBottom())
                            {
                                newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(c.getXCoord(), c.getYCoord()), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Yellow));
                            }

                            if (c.isTop())
                            {
                                newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(c.getXCoord(), c.getYCoord()), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Red));
                            }
                        }
                    }

                    int n = 1;

                    foreach (CollectibleRepresentation goal in this.collectiblesInfo)
                    {
                        newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(goal.X, goal.Y), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Blue));
                        newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(goal.X - Utils.GRID_SIZE, goal.Y), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Blue));
                        newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(goal.X, goal.Y - Utils.GRID_SIZE), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Blue));
                        newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(goal.X - Utils.GRID_SIZE, goal.Y - Utils.GRID_SIZE), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), GeometryFriends.XNAStub.Color.Blue));
                        newDebugInfo.Add(DebugInformationFactory.CreateTextDebugInfo(new PointF(goal.X, goal.Y), n.ToString(), GeometryFriends.XNAStub.Color.White));

                        n++;
                    }

                    // Print Graph nodes
                    int num_node = 0;

                    foreach (MyNode node in graph.getNodes())
                    {
                        float coord_x = grid.getCellByID(node.getCellID()).getXCoord();
                        float coord_y = grid.getCellByID(node.getCellID()).getYCoord();
                        GeometryFriends.XNAStub.Color color = GeometryFriends.XNAStub.Color.Red;

                        if (node.getType() == MyNode.nodeType.Platform)
                            color = GeometryFriends.XNAStub.Color.Green;
                        else if (node.getType() == MyNode.nodeType.Goal)
                            color = GeometryFriends.XNAStub.Color.Black;
                        else if (node.getType() == MyNode.nodeType.Start)
                            color = GeometryFriends.XNAStub.Color.HotPink;
                        else if (node.getType() == MyNode.nodeType.ToFall)
                            color = GeometryFriends.XNAStub.Color.Blue;
                        else if (node.getType() == MyNode.nodeType.FallDownPoint)
                            color = GeometryFriends.XNAStub.Color.MediumTurquoise;
                        else if (node.getType() == MyNode.nodeType.ToDiamond)
                            color = GeometryFriends.XNAStub.Color.White;
                        else if (node.getType() == MyNode.nodeType.Tunnel)
                            color = GeometryFriends.XNAStub.Color.Aquamarine;

                        newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(coord_x, coord_y), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), color));
                        newDebugInfo.Add(DebugInformationFactory.CreateTextDebugInfo(new PointF(coord_x, coord_y), node.getCellID().ToString(), GeometryFriends.XNAStub.Color.DarkOrange));

                        num_node++;
                    }

                    StreamWriter sw = new StreamWriter("lindodemorrer.csv");

                    for (int i = 0; i < Utils.ROW_CELLS; i++)
                    {
                        for (int j = 0; j < Utils.COL_CELLS; j++)
                        {
                            //Console.Write(random.Next(1, 49));
                            //sw.Write(this.grid.getGridMap()[j, i].isDiamond() + ";");
                            if(this.grid.getGridMap()[j, i].isPlatform())
                                sw.Write("*;");
                            else
                                sw.Write("-;");
                        }
                        sw.WriteLine();
                    }

                    sw.Close();

                    StreamWriter sw_actions = new StreamWriter("actions.csv");
                    StreamWriter sw_directions = new StreamWriter("directions.csv");
                    StreamWriter sw_distances = new StreamWriter("distances.csv");

                    for (int i = 0; i < this.graph.getNodes().Count; i++)
                    {
                        for (int j = 0; j < this.graph.getNodes().Count; j++)
                        {
                            sw_actions.Write(this.adjacencyMatrix_actions[i, j] + ";");
                            sw_directions.Write(this.adjacencyMatrix_directions[i, j] + ";");
                            sw_distances.Write(this.adjacencyMatrix_distances[i, j] + ";");
                        }
                        sw_actions.WriteLine();
                        sw_directions.WriteLine();
                        sw_distances.WriteLine();
                    }

                    sw_actions.Close();
                    sw_directions.Close();
                    sw_distances.Close();

                    debugInfo = newDebugInfo.ToArray();
                }
            }
        }

        //typically used console debugging used in previous implementations of GeometryFriends
        protected void DebugSensorsInfo()
        {
            Log.LogInformation("Circle Agent - " + numbersInfo.ToString());

            Log.LogInformation("Circle Agent - " + rectangleInfo.ToString());

            Log.LogInformation("Circle Agent - " + circleInfo.ToString());

            foreach (ObstacleRepresentation i in obstaclesInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Obstacle"));
            }

            foreach (ObstacleRepresentation i in rectanglePlatformsInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Rectangle Platform"));
            }

            foreach (ObstacleRepresentation i in circlePlatformsInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString("Circle Platform"));
            }

            foreach (CollectibleRepresentation i in collectiblesInfo)
            {
                Log.LogInformation("Circle Agent - " + i.ToString());
            }
        }

        //implements abstract circle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Log.LogInformation("CIRCLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
        }

        //implements abstract circle interface: gets the debug information that is to be visually represented by the agents manager
        public override DebugInformation[] GetDebugInformation()
        {
            return debugInfo;
        }

        //implememts abstract agent interface: send messages to the rectangle agent
        public override List<GeometryFriends.AI.Communication.AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the rectangle agent
        public override void HandleAgentMessages(List<GeometryFriends.AI.Communication.AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                Log.LogInformation("Circle: received message from rectangle: " + item.Message);
                if (item.Attachment != null)
                {
                    Log.LogInformation("Received message has attachment: " + item.Attachment.ToString());
                    if (item.Attachment.GetType() == typeof(Pen))
                    {
                        Log.LogInformation("The attachment is a pen, let's see its color: " + ((Pen)item.Attachment).Color.ToString());
                    }
                }
            }
        }
    }
}

