using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;
using System.Threading.Tasks;
using System.Windows.Forms;
using InvAddIn.Properties;

namespace RubikPlugin
{
    class RubikCubePlugin
    {
        public RubikCubePlugin(Inventor.Application inInventorApp)
        {
            mInventorApp = inInventorApp;
            Initiaze();
        }

        private void Initiaze()
        {
            mRibbon = new RibbonUI(mInventorApp);
            mGraphics = new GraphicsUI(mInventorApp);

            mRibbon.FinishClickEvent += new RibbonUI.RibbonButtonClickEvent(OnFinishClick);
            mRibbon.ActivateClickEvent += new RibbonUI.RibbonButtonClickEvent(OnActivateClick);
            mRibbon.OnNewDocumentUnwireEvent += new EventHandler(OnNewDocumentUnWire);
            mRibbon.OnNewDocumentWireEvent += new EventHandler(OnNewDocumentWire);
            InitTimer();
        }

        private void WireConnections()
        {
            //undo redo events
            mInventorApp.TransactionManager.TransactionEvents.OnUndo += new TransactionEventsSink_OnUndoEventHandler(OnUndo);
            mInventorApp.TransactionManager.TransactionEvents.OnRedo += new TransactionEventsSink_OnRedoEventHandler(OnRedo);

            // ribbon events
            mRibbon.PlayClickEvent += new RibbonUI.RibbonButtonClickEvent(mSolveCmd.OnClick);
            mRibbon.RandomizeClickEvent += new RibbonUI.RibbonButtonClickEvent(mScrambleCmd.OnClick);
            mRibbon.SolveClickEvent += new RibbonUI.RibbonButtonClickEvent(mPlayCmd.OnClick);

            mRibbon.NextClickEvent += new RibbonUI.RibbonButtonClickEvent(OnNextMoveClick);
            mSolveCmd.OnStartSimulation += new Commands.SolveCmd.OnSimulation(OnStartSimulation);
            mSolveCmd.OnEndSimulation += new Commands.SolveCmd.OnSimulation(OnEndSimulation);

            mScrambleCmd.OnScrambleStart += new Commands.RandomizeCmd.Invalidate(OnScrambleStart);
            mScrambleCmd.OnScrambleEnd += new Commands.RandomizeCmd.Invalidate(OnScrambleEnd);

            mPlayCmd.OnInvalidateEvent += new Commands.PlayCmd.Invalidate(OnResultInvalidate);
            mScrambleCmd.OnInvalidateEvent += new Commands.RandomizeCmd.Invalidate(OnResultInvalidate);

            mScrambleCmd.OnScrambleEvent += new Commands.RandomizeCmd.ScrambleEvent(OnScramble);
            mPlayCmd.OnPlayEvent += new Commands.PlayCmd.PlayEvent(OnPlay);
            mSolveCmd.OnOrientEvent += new Commands.SolveCmd.OrientEvent(OnOrient);
        }

        void OnRedo(Transaction TransactionObject, NameValueMap Context, EventTimingEnum BeforeOrAfter, out HandlingCodeEnum HandlingCode)
        {
            HandlingCode = HandlingCodeEnum.kEventCanceled;
        }

        void OnUndo(Transaction TransactionObject, NameValueMap Context, EventTimingEnum BeforeOrAfter, out HandlingCodeEnum HandlingCode)
        {
            HandlingCode = HandlingCodeEnum.kEventCanceled;
        }

