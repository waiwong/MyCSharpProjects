//------------------------------------------------------------------------------
// <copyright file="DeleteBlankLine.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace WeiVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DeleteBlankLine
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3045f227-b86b-41a2-99df-69c8e56ae9b0");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteBlankLine"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private DeleteBlankLine(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                //var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DeleteBlankLine Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new DeleteBlankLine(package);
        }

        private DTE2 _dte = null;
        DTE2 GetDTE()
        {
            if (_dte == null)
            {
                _dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            }
            return _dte;
        }



        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = GetDTE();
            try
            {
                Document activeDoc = dte.ActiveDocument;

                if (activeDoc != null && activeDoc.ProjectItem != null && activeDoc.ProjectItem.ContainingProject != null)
                {
                    TextDocument objTextDoc = activeDoc.Object("TextDocument") as TextDocument;
                    EditPoint2 startPoint = objTextDoc.StartPoint.CreateEditPoint() as EditPoint2;
                    EditPoint2 endPoint = objTextDoc.EndPoint.CreateEditPoint() as EditPoint2;
                    string wholeWindowText = startPoint.GetText(endPoint);
                    wholeWindowText = Regex.Replace(wholeWindowText, @"^\s+${2,}", string.Empty, RegexOptions.Multiline);

                    wholeWindowText = Regex.Replace(wholeWindowText, @"\r\n\n\s*\}\r\n\s*$", "\r\n}\r\n", RegexOptions.Multiline);
                    wholeWindowText = Regex.Replace(wholeWindowText, @"\r\n\n\s*\}", "\r\n}", RegexOptions.Multiline);
                    wholeWindowText = Regex.Replace(wholeWindowText, @"\{\r\n\n", "{\r\n", RegexOptions.Multiline);
                    startPoint.ReplaceText(endPoint, wholeWindowText, 3);
                    startPoint.SmartFormat(endPoint);

                    dte.ActiveDocument.Activate();
                    dte.ExecuteCommand("Edit.FormatDocument");
                    dte.ExecuteCommand("Edit.SortUsings");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand callCommand = (OleMenuCommand)sender;

            // hidden by default
            callCommand.Visible = false;
            callCommand.Enabled = false;

            Document activeDoc = GetDTE().ActiveDocument;
            if (activeDoc != null && activeDoc.ProjectItem != null && activeDoc.ProjectItem.ContainingProject != null)
            {
                string lang = activeDoc.Language;
                if (activeDoc.Language.Equals("CSharp"))
                {
                    // show command if active document is a csharp file.
                    callCommand.Visible = true;
                }

                // enable command, if command is visible and there is text on the clipboard
                //callCommand.Enabled = (callCommand.Visible) ? Clipboard.ContainsText() : false;
                callCommand.Enabled = callCommand.Visible;
            }
        }
    }
}
