using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.ActionSimulation;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
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

            graph = new Graph();
            SetupGraph();

            //send a message to the rectangle informing that the circle setup is complete and show how to pass an attachment: a pen object
            messages.Add(new AgentMessage("Setup complete, testing to send an object as an attachment.", new Pen(Color.AliceBlue)));

            DebugSensorsInfo();
        }

        public void SetupGrid()
        {
            this.grid.setCellObstacle(rectanglePlatformsInfo);
            this.grid.setCellObstacle(obstaclesInfo);
            //NOTE: Implement setCellObstacle before calcHeuristicValues
            this.grid.calcHeuristicValues(collectiblesInfo[0].X, collectiblesInfo[0].Y);
        }

        public void graphNodeObstacles(ObstacleRepresentation obstacle)
        {
            int id;

            //Obstacle CENTER position
            float obs_x = obstacle.X;
            float obs_y = obstacle.Y;

            //Obstacle P1 - upper left
            float x1 = obs_x - ((float)obstacle.Width / 2);
            float y1 = obs_y - ((float)obstacle.Height / 2);

            //Obstacle P2 - upper right
            float x2 = obs_x + ((float)obstacle.Width / 2);
            float y2 = obs_y - ((float)obstacle.Height / 2);

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
            if (c1_upper != null && c1_upper_left == null && !c1_upper.isPlatform())
            {
                id = c1_upper.getID();
                this.graph.addNode(new MyNode(id, MyNode.nodeType.Platform));
            }

            //Platform with falldown on the left side
            if (c1_upper != null && c1_upper_left != null && !c1_upper.isPlatform() && !c1_upper_left.isPlatform())
            {
                id = c1_upper_left.getID();
                this.graph.addNode(new MyNode(id, MyNode.nodeType.ToFall));
            }

            //Platform in the middle or with wall on the right side
            if (c1_upper != null && c1_upper_left != null && !c1_upper.isPlatform() && c1_upper_left.isPlatform())
            {
                id = c1_upper.getID();
                this.graph.addNode(new MyNode(id, MyNode.nodeType.Platform));
            }

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
                {
                    id = c1_translate.getID();
                    this.graph.addNode(new MyNode(id, MyNode.nodeType.Platform));
                }
            }

            //Add second node

            //Falldown right
            if (c2_upper != null && c2_upper_left != null && !c2_upper.isPlatform() && !c2_upper_left.isPlatform())
            {
                id = c2_upper.getID();
                this.graph.addNode(new MyNode(id, MyNode.nodeType.ToFall));
            }

            //Has right platform
            if (c2_upper != null && c2_upper_left != null && c2_upper.isPlatform() && !c2_upper_left.isPlatform())
            {
                id = c2_upper_left.getID();
                this.graph.addNode(new MyNode(id, MyNode.nodeType.Platform));
            }

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
                {
                    id = c2_translate.getID();
                    this.graph.addNode(new MyNode(id, MyNode.nodeType.Platform));
                }
            }
        }

        public void SetupGraph()
        {

            int id;

            foreach (ObstacleRepresentation obstacle in obstaclesInfo)
                this.graphNodeObstacles(obstacle);

            foreach (ObstacleRepresentation obstacle in rectanglePlatformsInfo)
                this.graphNodeObstacles(obstacle);


            foreach (CollectibleRepresentation diamond in collectiblesInfo)
            {
                Cell c = this.grid.getCellByCoords(diamond.X, diamond.Y);
                id = c.getID();
                this.graph.addNode(new MyNode(id, MyNode.nodeType.Goal));
            }

            id = grid.getCellByCoords(circleInfo.X, circleInfo.Y).getID();

            this.graph.addNode(new MyNode(id, MyNode.nodeType.Start));

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

                    foreach (MyNode node in graph.getNodes())
                    {
                        float coord_x = grid.getCellByID(node.getCellID()).getXCoord();
                        float coord_y = grid.getCellByID(node.getCellID()).getYCoord();
                        GeometryFriends.XNAStub.Color color = GeometryFriends.XNAStub.Color.Red;

                        if (node.getType() == MyNode.nodeType.Platform)
                            color = GeometryFriends.XNAStub.Color.HotPink;
                        else if (node.getType() == MyNode.nodeType.Goal)
                            color = GeometryFriends.XNAStub.Color.Red;
                        else if (node.getType() == MyNode.nodeType.Start)
                            color = GeometryFriends.XNAStub.Color.Khaki;
                        else if (node.getType() == MyNode.nodeType.ToFall)
                            color = GeometryFriends.XNAStub.Color.LightBlue;
                        else if(node.getType() == MyNode.nodeType.FallDownPoint)
                            color = GeometryFriends.XNAStub.Color.Chocolate;
                        else if (node.getType() == MyNode.nodeType.ToDiamond)
                            color = GeometryFriends.XNAStub.Color.Purple;

                        newDebugInfo.Add(DebugInformationFactory.CreateRectangleDebugInfo(new PointF(coord_x, coord_y), new Size(Utils.GRID_SIZE, Utils.GRID_SIZE), color));
                    }

                    StreamWriter sw = new StreamWriter("lindodemorrer.csv");

                    for (int i = 0; i < Utils.ROW_CELLS; i++)
                    {
                        for (int j = 0; j < Utils.COL_CELLS; j++)
                        {
                            //Console.Write(random.Next(1, 49));
                            sw.Write(this.grid.getGridMap()[j, i].getHeuristic() + ";");
                        }
                        sw.WriteLine();
                    }

                    sw.Close();

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

