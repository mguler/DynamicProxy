using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DynamicProxy.Core.Factory;
using System.Diagnostics;
using System.ComponentModel;

namespace DynamicProxy.Tests
{
    [TestClass]
    public class DynamicProxyTest
    {
        [TestMethod]
        public void ProxyCreationTest()
        {
            #region Create source instance and set initial values
            var employee = new EmployeeModel
            {
                Birthdate = new DateTime(1973, 10, 22),
                Id = 1,
                Lastname = "Rob",
                Name = "Halford",
                Salary = 192.55
            };
            #endregion Create source instance set initial values

            #region set property changed event handler of source object

            var propertyChanged = false;
            employee.PropertyChanged += (s, e) =>
            {
                propertyChanged = true;
                Debug.WriteLine($"Property {e.PropertyName} changed");
            };

            #endregion set property changed event handler of source object

            #region create proxy instance
            var factory = new DynamicProxyFactory();
            var proxy = factory.BuildProxyObject(employee);
            #endregion create proxy instance

            #region check source and proxy object property values are equal

            var sourceType = employee.GetType();
            var properties = proxy.GetType().GetProperties();
            foreach (var property in properties)
            {
                var sourceProperty = sourceType.GetProperty(property.Name);
                var sourcePropertyValue = sourceProperty.GetValue(employee);
                var propertyValue = property.GetValue(proxy);
                Assert.AreEqual(sourcePropertyValue, propertyValue);
            }

            #endregion check source and proxy object property values are equal

            #region compare method results 

            var percent = 3;
            var methodInfo = proxy.GetType().GetMethod("CalculateTax");
            Assert.AreEqual(employee.CalculateTax(percent), methodInfo.Invoke(proxy, new object[] { percent }));

            #endregion compare method results 

            #region check PropertyChanged event of source instance works 

            employee.Id = 7;
            Assert.IsTrue(propertyChanged);

            propertyChanged = false;

            proxy.GetType().GetProperty("Name").SetValue(proxy, "Jason Newsted");
            Assert.IsTrue(propertyChanged);

            #endregion check PropertyChanged event of source instance works 
        }

        [TestMethod]
        public void SourceToProxySynchronizationTest()
        {
            #region Create source and proxy instances
            var employee = new EmployeeModel
            {
                Birthdate = new DateTime(1973, 10, 22),
                Id = 1,
                Lastname = "Rob",
                Name = "Halford",
                Salary = 192.55
            };

            var factory = new DynamicProxyFactory();
            var proxy = factory.BuildProxyObject(employee);

            #endregion Create source and proxy instances

            #region change property values of source object 

            employee.Birthdate = new DateTime(1950, 10, 12);
            employee.Id = 4;
            employee.Name = "James";
            employee.Lastname = "Hetfield";
            employee.Salary = 122.3;

            #endregion change property values of source object 

            #region set property changed event handler of source object after proxy instance created

            var propertyChanged = false;

            employee.PropertyChanged += (s, e) =>
            {
                propertyChanged = true;
                Debug.WriteLine($"Property {e.PropertyName} changed");
            };

            #endregion set property changed event handler of source object after proxy instance created

            #region check source and proxy object property values are equal
            var sourceType = employee.GetType();
            var properties = proxy.GetType().GetProperties();
            foreach (var property in properties)
            {
                var sourceProperty = sourceType.GetProperty(property.Name);
                var sourcePropertyValue = sourceProperty.GetValue(employee);
                var propertyValue = property.GetValue(proxy);
                Assert.AreEqual(sourcePropertyValue, propertyValue);
            }
            #endregion check source and proxy object property values are equal

            #region compare method results 
            var percent = 3;
            var methodInfo = proxy.GetType().GetMethod("CalculateTax");
            Assert.AreEqual(employee.CalculateTax(percent), methodInfo.Invoke(proxy, new object[] { percent }));
            #endregion compare method results 

            #region check PropertyChanged event of source instance works 
            employee.Id = 7;
            Assert.IsTrue(propertyChanged);

            propertyChanged = false;

            proxy.GetType().GetProperty("Name").SetValue(proxy, "Jason Newsted");
            Assert.IsTrue(propertyChanged);
            #endregion check PropertyChanged event of source instance works 
        }

        [TestMethod]
        public void ProxyToSourceSynchronizationTest()
        {
            #region Create source and proxy instances
            var employee = new EmployeeModel
            {
                Birthdate = new DateTime(1973, 10, 22),
                Id = 1,
                Lastname = "Rob",
                Name = "Halford",
                Salary = 192.55
            };

            var factory = new DynamicProxyFactory();
            var proxy = factory.BuildProxyObject(employee);
            #endregion Create source and proxy instances

            #region change property values of proxy object 

            proxy.GetType().GetProperty("Birthdate").SetValue(proxy, new DateTime(1950, 10, 12));
            proxy.GetType().GetProperty("Id").SetValue(proxy, 5);
            proxy.GetType().GetProperty("Name").SetValue(proxy, "Cenk");
            proxy.GetType().GetProperty("Lastname").SetValue(proxy, "Taner");
            proxy.GetType().GetProperty("Salary").SetValue(proxy, 175.47);

            #endregion change property values of proxy object 

            #region set property changed event handler of proxy object 

            var propertyChanged = false;

            ((INotifyPropertyChanged)proxy).PropertyChanged += (s, e) =>
            {
                propertyChanged = true;
            };
            #endregion set property changed event handler of proxy object 

            #region check source and proxy object property values are equal
            var sourceType = employee.GetType();
            var properties = proxy.GetType().GetProperties();
            foreach (var property in properties)
            {
                var sourceProperty = sourceType.GetProperty(property.Name);
                var sourcePropertyValue = sourceProperty.GetValue(employee);
                var propertyValue = property.GetValue(proxy);
                Assert.AreEqual(sourcePropertyValue, propertyValue);
            }
            #endregion check source and proxy object property values are equal

            #region compare method results 
            var percent = 3;
            var methodInfo = proxy.GetType().GetMethod("CalculateTax");
            Assert.AreEqual(employee.CalculateTax(percent), methodInfo.Invoke(proxy, new object[] { percent }));
            #endregion compare method results 

            #region check PropertyChanged event of proxy instance works 
            employee.Id = 7;
            Assert.IsTrue(propertyChanged);

            propertyChanged = false;

            proxy.GetType().GetProperty("Name").SetValue(proxy, "Jason Newsted");
            Assert.IsTrue(propertyChanged);
            #endregion check PropertyChanged event of proxy instance works 
        }
    }
}

