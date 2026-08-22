namespace DevHabit.API.Services.Sorting;

public interface ISortMappingDefinition
{
    public IReadOnlyList<SortMapping> Mappings { get; }
}
