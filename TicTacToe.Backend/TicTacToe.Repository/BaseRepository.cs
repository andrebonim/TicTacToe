using AutoMapper;
using TicTacToe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using TicTacToe.Util;

namespace TicTacToe.Repository
{

    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(long id);
        Task<T?> GetByIdAsync<TKey>(
            TKey keyValue,
            Expression<Func<T, TKey>> keySelector,
            params Expression<Func<T, object>>[] includes);
        Task<T> InsertAsync(T entity);
        Task<RetornoPadraoSimples> UpdateAsync(T entity);
        Task DeleteByIdAsync(long id);
    }

    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly TicTacToeContext _context;        
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;
        protected readonly IMapper _mapper;   // 

        public BaseRepository(TicTacToeContext context, ILoggerFactory loggerFactory, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = loggerFactory.CreateLogger($"Repository.{typeof(T).Name}");
            _mapper = mapper;
        }


        public virtual async Task<T?> GetByIdAsync<TKey>(
            TKey keyValue,
            Expression<Func<T, TKey>> keySelector,
            params Expression<Func<T, object>>[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                // === CORREÇÃO AQUI ===
                var parameter = keySelector.Parameters[0];
                var equality = Expression.Equal(
                    keySelector.Body,
                    Expression.Constant(keyValue)
                );
                var predicate = Expression.Lambda<Func<T, bool>>(equality, parameter);

                return await query.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar {Entity} com chave {Key}", typeof(T).Name, keyValue);
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(long id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar {Entity} com ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<T> InsertAsync(T entity)
        {
            try
            {
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir {Entity}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<RetornoPadraoSimples> UpdateAsync(T entity)
        {
            RetornoPadraoSimples ret = new RetornoPadraoSimples();
            ret.Sucesso = false;
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                ret.Sucesso = true;
                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar {Entity}", typeof(T).Name);
                ret.Sucesso = false;
                throw;
            }
        }

        public virtual async Task DeleteByIdAsync(long id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar {Entity} com ID {Id}", typeof(T).Name, id);
                throw;
            }
        }
    }
}