using System;
using System.AddIn;
using System.Drawing;
using RightNow.AddIns.AddInViews;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Linq;

////////////////////////////////////////////////////////////////////////////////
//
// File: WorkspaceAddIn.cs
//
// Comments:
//
// Notes: 
//
// Pre-Conditions: 
//
////////////////////////////////////////////////////////////////////////////////
namespace FSAR_Fix
{
    public class WorkspaceAddIn : Panel, IWorkspaceComponent2
    {
        /// <summary>
        /// The current workspace record context.
        /// </summary>
        private IRecordContext _recordContext;
        RightNowConnectService _rnConnectService;
        public static IIncident _incidentRecord;
        public static IGlobalContext _globalContext { get; private set; }
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="inDesignMode">Flag which indicates if the control is being drawn on the Workspace Designer. (Use this flag to determine if code should perform any logic on the workspace record)</param>
        /// <param name="RecordContext">The current workspace record context.</param>
        public WorkspaceAddIn(bool inDesignMode, IRecordContext RecordContext, IGlobalContext GlobalContext)
        {

            if (!inDesignMode)
            {

                _recordContext = RecordContext;
                _globalContext = GlobalContext;
                //  _recordContext.Saved += _recordContext_Saved;
                _rnConnectService = RightNowConnectService.GetService(_globalContext);

            }
        }

        #region IAddInControl Members

        /// <summary>
        /// Method called by the Add-In framework to retrieve the control.
        /// </summary>
        /// <returns>The control, typically 'this'.</returns>
        public Control GetControl()
        {
            return this;
        }

        #endregion

        #region IWorkspaceComponent2 Members

        /// <summary>
        /// Sets the ReadOnly property of this control.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Method which is called when any Workspace Rule Action is invoked.
        /// </summary>
        /// <param name="ActionName">The name of the Workspace Rule Action that was invoked.</param>
        public void RuleActionInvoked(string ActionName)
        {
        }

        /// <summary>
        /// Method which is called when any Workspace Rule Condition is invoked.
        /// </summary>
        /// <param name="ConditionName">The name of the Workspace Rule Condition that was invoked.</param>
        /// <returns>The result of the condition.</returns>
        public string RuleConditionInvoked(string ConditionName)
        {
            return string.Empty;
        }

        public static void SetIncidentField(string pkgName, string fieldName, string value)
        {
            if (pkgName == "c")
            {
                IList<ICfVal> incCustomFields = _incidentRecord.CustomField;
                int fieldID = GetCustomFieldID(fieldName);
                foreach (ICfVal val in incCustomFields)
                {
                    if (val.CfId == fieldID)
                    {
                        switch (val.DataType)
                        {
                            case (int)RightNow.AddIns.Common.DataTypeEnum.BOOLEAN_LIST:
                            case (int)RightNow.AddIns.Common.DataTypeEnum.BOOLEAN:
                                if (value == "1" || value.ToLower() == "true")
                                {
                                    val.ValInt = 1;
                                }
                                else if (value == "0" || value.ToLower() == "false")
                                {
                                    val.ValInt = 0;
                                }
                                break;
                        }

                    }
                }
            }
            else
            {
                IList<ICustomAttribute> incCustomAttributes = _incidentRecord.CustomAttributes;

                foreach (ICustomAttribute val in incCustomAttributes)
                {
                    if (val.PackageName == pkgName)
                    {
                        if (val.GenericField.Name == pkgName + "$" + fieldName)
                        {
                            switch (val.GenericField.DataType)
                            {
                                case RightNow.AddIns.Common.DataTypeEnum.BOOLEAN:
                                    if (value == "1" || value.ToLower() == "true")
                                    {
                                        val.GenericField.DataValue.Value = true;
                                    }
                                    else if (value == "0" || value.ToLower() == "false")
                                    {
                                        val.GenericField.DataValue.Value = false;
                                    }
                                    break;
                                case RightNow.AddIns.Common.DataTypeEnum.INTEGER:
                                    if (value.Trim() == "" || value.Trim() == null)
                                    {
                                        val.GenericField.DataValue.Value = null;
                                    }
                                    else
                                    {
                                        val.GenericField.DataValue.Value = Convert.ToInt32(value);
                                    }
                                    break;
                                case RightNow.AddIns.Common.DataTypeEnum.STRING:
                                    val.GenericField.DataValue.Value = value;
                                    break;
                            }
                        }
                    }
                }
            }
            return;
        }
        public static int GetCustomFieldID(string fieldName)
        {
            IList<IOptlistItem> CustomFieldOptList = _globalContext.GetOptlist((int)RightNow.AddIns.Common.OptListID.CustomFields);//92 returns an OptList of custom fields in a hierarchy
            foreach (IOptlistItem CustomField in CustomFieldOptList)
            {
                if (CustomField.Label == fieldName)//Custom Field Name
                {
                    return (int)CustomField.ID;//Get Custom Field ID
                }
            }
            return -1;
        }
        #endregion
    }

    [AddIn("Workspace Factory AddIn", Version = "1.0.0.0")]
    public class WorkspaceAddInFactory : IWorkspaceComponentFactory2
    {
        #region IWorkspaceComponentFactory2 Members

            static public IGlobalContext _globalContext; 
        /// <summary>
        /// Method which is invoked by the AddIn framework when the control is created.
        /// </summary>
        /// <param name="inDesignMode">Flag which indicates if the control is being drawn on the Workspace Designer. (Use this flag to determine if code should perform any logic on the workspace record)</param>
        /// <param name="RecordContext">The current workspace record context.</param>
        /// <returns>The control which implements the IWorkspaceComponent2 interface.</returns>
        public IWorkspaceComponent2 CreateControl(bool inDesignMode, IRecordContext RecordContext)
        {
            return new WorkspaceAddIn(inDesignMode, RecordContext,_globalContext);
    
        }

        #endregion

        #region IFactoryBase Members

        /// <summary>
        /// The 16x16 pixel icon to represent the Add-In in the Ribbon of the Workspace Designer.
        /// </summary>
        public Image Image16
        {
            get { return Properties.Resources.AddIn16; }
        }

        /// <summary>
        /// The text to represent the Add-In in the Ribbon of the Workspace Designer.
        /// </summary>
        public string Text
        {
            get { return "Fix"; }
        }

        /// <summary>
        /// The tooltip displayed when hovering over the Add-In in the Ribbon of the Workspace Designer.
        /// </summary>
        public string Tooltip
        {
            get { return "WorkspaceAddIn Tooltip"; }
        }

        #endregion

        #region IAddInBase Members

        /// <summary>
        /// Method which is invoked from the Add-In framework and is used to programmatically control whether to load the Add-In.
        /// </summary>
        /// <param name="GlobalContext">The Global Context for the Add-In framework.</param>
        /// <returns>If true the Add-In to be loaded, if false the Add-In will not be loaded.</returns>
        public bool Initialize(IGlobalContext GlobalContext)
        {
            _globalContext = GlobalContext;
            return true;
        }

        #endregion
    }
}