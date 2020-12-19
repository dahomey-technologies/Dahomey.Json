# Dahomey.Json
 Extensions to System.Text.Json

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Dahomey.Json)
![](https://github.com/dahomey-technologies/Dahomey.Json/workflows/Build%20and%20Test/badge.svg)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

## Introduction
The main purpose of this library is to bring missing features to the official .Net namespace [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)

## Supported .NET versions
* .NET Standard 2.0
* .NET Core 3.1
* .NET 5.0.1

## Features
* Extensible Polymorphism support based on discriminator conventions ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/DiscriminatorTests.cs))
* Conditional Property Serialization support based on the existence of a method ShouldSerialize\[PropertyName\]() ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/ShouldSerializeTests.cs))
* Support for interfaces and abstract classes ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/DictionaryTests.cs))
* Support for numeric, enum and custom dictionary keys ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/DictionaryTests.cs))
* Support for non default constructors ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/CreatorMappingTests.cs))
* Can ignore default values ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/DefaultValueTests.cs))
* Can require properties or fields with different policies ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/ClassMemberModifierTests.cs))
* Object mapping to programmatically configure features in a non invasive way ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/ObjectMappingTests.cs))
* Support for Writable JSON Document Object Model (cf. [Spec](https://github.com/dotnet/corefx/blob/master/src/System.Text.Json/docs/writable_json_dom_spec.md)) ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/JsonNodeTests.cs))
* Support for serialization callbacks (before/after serialization/deserialization) ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/CallbackTests.cs))
* Support for anonymous types ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/AnonymousTests.cs))
* Support for DataContractAttribute and DataMemberAttribute ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/DataContractAndMemberTests.cs))
* Extended support for structs ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/StructTests.cs))
* Support for Nullables ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/NullableTests.cs))
* Support for collection interfaces: ISet<>, IList<>, ICollection<>, IEnumerable<>, IReadOnlyList<>, IReadOnlyCollection<>, IDictionary<>, IReadOnlyDictionary<> ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/InterfaceCollectionTests.cs))
* Support for immutable collections: ImmutableArray<>, ImmutableList<>, ImmutableSortedSet<>, ImmutableHashSet<>, ImmutableDictionary<>, ImmutableSortedDictionary<> ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/ImmutableCollectionTests.cs))
* Support for reference loop handling (cf. https://github.com/dotnet/corefx/issues/41002) ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/ReferenceHandlingTests.cs))
* Support for deserializing into read-only properties (cf. https://github.com/dotnet/corefx/issues/40602) ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/ReadOnlyPropertyTests.cs))
* Support for dynamics ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/DynamicObjectTests.cs))
* Support for C# 8 nullable reference types
* Support for MissingMemberHandling ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/MissingMemberHandlingTests.cs))
* Support for different property requirement policy ([example](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/RequiredTests.cs))
* Support for extended naming policies: Kebab case and Snake case ([example1](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/KebabCaseNamingPolicyTests.cs), [example2](https://github.com/dahomey-technologies/Dahomey.Json/blob/master/src/Dahomey.Json.Tests/SnakeCaseNamingPolicyTests.cs))

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

You can also instantiate options and setup extensions in one line:
```csharp
using Dahomey.Json;
using System.Text.Json;

JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
```

### Polymorphism

In order to distinguish inherited classes from a reference to a base class or a collection of references to a base class, a special property called **discriminator** can be added to the serialized json.

To describe the type of the discriminator property as well as the association between a specific inherited Type and a specific discriminator value, a discriminator convention supporting the interface **IDiscriminatorConvention** should be written.

Discriminator conventions can be registered to the *discriminator convention registry* accessible from the JsonSerilizedOptions.

Several conventions can be registered. When registering an inherited Type, an attempt to register it to each convention, begining with the last convention register. The first convention to accept to register a specific type will stop the registration process.
It means that different type inheritance hierarchy could serialize/deserialize their discriminator property in a different way.

The library offers 1 built-in discriminator convention:
- **DefaultDiscriminatorConvention<T>**: the discriminator value is defined by decorating classes with the attribute **JsonDiscriminatorAttribute**

This built-in convention setup the convention property name to **$type**

#### DefaultDiscriminatorConvention<T>
 
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
[JsonDiscriminator(1234)]
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
registry.RegisterConvention(new DefaultDiscriminatorConvention<int>(options, "_t"));
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
### Conditional Property Serialization

In the class to serialize, if a method exists which signature is bool ShouldSerialize\[PropertyName\](), it will be called to conditionally serialize the matching property.

 ```csharp
public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string Summary { get; set; }
    
    public bool ShouldSerializeSummary()
    {
        return !string.IsNullOrEmpty(Summary);
    }
}
```
