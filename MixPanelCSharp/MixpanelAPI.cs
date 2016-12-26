using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace MixPanelCSharp
{
    public class MixpanelAPI
    {
        protected string eventsEndpoint;
        protected string peopleEndpoint;

        public MixpanelAPI()
        {
            eventsEndpoint = Config.BASE_ENDPOINT + "/track";
            peopleEndpoint = Config.BASE_ENDPOINT + "/engage";
        }

        public MixpanelAPI(string eventsEndpoint, string peopleEndpoint)
        {
            this.eventsEndpoint = eventsEndpoint;
            this.peopleEndpoint = peopleEndpoint;
        }

        public void SendMessage(string apiKey, JObject message)
        {
            ClientDelivery delivery = new ClientDelivery();
            delivery.AddMessage(message);
            Deliver(apiKey, delivery);
        }

        public void Deliver(string apiKey, ClientDelivery toSend)
        {
            Deliver(apiKey, toSend, false);
        }

        public void Deliver(string apiKey, ClientDelivery toSend, bool useIpAddress)
        {
            // Send Event Messages
            string eventsUrl = eventsEndpoint; 
            List<JObject> events = toSend.EventsMessages;
            SendMessages(apiKey, events, eventsUrl);

            // Send People Messages
            string peopleUrl = peopleEndpoint;
            List<JObject> people = toSend.PeopleMessages;
            SendMessages(apiKey, people, peopleUrl);
        }

        private void SendMessages(string apiKey, List<JObject> messages, string endpointUrl)
        {
            for (int i = 0; i < messages.Count; i += Config.MAX_MESSAGE_SIZE)
            {
                int endIndex = i + Config.MAX_MESSAGE_SIZE;
                endIndex = Math.Min(endIndex, messages.Count);
                List<JObject> batch = messages.GetRange(i, endIndex);

                if (batch.Count > 0)
                {
                    string messagesString = DataString(batch);
                    bool accepted = SendData(apiKey, messagesString, endpointUrl);

                    if (!accepted)
                    {
                        string errorMessage = "Server refused to accept messages, they may be malformed." + batch;
                        Console.WriteLine(errorMessage);
                        throw new  ApplicationException(errorMessage);
                    }
                }
            }
        }

        private string DataString(List<JObject> messages)
        {
            JArray array = new JArray();
            foreach (var message in messages)
            {
                array.Add(message);
            }
            return array.ToString();
        }

        protected string EncodeDataString(string dataString)
        {
            try
            {
                return Base64Coder.ToBase64(Encoding.UTF8.GetBytes
                    (dataString));
            }
            catch (Exception e)
            {
                throw new ApplicationException("Mixpanel library requires utf-8 support", e);
            }
        }

        bool SendData(string apiKey, string dataString, string endpointUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

            string requestUrl = $"{endpointUrl}?data={EncodeDataString(dataString)}&api_key={apiKey}&verbose={1}&ip={0}";
            var client = new RestClient(requestUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "=", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response != null && response.Content != "")
            {
                dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
                return int.Parse(jsonResponse["status"].Value.ToString()) == 1;
            }

            return false;
        }
    }
}
