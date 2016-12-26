using Newtonsoft.Json;

namespace MixPanelCSharpDemo
{
    public class JsonObjectDumper
    {
        public string WriteToString(object objectToDump)
        {
            return JsonConvert.SerializeObject(objectToDump, Formatting.Indented);
        }
    }
}