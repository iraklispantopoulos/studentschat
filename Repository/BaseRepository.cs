using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Repository.Repositories
{
    public abstract class BaseRepository<DbEntity> : IRepository<DbEntity> where DbEntity : class, ITable, new()
    {
        public CacheConfiguration CacheConfiguration { get; set; }

        public bool LazyLoading { get; set; } = true;

        public bool RelationsEagerLoading { get; set; } = true;
        protected List<DbEntity> CachedEntities { get; set; }
        //private readonly AppDbContext _appDbContextFactory;
        private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;
        private readonly ILogger<BaseRepository<DbEntity>> _logger;

        public BaseRepository(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<BaseRepository<DbEntity>> logger)
        {
            //_appDbContextFactory = appDbContext;
            _appDbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<DbEntity> Get(Expression<Func<DbEntity, bool>> where)
        {
            return await _Get(where);
        }

        public async Task<TResult> Get<TResult>(int id, Expression<Func<DbEntity, TResult>> selector)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Where(p => p.Id == id)
                    .Select(selector)
                    .FirstOrDefault()!;
            }
        }

        public virtual async Task<DbContext> GetDbContext()
        {
            return _appDbContextFactory.CreateDbContext();
        }

        public async Task<int> Update(int? id, Action<DbEntity> editValues, bool createIfNotFound = false)
        {
            return await Update(p => p.Id == id, editValues, createIfNotFound);
        }

        protected async Task<DbEntity> _Get(Expression<Func<DbEntity, bool>> where)
        {
            try
            {
                DbEntity entity = default;
                var cachedEntities = await GetFromCache(where.Compile());
                if (cachedEntities != null && cachedEntities.Count > 0)
                    return cachedEntities.FirstOrDefault();
                using (DbContext db = await GetDbContext())
                {
                    entity = GetDbEntity(db, where);
                    AddToCache(new List<DbEntity>() { entity });
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return null!;
            }
        }
        public async Task<List<int>> AddMany(List<DbEntity> dbEntitiesToAdd)
        {
            if (dbEntitiesToAdd != null)
            {
                using (DbContext db = await GetDbContext())
                {
                    dbEntitiesToAdd.ForEach(p =>
                    {
                        db.Set<DbEntity>().Add(p);
                    });
                    db.SaveChanges();
                }

                return dbEntitiesToAdd
                                 .Select(p => p.Id)
                                 .ToList();
            }
            else
                return null;
        }

        protected virtual DbEntity AddNew(DbContext db)
        {
            var newEntity = new DbEntity();
            db.Set<DbEntity>().Add(newEntity);
            return newEntity;
        }

        protected void AddToCache(List<DbEntity> dbEntities)
        {
            try
            {
                if (CacheConfiguration != null && CacheConfiguration.ExpirationInSeconds > 0)
                {
                    if (CachedEntities == null)
                        CachedEntities = new List<DbEntity>();
                    dbEntities
                        .Where(p => p != null)
                        .ToList()
                        .ForEach(p =>
                        {
                            var existingEntity = CachedEntities.FirstOrDefault(x => x.Id == p.Id);
                            if (existingEntity != null)
                                existingEntity = p;
                            else
                                CachedEntities!.Add(p);
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
        }

        public async Task<int> Count(Expression<Func<DbEntity, bool>> where)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Count(where);
            }
        }

        public virtual async Task<bool> Delete(int id, Action<DbEntity, DbContext> onBeforeSave = null)
        {
            try
            {
                using (DbContext db = await GetDbContext())
                {
                    DbEntity dbEntity = GetDbSet(db).Where(p => p.Id == id).FirstOrDefault();
                    if (dbEntity != null)
                        Remove(db, dbEntity);
                    onBeforeSave?.Invoke(dbEntity, db);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Delete(List<int> ids)
        {
            try
            {
                using (DbContext db = await GetDbContext())
                {
                    var entities = db.Set<DbEntity>().Where(p => ids.FirstOrDefault(x => x == p.Id) != 0);
                    db.Set<DbEntity>().RemoveRange(entities);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                //_logger.Log(ex);
                return false;
            }
        }

        public async Task<bool> Delete(Expression<Func<DbEntity, bool>> where)
        {
            try
            {
                using (DbContext db = await GetDbContext())
                {
                    var entities = db.Set<DbEntity>().Where(where);
                    db.Set<DbEntity>().RemoveRange(entities);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
                return false;
            }
        }

        protected async Task<bool> DeleteAll()
        {
            try
            {
                using (DbContext db = await GetDbContext())
                {
                    var entities = db.Set<DbEntity>();
                    db.Set<DbEntity>().RemoveRange(entities);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return false;
            }
        }

        protected int ExecuteSqlCommand(string command, int? timeout = null)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<DbEntity> Get(int id, CancellationToken cancellationToken = default)
        {
            return await _Get(p => p.Id == id);
        }

        public async Task<TResult> Get<TResult>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where)
        {
            try
            {
                using (DbContext db = await GetDbContext())
                {
                    return db.Set<DbEntity>()
                        .Where(where)
                        .Select(selector)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return default(TResult);
            }
        }

        public async Task<List<DbEntity>> GetAll()
        {
            List<DbEntity> entities = new List<DbEntity>();
            using (DbContext db = await GetDbContext())
            {
                entities = GetDbSet(db).ToList();
            }
            return entities;
        }

        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<DbEntity, TResult>> selector)
        {
            List<DbEntity> entities = new List<DbEntity>();
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Select(selector)
                    .ToList();
            }
        }

        public async Task<List<DbEntity>> GetAllWithCriteria(Expression<Func<DbEntity, bool>> where,PageInfo pageInfo=null)
        {
            List<DbEntity> entities = new List<DbEntity>();
            using (DbContext db = await GetDbContext())
            {
                if(pageInfo==null)
                    entities = GetDbSet(db).AsQueryable().Where(where).ToList();
                else
                    entities = GetDbSet(db)
                        .AsQueryable()
                        .Where(where)
                        .Skip((pageInfo.Page-1) * pageInfo.PageSize)
                        .Take(pageInfo.PageSize)
                        .ToList();
            }
            //if (JsonParserInfo.ParseJsonProperties)
            //    entities.ForEach(p => ParseEntityWithJsonProperties(p));
            return entities;
        }

        public async Task<List<TResult>> GetAllWithCriteria<TResult>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Where(where)
                    .Select(selector)
                    .ToList();
            }
        }

        public async Task<List<TResult>> GetAllWithCriteria<TResult>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, int>> orderBy)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Where(where)
                    .OrderBy(orderBy)
                    .Select(selector)
                    .ToList();
            }
        }

        protected virtual DbEntity GetDbEntity(DbContext dbContext, Expression<Func<DbEntity, bool>> where)
        {
            return GetDbSet(dbContext).AsQueryable().Where(where).AsNoTracking().FirstOrDefault()!;
        }

        protected virtual IEnumerable<DbEntity> GetDbSetForSave(DbContext db)
        {
            return GetDbSet(db);
        }

        public async Task<TResult> GetFirst<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, TKey>> orderBy)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .OrderBy(orderBy)
                    .Where(where)
                    .Select(selector)
                    .FirstOrDefault()!;
            }
        }

        protected async Task<List<DbEntity>> GetFromCache(Func<DbEntity, bool> where)
        {
            try
            {
                if (CacheConfiguration != null && CachedEntities != null && CachedEntities.Count > 0)
                {
                    return CachedEntities!.Where(where).ToList();
                }
                else
                    return null!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return null!;
            }
        }

        public async Task<TResult> GetLast<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, TKey>> orderByDescending)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .OrderByDescending(orderByDescending)
                    .Select(selector)
                    .FirstOrDefault()!;
            }
        }

        public async Task<TResult> GetLast<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, TKey>> orderByDescending)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .OrderByDescending(orderByDescending)
                    .Where(where)
                    .Select(selector)
                    .FirstOrDefault()!;
            }
        }

        public async Task<List<TResult>> GetLast<TResult, TKey>(Expression<Func<DbEntity, TResult>> selector, Expression<Func<DbEntity, bool>> where, Expression<Func<DbEntity, TKey>> orderByDescending, int numberOfRows)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .OrderByDescending(orderByDescending)
                    .Where(where)
                    .Select(selector)
                    .Take(numberOfRows)
                    .ToList();
            }
        }

        public async Task<int> Last()
        {
            using (DbContext db = await GetDbContext())
            {
                var record = db.Set<DbEntity>()
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();
                return record != null ? record.Id : 0;
            }
        }

        protected virtual IQueryable<DbEntity> LoadRelations(IQueryable<DbEntity> dbSet)
        {
            return dbSet;
        }

        protected virtual void Map(DbEntity fromEntity, DbEntity toEntity, DbContext dbContext)
        {
            PropertyInfo[] props = fromEntity.GetType().GetProperties();
            foreach (PropertyInfo oneProperty in props)
            {
                var tableColAttribute = oneProperty.GetCustomAttribute(typeof(TableColumnAttr));
                if (tableColAttribute != null && ((TableColumnAttr)tableColAttribute).AutoSave)
                    toEntity.GetType().GetProperty(oneProperty.Name).SetValue(toEntity, oneProperty.GetValue(fromEntity));
            }
        }

        protected async Task<int> Max(Expression<Func<DbEntity, int>> selector)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Max(selector);
            }
        }

        protected async Task<int> Max(Expression<Func<DbEntity, int>> selector, Expression<Func<DbEntity, bool>> where)
        {
            using (DbContext db = await GetDbContext())
            {
                return db.Set<DbEntity>()
                    .Where(where)
                    .Select(selector)
                    .DefaultIfEmpty(0)
                    .Max();
            }
        }

        protected virtual void OnAfterSave(DbEntity dbEntity, DbContext db)
        { }

        protected virtual void OnBeforeSave(DbEntity dbEntity, DbContext db)
        {
        }

        protected virtual void Remove(DbContext db, DbEntity entity)
        {
            db.Set<DbEntity>().Remove(entity);
        }

        public virtual async Task<int> Save(DbEntity entity)
        {
            int dbEntityId = 0;
            try
            {
                dbEntityId = await SaveEntity(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
            return dbEntityId;
        }

        protected virtual async Task<int> SaveEntity(DbEntity updatedEntity)
        {
            DbEntity dbEntity = default;
            using (DbContext db = await GetDbContext())
            {
                if (updatedEntity.Id == 0)
                {
                    dbEntity = AddNew(db);
                    updatedEntity.Id = dbEntity.Id;
                }
                else
                {
                    dbEntity = GetDbSetForSave(db).Where(p => p.Id == updatedEntity.Id).FirstOrDefault();
                }
                Map(updatedEntity, dbEntity, db);
                OnBeforeSave(dbEntity, db);
                db.SaveChanges();
                OnAfterSave(dbEntity, db);
            }
            return dbEntity.Id;
        }

        protected async Task<int> Update(Expression<Func<DbEntity, bool>> where, Action<DbEntity> editValues, bool createIfNotFound = false, bool errorIfNotFound = true)
        {
            DbEntity dbEntity = default;
            using (DbContext db = await GetDbContext())
            {
                dbEntity = db.Set<DbEntity>()
                                        .Where(where)
                                        .FirstOrDefault();
                if (dbEntity == null)
                {
                    if (!createIfNotFound)
                    {
                        if (errorIfNotFound)
                            throw new Exception("Record not found to update!Error 909823498432092834");
                        else
                            return 0;
                    }
                    else
                        dbEntity = AddNew(db);
                }
                editValues(dbEntity);
                OnBeforeSave(dbEntity, db);
                //AddToCache(new List<DbEntity>() { dbEntity }); do not put it here!!It does not load the relations
                db.SaveChanges();
                OnAfterSave(dbEntity, db);
            }
            return dbEntity.Id;
        }

        protected async Task<List<int>> UpdateMany(Action<DbEntity> editValues, Expression<Func<DbEntity, bool>> where = null)
        {
            List<DbEntity> dbEntities = null;
            using (DbContext db = await GetDbContext())
            {
                if (where != null)
                    dbEntities = db.Set<DbEntity>().Where(where).ToList();
                else
                    dbEntities = db.Set<DbEntity>()
                                            .ToList();
                if (dbEntities != null)
                {
                    //we need to save each one separatelly, to avoid braking the sync between the databases.Its a performance hit but necessary.
                    dbEntities.ForEach(p =>
                    {
                        editValues(p);
                        db.SaveChanges();
                        OnAfterSave(p, db);
                    });
                }
            }
            return dbEntities
                             .Select(p => p.Id)
                             .ToList();
        }

        private IEnumerable<DbEntity> GetDbSet(DbContext db)
        {
            if (RelationsEagerLoading)
                return LoadRelations(db.Set<DbEntity>());
            else
                return db.Set<DbEntity>();
        }
    }
    public class PageInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class CacheConfiguration
    {
        public int ExpirationInSeconds { get; set; }
    }
}