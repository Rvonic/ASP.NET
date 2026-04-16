namespace PrviLabos.Data;

public interface IMockRepository<T>
{
    IReadOnlyList<T> GetAll();
    T? GetById(int id);
}
