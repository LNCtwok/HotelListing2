using HotelListing2.Data;
using HotelListing2.IRepository;
using HotelListing2.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using X.PagedList;

namespace HotelListing2.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        //Dependency Injection instead of creating a whole new bridge
        private readonly DatabaseContext _context;
        private readonly DbSet<T> _db;

        public GenericRepository(DatabaseContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task Delete(int id)
        {
            var entity = await _db.FindAsync(id);
            _db.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _db.RemoveRange(entities);
        }

        public async Task<T> Get(Expression<Func<T, bool>> expression, List<string> includes = null)
        {
            IQueryable<T> query = _db;
            if(includes != null)
            {
                foreach(var jstFrk in includes)
                {
                    query = query.Include(jstFrk);
                }
            }
            

            return await query.AsNoTracking().FirstOrDefaultAsync(expression);//look in the query takoff the tracking and return the first record that matches the expression

        }

        //old getAll
        public async Task<IList<T>> GetAll(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy = null, List<string> includes = null)
        {
            IQueryable<T> query = _db;
            
            if(expression != null)
            {
                query = query.Where(expression);
            }

            if (includes != null)
            {
                foreach (var jstFrk in includes)
                {
                    query = query.Include(jstFrk);
                }
            }
            if(OrderBy != null)
            {
                query = OrderBy(query);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        //for paging :
        public async Task<IPagedList<T>> GetAll(RequestParams requestParams, List<string> includes = null)
        {
            IQueryable<T> query = _db;

            if (includes != null)
            {
                foreach (var jstFrk in includes)
                {
                    query = query.Include(jstFrk);
                }
            }

            return await query.AsNoTracking()
                .ToPagedListAsync(requestParams.PageNumber,requestParams.PageSize);
        }

        public async Task Insert(T entity)
        {
             await _db.AddAsync(entity);
        }

        public async Task InsertRange(IEnumerable<T> entities)
        {
            await _db.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _db.Attach(entity);//pay attention to this and check if you have it already and check if there is any different between it and the data you have in the database
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}