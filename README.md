# MixPanelCSharp

MixPanelCSharp is a Java Port of Mixpanel API Client.

## Setup

Create an account in Mixpanel - https://mixpanel.com and set the below keys in the MixPanelCsharpDemo App

```
<appSettings>
    <add key="PROJECT_TOKEN" value="projectToken"/>
    <add key="API_KEY" value="apiKey"/>
</appSettings>
```

## Sample code

```
var builder = new MessageBuilder(PROJECT_TOKEN);
sampleProps = new JObject
{
    {"prop key", "prop value"},
    { "ratio", "\u03C0"}
};
sampleModifiers = new JObject
{
    {"$time", "A TIME"},
    {"Unexpected", "But OK"}
};
peopleData = new JObject
{
    {"prop key", "prop value"},
    {"temp_f", 46},
    {"temp_c", 8},
    {"humidity", "Humidity: 93%"},
    {"wind_condition", "Wind: SW at 18 mph"}
};

var jsonObjectDumper = new JsonObjectDumper();

ClientDelivery clientDelivery = new ClientDelivery();
var event1 = builder.Event("a distinct id", "login", sampleProps);
clientDelivery.AddMessage(event1);

Console.WriteLine("Event Message");
Console.WriteLine(jsonObjectDumper.WriteToString(event1));

var people1 = builder.Set("people2", peopleData);
clientDelivery.AddMessage(people1);

Console.WriteLine("People Message");
Console.WriteLine(jsonObjectDumper.WriteToString(people1));

var set = builder.Set("a distinct id", sampleProps);
clientDelivery.AddMessage(set);

var increments = new JObject { { "a key", 24L } };
JObject increment = builder.Increment("a distinct id", increments);
clientDelivery.AddMessage(increment);

// Create an instance of MixpanelAPI
var mixPanelAPI = new MixpanelAPI();
mixPanelAPI.Deliver(API_KEY, clientDelivery, false);
```
### Demo Data

![text](https://github.com/ranjancse26/MixPanelCSharp/blob/master/MixPanelDemoData.png)

### Running the Demo App on Raspberry PI

![text](https://github.com/ranjancse26/MixPanelCSharp/blob/master/MixPanelRaspberryPiDemo.png)

### Weather Data

![text](https://github.com/ranjancse26/MixPanelCSharp/blob/master/MixPanelWeatherData.png)
