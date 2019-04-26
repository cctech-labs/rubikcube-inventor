/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RubikPlugin.Commands
{
    class RandomizeCmd
    {
        // layer - 0,1,2 starting from +ve axis
        // direction - x-dir = 0, y-dir = 1, z-dir = 2
        // clockwise -0, anticlockwisw = 1
        public delegate void ScrambleEvent(int layer, int direction, int clockwise);
        public delegate void Invalidate();

        private int[] Configuration = new int[]
        {
            0,0,1,0,0,-1,
            1,0,1,1,0,-1,
            2,0,1,2,0,-1,

            0,1,1,0,1,-1,
            1,1,1,1,1,-1,
            2,1,1,2,1,-1,

            0,2,1,0,2,-1,
            1,2,1,1,2,-1,
            2,2,1,2,2,-1,
        };

        public RandomizeCmd(Inventor.Application inInventorApp)
        {
            mInventorApp = inInventorApp;
        }
        public void OnClick()
        {
            
            // out of 18 rotations, it can have any rotation 
            var currentScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task task = Task.Factory.StartNew(() =>
            {
                if (null != OnInvalidateEvent)
                    OnInvalidateEvent();

                if (null != OnScrambleStart)
                    OnScrambleStart();

                System.Threading.Thread.Sleep(500);
            })
            .ContinueWith(__ =>
            {
                Running = true;
                Random rnd = new Random();
                int min = 0, max = 17;
                for (int i = 0; i < 20; i++)
                {
                    if (Running)
                    {
                        int num = rnd.Next(min, max);

                        int layer = Configuration[3 * num + 0];
                        int direction = Configuration[3 * num + 1];
                        int clockwise = Configuration[3 * num + 2];

                        if (null != OnScrambleEvent)
                            OnScrambleEvent(layer, direction, clockwise);
                    }
                }
            }, currentScheduler).ContinueWith(_ =>
            {
                if (null != OnScrambleStart)
                    OnScrambleEnd();
            });
        }
        public void Stop()
        {
            Running = false;
        }

        public event ScrambleEvent OnScrambleEvent;
        public event Invalidate OnInvalidateEvent;

        public event Invalidate OnScrambleStart;
        public event Invalidate OnScrambleEnd;

        private bool Running;
        Inventor.Application mInventorApp;
    }
}
