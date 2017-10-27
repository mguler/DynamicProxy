using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DynamicProxy.Core.Abstract;

namespace DynamicProxy.Core.Factory
{
    public class DefaultInterceptor : IInterceptor
    {
        public object Intercept(object proxy, object source, MethodInfo methodInfo, Dictionary<string, object> parameters)
        {
            var result = methodInfo.Invoke(source, parameters.Select(item => item.Value).ToArray());
            Console.WriteLine($"The method {methodInfo.Name} has been invoked with the input parameters {string.Concat(parameters.Select(parameter => $"{parameter.Key} : {parameter.Value} , "))} on {DateTime.Now}  and the result value was {result}");
            return result;
        }
    }
}
