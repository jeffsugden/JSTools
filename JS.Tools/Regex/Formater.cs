using System;
using System.Text.RegularExpressions;

namespace JS.Tools.Regexx
{
    public static class Formatters
    {
        public static string FormatAs10DightPhoneNumber(this string phoneNumberString)
        {
            return Regex.Replace(phoneNumberString ?? String.Empty, @"(\d{3})(\d{3})(\d{4})", "$1-$2-$3");
        }
    }
}
