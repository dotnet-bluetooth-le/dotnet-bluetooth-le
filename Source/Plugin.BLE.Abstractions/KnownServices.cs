using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.Abstractions
{
    // Source: https://developer.bluetooth.org/gatt/services/Pages/ServicesHome.aspx
    public static class KnownServices
    {
        private static Dictionary<Guid, KnownService> _items;
        private static readonly object Lock = new object();

        static KnownServices()
        {

        }

        public static KnownService Lookup(Guid id)
        {
            lock (Lock)
            {
                if (_items == null)
                    LoadItemsFromJson();
            }

            if (_items.ContainsKey(id))
                return _items[id];
            else
                return new KnownService("Unknown Service", Guid.Empty);

        }

        public static void LoadItemsFromJson()
        {
            _items = new Dictionary<Guid, KnownService>();
            //TODO: switch over to ServiceStack.Text when it gets bound.
            KnownService service;
            string itemsJson = ResourceLoader.GetEmbeddedResourceString(typeof(KnownServices).GetTypeInfo().Assembly, "KnownServices.json");
            var json = JToken.Parse(itemsJson);

            var builder = new StringBuilder();
            foreach (var item in json.Children())
            {
                JProperty prop = item as JProperty;
                service = new KnownService(prop.Value.ToString(), Guid.ParseExact(prop.Name, "d"));
                _items.Add(service.Id, service);

                builder.AppendLine($"new KnownService(\"{service.Name}\", Guid.ParseExact(\"{service.Id}\", \"d\")),");
            }

            Debug.WriteLine(builder.ToString());
        }

    }

    public struct KnownService
    {
        public string Name { get; private set; }
        public Guid Id { get; private set; }

        public KnownService(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
    }

}

