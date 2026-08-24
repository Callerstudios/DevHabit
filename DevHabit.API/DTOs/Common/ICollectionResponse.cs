namespace DevHabit.API.DTOs.Common;

public interface ICollectionResponse<T>
{
    public IReadOnlyCollection<T> Items { get; }
}
