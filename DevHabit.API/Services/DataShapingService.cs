using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using DevHabit.API.DTOs.Common;

namespace DevHabit.API.Services;

public sealed class DataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];


        PropertyInfo[] propertyInfoList = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Count > 0)
        {
            propertyInfoList = [.. propertyInfoList.Where(p => fieldsSet.Contains(p.Name))];
        }

        IDictionary<string, object?> shapedObject = new ExpandoObject();
        foreach (PropertyInfo propertyInfo in propertyInfoList)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }

        return (ExpandoObject)shapedObject;
    }

    public IReadOnlyList<ExpandoObject> ShapeDataCollection<T>(IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];


        PropertyInfo[] propertyInfoList = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Count > 0)
        {
            propertyInfoList = [.. propertyInfoList.Where(p => fieldsSet.Contains(p.Name))];
        }

        List<ExpandoObject> shapedObjectsList = [];
        foreach (T entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }

            if(linksFactory is not null)
            {
                shapedObject["links"] = linksFactory(entity);
            }

            shapedObjectsList.Add((ExpandoObject) shapedObject);
        }
        return shapedObjectsList;
    }
    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] propertyInfoList = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldsSet.All(f => propertyInfoList.Any(p => p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
