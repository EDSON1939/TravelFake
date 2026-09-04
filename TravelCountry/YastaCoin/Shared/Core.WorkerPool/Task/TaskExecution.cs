using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System.Net;

namespace Core.WorkerPool.Task
{
    public abstract class TaskExecution<T, U>(IConfiguration configuration, ILogger<T> logger) : IJob where T : TaskExecution<T, U>
    {
        private const string ErrorMessageTemplate = "{eventId} {message} {exception}";

        public async System.Threading.Tasks.Task Execute(IJobExecutionContext context)
        {
            this.context = context;
            try
            {
                var response = await ExecuteTask();
                if (SendStatusEmailAfterExecution)
                {
                    SendStatusEmail(JsonConvert.SerializeObject(response));
                }
            }
            catch (Exception exception)
            {
                logger.LogError(ErrorMessageTemplate, new EventId(), exception, exception.Message);
                SendStatusEmail("Task: " + TaskName + " Message: " + exception.Message + " Error" + exception.StackTrace, "WorkerPool: Error al ejecutar una tarea");
            }
        }

        public abstract string TaskName { get; }

        protected abstract Task<U?> ExecuteTask();

        protected IConfiguration configuration = configuration;

        protected ILogger<T> logger = logger;

        protected IJobExecutionContext? context;

        protected virtual bool SendStatusEmailAfterExecution => false;

        protected virtual string StatusEmailSubject => "WorkerPool: Aviso de ejecución de proceso";

        protected virtual string[]? ToRecipients => configuration.GetSection("Connections:Notification:Recipients").Get<string[]>();

        private void SendStatusEmail(string content, string subject = "")
        {
            var smtpUrl = configuration.GetValue<string>("Connections:Notification:BaseAddress");
            if (string.IsNullOrEmpty(smtpUrl))
            {
                logger.LogError("Service base address is not found in configuration file");
                return;
            }
            var user = configuration.GetValue<string>("Connections:Notification:User");
            if (string.IsNullOrEmpty(smtpUrl))
            {
                logger.LogError("User service is not configured");
                return;
            }
            var password = configuration.GetValue<string>("Connections:Notification:Password");
            if (string.IsNullOrEmpty(smtpUrl))
            {
                logger.LogError("Password for service is not configured");
                return;
            }
            var service = new Microsoft.Exchange.WebServices.Data.ExchangeService(Microsoft.Exchange.WebServices.Data.ExchangeVersion.Exchange2010_SP1)
            {
                Credentials = new NetworkCredential(user, password),
                Url = new Uri(smtpUrl)
            };
            var emailMessage = new Microsoft.Exchange.WebServices.Data.EmailMessage(service)
            {
                Subject = string.IsNullOrEmpty(subject) ? StatusEmailSubject : subject,
                Body = new Microsoft.Exchange.WebServices.Data.MessageBody(content)
            };
            emailMessage.ToRecipients.AddRange(ToRecipients);
            emailMessage.SendAndSaveCopy();
        }
    }
}
