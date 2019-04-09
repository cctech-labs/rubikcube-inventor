using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;

namespace RubikPlugin.Commands
{
    class NextCmd
    {
        public delegate void NextEvents();

        public NextCmd(Application inInventorApp)
        {
            mInventorApp = inInventorApp;
        }

        public void OnClick()
        {
            if()
            // Stop the timer
        }

        private event NextEvents PauseTimerEvent;
        private event NextEvents PlayNextMoveEvent;

        Inventor.Application mInventorApp;
    }
}
