using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Camunda.Worker.Client;

namespace Camunda.Worker
{
    public sealed class BpmnErrorResult : IExecutionResult
    {
        public BpmnErrorResult(string errorCode, string errorMessage, IDictionary<string, Variable> variables = default)
        {
            ErrorCode = Guard.NotNull(errorCode, nameof(errorCode));
            ErrorMessage = Guard.NotNull(errorMessage, nameof(errorMessage));
            Variables = variables ?? new Dictionary<string, Variable>();
        }

        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public IDictionary<string, Variable> Variables { get; }

        public async Task ExecuteResultAsync(IExternalTaskContext context)
        {
            try
            {
                await context.ReportBpmnErrorAsync(ErrorCode, ErrorMessage, Variables);
            }
            catch (ClientException e) when (e.StatusCode == HttpStatusCode.InternalServerError)
            {
                await context.ReportFailureAsync(e.ErrorType, e.ErrorMessage);
            }
        }
    }
}
