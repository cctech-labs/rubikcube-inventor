using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Inventor;
using InvAddIn.Properties;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RubikPlugin
{
    class RibbonUI
    {
        public delegate void RibbonButtonClickEvent();

        internal class AxHostConverter : AxHost
        {
            private AxHostConverter()
                : base("")
            {
            }

            public static stdole.IPictureDisp ImageToPictureDisp(Image image)
            {
                return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
            }

            public static Image PictureDispToImage(stdole.IPictureDisp pictureDisp)
            {
                return GetPictureFromIPicture(pictureDisp);
            }
        }

        #region public methods

        public RibbonUI(Inventor.Application inInventorApp)
        {
            mInventorApp = inInventorApp;
            CreatePanels();
            CreateRibbon();
        }

        public void EnableScramble(bool valid)
        {
            if (null != mScramble)
                mScramble.Enabled = valid;
            if (null != mSolve)
                mSolve.Enabled = valid;
            if (null != mPlay)
                mPlay.Enabled = valid;
            if (null != mNext)
                mNext.Enabled = valid;

            mInventorApp.ActiveView.Update();
        }

        #endregion

        #region private methods

        private void CreatePanels()
        {
            CreateSolvepanel();
            CreateRandomizePanel();
            CreatePlayPanel();
            CreateNextPanel();
            CreateApplicationBtn();
        }

        private void CreateRibbon()
        {
            try
            {

                Inventor.UserInterfaceManager userInterFaceMgr = mInventorApp.UserInterfaceManager;
                userInterFaceMgr.UserInterfaceEvents.OnEnvironmentChange += new UserInterfaceEventsSink_OnEnvironmentChangeEventHandler(OnEnvironmentChange);

                //retrieve the GUID for this class
                GuidAttribute addInCLSID = (GuidAttribute)GuidAttribute.GetCustomAttribute(typeof(RubiksCube.RubiksAddInServer), typeof(GuidAttribute));
                string clientId = "{" + addInCLSID.Value + "}";

                Icon appLarge = Resources.Rubiks_Cube_32;
                object applargeIcon = AxHostConverter.ImageToPictureDisp(appLarge.ToBitmap());
                Icon appStand = Resources.Rubiks_Cube_16;
                object appStandIcon = AxHostConverter.ImageToPictureDisp(appStand.ToBitmap());
                Environments envs = userInterFaceMgr.Environments;
                mEnvironment = envs.Add(Resources.IDC_ENV_DISPLAY_NAME, Resources.IDC_ENV_INTERNAL_NAME, null, appStandIcon, applargeIcon);
                mEnvironment.AdditionalVisibleRibbonTabs = new string[] { Resources.IDC_ENV_INTERNAL_NAME };
               
                mObjCollection = mInventorApp.TransientObjects.CreateObjectCollection();

                //get the ribbon associated with part document
                Inventor.Ribbons ribbons = userInterFaceMgr.Ribbons;
                mPartRibbon = ribbons[Resources.IDC_ENV_PART];

                //get the tabs associated with part ribbon
                Inventor.RibbonTabs rubikRibbonTabs = mPartRibbon.RibbonTabs;
                mRubiksPartTab = rubikRibbonTabs.Add(Resources.IDC_TAB_DISPLAY_NAME, Resources.IDC_TAB_INTERNAL_NAME, "F0911DF2-478B-49EC-808D-D7C1F5271B6D", Resources.IDC_TARGET_TAB_NAME, true, true);
                
                //Adding solve Panel.
                RibbonPanel solvePanel = mRubiksPartTab.RibbonPanels.Add(Resources.IDC_SOLVE_DISPLAY_NAME, Resources.IDC_SOLVE_INTERNAL_NAME, "60A50C33-F7EE-4B74-BCB0-C5CE03C1B3E6");
                Inventor.CommandControl solveControl = solvePanel.CommandControls.AddButton(mSolve, true, true);

                //Adding randomize Panel.
                RibbonPanel scramblePanel = mRubiksPartTab.RibbonPanels.Add(Resources.IDC_SCRAMBLE_DISPLAY_NAME, Resources.IDC_SCRAMBLE_INTERNAL_NAME, "D20674CE-A855-4403-850B-FDE59C4A167B");
                Inventor.CommandControl scrambleControl = scramblePanel.CommandControls.AddButton(mScramble, true, true);

                //Adding randomize Panel.
                RibbonPanel playPanel = mRubiksPartTab.RibbonPanels.Add(Resources.IDC_PLAY_DISPLAY_NAME, Resources.IDC_PLAY_INTERNAL_NAME, "343D703C-1194-4715-BF54-3BE4E3B9FF64");
                //Inventor.CommandControl playControl = playPanel.CommandControls.AddButton(mPlay, true, true, "", false);

                mObjCollection.Add(mPlay);
                mObjCollection.Add(mNext);
                CommandControl partCmdCtrl = playPanel.CommandControls.AddSplitButtonMRU(mObjCollection, true);

                mEnvironment.DefaultRibbonTab = Resources.IDC_TAB_INTERNAL_NAME;
                userInterFaceMgr.ParallelEnvironments.Add(mEnvironment);
                mEnvCtrlDef = mInventorApp.CommandManager.ControlDefinitions[Resources.IDC_ENV_INTERNAL_NAME];
                mEnvCtrlDef.ProgressiveToolTip.ExpandedDescription = Resources.IDC_APPLICATION_BTN_TOOLTIP_EXPANDED;
                mEnvCtrlDef.ProgressiveToolTip.Description = Resources.IDC_APPLICATION_BTN_TOOLTIP_DESCRIPTION;
                mEnvCtrlDef.ProgressiveToolTip.Title = Resources.IDC_APPLICATION_BTN_TITLE;

                AddToDisabledList();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
            }
        }

        private void AddToDisabledList()
        {
            try
            {
                ControlDefinition parallelEnvButton = mInventorApp.CommandManager.ControlDefinitions[Resources.IDC_ENV_INTERNAL_NAME];
                if (null != parallelEnvButton)
                {
                    UserInterfaceManager userInterFaceMgr = mInventorApp.UserInterfaceManager;
                    int iEnvCount = userInterFaceMgr.Environments.Count;
                    Inventor.Environment env;
                    for (int i = 1; i <= iEnvCount; i++)
                    {
                        env = userInterFaceMgr.Environments[i];
                        if (env.InternalName != "PMxPartEnvironment" && env.BuiltIn == true)
                        {
                            // check for existing button in disabled list
                            int index = FindEnvironment(env, parallelEnvButton);
                            if (0 == index)
                                env.DisabledCommandList.Add(parallelEnvButton);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
            }
        }

        private void RemoveFromDisabledList()
        {
            try
            {
                ControlDefinition parallelEnvButton = mInventorApp.CommandManager.ControlDefinitions[Resources.IDC_ENV_INTERNAL_NAME];
                if (null != parallelEnvButton)
                {
                    UserInterfaceManager userInterFaceMgr = mInventorApp.UserInterfaceManager;
                    int iEnvCount = userInterFaceMgr.Environments.Count;
                    Inventor.Environment env;
                    for (int i = 1; i <= iEnvCount; i++)
                    {
                        env = userInterFaceMgr.Environments[i];
                        if (env.InternalName != "PMxPartEnvironment" && env.BuiltIn == true)
                        {
                            // check for existing button in disabled list
                            int index = FindEnvironment(env, parallelEnvButton);
                            if (0 != index)
                                env.DisabledCommandList.Remove(index);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
            }
        }

        private int FindEnvironment(Inventor.Environment env, ControlDefinition ctrlDef)
        {
            try
            {
                int index = 0;
                for (int i = 1; i <= env.DisabledCommandList.Count; i++)
                    if (ctrlDef == env.DisabledCommandList[i])
                    {
                        index = i;
                        break;
                    }
                return index;
            }
            catch (Exception e)
            {
            }
            return 0;
        }

        internal void LoadRibbon()
        {
            AddToDisabledList();

            EnvironmentList envList = mInventorApp.UserInterfaceManager.ParallelEnvironments;
            envList.Add(mEnvironment);
        }

        internal void UnloadRibbon()
        {
            // remove from disbaled list
            RemoveFromDisabledList();

            EnvironmentList envList = mInventorApp.UserInterfaceManager.ParallelEnvironments;
            for (int i = 1; i <= envList.Count; i++)
                if (envList[i].InternalName == Resources.IDC_ENV_INTERNAL_NAME)
                    envList.Remove(i);

            mInventorApp.UserInterfaceManager.UserInterfaceEvents.OnEnvironmentChange -= new UserInterfaceEventsSink_OnEnvironmentChangeEventHandler(OnEnvironmentChange);
            if (null != mNext)
            {
                mNext.Delete();
                mNext = null;
            }
            if (null != mPlay)
            {
                mPlay.Delete();
                mPlay = null;
            }
            if (null != mScramble)
            {
                mScramble.Delete();
                mScramble = null;
            }
            if (null != mSolve)
            {
                mSolve.Delete();
                mSolve = null;
            }
            try
            {
                if (mApplicationBtn != null)
                {
                    mApplicationBtn.Delete();
                    mApplicationBtn = null;
                }
            }
            catch (Exception ex) { }

        }

        private void CreateApplicationBtn()
        {
            Icon mainLarge = Resources.Rubiks_Cube_32;
            object mainLargeIcon = AxHostConverter.ImageToPictureDisp(mainLarge.ToBitmap());
            Icon mainStand = Resources.Rubiks_Cube_16;
            object mainStandIcon = AxHostConverter.ImageToPictureDisp(mainStand.ToBitmap());
            mApplicationBtn = mInventorApp.CommandManager.ControlDefinitions.AddButtonDefinition(Resources.IDC_ENV_DISPLAY_NAME, Resources.IDC_ENV_DISPLAY_NAME, Inventor.CommandTypesEnum.kQueryOnlyCmdType, Resources.IDC_EMPTY, Resources.IDC_EMPTY, "", mainStandIcon, mainLargeIcon);
            mApplicationBtn.OnHelp += new ButtonDefinitionSink_OnHelpEventHandler(OnHelpButtonClick);
            mApplicationBtn.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(OnRubikClick);

        }

        private void CreatePlayPanel()
        {
            Icon CalibrationLarge = Resources.Play_32;
            object CalibrationLargeIcon = AxHostConverter.ImageToPictureDisp(CalibrationLarge.ToBitmap());
            Icon CalibrationStand = Resources.Play_16;
            object CalibrationStandIcon = AxHostConverter.ImageToPictureDisp(CalibrationLarge.ToBitmap());

            mPlay = mInventorApp.CommandManager.ControlDefinitions.AddButtonDefinition(Resources.IDC_PLAY_DISPLAY_NAME, Resources.IDC_PLAY_INTERNAL_NAME, Inventor.CommandTypesEnum.kQueryOnlyCmdType, Resources.IDC_EMPTY, Resources.IDC_EMPTY, Resources.IDC_EMPTY, CalibrationStandIcon, CalibrationLargeIcon);
            mPlay.OnExecute += new Inventor.ButtonDefinitionSink_OnExecuteEventHandler(OnPlay);
            mPlay.OnHelp += new ButtonDefinitionSink_OnHelpEventHandler(OnHelpButtonClick);

            mPlay.ProgressiveToolTip.Title = Resources.IDC_PLAY_DISPLAY_NAME;
            mPlay.ProgressiveToolTip.Description = Resources.IDC_PLAY_BTN_TOOLTIP_DESCRIPTION;
            mPlay.ProgressiveToolTip.ExpandedDescription = Resources.IDC_PLAY_BTN_TOOLTIP_EXPANDED;

        }
        private void CreateNextPanel()
        {
            Icon CalibrationLarge = Resources.Next_32;
            object CalibrationLargeIcon = AxHostConverter.ImageToPictureDisp(CalibrationLarge.ToBitmap());
            Icon CalibrationStand = Resources.Next_16;
            object CalibrationStandIcon = AxHostConverter.ImageToPictureDisp(CalibrationLarge.ToBitmap());
            mNext = mInventorApp.CommandManager.ControlDefinitions.AddButtonDefinition(Resources.IDC_NEXT_DISPLAY_NAME, Resources.IDC_NEXT_INTERNAL_NAME, Inventor.CommandTypesEnum.kQueryOnlyCmdType, Resources.IDC_EMPTY, Resources.IDC_EMPTY, Resources.IDC_EMPTY, CalibrationStandIcon, CalibrationLargeIcon);
            mNext.OnExecute += new Inventor.ButtonDefinitionSink_OnExecuteEventHandler(OnNext);
            mNext.OnHelp += new ButtonDefinitionSink_OnHelpEventHandler(OnHelpButtonClick);

            mNext.ProgressiveToolTip.Title = Resources.IDC_NEXT_DISPLAY_NAME;
            mNext.ProgressiveToolTip.Description = Resources.IDC_NEXT_BTN_TOOLTIP_DESCRIPTION;
            mNext.ProgressiveToolTip.ExpandedDescription = Resources.IDC_NEXT_BTN_TOOLTIP_EXPANDED;
        }
        private void CreateRandomizePanel()
        {
            Icon CalibrationLarge = Resources.Scramble_32;
            object CalibrationLargeIcon = AxHostConverter.ImageToPictureDisp(CalibrationLarge.ToBitmap());
            Icon CalibrationStand = Resources.Scramble_16;
            object CalibrationStandIcon = AxHostConverter.ImageToPictureDisp(CalibrationLarge.ToBitmap());
            mScramble = mInventorApp.CommandManager.ControlDefinitions.AddButtonDefinition(Resources.IDC_SCRAMBLE_DISPLAY_NAME, Resources.IDC_SCRAMBLE_INTERNAL_NAME, Inventor.CommandTypesEnum.kQueryOnlyCmdType, Resources.IDC_EMPTY, Resources.IDC_EMPTY, Resources.IDC_EMPTY, CalibrationStandIcon, CalibrationLargeIcon);
            mScramble.OnExecute += new Inventor.ButtonDefinitionSink_OnExecuteEventHandler(OnScramble);
            mScramble.OnHelp += new ButtonDefinitionSink_OnHelpEventHandler(OnHelpButtonClick);

            mScramble.ProgressiveToolTip.Title = Resources.IDC_SCRAMBLE_DISPLAY_NAME;
            mScramble.ProgressiveToolTip.Description = Resources.IDC_SCRAMBLE_BTN_TOOLTIP_DESCRIPTION;
            mScramble.ProgressiveToolTip.ExpandedDescription = Resources.IDC_SCRAMBLE_BTN_TOOLTIP_EXPANDED;
        }
        private void CreateSolvepanel()
        {
            Icon settingsLarge = Resources.Solve_32;
            object settingsLargeIcon = AxHostConverter.ImageToPictureDisp(settingsLarge.ToBitmap());
            Icon settingsStand = Resources.Solve_16;
            object settingsStandIcon = AxHostConverter.ImageToPictureDisp(settingsStand.ToBitmap());

            mSolve = mInventorApp.CommandManager.ControlDefinitions.AddButtonDefinition(Resources.IDC_SOLVE_DISPLAY_NAME, Resources.IDC_SOLVE_INTERNAL_NAME, Inventor.CommandTypesEnum.kQueryOnlyCmdType, Resources.IDC_EMPTY, Resources.IDC_EMPTY, Resources.IDC_EMPTY, settingsStandIcon, settingsLargeIcon);
            mSolve.OnExecute += new Inventor.ButtonDefinitionSink_OnExecuteEventHandler(OnSolve);
            mSolve.OnHelp += new ButtonDefinitionSink_OnHelpEventHandler(OnHelpButtonClick);

            mSolve.ProgressiveToolTip.Title = Resources.IDC_SOLVE_DISPLAY_NAME;
            mSolve.ProgressiveToolTip.Description = Resources.IDC_SOLVE_BTN_TOOLTIP_DESCRIPTION;
            mSolve.ProgressiveToolTip.ExpandedDescription = Resources.IDC_SOLVE_BTN_TOOLTIP_EXPANDED;
        }

        private void OnRubikClick(NameValueMap Context)
        {
            if (null != ActivateClickEvent)
                ActivateClickEvent();

            mPlay.Enabled = true;
            mScramble.Enabled = true;
            mSolve.Enabled = true;
            mNext.Enabled = true;
        }
        private void OnPlay(NameValueMap Context)
        {
            if (null != PlayClickEvent)
                PlayClickEvent();
        }
        private void OnNext(NameValueMap Context)
        {
            if (null != NextClickEvent)
                NextClickEvent();
        }
        private void OnScramble(NameValueMap Context)
        {
            if (null != RandomizeClickEvent)
                RandomizeClickEvent();

        }
        private void OnSolve(NameValueMap Context)
        {
            if (null != SolveClickEvent)
                SolveClickEvent();
        }

        private void OnEnvironmentChange(Inventor.Environment Environment, EnvironmentStateEnum EnvironmentState, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kAfter)
            {
                if (Environment.InternalName == Resources.IDC_ENV_INTERNAL_NAME)
                    if (EnvironmentState == EnvironmentStateEnum.kTerminateEnvironmentState)
                    {
                        if (null != FinishClickEvent)
                        {
                            FinishClickEvent();
                        }
                    }
            }
            else
            {
                if (Environment.InternalName == Resources.IDC_ENV_INTERNAL_NAME)
                    if (EnvironmentState == EnvironmentStateEnum.kActivateEnvironmentState)
                    {
                        mEmptyPartDoc = true;
                        PartDocument partDoc = null;
                        if (mInventorApp.ActiveDocumentType != DocumentTypeEnum.kPartDocumentObject)
                            mEmptyPartDoc = false;
                        else if (mInventorApp.ActiveDocumentType == DocumentTypeEnum.kPartDocumentObject)
                        {
                            partDoc = mInventorApp.ActiveDocument as PartDocument;
                            if (partDoc.ComponentDefinition.SurfaceBodies.Count != 0 /*&& IsFinishFired== true*/)
                                mEmptyPartDoc = false;
                        }
                        if (mEmptyPartDoc == false/* && IsFinishFired==false*/)
                        {
                            MessageBox.Show(Resources.IDC_NOT_A_NEW_PART_DOC, "Rubik's Cube", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            HandlingCode = HandlingCodeEnum.kEventNotHandled;

                            mPlay.Enabled = false;
                            mScramble.Enabled = false;
                            mSolve.Enabled = false;
                            mNext.Enabled = false;
                        }
                        else if (mEmptyPartDoc == true)
                        {
                            if (null != ActivateClickEvent)
                                ActivateClickEvent();

                            mPlay.Enabled = true;
                            mScramble.Enabled = true;
                            mSolve.Enabled = true;
                            mNext.Enabled = true;
                        }
                        else
                        {
                            HandlingCode = HandlingCodeEnum.kEventNotHandled;
                            return;
                        }
                    }
            }
            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        private void OnHelpButtonClick(NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            try
            {
                string path = new Uri(Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.FullName + @"\Resources\Rubiks_cube_Help.htm").ToString();
                System.Diagnostics.Process.Start(path);
                HandlingCode = HandlingCodeEnum.kEventHandled;
            }
            catch (Exception e) { HandlingCode = HandlingCodeEnum.kEventNotHandled; }
        }


        #endregion

        #region public Members

        public Inventor.ButtonDefinition Play { get { return mPlay; } }
        public Inventor.ButtonDefinition Next { get { return mNext; } }
        public Inventor.ButtonDefinition Scramble { get { return mScramble; } }
        public Inventor.ButtonDefinition Solve { get { return mSolve; } }

        #endregion

        #region private members

        private Inventor.Application mInventorApp;
        private Inventor.ButtonDefinition mPlay, mNext, mScramble, mSolve, mApplicationBtn;
        private Inventor.Ribbon mPartRibbon;
        public Inventor.Environment mEnvironment;
        private ObjectCollection mObjCollection;
        internal RibbonTab mRubiksPartTab;
        Inventor.ControlDefinition mEnvCtrlDef;
        //private bool IsFinishFired ;
        private bool mEmptyPartDoc;
        #endregion

        #region public events

        public event RibbonButtonClickEvent PlayClickEvent;
        public event RibbonButtonClickEvent NextClickEvent;
        public event RibbonButtonClickEvent RandomizeClickEvent;
        public event RibbonButtonClickEvent SolveClickEvent;
        public event RibbonButtonClickEvent FinishClickEvent;
        public event RibbonButtonClickEvent ActivateClickEvent;
        public event EventHandler OnNewDocumentUnwireEvent;
        public event EventHandler OnNewDocumentWireEvent;

        #endregion
    }
}
