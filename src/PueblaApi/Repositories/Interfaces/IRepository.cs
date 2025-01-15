using System.Linq.Expressions;

namespace PueblaApi.Repositories.Interfaces;
public interface IRepository<T>
{
    Task<T> Create(T item);
    Task<T> Update(T item);
    Task Delete(T item);
    Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
