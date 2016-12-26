using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MixPanelCSharp
{
    public class ClientDelivery
    {
        public List<JObject> EventsMessages { get; }
        public List<JObject> PeopleMessages { get; }

        public ClientDelivery()
        {
            EventsMessages = new List<JObject>();
            PeopleMessages = new List<JObject>();
        }

        public bool IsValidMessage(JObject message)
        {
            bool ret = true;
            try
            {
                int envelopeVersion = int.Parse(message["envelope_version"].ToString());
                if (envelopeVersion > 0)
                {
                    string messageType = message["message_type"].ToString();
                    JObject messageContent = 
                        JObject.Parse(message["message"].ToString());

                    if (messageContent == null)
                        ret = false;
                    else if (messageType != "event" && messageType != "people")
                        ret = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ret = false;
            }

            return ret;
        }

        public void AddMessage(JObject message)
        {
            if (!IsValidMessage(message))
            {
                throw new ApplicationException("Given JSONObject was not a valid Mixpanel message " + message);
            }
       
            try
            {
                string messageType = message["message_type"].ToString();
                JObject messageContent =
                    JObject.Parse(message["message"].ToString());

                if (messageType == "event")
                {
                    EventsMessages.Add(messageContent);
                }
                else if (messageType == "people")
                {
                    PeopleMessages.Add(messageContent);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Apparently valid mixpanel message could not be interpreted.", e);
            }
        }
    }
}
