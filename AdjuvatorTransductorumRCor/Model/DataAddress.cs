using System.Collections;
using System.Diagnostics;

namespace AdjuvatorTransductorumRCor.Model
{
    public sealed class DataAddress
    {
        public static implicit operator string(DataAddress address) => Compress(address.AddressQueue);

        public static TCollection Split<TCollection>(string address)
            where TCollection : class, IEnumerable<string>, new() 
        {
            string[] arr = (address.Split(':'));
            return arr as TCollection ?? throw new InvalidCastException("[internal|Core:DataAddress] Couldn't cast address string to " + nameof(TCollection));
        }

        public static Queue<string> Split(string address) => new(address.Split(':'));
        public static Stack<string> RevertedSplit(string address) => new(address.Split(':'));
        public static string Compress(IEnumerable<string> address) => string.Join(":", address.ToArray());

        public static string SmartCompress(string firstPart, string secondPart)
            => string.IsNullOrWhiteSpace(firstPart) ? secondPart :
                string.IsNullOrWhiteSpace(secondPart) ? firstPart :
                $"{firstPart}:{secondPart}";
        
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

        public string ReplaceAddress(string oldAddress, string newAddress)
            => Compress(ReplaceAddress(Split(oldAddress), Split(newAddress)));
        
        /// <summary>
        /// Replaces part of old address with new. Old address must be longer than new one.
        /// </summary>
        /// <param name="oldAddress">Address to be updated</param>
        /// <param name="newAddress">New part of address for update</param>
        /// <returns>Updated old address</returns>
        /// <example>Consider old address = 'parent:child:file:key'. After renaming 'child' node to 'kodomo'
        /// this method is called: ReplaceAddress('parent:child:file:key', 'parent:kodomo'),
        /// as it could be seen, file:key is nested address for renamed node, so this method makes next:
        /// replaces part of parent:child with parent:kodomo and returns updated address (parent:kodomo:file:key)</example>
        public static Queue<string> ReplaceAddress(Queue<string> oldAddress, Queue<string> newAddress)
        {
            if (newAddress.Count > oldAddress.Count)
                throw new ArgumentException("[internal|Core:DataAddress] Old address is shorter than new address. ");
            return ReplaceAddress(oldAddress, newAddress, newAddress.Count);
        }

        /// <summary>
        /// Replaces old part of given address with new.
        /// </summary>
        /// <param name="addressToReplace">Address where replacing takes place</param>
        /// <param name="replacementLength">Count of nodes to replace in old address</param>
        /// <param name="newPart">New part for replacement</param>
        /// <returns>Address with replaced part</returns>
        public static Queue<string> ReplaceAddress(Queue<string> addressToReplace, Queue<string> newPart, int replacementLength)
        {
            if (replacementLength < 0 && replacementLength >= addressToReplace.Count)
                throw new ArgumentOutOfRangeException(nameof(replacementLength), "[internal|Core:DataAddress] Replaced part length should be in range from 0 to length of given address");
            var nestedAddress = addressToReplace.ToList().GetRange(replacementLength, addressToReplace.Count - replacementLength);
            return new Queue<string>(newPart.Concat(nestedAddress));
        }
        
        private LinkedList<string> _address = new();

        public LinkedList<string> AddressLinkedList
        {
            get => new(_address);
        }

        /// <summary>
        /// Returns new instance of Queue representing Address;
        /// </summary>
        public Queue<string> AddressQueue
        {
            get => new(_address); //implicit cloning
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

        public string Pop()
        {
            string item = _address.Last();
            _address.RemoveLast();
            OnChanged();
            return item;
        }
        
        public void Enqueue(string address)
        {
            _address.AddLast(address);
            OnChanged();
        }

        public string Dequeue()
        {

            string item = _address.First(); 
            _address.RemoveFirst();
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
            _address = Split<LinkedList<string>>(address);
        }
    }
}
