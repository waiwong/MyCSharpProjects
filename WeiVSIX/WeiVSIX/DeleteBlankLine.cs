using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using EnvDTE100;

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
        public static readonly Guid CommandSet = new Guid("8d0faba8-cb0d-4bc5-b40d-fb5feba13450");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteBlankLine"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DeleteBlankLine(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
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
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
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
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DeleteBlankLine's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new DeleteBlankLine(package, commandService);
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
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "DeleteBlankLine";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

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
    }
}

