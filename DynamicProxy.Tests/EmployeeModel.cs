using System;
using System.ComponentModel;

namespace DynamicProxy.Tests
{
    public class EmployeeModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _id;
        private string _name;
        private string _lastname;
        private double _salary;
        private DateTime _birthdate;

        public int Id { get { return _id; }
            set
            {
                if (value != _id) OnPropertyChanged("Id");
                _id = value;
            }
        }
        public string Name { get { return _name; } set { if (value != _name) OnPropertyChanged("Name"); _name = value; } }
        public string Lastname { get { return _lastname; } set { if (value != _lastname) OnPropertyChanged("Lastame"); _lastname = value; } }
        public double Salary { get { return _salary; } set { if (value != _salary) OnPropertyChanged("Salary"); _salary = value; } }
        public DateTime Birthdate { get { return _birthdate; } set { if (value != _birthdate) OnPropertyChanged("Birthdate"); _birthdate = value; } }
        public int Age { get { return (int)(DateTime.Now - Birthdate).TotalDays / 365; } }
        public double CalculateTax(double percent) => Salary * (percent / 100);

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
