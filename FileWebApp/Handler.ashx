<%@ WebHandler Language="C#" Class="Handler" %>
using System.Web;

public class Handler : IHttpHandler
{
	#region IHttpHandler Members

	private FileManagementService fileServer;
	private FileManagementService FileServer
	{
		get
		{
			if (fileServer == null)
			{
				fileServer = new FileManagementService(System.Configuration.ConfigurationManager.ConnectionStrings["TelerikConnectionString"].ConnectionString);
			}
			return fileServer;
		}
	}

	private HttpContext Context { get; set; }

	public void ProcessRequest(HttpContext context)
	{
		Context = context;

		if (context.Request.QueryString["path"] == null)
		{
			return;
		}
		string path = Context.Server.UrlDecode(Context.Request.QueryString["path"]);

		var item = FileServer.GetItem(path);
		if (item == null) return;

		WriteFile((byte[])item["FileContent"], item["Name"].ToString(), item["FileType"].ToString(), context.Response);
	}

	/// <summary>
	/// Sends a byte array to the client
	/// </summary>
	/// <param name="content">binary file content</param>
	/// <param name="fileName">the filename to be sent to the client</param>
	/// <param name="contentType">the file content type</param>
	private void WriteFile(byte[] content, string fileName, string contentType, HttpResponse response)
	{
		response.Buffer = true;
		response.Clear();
		response.ContentType = contentType;
		string extension = System.IO.Path.GetExtension(fileName).ToLower();
		if (extension != ".htm" && extension != ".html" && extension != ".xml" && extension != ".jpg" && extension != ".gif" && extension != ".png")
		{
			response.AddHeader("content-disposition", "attachment; filename=" + fileName);
		}
		response.BinaryWrite(content);
		response.Flush();
		response.End();
	}

	public bool IsReusable
	{
		get
		{
			return false;
		}
	}

	#endregion
}