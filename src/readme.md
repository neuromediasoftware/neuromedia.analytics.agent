# NeuroMedia Analytics Agent

## Description
NeuroMedia Analytics Agent is a service that aims to deliver logs related to content delivery services that are measured by NeuroMedia Analytics. The service can be configured to pre-process, filter and even enrich this data before it is sent to meet specific needs.

The bot will listen to changes in price accross all coins on Binance. By default we're only picking USDT pairs. We're excluding Margin (like BTCDOWNUSDT) and Fiat pairs

> The main features are

- Extraction et expédition du statut en direct
- Support for Icecast
- Support for SHOUTcast
- Support for Wowza Media Server
- Log Shipping (soon)

## How to use

### Configure

First, you need to create a configuration file that will allow you to specify what you want the agent to do. This is a JSON file with the following format.

```csharp
{
  "name": "Demo Agent",
  "apiKey": "PUT YOUR API KEY HERE",
  "endpoint":  "https://example.org",
  "sources": [
    {
      "name": "streamname",
      "uri": "https://admin:pass@example.org/publishingPoint",
      "parserName": "IcecastLiveStateParser",
      "frequency": 1,
      "filters": [
          "LiveStateMaxMindLocationDetectionFilter",
          "LiveStateIpHashingFilter"
        ]
    }
  ]
}

```

#### General settings

Instead of using an external file, you can also edit the `appsettings.json`.

`name`: name of the agent

`apiKey`: API key to use to ship data

`endpoint`: the server to which you want to deliver the data

`sources`: the different sources of the data to be delivered

#### Sources 
Each source can be configured as follows

`name`: name of the source

`uri`: location of the data

`parserName`: specification of the data

`frequency`: frequency of data extraction in minutes

`filters`: filters to apply to the data before shipping

#### Available filters

`LiveStateMaxMindLocationDetectionFilter`: geolocate the devices using MaxMind Geo databases. The propertly licensed database has to be located in the agent directory and download from MaxMind 

`LiveStateIpHashingFilter`: hash the IP address (pseudoanonymization)

### Build &amp; Run the agent

#### Build and run the tool with appsettings.json as the configuration
```bash
dotnet restore
dotnet build
dotnet run
```

#### Build and run the tool with an external configuration file
```bash
dotnet restore
dotnet build
dotnet run agentConfig:name="your agent name" agentConfig:configFile="sampleconfig.json"
```

### Install as a windows service

You can also install the agent on Windows as a service. To do this, use the following commands to install and uninstall it

```bash
install-agent-service.ps1
```

```bash
uninstall-agent-service.ps1
```

