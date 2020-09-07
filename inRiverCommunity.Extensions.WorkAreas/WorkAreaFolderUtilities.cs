using inRiver.Remoting.Extension;
using inRiver.Remoting.Log;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace inRiverCommunity.Extensions.WorkAreas
{
    // TODO: Document
    public static class WorkAreaFolderUtilities
    {


        // TODO: Document
        public class Settings
        {
            public bool HitCounterEnabled = true;

            // TODO: Add more hit counter settings here

            // TODO: Add OrQuery and AndQuery settings here
        }


        // TODO: Document
        public static void UpdateAllWorkAreaFolders(inRiverContext context, Settings settings = null)
        {
            // Validate and adjust input
            if (context == null)
                return;

            if (settings == null)
                settings = new Settings();


            // Get and initialize the cache
            var cache = new WorkAreaFolderCache(context);


            // Update each work area folder
            foreach (var item in cache.ItemList)
            {
                // Validate item
                if (item == null)
                    continue;


                #region Hit Counter

                if (settings.HitCounterEnabled)
                {
                    // TODO: implement code below
                    /*
                    // Get number of entities
                    var numberOfEntities = item.EntityIdList?.Count();


                    // Get new name if needed
                    var newName = item.Folder.Name;

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
                        context.Logger.Log(LogLevel.Warning, $"Failed to generate new name for work area folder with id '{item.Id}'!", ex);
                    }


                    // Update work area folder with new name if needed
                    if (newName != item.Folder.Name)
                    {
                        try
                        {
                            if (item.FolderType == WorkAreaFolderType.Personal)
                                context.ExtensionManager.UtilityService.UpdatePersonalWorkAreaFolderName(item.Id, newName);
                            else
                                context.ExtensionManager.UtilityService.UpdateSharedWorkAreaFolderName(item.Id, newName);
                        }
                        catch (Exception ex)
                        {
                            context.Logger.Log(LogLevel.Warning, $"Failed to update name for work area folder with id '{item.Id}' for user '{item.Username}'!", ex);
                        }
                    }
                    */
                }

                #endregion

                #region OrQuery

                // TODO: Add code to calculate child folder matches and set to static work area if folder is marked to be executed as OrQuery

                #endregion

                #region AndQuery

                // TODO: Add code to calculate child folder matches and set to static work area if folder is marked to be executed as AndQuery

                #endregion
            }
        }


    }
}
