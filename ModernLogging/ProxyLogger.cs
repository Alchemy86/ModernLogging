using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ModernLogging
{
    public class ProxyLogger<TDecorated> : DispatchProxy
    {
        private TDecorated _decorated;
        private ILogger<ProxyLogger<TDecorated>> _logger;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                LogBefore(targetMethod, args);
                var result = targetMethod.Invoke(_decorated, args);

                if (result is Task resultTask)
                {
                    resultTask.ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            _logger.LogError(task.Exception,
                                "An unhandled exception was raised during execution of {decoratedClass}.{methodName}",
                                typeof(TDecorated), targetMethod.Name);
                        }

                        object taskResult = null;
                        if (task.GetType().GetTypeInfo().IsGenericType &&
                            task.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            var property = task.GetType().GetTypeInfo().GetProperties()
                                .FirstOrDefault(p => p.Name == "Result");
                            if (property != null)
                            {
                                taskResult = property.GetValue(task);
                            }
                        }
                        LogAfter(targetMethod, args, taskResult);
                    });
                }
                else
                {
                    LogAfter(targetMethod, args, result);
                }

                return result;
            }
            catch (TargetInvocationException ex)
            {
                _logger.LogError(ex.InnerException ?? ex,
                    "Error during invocation of {decoratedClass}.{methodName}",
                    typeof(TDecorated), targetMethod.Name);
                throw ex.InnerException ?? ex;
            }
        }

        public static TDecorated Create(TDecorated decorated, ILogger<ProxyLogger<TDecorated>> logger)
        {
            object proxy = Create<TDecorated, ProxyLogger<TDecorated>>();
            ((ProxyLogger<TDecorated>)proxy).SetParameters(decorated, logger);

            return (TDecorated)proxy;
        }

        private void SetParameters(TDecorated decorated, ILogger<ProxyLogger<TDecorated>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        }
        
        private void LogAfter(MethodInfo targetMethod, object[] args, object result = null)
        {
            _logger.LogInformation("Executed:  {object}, Result: {result}", 
                JsonConvert.SerializeObject(GetDetails(targetMethod, args)),
                JsonConvert.SerializeObject(result));
        }

        private void LogBefore(MethodInfo targetMethod, object[] args)
        {
            _logger.LogInformation("Invoking:  {object}", JsonConvert.SerializeObject(GetDetails(targetMethod, args)));
        }

        private object GetDetails(MethodInfo targetMethod, object[] args)
        {
            var beforeMessage = new StringBuilder();
            var obj = new { Class = _decorated.GetType().FullName, Method = targetMethod.Name, paramList = new List<object>()};

            var parameters = targetMethod.GetParameters();
            if (parameters.Any())
            {
                beforeMessage.AppendLine("Parameters:");
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var arg = args[i];
                    obj.paramList.Add(new { name = parameter.Name, type = parameter.ParameterType.Name, value = arg });
                }
            }

            return obj;
        }
    }
}