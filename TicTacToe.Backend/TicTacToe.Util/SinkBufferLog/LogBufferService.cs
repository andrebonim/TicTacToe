using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TicTacToe.Util.SinkBufferLog
{
    public interface ILogBufferService : IDisposable
    {
        /// <summary>
        /// Canal de leitura para o background service consumir os logs acumulados
        /// </summary>
        ChannelReader<string> Reader { get; }

        /// <summary>
        /// Escreve uma linha de log no buffer (usado pelo sink do Serilog)
        /// </summary>
        ValueTask WriteAsync(string logLine);

        /// <summary>
        /// (Opcional) Marca o canal como concluído (útil no shutdown da aplicação)
        /// </summary>
        void Complete();
    }

    public class LogBufferService : ILogBufferService, IDisposable
    {
        private readonly Channel<string> _channel;

        public LogBufferService()
        {
            // Capacidade ilimitada ou limitada para evitar memory leak em caso de falha
            _channel = Channel.CreateUnbounded<string>();
            // Alternativa com limite: Channel.CreateBounded<string>(new BoundedChannelOptions(5000) { FullMode = BoundedChannelFullMode.Wait });
        }

        public ChannelReader<string> Reader => _channel.Reader;

        public ValueTask WriteAsync(string logLine)
            => _channel.Writer.WriteAsync(logLine);

        public void Complete()
            => _channel.Writer.Complete();

        public void Dispose()
            => _channel.Writer.TryComplete();
    }
}
