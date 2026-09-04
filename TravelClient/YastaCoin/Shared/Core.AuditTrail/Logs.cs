using Destructurama;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;
using Serilog.Sinks.Grafana.Loki;

namespace Core.AuditTrail
{
    public class LoggerSetup
    {
        public static Logger Init(WebApplicationBuilder builder)
        {
            var serilog = new LoggerConfiguration().Destructure.UsingAttributes().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(serilog);
            builder.Logging.AddOpenTelemetry(x =>
            {
                x.IncludeFormattedMessage = true;
                x.IncludeScopes = true;
            });
            builder.Services.AddSingleton(serilog);
            return serilog;
        }
    }

    public class LokiTextFormatter(IReservedPropertyRenamingStrategy renamingStrategy) : ITextFormatter
    {
        protected readonly JsonValueFormatter ValueFormatter = new("$type");

        private static readonly string[] ReservedKeywords = ["Message", "MessageTemplate", "Renderings", "Exception"];

        public LokiTextFormatter() : this(new DefaultReservedPropertyRenamingStrategy())
        {
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            ArgumentNullException.ThrowIfNull(logEvent);
            ArgumentNullException.ThrowIfNull(output);
            output.Write("{");
            var tokensWithFormat = logEvent.MessageTemplate.Tokens.OfType<PropertyToken>().Where(pt => pt.Format != null);
            if (tokensWithFormat.Any())
            {
                output.Write("\"Renderings\":[");
                var delimiter = string.Empty;
                foreach (var r in tokensWithFormat)
                {
                    output.Write(delimiter);
                    delimiter = ",";
                    var space = new StringWriter();
                    r.Render(logEvent.Properties, space);
                    JsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
                }
                output.Write(']');
            }
            if (logEvent.Exception != null)
            {
                output.Write("\"Exception\":");
                SerializeException(output, logEvent.Exception, 1);
            }
            foreach (var (key, value) in logEvent.Properties)
            {
                var name = GetSanitizedPropertyName(key);
                if (output.ToString()?.Length > 1) 
                {
                    output.Write(',');
                }
                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                ValueFormatter.Format(value, output);
            }
            output.Write('}');
        }

        protected virtual string GetSanitizedPropertyName(string propertyName) =>
            ReservedKeywords.Contains(propertyName) ? (renamingStrategy.Rename(propertyName) ?? string.Empty) : propertyName;

        protected virtual void SerializeException(TextWriter output, Exception exception, int level)
        {
            if (level == 4)
            {
                JsonValueFormatter.WriteQuotedJsonString(exception.ToString(), output);
                return;
            }
            output.Write("{\"Type\":");
            var typeNamespace = exception.GetType().Namespace;
            var typeName = typeNamespace != null && typeNamespace.StartsWith("System.") ? exception.GetType().Name : exception.GetType().ToString();
            JsonValueFormatter.WriteQuotedJsonString(typeName, output);
            if (!string.IsNullOrWhiteSpace(exception.Message))
            {
                output.Write(",\"Message\":");
                JsonValueFormatter.WriteQuotedJsonString(exception.Message, output);
            }
            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                output.Write(",\"StackTrace\":");
                JsonValueFormatter.WriteQuotedJsonString(exception.StackTrace, output);
            }
            if (exception is AggregateException aggregateException)
            {
                output.Write(",\"InnerExceptions\":[");
                var count = aggregateException.InnerExceptions.Count;
                for (var i = 0; i < count; i++)
                {
                    var isLast = i == count - 1;
                    SerializeException(output, aggregateException.InnerExceptions[i], level + 1);
                    if (!isLast)
                    {
                        output.Write(',');
                    }
                }
                output.Write("]");
            }
            else if (exception.InnerException != null)
            {
                output.Write(",\"InnerException\":");
                SerializeException(output, exception.InnerException, level + 1);
            }
            output.Write('}');
        }
    }
}
