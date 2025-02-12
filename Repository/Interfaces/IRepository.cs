using System.Linq.Expressions;

namespace Repository.Repositories
{
    public interface IRepository<DbEntity> where DbEntity : class, ITable, new()
    {
        //CacheConfiguration CacheConfiguration { get; set; }
        bool LazyLoading { get; set; }
        bool RelationsEagerLoading { get; set; }
        Task<int> Save(DbEntity entity);
        Task<List<int>> AddMany(List<DbEntity> dbEntitiesToAdd);
        Task<int> Count(Expression<Func<DbEntity, bool>> where);
        Task<bool> Delete(Expression<Func<DbEntity, bool>> where);
        Task<bool> Delete(List<int> ids);
        Task<DbEntity> Get(Expression<Func<DbEntity, bool>> where);
        Task<DbEntity> Get(int id, CancellationToken cancellationToken = default);
        Task<TResult> Get<TResult>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where);
        Task<TResult> Get<TResult>(int id, Expression<Func<DbEntity, TResult>> selector);
        Task<List<DbEntity>> GetAll();
        Task<List<TResult>> GetAll<TResult>(Expression<Func<DbEntity, TResult>> selector);
        Task<List<DbEntity>> GetAllWithCriteria(Expression<Func<DbEntity, bool>> where,PageInfo pageInfo=null);
        Task<List<TResult>> GetAllWithCriteria<TResult>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where);
        Task<List<TResult>> GetAllWithCriteria<TResult>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, int>> orderBy);
        Task<TResult> GetFirst<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, TKey>> orderBy);
        Task<TResult> GetLast<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, TKey>> orderByDescending);
        Task<List<TResult>> GetLast<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, TKey>> orderByDescending, int numberOfRows);
        Task<TResult> GetLast<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, TKey>> orderByDescending);
        Task<int> Last();
        Task<int> Update(int? id, Action<DbEntity> editValues, bool createIfNotFound = false);
    }
}