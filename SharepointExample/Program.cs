using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.SharePoint.Client;

namespace SharePointFolderCreation
{
    class Program
    {
        static void Main(string[] args)
        {
            string siteUrl = "https://yoursharepointsite.sharepoint.com/sites/yoursite";

            // Credentials
            string username = "yourusername@yourdomain.com";
            string password = "yourpassword";

            // Folder details
            string libraryName = "Documents";  // Name of your document library
            string generalFolderName = "General";  // Name of the existing folder inside Documents
            string projectFolderName = Guid.NewGuid().ToString();

            // Create a ClientContext
            using (ClientContext context = new ClientContext(siteUrl))
            {
                // Authentication
                SecureString securePassword = new SecureString();
                foreach (char c in password.ToCharArray()) securePassword.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(username, securePassword);

                // Create folders
                CreateNestedFolders(context, libraryName, generalFolderName, projectFolderName);
            }

            Console.WriteLine("Folders created successfully!");
            Console.ReadLine();
        }

        static void CreateNestedFolders(ClientContext context, string libraryName, string generalFolderName, string projectFolderName)
        {
            // Get the document library
            List library = context.Web.Lists.GetByTitle(libraryName);
            context.Load(library.RootFolder);
            context.ExecuteQuery();

            // Ensure the General folder exists
            Folder generalFolder = EnsureFolder(context, library.RootFolder, generalFolderName);
            //context.Load(generalFolder, pf => pf.ServerRelativeUrl); // Load the ServerRelativeUrl property

            // Create the project folder inside General
            Folder projectFolder = generalFolder.Folders.Add(projectFolderName);
            context.Load(projectFolder, pf => pf.ServerRelativeUrl); // Load the ServerRelativeUrl property
            context.ExecuteQuery();

            // Folder structure to create under the project folder
            string[] folders = { "Folder1", "Folder2", "Folder3", "Folder4", "Folder5" };

            foreach (var folder in folders)
            {
                string folderUrl = $"{projectFolder.ServerRelativeUrl}/{folder}";
                projectFolder.Folders.Add(folder);
            }

            context.ExecuteQuery();
        }

        static Folder EnsureFolder(ClientContext context, Folder parentFolder, string folderName)
        {
            // Check if the folder already exists
            Folder folder = parentFolder.Folders.Add(folderName);
            try
            {
                context.Load(folder);
                context.ExecuteQuery();
            }
            catch (ServerException ex) when (ex.Message.Contains("File Not Found"))
            {
                // Folder does not exist, so we create it
                folder = parentFolder.Folders.Add(folderName);
                context.ExecuteQuery();
            }

            return folder;
        }
    }
}
