# File Web Application

   File Application is an ASP.NET AJAX application that lets the user upload, store, browse, and copy folders and files on a web server.
The application should support the following functionality:

## Technical Specifications
  
  * IDE: Visual Studio 2019
  * Code bihind Language: C#
  * Third Party Library: Telerik Web UI
  * DataBase: SQL Server 2019
  
## Features

   File Application is an ASP.NET AJAX application that lets the user upload, store, browse, and copy folders and files on a web server.
The application should support the following functionality:


* A treeview to browse folders and files on the server, similar to the Navigation Pane in Windows Explorer.  The treeview should show all of the available content:
  folders and files.  The user should be able to select an individual item in the treeview and perform actions on that item. The actions may be run via a button-invoked menu or a context-sensitive right-click menu.
     * All items (files and folders) should support the following actions:
          * Delete
          * Rename
          *	Copy: this action creates a copy of the item A and places this copy in the same folder, with the name “Copy of A”.  When a folder is copied, all of its sub-folders and files should be copied as well (a “deep” copy).
     * Files should support the additional action:
          * Download: this action should stream the file to the user’s browser 
     * Folders should support these additional actions:
          * Create new sub- folder
          * Upload a file from the client’s hard drive to the folder (requires a mechanism to select the file to be uploaded)
* There should be a way to display meta-data about each file and folder.  This can be done in another pane on the same screen (visually separated from the treeview).  When the user selects an item in the treeview, its meta-data should be displayed automatically.
     * File meta-data:
          * File Name
          * Size
     * Folder meta-data:
          * Folder Name
          * Cumulative size of all files in the sub-tree rooted at the folder

- - - -

- [x] A completed task
```python
  As a File Web Application, by using the telerik File Explorer tool, i had integrated with SQL Server and done the part.
```
- [ ] An uncompleted task
```python
  Unfortunately due to not getting enough time to build the actual design requirements which are expecting to seperate the 
  business logic from UI. I had tried to seperate out the business models (data access layer) out and tried to create some models to make use of those
  in UI layer to get the data. But unfortunately most of the UI models are tightly bind with the telerik UI tools, so to convert 
  our DAL layer model to UI layer telerik model, i faced difficulties and as i mentioned couldn't get enough time to build that part.
```
