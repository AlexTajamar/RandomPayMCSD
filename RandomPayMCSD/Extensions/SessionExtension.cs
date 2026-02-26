using Newtonsoft.Json;

namespace RandomPayMCSD.Extensions
{
    public static class SessionExtension
    {
        public static void setObject (this ISession session,string key ,object value)
        {
            string json = JsonConvert.SerializeObject(value);
            session.SetString(key, json);
        }
        public static T getObject<T>(this ISession session ,string key)
        {
            string data = session.GetString(key);
            if(data==null)
            {
                return default(T);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
        }
    }
}
