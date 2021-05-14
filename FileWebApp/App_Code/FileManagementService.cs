using System;
using System.IO;
using Telerik.Web.UI.Widgets;
using System.Web;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using BusinessAccessLayer;

public class FileManagementService
{
    private readonly BusinessLogicalService businessServer;
    private readonly char pathSeparator;
    public FileManagementService(string connectionString) {
        this.businessServer = new BusinessLogicalService(connectionString);
        this.pathSeparator = '/';
    }

    #region GetDirectoryItem
    public DirectoryItem GetDirectoryItem(string path, bool includeSubfolders)
    {
        DataRow item = this.GetItemRowFromPath(path);

        return (item != null && Convert.ToInt32(item["IsDirectory"]) == 1) ? this.CreateDirectoryItem(item, includeSubfolders) : null;
    }

    private DataRow GetItemRowFromPath(string path)
    {
        return businessServer.GetItemRowFromPath(path);
    }

    private DataRow[] GetChildDirectories(DataRow item)
    {
        return this.businessServer.Data.Select(String.Format("ParentID = {0} AND IsDirectory = 1", item["ItemID"].ToString()), "[Name]");
    }
    private DirectoryItem CreateDirectoryItem(DataRow item, bool includeSubfolders)
    {
        DirectoryItem directory = new DirectoryItem(item["Name"].ToString(),
                                                    this.GetLoaction(item),
                                                    this.GetFullPath(item),
                                                    String.Empty,
                                                    PathPermissions.Read, //correct permissions should be applied from the content provider
                                                    null,
                                                    null
                                                    );

        if (includeSubfolders)
        {
            DataRow[] subDirItems = GetChildDirectories(item);
            List<DirectoryItem> subDirs = new List<DirectoryItem>();

            foreach (DataRow subDir in subDirItems)
            {
                subDirs.Add(CreateDirectoryItem(subDir, false));
            }

            directory.Directories = subDirs.ToArray();
        }

        return directory;
    }
    private string GetLoaction(DataRow item)
    {
        if (String.IsNullOrEmpty(item["ParentID"].ToString()))
        {
            return String.Empty;
        }
        return "";
    }

    #endregion

    #region GetChildFiles
    public FileItem[] GetChildFiles(string folderPath, string[] searchPatterns, string handlerPath)
    {
        DataRow parentFolder = this.GetItemRowFromPath(folderPath);

        DataRow[] fileRows = this.businessServer.Data.Select(String.Format("ParentID = {0} AND IsDirectory = 0{1}", parentFolder["ItemID"].ToString(), this.GetSearchPatternsFilter(searchPatterns)), "[Name]");

        List<FileItem> result = new List<FileItem>(fileRows.Length);
        foreach (DataRow fileRow in fileRows)
        {
            result.Add(this.CreateFileItem(fileRow, handlerPath));
        }

        return result.ToArray();
    }
    private string GetSearchPatternsFilter(string[] searchPatterns)
    {
        if (Array.IndexOf(searchPatterns, "*.*") > -1)
            return String.Empty;


        string searchPatterntsFilterExpression = " AND (Name LIKE '%";
        for (int i = 0; i < searchPatterns.Length; i++)
        {
            searchPatterntsFilterExpression += searchPatterns[i].Substring(searchPatterns[i].LastIndexOf('.'));
            if (i < searchPatterns.Length - 1)
                searchPatterntsFilterExpression += "' OR Name LIKE '%";
            else
                searchPatterntsFilterExpression += "')";
        }

        return searchPatterntsFilterExpression;
    }
    private FileItem CreateFileItem(DataRow item, string handlerPath)
    {
        string itemPath = this.GetFullPath(item);
        return new FileItem(item["Name"].ToString(),
                            Path.GetExtension(itemPath),
                            Convert.ToInt64(item["FileSize"]),
                            itemPath,
                            GetItemUrl(itemPath, handlerPath),
                            String.Empty,
                            PathPermissions.Read //correct permissions should be applied from the content provider
                            );
    }
    private string GetItemUrl(string itemPath, string handlerPath)
    {
        string escapedPath = HttpUtility.UrlEncode(itemPath);
        return string.Format("{0}?path={1}", handlerPath, escapedPath);
    }
    private string GetFullPath(DataRow item)
    {
        string path = item["Name"].ToString();
        if (Convert.ToInt32(item["IsDirectory"]) == 1) path += this.pathSeparator;

        do
        {
            DataRow[] parentSearch = !String.IsNullOrEmpty(item["ParentID"].ToString()) ? this.businessServer.Data.Select(String.Format("ItemID = {0}", item["ParentID"].ToString()), "[Name]") : new DataRow[0];

            if (parentSearch.Length > 0)
            {
                item = parentSearch[0];
                path = String.Format("{0}{1}{2}", item["Name"].ToString(), this.pathSeparator, path);
            }
        } while (!String.IsNullOrEmpty(item["ParentID"].ToString()));

        return path;
    }
    #endregion

    #region HandlerMethod
    public DataRow GetItem(string path)
    {
        return this.GetItemRowFromPath(path);
    }
    #endregion
}
