using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.AzureTableStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Util
{
    public class KeyGenerator : IKeyGenerator
    {
        string IKeyGenerator.GeneratePartitionKey(LogEvent logEvent)
        {
            string dataAtual = DateTimeBr.NowBr().ToString("yyyyMMdd-HH");
            return dataAtual;
        }

        string IKeyGenerator.GenerateRowKey(LogEvent logEvent)
        {
            string propriedade = "";
            LogEventPropertyValue sourceContext;
            if (logEvent.Properties.TryGetValue(Constants.SourceContextPropertyName, out sourceContext))
            {
                var sv = sourceContext as ScalarValue;
                if (sv?.Value is string)
                {
                    propriedade = (string)sv.Value;
                }
            }            
            
            string horaAtual = DateTimeBr.NowBr().ToString("HHmmss");
            Random random = new Random();
            int randomNumber = random.Next(1, 999999);
            string chave = $"{propriedade}-{horaAtual}-{randomNumber.ToString("D6")}";
            return chave;
        }
    }
}
