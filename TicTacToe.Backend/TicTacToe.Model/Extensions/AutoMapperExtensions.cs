// Extensions/AutoMapperExtensions.cs
using AutoMapper;

namespace TicTacToe.Model.Extensions
{
    public static class AutoMapperExtensions
    {
        private static IMapper _mapper;

        public static void Configure(IMapper mapper)
        {
            _mapper = mapper;
        }

        public static TDestiny SecureMap<TDestiny>(this object source)
        {
            if (source == null) return default;
            if (_mapper == null)
                throw new InvalidOperationException("AutoMapper não configurado.");

            try
            {
                return _mapper.Map<TDestiny>(source);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new InvalidOperationException(
                    $"Mapeamento falhou: {source.GetType().Name} → {typeof(TDestiny).Name}", ex);
            }
        }
    }
}