using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace JsonFileCache
{
    public class JsonFileCache<T>
    {
        private readonly string Path;
        public readonly ConcurrentDictionary<string, T> LocalCache;

        public JsonFileCache(string path)
        {
            Path = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            LocalCache = LoadCache();
        }

        public void CacheItem(string name, T item)
        {
            if (LocalCache.TryAdd(name, item))
            {
                var json = JsonConvert.SerializeObject(item);
                File.WriteAllText($"{Path}{name}.json", json);
            }
        }

        public ConcurrentDictionary<string, T> LoadCache()
        {
            ConcurrentDictionary<string, T> data = new ConcurrentDictionary<string, T>();
            var files = Directory.GetFiles(Path);
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var info = new FileInfo(file);
                var item = JsonConvert.DeserializeObject<T>(json);
                if(!data.TryAdd(info.Name.Substring(0, info.Name.Length - ".json".Length), item!))
                {
                    throw new Exception($"Failed adding {info.Name} to hash table.");
                }
            }
            return data;
        }
    }
}
