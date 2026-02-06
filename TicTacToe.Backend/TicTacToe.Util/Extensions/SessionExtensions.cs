using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


public static class SessionExtensions
{
    public static void SetObject(this ISession session, string key, object value)
    {
        session.Set(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var ret = session.TryGetValue(key, out byte[] value);
        return value == null || value.Length == 0 ? default(T) : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value));
    }
}


