using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriendsAgents
{
    class Utils
    {
        public const int WIDTH = 1200;
        public const int HEIGHT = 720 + 2*GRID_SIZE; // To take into consideration the floor and the ceiling
        public const int GRID_SIZE = 16;

        public const int COL_CELLS = WIDTH/GRID_SIZE;
        public const int ROW_CELLS = HEIGHT/GRID_SIZE;

        public const int TRANSLATION_X = 40;
        public const int TRANSLATION_Y = 40 - GRID_SIZE; // To take into consideration the floor and the ceiling

        public const int TRESHOLD_DIAMOND = 80;

        public const int SMALL_RADIUS = 40;
        public const int BIG_RADIUS = 60;
        public const int JUMP_HEIGHT = 276;

        public const int MAX_MORPH_UP = 200;
        // public const int MAX_MORPH_DOWN = ???; /TODO

        public const int INFINITY = 5000;
    }
}
