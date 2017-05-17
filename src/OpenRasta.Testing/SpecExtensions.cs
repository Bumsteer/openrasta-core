using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Collections.Specialized;
using Shouldly;

namespace OpenRasta.Testing
{
  public static class SpecExtensions
  {
    public static void LegacyShouldAllBe<T>(this IEnumerable<T> values, T expected)
    {
      values.ShouldAllBe(item => Equals(expected, item));
    }

    public static T LegacyShouldBe<T>(this T valueToAnalyse, T expectedValue)
    {
      valueToAnalyse.ShouldBe(expectedValue);
      return valueToAnalyse;
    }

    public static void LegacyShouldBe<T>(this Type valueToAnalyse)
    {
      valueToAnalyse.ShouldBe(typeof(T));
    }

    public static void LegacyShouldBe(this Uri actual, string expected)
    {
      actual.ShouldBe(new Uri(expected));
    }

    public static void LegacyShouldBeAssignableTo<TExpected>(this object obj)
    {
      obj.ShouldBeAssignableTo<TExpected>();
    }

    public static void LegacyShouldBeEmpty<T>(this IEnumerable<T> values)
    {
      values.ShouldBeEmpty();
    }

    public static bool LegacyShouldBeFalse(this bool value)
    {
      value.ShouldBeFalse();
      return false;
    }

    public static void LegacyShouldBeGreaterThan<T>(this T actual, T expected) where T : IComparable<T>
    {
      actual.ShouldBeGreaterThan(expected);
    }

    public static void LegacyShouldBeNull<T>(this T obj)
    {
      obj.ShouldBeNull();
    }

    public static TExpected LegacyShouldBeOfType<TExpected>(this object obj)
    {
      obj.ShouldBeOfType<TExpected>();
      return (TExpected) obj;
    }

    public static void LegacyShouldBeTheSameInstanceAs(this object actual, object expected)
    {
      actual.ShouldBeSameAs(expected);
    }

    public static void LegacyShouldBeTrue(this bool value)
    {
      value.ShouldBeTrue();
    }

    public static void LegacyShouldCompleteSuccessfully(this Action codeToExecute)
    {
      codeToExecute();
    }

    public static IDictionary<T, U> LegacyShouldContain<T, U>(this IDictionary<T, U> dic, T key, U value)
    {
      dic.ShouldContainKeyAndValue(key, value);

      return dic;
    }

    public static IEnumerable<T> LegacyShouldContain<T>(this IEnumerable<T> list, T expected)
    {
      LegacyShouldContain(list, expected, (t, t2) => t.Equals(t2));
      return list;
    }

    public static void LegacyShouldContain<T>(this IEnumerable<T> list, T expected, Func<T, T, bool> match)
    {
      list.ShouldContain(item => match(item, expected));
    }

    public static string LegacyShouldContain(this string baseString, string textToFind)
    {
      baseString.ShouldContain(textToFind,Case.Sensitive);
      return baseString;
    }

    public static IEnumerable<T> LegacyShouldHaveCountOf<T>(this IEnumerable<T> values, int count)
    {
      values.Count().ShouldBe(count);
      return values;
    }

    public static void LegacyShouldHaveSameElementsAs(this NameValueCollection actual, NameValueCollection expected)
    {
      actual.ToDictionary().ShouldBe(expected.ToDictionary());
    }

    public static void LegacyShouldHaveSameElementsAs<T>(this IEnumerable<T> r1, IEnumerable<T> r2)
    {
      r1.ShouldBe(r2);
    }

    public static void LegacyShouldNotBe<T>(this T valueToAnalyse, T expectedValue)
    {
      valueToAnalyse.ShouldNotBe(expectedValue);
      return;
    }

    public static T legacyShouldNotBeNull<T>(this T obj) where T : class
    {
      obj.ShouldNotBeNull();
      return obj;
    }

    public static void LegacyShouldNotBeTheSameInstanceAs(this object actual, object expected)
    {
      actual.ShouldNotBeSameAs(expected);
    }

    public static void LegacyShouldNotContain(this string baseString, string textToFind)
    {
      baseString.ShouldNotContain(textToFind);
    }

    public static void LegacyShouldThrow<T>(this Func<Task> codeToExecute) where T : Exception
    {
      codeToExecute.ShouldThrow<T>();
    }

    public static T LegacyShouldThrow<T>(this Action codeToExecute) where T : Exception
    {
      return codeToExecute.ShouldThrow<T>();
    }
  }
}