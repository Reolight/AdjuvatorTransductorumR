using System.Diagnostics;

namespace AdjuvatorTransductorumRCor.Model
{
    public sealed class DataAddress
    {
        public static implicit operator string(DataAddress address) => Compress(address.Address);
        public static Queue<string> Split(string address) => new(address.Split(':'));
        public static Stack<string> RevertedSplit(string address) => new(address.Split(':'));
        public static string Compress(IEnumerable<string> address) => string.Join(":", address.ToArray());
        
        public static bool Contains(string addressToSearchIn, string addressToSearchFor)
            => Contains(Split(addressToSearchIn), Split(addressToSearchFor));
        public static bool Contains(Queue<string> baseAddress, Queue<string> address)
        {
            while(baseAddress.TryDequeue(out string? baseAddressKey) && baseAddressKey != null
                && address.TryDequeue(out string? addressKey) && addressKey != null)
            {
                if (baseAddressKey != addressKey)
                    return false;
            }

            return true;
        }

        private Queue<string> _address = new();

        /// <summary>
        /// Returns new instance of Queue representing Address;
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

        public override string ToString()
        {
            return Compress(_address);
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
