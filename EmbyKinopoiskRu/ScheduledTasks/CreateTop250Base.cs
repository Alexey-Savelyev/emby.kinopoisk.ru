using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public abstract class CreateTop250Base
    {
        protected ILogger Log { get; private set; }
        protected ILibraryManager LibraryManager { get; private set; }
        protected ICollectionManager CollectionManager { get; private set; }
        public string Key { get; private set; }
        protected Plugin Plugin { get; private set; }
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations = new Dictionary<string, string>();


        protected CreateTop250Base(
            ILibraryManager libraryManager,
            ICollectionManager collectionManager,
            ILogger log,
            IJsonSerializer jsonSerializer,
            IServerConfigurationManager serverConfigurationManager,
            string key)
        {
            Log = log;
            CollectionManager = collectionManager;
            LibraryManager = libraryManager;
            Key = key;
            _jsonSerializer = jsonSerializer;
            _serverConfigurationManager = serverConfigurationManager;
            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            Plugin = Plugin.Instance;

            _availableTranslations = EmbyHelper.GetAvailableTransactions($"ScheduledTasks.{Key}");
        }

        public virtual IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        protected async Task UpdateLibrary(List<BaseItem> itemsList, string collectionName)
        {
            if (!itemsList.Any())
            {
                return;
            }
            Log.Info($"Check if '{collectionName}' already exists");
            QueryResult<BaseItem> existingCollectionResult = LibraryManager.QueryItems(new InternalItemsQuery()
            {
                IncludeItemTypes = new[] { "BoxSet" },
                Recursive = false,
                Name = collectionName
            });
            Log.Info($"Found {existingCollectionResult.TotalRecordCount} collections: '{string.Join("', '", existingCollectionResult.Items.Select(m => m.Name))}'");
            if (existingCollectionResult.TotalRecordCount == 1)
            {
                var existingCollection = (BoxSet)existingCollectionResult.Items[0];
                Log.Info($"Updating collection with name '{existingCollection.Name}' with following internal ids: '{string.Join(", ", itemsList.Select(m => m.InternalId))}'");
                foreach (BaseItem item in itemsList)
                {
                    if (item.AddCollection(existingCollection))
                    {
                        Log.Info($"Adding '{item.Name}' (internalId '{item.InternalId}') to collection '{existingCollection.Name}'");
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        Log.Info($"'{item.Name}' (internalId '{item.InternalId}') already in the collection '{existingCollection.Name}'");
                    }
                }
            }
            else
            {
                Log.Info($"Creating '{collectionName}' collection with following items: '{string.Join("', '", itemsList.Select(m => m.Name))}'");
                CollectionFolder rootCollectionFolder = await EmbyHelper.InsureCollectionLibraryFolder(LibraryManager, Log);
                if (rootCollectionFolder == null)
                {
                    Log.Info($"The virtual folder 'Collections' was not found nor created. {collectionName} will not be created");
                }
                else
                {
                    _ = await CollectionManager.CreateCollection(new CollectionCreationOptions()
                    {
                        IsLocked = false,
                        Name = collectionName,
                        ParentId = rootCollectionFolder.InternalId,
                        ItemIdList = itemsList.Select(m => m.InternalId).ToArray()
                    });
                    Log.Info("The collection created");
                }
            }
        }

        protected TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
    }
}
