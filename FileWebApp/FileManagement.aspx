<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FileManagement.aspx.cs" Inherits="FileManagement" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>File Explorer Management</title>
    <script src="scripts.js" type="text/javascript"></script>
    <!-- Latest compiled and minified CSS -->
   <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" />
</head>
<body>
   <form id="FormFileManagement" runat="server">
    <div style="text-align:center; font-size:medium; font-weight:bold; color:green;">File Explorer Management</div>
    <telerik:RadScriptManager runat="server" ID="RadScriptManager1" />
    <telerik:RadSkinManager ID="RadSkinManager1" runat="server" Skin="Windows7" ShowChooser="false" />
    <div class="demo-container size-medium">
        <telerik:RadFileExplorer RenderMode="Lightweight" runat="server" ID="FileExplorer1" 
            Width="1000px" Height="600px" EnableCopy="true" style="top: 10px; left: 0px"
            >
        </telerik:RadFileExplorer>
    </div>
   </form>
</body>
</html>
