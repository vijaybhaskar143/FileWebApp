using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
   
    [Flags]
    public enum PathPermissionsModel
    {
        Read = 1,
        Upload = 2,
        Delete = 4
    }
}
