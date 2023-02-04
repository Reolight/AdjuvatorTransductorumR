using System.Collections;
using System.Net;
using System.Text.RegularExpressions;

namespace AdjuvatorTransductorumRCor.Model
{
    public sealed class DataAddress
    {
        private static readonly Regex PathFormatter = new Regex(@"[\w.]+(?=($|:))"); //any word before ":" and end of lane

        public static implicit operator string(DataAddress address) => Compress(address.Address);
        public static Queue<string> Split(string address) => new Queue<string>(address.Split(':'));
        public static Stack<string> RevertedSplit(string address) => new Stack<string>(address.Split(':'));
        public static string Compress(IEnumerable<string> address) => string.Join(":", address.ToArray());


        public static string ConvertToXPath(string address)
        {
            var matches = PathFormatter.Matches(address);
            System.Text.StringBuilder xpath = new System.Text.StringBuilder(@"DataModel/Root");
            foreach (Match match in matches)
            {
                xpath.Append($"/Node[@Name='{match.Value}']");
            }

            return xpath.ToString();
        }


    private Queue<string> _address = new();

        /// <summary>
        /// Returns new instance of Queue<string> representing Address;
        /// </summary>
        public Queue<string> Address
        {
            get => new(_address); //implicit cloning
            set
            {
                _address = value;
                OnChanged();
            }
        }

        public int Count => _address.Count;
        public string Last => _address.Last();
        public event EventHandler? Changed;

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Enqueue(string address)
        {
            _address.Enqueue(address);
            OnChanged();
        }

        public string Dequeue()
        {
            string item = _address.Dequeue();
            OnChanged();
            return item;
        }

        public void Clear()
        {
            _address.Clear();
            OnChanged();
        }
        
        public DataAddress(){}

        public DataAddress(string address)
        {
            _address = Split(address);
        }
    }
}
