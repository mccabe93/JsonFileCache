using Newtonsoft.Json;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JsonFileCache
{
    public class JsonFileCache<T>
    {
        private readonly string Path;
        private readonly ConcurrentDictionary<string, DateTime> _localCacheCreationTime;
        private readonly FileSystemWatcher _cacheFolderWatcher = new FileSystemWatcher();
        private static readonly object _lock = new object();

        public readonly ConcurrentDictionary<string, T> LocalCache;

        public JsonFileCache(string path)
        {
            Path = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _localCacheCreationTime = new ConcurrentDictionary<string, DateTime>();

            _cacheFolderWatcher = new FileSystemWatcher(path);
            _cacheFolderWatcher.Filter = "*.json";
            _cacheFolderWatcher.Created += _cacheFolderWatcher_Created;
            _cacheFolderWatcher.Deleted += _cacheFolderWatcher_Deleted;
            _cacheFolderWatcher.EnableRaisingEvents = true;

            LocalCache = LoadCache();
        }

        private void _cacheFolderWatcher_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void _cacheFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            var item = LoadCacheItem(e.FullPath);
            CacheItem(item.cacheItemName, item.cacheItem);
        }

        public void CacheItem(string name, T item)
        {
            lock (_lock)
            {
                if (LocalCache.TryAdd(name, item))
                {
                    var json = JsonConvert.SerializeObject(item);
                    File.WriteAllText($"{Path}{name}.json", json);
                    _localCacheCreationTime.AddOrUpdate(name, DateTime.Now, (s, v) => v = DateTime.Now);
                }
            }
        }

        public void RemoveItem(string name)
        {
            lock(_lock)
            {
                if(LocalCache.TryRemove(name, out _))
                {
                    File.Delete($"{Path}{name}.json");
                    _localCacheCreationTime.TryRemove(name, out _);
                }
            }
        }

        public DateTime GetLastItemUpdate(string item)
        {
            if (_localCacheCreationTime.TryGetValue(item, out var lastUpdate))
            {
                return lastUpdate;
            }
            return DateTime.MinValue;
        }

        public ConcurrentDictionary<string, T> LoadCache()
        {
            ConcurrentDictionary<string, T> data = new ConcurrentDictionary<string, T>();
            var files = Directory.GetFiles(Path);
            foreach (var file in files)
            {
                var item = LoadCacheItem(file);
                if (!data.TryAdd(item.cacheItemName, item.cacheItem!))
                {
                    throw new Exception($"Failed adding {item.cacheItemName} to hash table. Please ensure the cache folder only contains items for this cache.");
                }
            }
            return data;
        }

        private (string cacheItemName, T cacheItem) LoadCacheItem(string file)
        {
            var json = File.ReadAllText(file);
            var info = new FileInfo(file);
            string itemName = info.Name.Substring(0, info.Name.Length - ".json".Length);
            _localCacheCreationTime.AddOrUpdate(itemName, info.CreationTime, (s, v) => v = info.CreationTime);
            return (itemName, JsonConvert.DeserializeObject<T>(json) ?? throw new Exception($"Could not deserialize {itemName} @ {file}"));
        }
    }
}
