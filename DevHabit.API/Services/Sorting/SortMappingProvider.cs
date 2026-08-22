using System.Linq.Dynamic.Core;

namespace DevHabit.API.Services.Sorting;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public IReadOnlyList<SortMapping> GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource, TDestination>? sortMappingDefinition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (sortMappingDefinition == null)
        {
            throw new InvalidOperationException(
                $"The mapping from '{typeof(TSource).Name}' into {typeof(TDestination).Name} is not defined"
                );
        }

        return sortMappingDefinition.Mappings;
    }
    public bool Validatemappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrEmpty(sort))
        {
            return true;
        }
        var sortFields = sort
            .Split(',')
            .Select(f => f.Trim().Split(' ')[0])
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();
        IReadOnlyList<SortMapping> mapping = GetMappings<TSource, TDestination>();

        return sortFields.All(f => mapping.Any(m => m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
