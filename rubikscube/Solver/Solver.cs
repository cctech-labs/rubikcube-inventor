using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RubikSolver
{
    class Solver
    {
        public Solver()
        {

        }
        public void AddMoves(List<int[]> moves)
        {
            foreach (int[] move in moves)
            {
                var tuple = GetRubikPosition(move[0], move[1], move[2]);
                mRubikManager.Rotate90Sync(tuple.Item1, tuple.Item2);
            }
        }
        public List<int[]> Solve()
        {
            List<int[]> solutionMoves = new List<int[]>();
            CubeSolver cs = new CubeSolver(mRubikManager);
            if (cs.CanSolve())
            {
                mRubikManager = cs.ReturnRubik().Clone();

                foreach (LayerMove move in mRubikManager.Moves)
                    solutionMoves.Add(FromRubikPosition(move));
            }
            return solutionMoves;
        }
        private Tuple<Cube3D.RubikPosition, bool> GetRubikPosition(int layer, int direction, int clockwise)
        {
            Cube3D.RubikPosition pos = Cube3D.RubikPosition.None;
            bool orientation = false;
            if (direction == 0 && layer == 0)
            {
                pos = Cube3D.RubikPosition.LeftSlice;
                orientation = clockwise > 0;
            }
            else if (direction == 0 && layer == 1)
            {
                pos = Cube3D.RubikPosition.MiddleSlice_Sides;
                orientation = clockwise < 0;
            }
            else if (direction == 0 && layer == 2)
            {
                pos = Cube3D.RubikPosition.RightSlice;
                orientation = clockwise < 0;
            }
            else if (direction == 1 && layer == 0)
            {
                pos = Cube3D.RubikPosition.TopLayer;
                orientation = clockwise > 0;
            }
            else if (direction == 1 && layer == 1)
            {
                pos = Cube3D.RubikPosition.MiddleLayer;
                orientation = clockwise < 0;
            }
            else if (direction == 1 && layer == 2)
            {
                pos = Cube3D.RubikPosition.BottomLayer;
                orientation = clockwise < 0;
            }
            else if (direction == 2 && layer == 0)
            {
                pos = Cube3D.RubikPosition.FrontSlice;
                orientation = clockwise > 0;
            }
            else if (direction == 2 && layer == 1)
            {
                pos = Cube3D.RubikPosition.MiddleSlice;
                orientation = clockwise < 0;
            }
            else if (direction == 2 && layer == 2)
            {
                pos = Cube3D.RubikPosition.BackSlice;
                orientation = clockwise < 0;
            }
            return new Tuple<Cube3D.RubikPosition, bool>(pos, orientation);
        }
        private int[] FromRubikPosition(LayerMove layerMove)
        {
            int layer = 0, direction = 0, clockwise = 0;
            if (layerMove.Layer == Cube3D.RubikPosition.LeftSlice)
            {
                layer = 0;
                direction = 0;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.MiddleSlice_Sides)
            {
                layer = 1;
                direction = 0;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.RightSlice)
            {
                layer = 2;
                direction = 0;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.TopLayer)
            {
                layer = 0;
                direction = 1;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.MiddleLayer)
            {
                layer = 1;
                direction = 1;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.BottomLayer)
            {
                layer = 2;
                direction = 1;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.FrontSlice)
            {
                layer = 0;
                direction = 2;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.MiddleSlice)
            {
                layer = 1;
                direction = 2;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            else if (layerMove.Layer == Cube3D.RubikPosition.BackSlice)
            {
                layer = 2;
                direction = 2;
                clockwise = layerMove.Direction ? -1 : 1;
            }
            return new int[] { layer, direction, clockwise };
        }

        private RubikManager mRubikManager = new RubikManager();
    }
}