        public void OnNewDocumentUnWire(object sender, EventArgs e)
        {
            mRibbon.FinishClickEvent -= new RibbonUI.RibbonButtonClickEvent(OnFinishClick);
            mRibbon.ActivateClickEvent -= new RibbonUI.RibbonButtonClickEvent(OnActivateClick);
        }
        public void OnNewDocumentWire(object sender, EventArgs e)
        {
            mRibbon.FinishClickEvent += new RibbonUI.RibbonButtonClickEvent(OnFinishClick);
            mRibbon.ActivateClickEvent += new RibbonUI.RibbonButtonClickEvent(OnActivateClick);
        }
        private void UnWireConnections()
        {
            try
            {
                //finish event
                //mRibbon.FinishClickEvent -= new RibbonUI.RibbonButtonClickEvent(OnFinishClick);
                //mRibbon.ActivateClickEvent -= new RibbonUI.RibbonButtonClickEvent(OnActivateClick);
                // ribbon events
                if (isActivatedClicked)
                {
                    mRibbon.PlayClickEvent -= new RibbonUI.RibbonButtonClickEvent(mSolveCmd.OnClick);
                    mRibbon.RandomizeClickEvent -= new RibbonUI.RibbonButtonClickEvent(mScrambleCmd.OnClick);
                    mRibbon.SolveClickEvent -= new RibbonUI.RibbonButtonClickEvent(mPlayCmd.OnClick);

                    mRibbon.NextClickEvent -= new RibbonUI.RibbonButtonClickEvent(OnNextMoveClick);
                    mSolveCmd.OnStartSimulation -= new Commands.SolveCmd.OnSimulation(OnStartSimulation);
                    mSolveCmd.OnEndSimulation -= new Commands.SolveCmd.OnSimulation(OnEndSimulation);
                    //mPlayCmd.OnInvalidateEvent -= new Commands.PlayCmd.Invalidate(OnResultInvalidate);
                    //mScrambleCmd.OnInvalidateEvent -= new Commands.RandomizeCmd.Invalidate(OnResultInvalidate);

                    mScrambleCmd.OnScrambleEvent -= new Commands.RandomizeCmd.ScrambleEvent(OnScramble);
                    mPlayCmd.OnPlayEvent -= new Commands.PlayCmd.PlayEvent(OnPlay);
                    mSolveCmd.OnOrientEvent -= new Commands.SolveCmd.OrientEvent(OnOrient);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.InnerException.StackTrace); }
        }

        void OnResultInvalidate()
        {
            if (null != mTimer)
                mTimer.Stop();

            if (null != mSolveCmd)
                mSolveCmd.Invalidate();
        }

        void OnOrient(int layer, int direction, int clockwise)
        {
            if (null != mGraphics)
                mGraphics.Orient(layer, direction, clockwise);
        }

        void OnPlay(int layer, int direction, int clockwise)
        {
            if (null != mGraphics)
            {
                //mPlayCmd.EnableInteration(false);
                mGraphics.Orient(layer, direction, clockwise);
                //mPlayCmd.EnableInteration(true);
            }
            if (null != mSolveCmd)
            {
                mSolveCmd.AddMove(layer, direction, clockwise);
                mSolveCmd.Invalidate();
            }
        }

        void OnScramble(int layer, int direction, int clockwise)
        {
            if (null != mGraphics)
                mGraphics.Orient(layer, direction, clockwise);
            if (null != mSolveCmd)
            {
                mSolveCmd.AddMove(layer, direction, clockwise);
                mSolveCmd.Invalidate();
            }
        }

        void OnScrambleStart()
        {
            mPlayCmd.EnableInteration(false);
            mRibbon.Scramble.Enabled = false;
            mRibbon.Play.Enabled = false;
            mRibbon.Solve.Enabled = false;
            mRibbon.Next.Enabled = false;
        }

        void OnScrambleEnd()
        {
            mRibbon.Scramble.Enabled = true;
            mRibbon.Play.Enabled = true;
            mRibbon.Solve.Enabled = true;
            mRibbon.Next.Enabled = true;
        }

