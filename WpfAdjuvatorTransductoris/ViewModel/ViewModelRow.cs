using System.Collections.Generic;

namespace WpfAdjuvatorTransductoris.ViewModel
{
    public class ViewModelRow
    {
        public List<string> Cells { get; set; } = new();

        public ViewModelRow(string key, IEnumerable<string> langVals) {
            Cells.Add(key);
            Cells.AddRange(langVals);
        }
    }
}
