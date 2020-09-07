using inRiver.Remoting.Extension;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace inRiverCommunity.Extensions.WorkAreas
{
    // TODO: Document
    public class WorkAreaFolderCache
    {


        // TODO: Document
        public List<WorkAreaFolderCacheItem> ItemList;


        // TODO: Document
        /// <summary>
        /// Doesn't initialize the cache
        /// </summary>
        public WorkAreaFolderCache() { }

        // TODO: Document
        /// <summary>
        /// Initializes the cache
        /// </summary>
        /// <param name="context">inRiverContext used to initialize the cache</param>
        public WorkAreaFolderCache(inRiverContext context)
        {
            CacheAllWorkAreaFolders(context);
        }


        // TODO: Document
        public void CacheAllWorkAreaFolders(inRiverContext context)
        {
            // Create new cache
            var newCache = new List<WorkAreaFolderCacheItem>();


            // Cache all shared work area folders
            try
            {
                var sharedWorkAreaFolderList = context.ExtensionManager.UtilityService.GetAllSharedWorkAreaFolders(false);

                if (sharedWorkAreaFolderList?.Count > 0)
                {
                    foreach (var workAreaFolder in sharedWorkAreaFolderList)
                        CacheWorkAreaFolder(workAreaFolder, newCache, context);
                }
            }
            catch (Exception ex)
            {
                context.Log(LogLevel.Error, "Failed to get and cache all shared work area folders!", ex);
            }


            // Get a list of all users
            List<User> userList = null;

            try
            {
                userList = context.ExtensionManager.UserService.GetAllUsers();
            }
            catch (Exception ex)
            {
                context.Log(LogLevel.Warning, "Failed to get list of users!", ex);
            }


            // Get and cache work area folders for user
            if (userList?.Count > 0)
            {
                foreach (var user in userList)
                {
                    try
                    {
                        var userWorkAreaFolderList = context.ExtensionManager.UtilityService.GetAllPersonalWorkAreaFoldersForUser(user.Username, false);

                        if (userWorkAreaFolderList?.Count >= 0)
                            return;
                    }
                    catch (Exception ex)
                    {
                        context.Log(LogLevel.Warning, $"Failed to get and cache all work area folders for username '{user.Username}'!", ex);
                    }
                }
            }


            // Overwrite global cache
            ItemList = newCache;
        }


        // TODO: Document
        private void CacheWorkAreaFolder(WorkAreaFolder workAreaFolder, List<WorkAreaFolderCacheItem> cache, inRiverContext context)
        {
            // Validate work area folder
            if (workAreaFolder == null)
                return;


            // Create cache item
            var item = new WorkAreaFolderCacheItem
            {
                Id = workAreaFolder.Id,
                EntityIdList = new List<int>()
            };


            try
            {
                // Get the work area again because we don't know if it was loaded with the entities or not
                // Get the work area again because we don't get the query when we get all work areas (TODO: Report as bug to inRiver)
                if (!string.IsNullOrEmpty(workAreaFolder.Username))
                {
                    item.FolderType = WorkAreaFolderType.Personal;
                    item.Folder = context.ExtensionManager.UtilityService.GetPersonalWorkAreaFolder(item.Id);
                }
                else
                {
                    item.FolderType = WorkAreaFolderType.Shared;
                    item.Folder = context.ExtensionManager.UtilityService.GetSharedWorkAreaFolder(item.Id);
                }

                if (workAreaFolder == null)
                    throw new NullReferenceException($"Fetched folder with id '{item.Id}' is null!");
            }
            catch (Exception ex)
            {
                context.Logger.Log(LogLevel.Error, $"Failed to get work area folder with id '{item.Id}'!", ex);
                return;
            }


            // Execute query and store result entity ids
            if (item.Folder.IsQuery && item.Folder.Query != null)
            {
                try
                {
                    var result = context.ExtensionManager.DataService.Search(item.Folder.Query, LoadLevel.Shallow);

                    if (result?.Count > 0)
                        item.EntityIdList.AddRange(result.Select(e => e.Id).ToList());
                }
                catch (Exception ex)
                {
                    context.Logger.Log(LogLevel.Warning, $"Failed to execute query for work area folder with id '{item.Id}'!", ex);
                }
            }
            // Store entity ids if no query
            else
            {
                if (item.Folder.FolderEntities?.Count > 0)
                    item.EntityIdList.AddRange(item.Folder.FolderEntities);
            }


            // Add item to cache
            var existingItem = cache.SingleOrDefault(f => f.Id == workAreaFolder.Id);

            if (existingItem != null)
                existingItem = item;
            else
                cache.Add(item);
        }


    }
}
