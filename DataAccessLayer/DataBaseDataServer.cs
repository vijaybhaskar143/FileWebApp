using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace DataAccessLayer
{
	public class DataBaseDataServer
	{
		private readonly string connectionString;
		private readonly char pathSeparator;

		private DataTable _data;
		public DataTable Data
		{
			get
			{
				if (_data == null)
				{
					using (SqlConnection connection = this.GetConnection(this.connectionString))
					{
						SqlDataAdapter adapter = new SqlDataAdapter("SELECT Name, ItemID, ParentID, FileType,IsDirectory, FileSize, FileContent FROM [FilesData] ORDER BY ItemID, [Name]", connection);
						using (connection)
						{
							connection.Open();
							_data = new DataTable();
							adapter.Fill(_data);
						}
					}
				}
				return _data;
			}
		}


		public DataBaseDataServer(string connectionString) : this(connectionString, '/') { }
		public DataBaseDataServer(string connectionString, char pathSeparator)
		{
			this.connectionString = connectionString;
			this.pathSeparator = pathSeparator;
		}

		private SqlConnection GetConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

        #region UpdateItem
        private string TrimSeparator(string path)
        {
            return path.Trim(this.pathSeparator);
        }
        public void UpdateItem(int itemId, string[] fields, string[] values)
		{
			if (fields.Length != values.Length)
				return;

			string updateCommandStr = "UPDATE [FilesData] SET";
			for (int i = 0; i < fields.Length; i++)
			{
				updateCommandStr += String.Format(" [{0}]='{1}'", fields[i], values[i]);
				if (i < fields.Length - 1)
					updateCommandStr += ",";
			}
			updateCommandStr += String.Format(" WHERE [ItemID] = {0}", itemId);

			SqlConnection connection = this.GetConnection(this.connectionString);

			using (connection)
			{
				connection.Open();
				SqlCommand command = new SqlCommand(updateCommandStr, connection);
				command.ExecuteNonQuery();

				this._data = null; //force update
			}
		}
		#endregion

		#region DeletingItem
		public void DeleteItem(string path)
		{
			SqlConnection connection = this.GetConnection(this.connectionString);

			using (connection)
			{
				connection.Open();

				SqlCommand command = new SqlCommand(String.Format("DELETE FROM [FilesData] WHERE ItemID = {0}", GetItemIdFromPath(path)), connection);
				command.ExecuteNonQuery();

				this._data = null;
			}
		}
        #endregion

        #region CreateDirectory
        public string AddDirectory(string name, int parentId)
        {
            return this.AddItem(name, parentId, String.Empty, 1, 0, new byte[0]);
        }
        private int? GetItemIdFromPath(string path)
        {
            int[] ancestors = this.ConvertPathToIds(path);

            return ancestors != null && ancestors.Length > 0 ? (int?)ancestors[ancestors.Length - 1] : null;
        }
        public string AddItem(string name, int parentId, string mimeType, int isDirectory, long size, byte[] content)
		{
			try
			{
				SqlConnection connection = this.GetConnection(this.connectionString);

				SqlCommand command =
					new SqlCommand(
						"INSERT INTO FilesData ([Name], ParentId, FileType, IsDirectory, [FileSize], FileContent) VALUES (@Name, @ParentId, @FileType, @IsDirectory, @FileSize, @FileContent)", connection);
				command.Parameters.Add(new SqlParameter("@Name", name));
				command.Parameters.Add(new SqlParameter("@ParentId", parentId));
				command.Parameters.Add(new SqlParameter("@FileType", mimeType));
				command.Parameters.Add(new SqlParameter("@IsDirectory", isDirectory));
				command.Parameters.Add(new SqlParameter("@FileSize", size));
				command.Parameters.Add(new SqlParameter("@FileContent", content));

				using (connection)
				{
					connection.Open();
					command.ExecuteNonQuery();
					this._data = null; //force update
				}

				return String.Empty;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
		public int[] ConvertPathToIds(string path)
		{
			path = this.TrimSeparator(path);
			string[] names = path.Split('/');

			List<int> result = new List<int>(names.Length);

			int itemId = 0;
			for (int i = 0; i < names.Length; i++)
			{
				string name = names[i];
				DataRow[] rows = this.Data.Select(string.Format("Name='{0}' " +
					"AND (ParentID={1} OR {1}=0)",
					name.Replace("'", "''"), itemId), "[Name]");
				if (rows.Length > 0)
				{
					result.Add((int)rows[0]["ItemID"]);
					itemId = (int)rows[0]["ItemID"];
				}
			}

			return names.Length == result.Count ? result.ToArray() : null;
		}
		#endregion

        #region StoreFile
		public string AddFile(string name, int parentId, string mimeType, byte[] content)
		{
			return this.AddItem(name, parentId, mimeType, 0, content.LongLength, content);
		}
        #endregion

        #region CopyItem
		public void CopyItemInternal(string path, string newPath)
		{
			DataRow itemRow = this.GetItemRowFromPath(path);
			DataRow parent = this.GetParentFromPath(newPath);

			if (Convert.ToInt32(itemRow["IsDirectory"]) == 1)
			{
				this.AddDirectory(itemRow["Name"].ToString(), (int)parent["ItemID"]);

				DataRow[] children = this.Data.Select(String.Format("ParentId = {0}", (int)itemRow["ItemID"]), "[Name]");
				foreach (DataRow child in children)
				{
					this.CopyItemInternal(String.Format("{0}{1}{2}", this.TrimSeparator(path), this.pathSeparator, child["Name"].ToString()), String.Format("{0}{1}{2}", newPath, this.pathSeparator, child["Name"].ToString()));
				}
			}
			else
			{
				this.AddFile(itemRow["Name"].ToString(), (int)parent["ItemID"], itemRow["FileType"].ToString(), (byte[])itemRow["FileContent"]);
			}
			this._data = null;
		}
        private DataRow GetParentFromPath(string path)
        {
            string parentPath = path.Substring(0, TrimSeparator(path).LastIndexOf(pathSeparator));

            return this.GetItemRowFromPath(parentPath);
        }
        #endregion

        #region SupportingMethods

		public DataRow GetItemRowFromPath(string path)
		{
			int? itemId = GetItemIdFromPath(path);
			if (itemId == null) return null;

			DataRow[] result = this.Data.Select(String.Format("ItemID = {0}", itemId), "[Name]");
			return result.Length > 0 ? result[0] : null;
		}
		public string GetFullPath(DataRow item)
		{
			string path = item["Name"].ToString();
			if (Convert.ToInt32(item["IsDirectory"]) == 1) path += this.pathSeparator;

			do
			{
				DataRow[] parentSearch = !String.IsNullOrEmpty(item["ParentID"].ToString()) ? this.Data.Select(String.Format("ItemID = {0}", item["ParentID"].ToString()), "[Name]") : new DataRow[0];

				if (parentSearch.Length > 0)
				{
					item = parentSearch[0];
					path = String.Format("{0}{1}{2}", item["Name"].ToString(), this.pathSeparator, path);
				}
			} while (!String.IsNullOrEmpty(item["ParentID"].ToString()));

			return path;
		}
		public string GetLoaction(DataRow item)
		{
			if (String.IsNullOrEmpty(item["ParentID"].ToString()))
			{
				return String.Empty;
			}

			DataRow parentFolder = this.Data.Select(String.Format("ItemID = {0}", item["ParentID"].ToString()), "[Name]")[0];
			return this.GetFullPath(parentFolder);
		}

        #endregion
    }
}
