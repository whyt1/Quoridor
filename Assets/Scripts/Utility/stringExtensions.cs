using System;

public static class StringExtensions 
 {
        // Suggested by hossein sedighian on StackOverflow
        public static string TextAfter(this string value ,string search) 
        {
            return value.Substring(value.IndexOf(search) + search.Length);
        }
        public static string RandomString(this string _)
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "");
        }
  }