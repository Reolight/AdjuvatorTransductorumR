using System.Collections.Generic;

namespace WpfAdjuvatorTransductoris.ViewModel
{
    public class ViewModelHeader : ViewModelRow
    {
        public ViewModelHeader(IEnumerable<string> Languages) : base("Key", Languages) { }
    }
}
