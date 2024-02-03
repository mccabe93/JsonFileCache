using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace JsonFileCache
{
    public class JsonFileCache<T>
    {
        private readonly string Path;
        private readonly ConcurrentDictionary<string, DateTime> _localCacheCreationTime;
        public readonly ConcurrentDictionary<string, T> LocalCache;

        public JsonFileCache(string path)
        {
            Path = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _localCacheCreationTime = new ConcurrentDictionary<string, DateTime>();
            LocalCache = LoadCache();
        }

        public void CacheItem(string name, T item)
        {
            if (LocalCache.TryAdd(name, item))
            {
                var json = JsonConvert.SerializeObject(item);
                File.WriteAllText($"{Path}{name}.json", json);
                _localCacheCreationTime.AddOrUpdate(name, DateTime.Now, (s, v) => v = DateTime.Now);
            }
        }

        public DateTime GetLastItemUpdate(string item)
        {
            if(_localCacheCreationTime.TryGetValue(item, out var lastUpdate))
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
                var json = File.ReadAllText(file);
                var info = new FileInfo(file);
                string itemName = info.Name.Substring(0, info.Name.Length - ".json".Length);
                _localCacheCreationTime.AddOrUpdate(itemName, info.CreationTime, (s, v) => v = info.CreationTime);
                var item = JsonConvert.DeserializeObject<T>(json);
                if(!data.TryAdd(itemName, item!))
                {
                    throw new Exception($"Failed adding {info.Name} to hash table.");
                }
            }
            return data;
        }
    }
}