        public void UnloadPlugin()
        {
            try
            {
                OnFinishClick();
                mRibbon.UnloadRibbon();
                if (mInventorApp.ActiveEnvironment.DisplayName == Resources.IDC_TAB_DISPLAY_NAME)
                {
                    if (mInventorApp.ActiveDocument.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                    {
                        AssemblyDocument asmDoc = (AssemblyDocument)mInventorApp.ActiveDocument;
                        Inventor.Environment asmEnv = asmDoc.EnvironmentManager.BaseEnvironment;
                        asmDoc.EnvironmentManager.SetCurrentEnvironment(asmEnv);
                    }
                    if (mInventorApp.ActiveDocument.DocumentType == DocumentTypeEnum.kPartDocumentObject)
                    {
                        PartDocument partDoc = (PartDocument)mInventorApp.ActiveDocument;
                        Inventor.Environment partEnv = partDoc.EnvironmentManager.BaseEnvironment;
                        partDoc.EnvironmentManager.SetCurrentEnvironment(partEnv);
                    }
                    if (mInventorApp.ActiveDocument.DocumentType == DocumentTypeEnum.kDrawingDocumentObject)
                    {
                        DrawingDocument drwDoc = (DrawingDocument)mInventorApp.ActiveDocument;
                        Inventor.Environment drwEnv = drwDoc.EnvironmentManager.BaseEnvironment;
                        drwDoc.EnvironmentManager.SetCurrentEnvironment(drwEnv);
                    }
                }

                if (null != mRibbon.mRubiksPartTab)
                {
                    mRibbon.mRubiksPartTab.Delete();
                    mRibbon.mRubiksPartTab = null;
                }

                if (null != mRibbon.mEnvironment)
                {
                    mRibbon.mEnvironment.Delete();
                    mRibbon.mEnvironment = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.StackTrace);
            }
        }

        void OnNextMoveClick()
        {
            if (null != mTimer)
            {
                mTimer.Stop();
                mRibbon.Solve.Enabled = true;
                mRibbon.Scramble.Enabled = true;
            }

            if (null != mGraphics && mGraphics.IsActive == false)
            {
                if (null != mSolveCmd)
                    mSolveCmd.PlayNext();
            }
        }

        void OnStartSimulation()
        {
            if (null != mTimer)
            {
                Task task = Task.Factory.StartNew(() =>
                {
                    mRibbon.Solve.Enabled = false;
                    mRibbon.Scramble.Enabled = false;
                    System.Threading.Thread.Sleep(500);

                }).ContinueWith(__ =>
                {
                    mTimer.Tick += OnTick;
                    mTimer.Start();
                    mTimer.IsEnabled = true;
                });
            }
        }

        void OnEndSimulation()
        {
            if (null != mTimer)
            {
                mTimer.Stop();
                mRibbon.Solve.Enabled = true;
                mRibbon.Scramble.Enabled = true;
            }
        }

        private void OnFinishClick()
        {
            if (null != mTimer)
                mTimer.Stop();
            UnWireConnections();
            if (mScrambleCmd != null)
                mScrambleCmd.Stop();
            mGraphics.Stop();
        }
        private void OnActivateClick()
        {
            isActivatedClicked = true;
            mPlayCmd = new Commands.PlayCmd(mInventorApp);
            mScrambleCmd = new Commands.RandomizeCmd(mInventorApp);
            mSolveCmd = new Commands.SolveCmd(mInventorApp);

            WireConnections();
            mGraphics.Start();
        }

        private void InitTimer()
        {
            mTimer = new System.Windows.Threading.DispatcherTimer();
            mTimer.Tick += OnTick;
            mTimer.Interval = new TimeSpan(TimeSpan.TicksPerMillisecond * 5);
        }

        private void OnTick(object sender, EventArgs evntArgs)
        {
            if (null != mSolveCmd)
                mSolveCmd.PlayNext();
        }

        private Inventor.Application mInventorApp;
        private System.Windows.Threading.DispatcherTimer mTimer;

        internal RibbonUI mRibbon;
        private bool isActivatedClicked;
        private GraphicsUI mGraphics;
        private Commands.PlayCmd mPlayCmd;
        private Commands.RandomizeCmd mScrambleCmd;
        private Commands.SolveCmd mSolveCmd;

    }
}
