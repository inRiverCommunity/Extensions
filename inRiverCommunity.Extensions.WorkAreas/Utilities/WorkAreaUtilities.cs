using inRiver.Remoting.Extension;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace inRiverCommunity.Extensions.WorkAreas.Utilities
{
    public class WorkAreaUtilities
    {


        public static void UpdateAllWorkAreaFolders(inRiverContext context)
        {
            UpdateAllSharedWorkAreaFolders(context);
            UpdateAllUserWorkAreaFolders(context);
        }


        public static void UpdateAllSharedWorkAreaFolders(inRiverContext context)
        {
            // Get the list of the shared work area folders
            List<WorkAreaFolder> sharedWorkAreaFolderList = null;

            try
            {
                sharedWorkAreaFolderList = context.ExtensionManager.UtilityService.GetAllSharedWorkAreaFolders(true);
            }
            catch (Exception ex)
            {
                context.Log(LogLevel.Warning, "Failed to update shared work area folders!", ex);
            }

            if (sharedWorkAreaFolderList?.Count == 0)
                return;


            // Updates all shared work area folders
            foreach (var workArea in sharedWorkAreaFolderList)
                UpdateWorkAreaFolder(context, workArea);
        }


        public static void UpdateAllUserWorkAreaFolders(inRiverContext context)
        {
            // Get the list of users
            List<User> userList = null;

            try
            {
                userList = context.ExtensionManager.UserService.GetAllUsers();
            }
            catch (Exception ex)
            {
                context.Log(LogLevel.Warning, "Failed to get list of users!", ex);
            }

            if (userList?.Count == 0)
                return;


            foreach (var user in userList)
                UpdateAllWorkAreaFoldersForUsername(context, user.Username);
        }


        public static void UpdateAllWorkAreaFoldersForUsername(inRiverContext context, string username)
        {
            try
            {
                var userWorkAreaFolderList = context.ExtensionManager.UtilityService.GetAllPersonalWorkAreaFoldersForUser(username, true);

                if (userWorkAreaFolderList?.Count == 0)
                    return;


                foreach (var workAreaFolder in userWorkAreaFolderList)
                    UpdateWorkAreaFolder(context, workAreaFolder, username);
            }
            catch (Exception ex)
            {
                // TODO: This will some times fail if user hasn't logged in before?
                context.Log(LogLevel.Warning, "Failed to get work areas for user: " + username, ex);
            }
        }


        public static void UpdateWorkAreaFolder(inRiverContext context, WorkAreaFolder workAreaFolder, string username = null)
        {
            if (workAreaFolder == null)
                return;


            var numberOfEntities = 0;

            if (workAreaFolder.IsQuery)
            {
                try
                {
                    // Get the work area again because we don't get the query when we get all work areas (TODO: Report as bug to inRiver)
                    if (!string.IsNullOrEmpty(username))
                        workAreaFolder = context.ExtensionManager.UtilityService.GetPersonalWorkAreaFolder(workAreaFolder.Id);
                    else
                        workAreaFolder = context.ExtensionManager.UtilityService.GetSharedWorkAreaFolder(workAreaFolder.Id);
                }
                catch (Exception ex)
                {
                    context.Logger.Log(LogLevel.Warning, $"Failed to get work area folder with id '{workAreaFolder?.Id}' for user '{username}'!", ex);
                    return;
                }

                if (workAreaFolder?.Query == null)
                    return;


                try
                {
                    var result = context.ExtensionManager.DataService.Search(workAreaFolder.Query, LoadLevel.Shallow);

                    if (result != null)
                        numberOfEntities = result.Count;
                }
                catch (Exception ex)
                {
                    context.Logger.Log(LogLevel.Warning, $"Failed to execute query for work area folder with id '{workAreaFolder?.Id}' for user '{username}'!", ex);
                }
            }
            else
            {
                if (workAreaFolder.FolderEntities?.Count > 0)
                    numberOfEntities = workAreaFolder.FolderEntities.Count;
            }


            // Get new name if needed
            var newName = workAreaFolder.Name;

            try
            {
                Regex regex = new Regex(@"^\(\d+\) .+");
                Match match = regex.Match(newName);

                if (match.Success)
                {
                    // TODO: Check match instead of substring?

                    var numberString = newName.Substring(1, newName.IndexOf(')') - 1);
                    var nameAfterNumber = newName.Substring(newName.IndexOf(')') + 2);

                    if (int.TryParse(numberString, out int currentNumber))
                    {
                        if (currentNumber == numberOfEntities)
                            return;


                        newName = $"({numberOfEntities}) {nameAfterNumber}";
                    }
                }
                else
                {
                    newName = $"({numberOfEntities}) {newName}";
                }
            }
            catch (Exception ex)
            {
                context.Logger.Log(LogLevel.Warning, $"Failed to get new name for work area folder with id '{workAreaFolder?.Id}' for user '{username}'!", ex);
            }

            if (newName == workAreaFolder.Name)
                return;


            // Update work area folder with new name
            try
            {
                if (!string.IsNullOrEmpty(username))
                    context.ExtensionManager.UtilityService.UpdatePersonalWorkAreaFolderName(workAreaFolder.Id, newName);
                else
                    context.ExtensionManager.UtilityService.UpdateSharedWorkAreaFolderName(workAreaFolder.Id, newName);
            }
            catch (Exception ex)
            {
                context.Logger.Log(LogLevel.Warning, $"Failed to update name for work area folder with id '{workAreaFolder?.Id}' for user '{username}'!", ex);
            }
        }


    }
}
