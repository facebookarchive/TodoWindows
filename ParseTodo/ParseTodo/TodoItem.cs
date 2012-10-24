using Parse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ParseTodo
{
    public class TodoItem : INotifyPropertyChanged
    {
        public static readonly string ClassName = "TodoItem";

        public TodoItem() : this(new ParseObject(ClassName)) { }
        public TodoItem(ParseObject backingObject)
        {
            if (backingObject.ClassName != ClassName)
            {
                throw new ArgumentException("Must create TodoItems with the proper ClassName");
            }
            this.BackingObject = backingObject;
        }

        public ParseObject BackingObject { get; private set; }

        public bool IsDirty
        {
            get
            {
                return BackingObject.IsDirty;
            }
        }

        public bool IsComplete
        {
            get
            {
                return BackingObject.ContainsKey("isComplete") ? BackingObject.Get<bool>("isComplete") : false;
            }
            set
            {
                if (value != IsComplete)
                {
                    BackingObject["isComplete"] = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get
            {
                return BackingObject.ContainsKey("title") ? BackingObject.Get<string>("title") : null;
            }
            set
            {
                if (value != Title)
                {
                    BackingObject["title"] = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get
            {
                return BackingObject.ContainsKey("description") ? BackingObject.Get<string>("description") : null;
            }
            set
            {
                if (value != Description)
                {
                    BackingObject["description"] = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var del = PropertyChanged;
            if (del != null)
            {
                del(this, new PropertyChangedEventArgs(propertyName));
                del(this, new PropertyChangedEventArgs("IsDirty"));
            }
        }

        public async Task SaveAsync()
        {
            await BackingObject.SaveAsync();
            OnPropertyChanged("IsDirty");
        }

        public void Revert()
        {
            BackingObject.Revert();
            OnPropertyChanged(String.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
