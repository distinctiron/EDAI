using System.Reflection;
using Json.Schema;

namespace EDAI.Shared.Tools;

public static class JsonHelper
{
    public static JsonSchema getJsonSchema<T>()
    {
        return GenerateResolvedJsonSchema(typeof(T));
    }
    
    private static JsonSchema GenerateResolvedJsonSchema(Type type)
    {

        if (IsSimpleType(type))
        {
            return new JsonSchemaBuilder().Type(MapToJsonType(type));
        }
        
        if (IsEnumerableType(type, out var elementType))
        {
            var itemSchema = GenerateResolvedJsonSchema(elementType);
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.Array)
                .Items(itemSchema);
        }
        
        var propertiesSchema = new Dictionary<string, JsonSchema>();
        var requiredFields = new List<string>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyType = property.PropertyType;
            var propertySchema = GenerateResolvedJsonSchema(propertyType);
            propertiesSchema[property.Name] = propertySchema;
            requiredFields.Add(property.Name);
        }

        return new JsonSchemaBuilder().Type(SchemaValueType.Object).Properties(propertiesSchema).Required(requiredFields).AdditionalProperties(false);
    }
    
    private static bool IsEnumerableType(Type type, out Type elementType)
    {
        // Check if the type implements IEnumerable<T>
        if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        // Check if it is a non-generic IEnumerable
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            elementType = typeof(object); // Default to object if non-generic
            return true;
        }

        elementType = null;
        return false;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsValueType || type == typeof(string);
    }

    private static SchemaValueType MapToJsonType(Type type)
    {
        if (type == typeof(string)) return SchemaValueType.String;
        if (type == typeof(int) || type == typeof(long) || type == typeof(int?)) return SchemaValueType.Integer;
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return SchemaValueType.Number;
        if (type == typeof(bool)) return SchemaValueType.Boolean;

        throw new NotSupportedException($"Type {type.Name} is not supported");
    }
}