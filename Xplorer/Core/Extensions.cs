using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace WebSite.Core
{
    public static class Extensions
    {
        public static string ToJson(this object o, int recursionLimit)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionLimit;
            return serializer.Serialize(o);
        }
        public static string ToJson(this object o)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(o);
        }
    }
}