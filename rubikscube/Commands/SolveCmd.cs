/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;


namespace RubikPlugin.Commands
{
    class SolveCmd
    {
        public delegate void OrientEvent(int layer, int direction, int clockwise);
        public delegate void OnSimulation();

        public SolveCmd(Application inInventorApp)
        {
            mInventorApp = inInventorApp;
        }

        public void OnClick()
        {
            if (null == mCubeSolver)
            {
                Solve();

                if (mMoves.Count == 0)
                    return;
                string msg;
                if (mMoves.Count == 1)
                    msg = "Rubik's cube solved with " + mMoves.Count.ToString() + " move. \nDo you want to proceed with simulation.";
                else
                    msg = "Rubik's cube solved with " + mMoves.Count.ToString() + " moves. \nDo you want to proceed with simulation.";
                var dlgResult = System.Windows.Forms.MessageBox.Show(msg, "Rubik's Cube", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);
                if (dlgResult == System.Windows.Forms.DialogResult.Yes)
                {
                    if (null != OnStartSimulation)
                        OnStartSimulation();

                }
                else if (dlgResult == System.Windows.Forms.DialogResult.No)
                {
                    if (OnEndSimulation != null)
                        OnEndSimulation();
                    return;
                }
            }
            else
            {
                if (null != OnStartSimulation)
                    OnStartSimulation();
            }
        }

        public void AddMove(int layer, int direction, int clockwise)
        {
            mInputMoves.Add(new int[] { layer, direction, clockwise });
        }

        public void PlayNext()
        {
            if (IsDirty == true)
            {
                Solve();

                if (mMoves.Count == 0)
                {
                    if (OnEndSimulation != null)
                        OnEndSimulation();
                    return;
                }
                string msg;
                if (mMoves.Count == 1)
                    msg = "Rubik's cube solved with " + mMoves.Count.ToString() + " move." + "\nClick Yes to proceed, click No to stop simulation";
                else
                    msg = "Rubik's cube solved with " + mMoves.Count.ToString() + " moves." + "\nClick Yes to proceed, click No to stop simulation";

                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(msg, "Rubik's Cube", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);
                if (dr == System.Windows.Forms.DialogResult.No)
                {
                    if (OnEndSimulation != null)
                        OnEndSimulation();
                    return;
                }
            }
            //else if (OnEndSimulation != null)
            //    OnEndSimulation();

            if (mStep >= 0 && mStep < mMoves.Count)
            {
                mInputMoves.Add(mMoves[mStep]);
                int[] move = mMoves[mStep];
                if (null != OnOrientEvent)
                    OnOrientEvent(move[0], move[1], move[2]);

                if (mStep == mMoves.Count - 1)
                {
                    mInputMoves.Clear();
                    if (OnEndSimulation != null)
                        OnEndSimulation();
                }
                mStep++;
            }
        }

        public void Invalidate()
        {
            IsDirty = true;
        }

        private void Solve()
        {
            mCubeSolver = new RubikSolver.Solver();
            mCubeSolver.AddMoves(mInputMoves);
            mMoves = mCubeSolver.Solve();
            mStep = 0;
            IsDirty = false;
        }

        public event OrientEvent OnOrientEvent;
        public event OnSimulation OnStartSimulation;
        public event OnSimulation OnEndSimulation;
        public bool Dirty { get { return IsDirty; } }


        private bool IsDirty = false;
        private Application mInventorApp;
        private RubikSolver.Solver mCubeSolver;
        private List<int[]> mMoves = new List<int[]>();
        private List<int[]> mInputMoves = new List<int[]>();

        private int mStep = 0;
        //private RubikManager mRubikManager = new RubikManager();
    }
}
