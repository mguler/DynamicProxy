using System;

namespace DynamicProxy.Core.Abstract
{
    public interface IProxyBuilder
    {
        Type Build(Type type);
    }
}
