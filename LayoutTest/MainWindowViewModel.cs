using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutTest
{
    internal class ObservablePerson : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return "Person's Name: " + _name; }
            set { SetProperty(ref _name, value); }
        }
    }

    internal class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ObservablePerson> _people = new ObservableCollection<ObservablePerson>();
        public ObservableCollection<ObservablePerson> People
        {
            get { return _people;  }
            set { SetProperty(ref _people, value); }
        }

        public void Initialize()
        {
            var testPeople = new ObservablePerson[]
            {
                new ObservablePerson { Name = "Donald Duck" },
                new ObservablePerson { Name = "James T. Kirk" },
                new ObservablePerson { Name = "Obi-Wan Kenobi" },
                new ObservablePerson { Name = "Gandalf" },
                new ObservablePerson { Name = "Michael Knight" },
                new ObservablePerson { Name = "Stringfellow Hawke" },
                new ObservablePerson { Name = "Dominic Santini" }
            };

            People = new ObservableCollection<ObservablePerson>(testPeople);
        }
    }
}
