using inRiver.Remoting.Objects;
using System;
using System.Collections.Generic;

namespace inRiverCommunity.Extensions.WorkAreas
{
    // TODO: Document
    public class WorkAreaFolderCacheItem
    {


        public Guid Id { get; set; }

        public WorkAreaFolderType FolderType { get; set; }

        public WorkAreaFolder Folder { get; set; }

        public List<int> EntityIdList { get; set; }


        public string Username
        {
            get
            {
                return Folder?.Username;
            }
        }


        public Guid? ParentId
        {
            get
            {
                return Folder?.ParentId;
            }
        }

        public bool IsQuery
        {
            get
            {
                if (Folder == null)
                    return false;

                return Folder.IsQuery;
            }
        }


    }
}
