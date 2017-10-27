using System.Collections.Generic;
using System.Reflection;

namespace DynamicProxy.Core.Abstract
{
    public interface IInterceptor
    {
        object Intercept(object proxy, object source, MethodInfo methodInfo, Dictionary<string, object> parameters);
    }
}
