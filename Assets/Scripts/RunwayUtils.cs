using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class RunwayUtils
{
  public static string UppercaseFirst(string s)
  {
    if (string.IsNullOrEmpty(s))
    {
      return string.Empty;
    }
    char[] a = s.ToCharArray();
    a[0] = char.ToUpper(a[0]);
    return new string(a);
  }
  public static string SplitCamelCase(string str)
  {
    return Regex.Replace(
        Regex.Replace(
            str,
            @"(\P{Ll})(\P{Ll}\p{Ll})",
            "$1 $2"
        ),
        @"(\p{Ll})(\P{Ll})",
        "$1 $2"
    );
  }

  public static string FormatFieldName(string fieldName)
  {
    return UppercaseFirst(SplitCamelCase(fieldName));
  }
}
