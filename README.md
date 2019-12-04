# Dahomey.Json
 Extensions to System.Text.Json

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Dahomey.Json)
![CircleCI](https://img.shields.io/circleci/build/github/dahomey-technologies/Dahomey.Json/master)

## Introduction
The main purpose of this library is to bring missing features to the official .Net namespace [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)

## Features
* Extensible Polymorphism support based on discriminator conventions

## Installation
### NuGet
https://www.nuget.org/packages/Dahomey.Json/

`Install-Package Dahomey.Json`

### Compilation from source
  1. `dotnet restore`
  2. `dotnet pack -c Release`
  
## How to use Dahomey.Json
### Common Setup

```csharp
using Dahomey.Json;
using System.Text.Json;

JsonSerializerOptions options = new JsonSerializerOptions();
options.SetupExtensions();
```

### Polymorphism

In order to distinguish inherited classes from a reference to a base class or a collection of references to a base class, a special property called **discriminator** can be added to the serialized json.

To describe the type of the discriminator property as well as the association between a specific inherited Type and a specific discriminator value, a discriminator convention supporting the interface **IDiscriminatorConvention** should be written.

Discriminator conventions can be registered to the *discriminator convention registry* accessible from the JsonSerilizedOptions.

Several conventions can be registered. When registering an inherited Type, an attempt to register it to each convention, begining with the last convention register. The first convention to accept to register a specific type will stop the registration process.
It means that different type inheritance hierarchy could serialize/deserialize their discriminator property in a different way.

The library offers 2 built-in discriminator conventions:
- **DefaultDiscriminatorConvention**: the discriminator value is the type [Fully Qualified Name](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/specifying-fully-qualified-type-names)
- **AttributeBasedDiscriminatorConvention<T>**: the discriminator value is defined by decorating classes with the attribute **JsonDiscriminatorAttribute**

Both built-in conventions setup the convention property name to **$type**

#### DefaultDiscriminatorConvention

If the actual type of a reference instance differs from the declared type, the discriminator property will be automatically added to the output json:

```csharp
public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string Summary { get; set; }
}
```

```csharp
public class WeatherForecastDerived : WeatherForecast
{
    public int WindSpeed { get; set; }
}
```

Inherited classes must be manually registered to the discriminator convention registry in order to let the framework know about the mapping between a discriminator value and a type
```csharp
JsonSerializerOptions options = new JsonSerializerOptions();
options.SetupExtensions();
DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
registry.RegisterType<WeatherForecastDerived>();

string json = JsonSerializer.Serialize<WeatherForecast>(weatherForecastDerived, options);
```

```json
{
  "$type": "Tests.WeatherForecastDerived, Tests",
  "Date": "2019-08-01T00:00:00-07:00",
  "TemperatureCelsius": 25,
  "Summary": "Hot",
  "WindSpeed": 35
}
```

This convention is added by default to the JsonSerializedOptions but can be removed by clearing the discriminator conventions:

```csharp
JsonSerializerOptions options = new JsonSerializerOptions();
options.SetupExtensions();
DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
registry.ClearConventions();
```

The discriminator property name can be changed by clearing the registry and manually registring the default convention:
```csharp
JsonSerializerOptions options = new JsonSerializerOptions();
options.SetupExtensions();
DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
registry.ClearConventions();
registry.RegisterConvention(new DefaultDiscriminatorConvention(options, "_t"));
registry.RegisterType<WeatherForecastDerived>();

string json = JsonSerializer.Serialize<WeatherForecast>(weatherForecastDerived, options);
```

```json
{
  "_t": "Tests.WeatherForecastDerived, Tests",
  "Date": "2019-08-01T00:00:00-07:00",
  "TemperatureCelsius": 25,
  "Summary": "Hot",
  "WindSpeed": 35
}
```

#### AttributeBasedDiscriminatorConvention<T>
 
 This type of the discriminator value is configured via the generic parameter of the convention class.
 The value type passed to the JsonDiscriminatorAttribute must match this type.
 
 ```csharp
public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string Summary { get; set; }
}
```

```csharp
[JsonAttribute(1234)]
public class WeatherForecastDerived : WeatherForecast
{
    public int WindSpeed { get; set; }
}
```
 
 ```csharp
JsonSerializerOptions options = new JsonSerializerOptions();
options.SetupExtensions();
DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
registry.ClearConventions();
registry.RegisterConvention(new AttributeBasedDiscriminatorConvention<int>(options, "_t"));
registry.RegisterType<WeatherForecastDerived>();

string json = JsonSerializer.Serialize<WeatherForecast>(weatherForecastDerived, options);
```

```json
{
  "_t": 1234,
  "Date": "2019-08-01T00:00:00-07:00",
  "TemperatureCelsius": 25,
  "Summary": "Hot",
  "WindSpeed": 35
}
```

#### Discriminator policies

- **Auto** (default value): the discriminator property is written only the declared type and the actual type are different.
- **Always**: the discriminator property is forced to always be written.
- **Never**: he discriminator property is forced to never be written

 ```csharp
JsonSerializerOptions options = new JsonSerializerOptions();
options.SetupExtensions();
DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
registry.DiscriminatorPolicy = DiscriminatorPolicy.Always;
```
