using TicTacToe.Util.SinkBufferLog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting;

public class BufferedDatabaseSink : ILogEventSink
{
    private readonly ITextFormatter _formatter;
    private readonly ILogBufferService _bufferService;
    private readonly string _tempLogPath = Path.Combine("logs", "pending-logs.temp.txt"); // arquivo único ou rotativo

    public BufferedDatabaseSink(ILogBufferService bufferService)
    {
        _bufferService = bufferService;
        _formatter = new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        Directory.CreateDirectory("logs");
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null) return;

        using var writer = new StringWriter();
        _formatter.Format(logEvent, writer);
        string logLine = writer.ToString().TrimEnd();

        // 1. Sempre persiste em disco PRIMEIRO (append atômico)
        try
        {
            File.AppendAllText(_tempLogPath, logLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Último recurso: console se disco falhar
            Console.Error.WriteLine($"Falha ao persistir log temporário: {ex.Message}");
        }

        // 2. Depois enfileira pro processamento normal
        _ = _bufferService.WriteAsync(logLine);
    }
}