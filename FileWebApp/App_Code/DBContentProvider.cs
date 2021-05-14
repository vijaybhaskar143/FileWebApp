using System;
using System.Linq;
using System.Web;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Telerik.Web.UI;
using Telerik.Web.UI.Widgets;

/// <summary>
/// Summary description for FileNewProvider
/// </summary>
public class DBContentProvider : Telerik.Web.UI.Widgets.FileBrowserContentProvider
{
	private readonly DBDataServer dataServer;

	private readonly string itemHandlerPath;

	public DBContentProvider(HttpContext context, string[] searchPatterns, string[] viewPaths, string[] uploadPaths, string[] deletePaths, string selectedUrl, string selectedItemTag)
		: base(context, searchPatterns, viewPaths, uploadPaths, deletePaths, selectedUrl, selectedItemTag)
	{
		this.dataServer = new DBDataServer(System.Configuration.ConfigurationManager.ConnectionStrings["TelerikConnectionString"].ConnectionString);
		this.itemHandlerPath = System.Configuration.ConfigurationManager.AppSettings["Telerik.FileApp.ItemHandler"];
		if (itemHandlerPath.StartsWith("~/"))
		{
			itemHandlerPath = HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + itemHandlerPath.Substring(1);
		}
	}

	#region OVERRIDES
	public override DirectoryItem ResolveRootDirectoryAsTree(string path)
	{
		DirectoryItem directory = dataServer.GetDirectoryItem(path, true);

		if (directory == null) return null;

		directory.Permissions = GetPermissions(path);
		foreach (DirectoryItem dir in directory.Directories)
		{
			dir.Permissions = GetPermissions(path);
		}

		return directory;
	}

	public override DirectoryItem ResolveDirectory(string path)
	{
		DirectoryItem directory = dataServer.GetDirectoryItem(path, false);

		if (directory == null) return null;

		directory.Permissions = GetPermissions(directory.Path);
		directory.Files = dataServer.GetChildFiles(path, this.SearchPatterns, this.itemHandlerPath);
		foreach (FileItem file in directory.Files)
		{
			file.Permissions = GetPermissions(file.Location);
		}

		return directory;
	}

	public override string GetFileName(string url)
	{
		return Path.GetFileName(ExtractPathFromUrl(url));
	}
	public override string GetPath(string url)
	{
		return dataServer.GetPath(ExtractPathFromUrl(url));
	}
	public override Stream GetFile(string url)
	{
		byte[] buffer = dataServer.GetItemContent(ExtractPathFromUrl(url));
		if (!Object.Equals(buffer, null))
		{
			return new MemoryStream(buffer);
		}
		return null;
	}
	public override string StoreBitmap(Bitmap bitmap, string url, ImageFormat format)
	{
		string newItemPath = ExtractPathFromUrl(url);
		string name = GetFileName(newItemPath);
		string path = GetPath(newItemPath);
		string tempFilePath = System.IO.Path.GetTempFileName();
		bitmap.Save(tempFilePath);
		byte[] content;
		using (FileStream inputStream = File.OpenRead(tempFilePath))
		{
			long size = inputStream.Length;
			content = new byte[size];
			inputStream.Read(content, 0, Convert.ToInt32(size));
		}

		if (File.Exists(tempFilePath))
		{
			File.Delete(tempFilePath);
		}

		string error = dataServer.StoreFile(name, path, GetImageMimeType(bitmap), content);

		return String.IsNullOrEmpty(error) ? String.Format("{0}{1}{2}", path, PathSeparator, name) : String.Empty;
	}
	public override string StoreFile(UploadedFile file, string path, string name, params string[] arguments)
	{
		long fileLength = file.InputStream.Length;
		byte[] content = new byte[fileLength];
		file.InputStream.Read(content, 0, (int)fileLength);

		string error = dataServer.StoreFile(name, path, file.ContentType, content);

		return String.IsNullOrEmpty(error) ? String.Format("{0}{1}{2}", path, PathSeparator, name) : String.Empty;
	}
	public override string DeleteFile(string path)
	{
		dataServer.DeleteItem(path);
		return String.Empty;
	}
	public override string DeleteDirectory(string path)
	{
		dataServer.DeleteItem(path);
		return String.Empty;
	}
	public override string CreateDirectory(string location, string name)
	{
		if (dataServer.ItemExists(String.Format("{0}{1}", location, name)))
			return "Directory with the same name already exists!";

		string error = dataServer.CreateDirectory(name, location);
		return !String.IsNullOrEmpty(error) ? String.Format("{0}{1}", location, name) : String.Empty;
	}
	public override string MoveFile(string path, string newPath)
	{
		if (!dataServer.ItemExists(newPath))
		{
			dataServer.UpdateItem(path, newPath);
			return String.Empty;
		}
		else return "File or folder with the same name already exists!";
	}
	public override string MoveDirectory(string path, string newPath)
	{
		return MoveFile(path, newPath);
	}
	public override string CopyFile(string path, string newPath)
	{
		if (!dataServer.ItemExists(newPath))
		{
			dataServer.CopyItem(path, newPath);
			return String.Empty;
		}
		else return "File or folder with the same name already exists!";
	}
	public override string CopyDirectory(string path, string destinationPath)
	{
		string destFullName = destinationPath + path.Trim(PathSeparator).Substring(path.Trim(PathSeparator).LastIndexOf(PathSeparator) + 1);

		return CopyFile(path, destFullName);
	}

	public override bool CheckDeletePermissions(string folderPath)
	{
		foreach (string path in this.DeletePaths)
		{
			if (folderPath.StartsWith(path, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
	public override bool CheckWritePermissions(string folderPath)
	{
		foreach (string path in this.UploadPaths)
		{
			if (folderPath.StartsWith(path, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
	public override bool CheckReadPermissions(string folderPath)
	{
		foreach (string viewPath in this.ViewPaths)
		{
			if (folderPath.StartsWith(viewPath, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
	public override bool CanCreateDirectory
	{
		get
		{
			return true;
		}
	}
	#endregion

	#region PRIVATE METHODS

	private PathPermissions GetPermissions(string folderPath)
	{
		PathPermissions permissions = PathPermissions.Read;
		if (CheckDeletePermissions(folderPath)) permissions = PathPermissions.Delete | permissions;
		if (CheckWritePermissions(folderPath)) permissions = PathPermissions.Upload | permissions;

		return permissions;
	}
	private string ExtractPathFromUrl(string url)
	{
		string itemUrl = RemoveProtocolNameAndServerName(url);
		if (itemUrl == null)
		{
			return string.Empty;
		}
		if (itemUrl.StartsWith(this.itemHandlerPath))
		{
			return itemUrl.Substring(GetItemUrl(string.Empty).Length);
		}
		return itemUrl;
	}
	private string GetItemUrl(string virtualItemPath)
	{
		string escapedPath = Context.Server.UrlEncode(virtualItemPath);
		return string.Format("{0}?path={1}", this.itemHandlerPath, escapedPath);
	}
	private string GetImageMimeType(Bitmap bitmap)
	{
		foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
		{
			if (codec.FormatID == bitmap.RawFormat.Guid)
				return codec.MimeType;
		}

		return "image/unknown";
	}

	#endregion
}