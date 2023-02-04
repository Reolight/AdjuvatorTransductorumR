using System.Text.RegularExpressions;

namespace WpfAdjuvatorTransductoris.Helpers;

public static class Regexes
{
    public static Regex NameCompressorRestriction = new Regex(@"\W");
    public static Regex NameRestriction = new Regex(@"\w+");
}