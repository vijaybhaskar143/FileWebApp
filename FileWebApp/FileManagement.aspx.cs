using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Telerik.Web.UI.Widgets;

public partial class FileManagement : System.Web.UI.Page
{
    static bool _isSqlTypesLoaded = false;

    public FileManagement()
    {
        //if (!_isSqlTypesLoaded)
        //{
        //    SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/"));
        //    _isSqlTypesLoaded = true;
        //}

    }
    protected void Page_Load(object sender, EventArgs e)
    {
        FileExplorer1.Configuration.SearchPatterns = new string[] { "*.jpg", "*.jpeg", "*.gif", "*.png", "*.pdf", "*.doc", "*.docx" };

        FileExplorer1.Configuration.ViewPaths = new string[] { "ROOT/Files"};
        FileExplorer1.Configuration.UploadPaths = new string[] { "ROOT/Files" };
        FileExplorer1.Configuration.DeletePaths = new string[] { "ROOT/Files" };

        // Sets Max file size
        FileExplorer1.Configuration.MaxUploadFileSize = 10485760;

        FileExplorer1.Configuration.EnableAsyncUpload = true;

        FileExplorer1.ExplorerMode = Telerik.Web.UI.FileExplorer.FileExplorerMode.Default;

        // Enables Paging on the Grid
        FileExplorer1.AllowPaging = true;
        // Sets the page size
        FileExplorer1.PageSize = 20;

        //Load the default FileSystemContentProvider
        FileExplorer1.Configuration.ContentProviderTypeName =
            typeof(DBContentProvider).AssemblyQualifiedName;

    }
}