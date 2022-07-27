using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TcfBackup.Database.JsonDatabase;

public class PropertyDependentJsonConverter<TProperty, TBaseType> : JsonConverter
{
    private readonly string _propertyName;
    private readonly Func<TProperty, Type> _selector;

    public override bool CanWrite => false;

    private TBaseType? Deserialize(JObject obj, Type objType, object? existingValue, JsonSerializer serializer)
    {
        if (!objType.IsAssignableTo(typeof(TBaseType)))
        {
            throw new Exception();
        }
        
        var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(objType);
        var result = existingValue ?? contract.DefaultCreator?.Invoke();

        if (result == null)
        {
            return default;
        }

        using var reader = obj.CreateReader();

        serializer.Populate(reader, result);

        return (TBaseType?)result;
    }

    public PropertyDependentJsonConverter(string propertyName, Func<TProperty, Type> selector)
    {
        _propertyName = propertyName;
        _selector = selector;
    }
    
    public PropertyDependentJsonConverter(string propertyName, IDictionary<TProperty, Type> types)
        : this(propertyName, p => types[p])
    {

    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var obj = JObject.Load(reader);
        var property = obj[_propertyName];
        var propertyValue = property == null
            ? default
            : typeof(TProperty).IsEnum
                ? (TProperty?)property.ToObject(typeof(TProperty).GetEnumUnderlyingType())
                : property.Value<TProperty>();


        if (propertyValue == null)
        {
            throw new Exception();
        }

        return Deserialize(obj, _selector(propertyValue), existingValue, serializer);
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(TBaseType);
}