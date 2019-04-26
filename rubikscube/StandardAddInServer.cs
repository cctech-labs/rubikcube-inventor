/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;

namespace RubiksCube
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("07e1fdef-7a8b-4920-8501-51810a71b2bf")]
    public class RubiksAddInServer : Inventor.ApplicationAddInServer
    {

        // Inventor application object.
        private Inventor.Application mInventorApp;
        private RubikPlugin.RubikCubePlugin mRubiksCubePlugin;

        public RubiksAddInServer()
        {

        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.

            // Initialize AddIn members.
            mInventorApp = addInSiteObject.Application;
            mRubiksCubePlugin = new RubikPlugin.RubikCubePlugin(mInventorApp);
            // TODO: Add ApplicationAddInServer.Activate implementation.
            // e.g. event initialization, command creation etc.
        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation

            // Release objects.
            mRubiksCubePlugin.UnloadPlugin();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandID)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }

        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        #endregion

    }
}
