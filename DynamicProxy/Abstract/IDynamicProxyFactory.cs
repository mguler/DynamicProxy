using System;

namespace DynamicProxy.Core.Abstract
{
    public interface IDynamicProxyFactory
    {
        Type BuildProxyType(Type type);
        Type BuildProxyType(object obj);
        Type BuildProxyType<T>();
        object BuildProxyObject(object obj);
    }
}
