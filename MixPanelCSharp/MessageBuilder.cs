using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MixPanelCSharp
{
    public class MessageBuilder
    {
        private string ApiToken { get; }
        private const string ENGAGE_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss";

        public MessageBuilder(string apiToken)
        {
            ApiToken = apiToken;
        }

        public JObject Set(string distinctId, JObject properties)
        {
            return PeopleMessage(distinctId, "$set", properties, null);
        }

        public JObject Set(string distinctId, JObject properties, JObject modifiers)
        {
            return PeopleMessage(distinctId, "$set", properties, modifiers);
        }

        public JObject SetOnce(string distinctId, JObject properties, JObject modifiers)
        {
            return PeopleMessage(distinctId, "$set_once", properties, modifiers);
        }

        public JObject Delete(string distinctId)
        {
            return Delete(distinctId, null);
        }

        public JObject Delete(string distinctId, JObject modifiers)
        {
            return PeopleMessage(distinctId, "$delete", new JObject(), modifiers);
        }

        private JObject PeopleMessage(string distinctId, string actionType,
                  object properties, JObject modifiers)
        {
            JObject dataObj = new JObject();
            if (null == properties)
            {
                throw new ApplicationException("Cannot send null properties, use JSONObject.NULL instead");
            }

            try
            {
                if (properties.GetType() == typeof (JArray))
                    dataObj.Add(actionType, (JArray) properties);
                else
                    dataObj.Add(actionType, (JObject)properties);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Cannot interpret properties as a JSON payload", e);
            }

            // At this point, nothing should ever throw a JSONException
            try
            {
                long time = DateTime.Now.Ticks;
                dataObj.Add("$token", ApiToken);
                dataObj.Add("$distinct_id", distinctId);
                dataObj.Add("$time", time);

                if (modifiers != null)
                {
                    var jsonProperties = new JObject(modifiers.ToString());
                    foreach (JProperty property in jsonProperties.Properties())
                    {
                        dataObj[property.Name] = property.Value;
                    }
                }
               
                return ConstructEnvelope(dataObj, "people");
            }
            catch (Exception e)
            {
                throw new ApplicationException("Can't construct a Mixpanel message", e);
            }
        }

        public JObject Event(string distinctId,
                             string eventName, JObject properties)
        {
            try
            {
                long time = DateTime.Now.Ticks;
                var dataObject = new JObject
                {
                    {"event", eventName}
                };

                if (properties != null)
                {
                    if(properties.Property("token") == null)
                        properties.Add("token", ApiToken);

                    if (properties.Property("mp_lib") == null)
                        properties.Add("mp_lib", "jdk");

                    if (properties.Property("time") == null)
                        properties.Add("time", time);

                    if (properties.Property("distinct_id") == null)
                        properties.Add("distinct_id", distinctId);
                    
                    dataObject.Add("properties", properties);
                }
                
                return ConstructEnvelope(dataObject, "event");
            }
            catch (Exception e)
            {
                throw new ApplicationException("Can't construct a Mixpanel message", e);
            }
        }

        public JObject Append(string distinctId, JObject properties)
        {
            return Append(distinctId, properties, null);
        }

        public JObject Append(string distinctId, JObject properties, JObject modifiers)
        {
            return PeopleMessage(distinctId, "$append", properties, modifiers);
        }

        public JObject Increment(string distinctId, JObject properties)
        {
            return Increment(distinctId, properties, null);
        }

        public JObject Increment(string distinctId, JObject properties, JObject modifiers)
        {
            return PeopleMessage(distinctId, "$add", properties, modifiers);
        }

        public JObject Union(string distinctId, JObject properties, JObject modifiers)
        {
            return PeopleMessage(distinctId, "$union", properties, modifiers);
        }

        public JObject Unset(string distinctId, List<string> propertyNames, JObject modifiers)
        {
            var propNamesArray = new JArray(propertyNames);
            return PeopleMessage(distinctId, "$unset", propNamesArray, modifiers);
        }

        public JObject TrackCharge(string distinctId, double amount, JObject properties)
        {
            return TrackCharge(distinctId, amount, properties, null);
        }

        public JObject TrackCharge(string distinctId, double amount,
                  JObject properties, JObject modifiers)
        {
            var transactionValue = new JObject();
            var appendProperties = new JObject();

            try
            {
                transactionValue.Add("$amount", amount);
                transactionValue.Add("$time",DateTime.Now.ToString(ENGAGE_DATE_FORMAT));
                if (properties != null)
                {
                    foreach (JProperty property in properties.Properties())
                    {
                        transactionValue.Add(property);
                    }
                }
                appendProperties.Add("$transactions", transactionValue);
                return Append(distinctId, appendProperties, modifiers);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw new ApplicationException("Cannot create trackCharge message", ex);
            }
        }

        // Build Envelope Json Object
        private JObject ConstructEnvelope(JObject dataObject,
            string messageType)
        {
            return new JObject
            {
                {"envelope_version", 1},
                {"message_type", messageType},
                {"message", dataObject}
            };
        }
    }
}
