using DataAccessLayer;
using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccessLayer
{
    public class BusinessLogicalService
    {
        private readonly DataBaseDataServer dBService;
        private readonly string connectionString;
        private DataTable _data;
        public DataTable Data
        {
            get
            {
                if (_data == null)
                {
                    _data = this.dBService.Data;
                }
                return _data;
            }
        }
        public BusinessLogicalService(string connectionString)
        {
            this.connectionString = connectionString;
            this.dBService = new DataBaseDataServer(this.connectionString);
        }

        #region DeletingItem
        public void DeleteItem(string path)
        {
            this.dBService.DeleteItem(path);
        }
        #endregion

        #region UpdatingItem
        public void UpdateItem(string path, string newPath)
        {
            string oldName = this.GetName(path);
            string newName = this.GetName(newPath);


            int itemId = (int)GetItemIdFromPath(path);

            if (oldName == newName) //move
            {
                DataRow newParent = this.GetParentFromPath(newPath);
                this.dBService.UpdateItem(itemId, new string[] { "ParentID" }, new string[] { newParent["ItemID"].ToString() });
            }
            else //rename
            {
                this.dBService.UpdateItem(itemId, new string[] { "Name" }, new string[] { newName });
            }
        }
        private string GetName(string path)
        {
            string tmpPath = this.TrimSeparator(path);
            return tmpPath.Substring(tmpPath.LastIndexOf('/') + 1);
        }
        private string TrimSeparator(string path)
        {
            return path.Trim('/');
        }
        private int? GetItemIdFromPath(string path)
        {
            int[] ancestors = this.dBService.ConvertPathToIds(path);

            return ancestors != null && ancestors.Length > 0 ? (int?)ancestors[ancestors.Length - 1] : null;
        }
        private DataRow GetParentFromPath(string path)
        {
            string parentPath = path.Substring(0, TrimSeparator(path).LastIndexOf('/'));

            return this.dBService.GetItemRowFromPath(parentPath);
        }
        #endregion

        #region CreateFolder
        public string CreateDirectory(string name, string location)
        {
            return this.dBService.AddDirectory(name, (int)this.GetItemIdFromPath(location));
        }
        #endregion

        #region StoreFile
        public string StoreFile(string name, string location, string contentType, byte[] content)
        {
            DataRow parent = this.dBService.GetItemRowFromPath(location);
            if (parent == null) return "Invalid location path.";
            return this.dBService.AddFile(name, (int)parent["ItemID"], contentType, content);
        }
        #endregion

        #region CopyItem
        public void CopyItem(string path, string newPath)
        {
            this.dBService.CopyItemInternal(path, newPath);
        }
        #endregion

        #region GetItem
        public DataRow GetItem(string path)
        {
            return this.dBService.GetItemRowFromPath(path);
        }
        #endregion

        #region GetItemRowFromPath
        public DataRow GetItemRowFromPath(string path)
        {
            return this.dBService.GetItemRowFromPath(path);
        }
        #endregion

        #region GetPath
        public string GetPath(string path)
        {
            DataRow item = this.dBService.GetItemRowFromPath(path);
            if (item == null)
                item = this.GetParentFromPath(path);

            return Convert.ToInt32(item["IsDirectory"]) == 1 ? this.dBService.GetFullPath(item) : this.dBService.GetLoaction(item);
        }
        #endregion

        #region SupportingMethods
        public byte[] GetItemContent(string path)
        {
            DataRow item = this.GetItemRowFromPath(path);

            return item != null ? (byte[])item["FileContent"] : null;
        }

        public bool ItemExists(string path)
        {
            DataRow item = this.GetItemRowFromPath(path);
            return !Object.Equals(item, null);
        }
        #endregion
    }
}
