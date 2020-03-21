using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace tgbot_DIK
{

    public class User : INotifyPropertyChanged, IEquatable<User>
    {
        private string nickname;
        private long id;

        public string Nickname {
            get { return this.nickname; }
            set
            {
                this.nickname = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(nameof(this.Nickname)));     
            }

        }
        public long Id
        {
            get { return this.id; }
            set
            {
                this.id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Id)));
            }

        }

        public User(string Nickname, long Id)
        {
            this.nickname = Nickname;
            this.id = Id;
            Messages = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
       
        /// <summary>
        /// Метод сравнения двух пользователей
        /// </summary>
        /// <param name="ConcreteUser"></param>
        /// <returns></returns>
        public bool Equals(User ConcreteUser) => (ConcreteUser.Id == this.Id);

        //коллекция сообщений пользователя
        public ObservableCollection<string> Messages { get; set; }

        /// <summary>
        /// Метод добавления сообщения в коллекцию
        /// </summary>
        /// <param name="text"></param>
        public void AddMsg(string text) => Messages.Add(text);
        


        

        
        
    }
}
