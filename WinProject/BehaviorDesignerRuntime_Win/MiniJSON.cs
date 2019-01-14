// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.MiniJSON
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  public static class MiniJSON
  {
    public static object Deserialize(string json)
    {
      if (json == null)
        return (object) null;
      return MiniJSON.Parser.Parse(json);
    }

    public static string Serialize(object obj)
    {
      return MiniJSON.Serializer.Serialize(obj);
    }

    private sealed class Parser : IDisposable
    {
      private const string WORD_BREAK = "{}[],:\"";
      private StringReader json;

      private Parser(string jsonString)
      {
        this.json = new StringReader(jsonString);
      }

      public static bool IsWordBreak(char c)
      {
        if (!char.IsWhiteSpace(c))
          return "{}[],:\"".IndexOf(c) != -1;
        return true;
      }

      public static object Parse(string jsonString)
      {
        using (MiniJSON.Parser parser = new MiniJSON.Parser(jsonString))
          return parser.ParseValue();
      }

      public void Dispose()
      {
        this.json.Dispose();
        this.json = (StringReader) null;
      }

      private Dictionary<string, object> ParseObject()
      {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        this.json.Read();
        while (true)
        {
          MiniJSON.Parser.TOKEN nextToken;
          do
          {
            nextToken = this.NextToken;
            switch (nextToken)
            {
              case MiniJSON.Parser.TOKEN.NONE:
                goto label_3;
              case MiniJSON.Parser.TOKEN.CURLY_CLOSE:
                goto label_4;
              default:
                continue;
            }
          }
          while (nextToken == MiniJSON.Parser.TOKEN.COMMA);
          string index = this.ParseString();
          if (index != null)
          {
            if (this.NextToken == MiniJSON.Parser.TOKEN.COLON)
            {
              this.json.Read();
              dictionary[index] = this.ParseValue();
            }
            else
              goto label_8;
          }
          else
            goto label_6;
        }
label_3:
        return (Dictionary<string, object>) null;
label_4:
        return dictionary;
label_6:
        return (Dictionary<string, object>) null;
label_8:
        return (Dictionary<string, object>) null;
      }

      private List<object> ParseArray()
      {
        List<object> objectList = new List<object>();
        this.json.Read();
        bool flag = true;
        while (flag)
        {
          MiniJSON.Parser.TOKEN nextToken = this.NextToken;
          MiniJSON.Parser.TOKEN token = nextToken;
          switch (token)
          {
            case MiniJSON.Parser.TOKEN.SQUARED_CLOSE:
              flag = false;
              continue;
            case MiniJSON.Parser.TOKEN.COMMA:
              continue;
            default:
              if (token == MiniJSON.Parser.TOKEN.NONE)
                return (List<object>) null;
              object byToken = this.ParseByToken(nextToken);
              objectList.Add(byToken);
              continue;
          }
        }
        return objectList;
      }

      private object ParseValue()
      {
        return this.ParseByToken(this.NextToken);
      }

      private object ParseByToken(MiniJSON.Parser.TOKEN token)
      {
        switch (token)
        {
          case MiniJSON.Parser.TOKEN.CURLY_OPEN:
            return (object) this.ParseObject();
          case MiniJSON.Parser.TOKEN.SQUARED_OPEN:
            return (object) this.ParseArray();
          case MiniJSON.Parser.TOKEN.STRING:
            return (object) this.ParseString();
          case MiniJSON.Parser.TOKEN.NUMBER:
            return this.ParseNumber();
          case MiniJSON.Parser.TOKEN.INFINITY:
            return (object) float.PositiveInfinity;
          case MiniJSON.Parser.TOKEN.NEGINFINITY:
            return (object) float.NegativeInfinity;
          case MiniJSON.Parser.TOKEN.TRUE:
            return (object) true;
          case MiniJSON.Parser.TOKEN.FALSE:
            return (object) false;
          case MiniJSON.Parser.TOKEN.NULL:
            return (object) null;
          default:
            return (object) null;
        }
      }

      private string ParseString()
      {
        StringBuilder stringBuilder = new StringBuilder();
        this.json.Read();
        bool flag = true;
        while (flag)
        {
          if (this.json.Peek() == -1)
            break;
          char nextChar1 = this.NextChar;
          switch (nextChar1)
          {
            case '"':
              flag = false;
              continue;
            case '\\':
              if (this.json.Peek() == -1)
              {
                flag = false;
                continue;
              }
              char nextChar2 = this.NextChar;
              char ch = nextChar2;
              switch (ch)
              {
                case 'n':
                  stringBuilder.Append('\n');
                  continue;
                case 'r':
                  stringBuilder.Append('\r');
                  continue;
                case 't':
                  stringBuilder.Append('\t');
                  continue;
                case 'u':
                  char[] chArray = new char[4];
                  for (int index = 0; index < 4; ++index)
                    chArray[index] = this.NextChar;
                  stringBuilder.Append((char) Convert.ToInt32(new string(chArray), 16));
                  continue;
                default:
                  if (ch != '"' && ch != '/' && ch != '\\')
                  {
                    switch (ch)
                    {
                      case 'b':
                        stringBuilder.Append('\b');
                        continue;
                      case 'f':
                        stringBuilder.Append('\f');
                        continue;
                      default:
                        continue;
                    }
                  }
                  else
                  {
                    stringBuilder.Append(nextChar2);
                    continue;
                  }
              }
            default:
              stringBuilder.Append(nextChar1);
              continue;
          }
        }
        return stringBuilder.ToString();
      }

      private object ParseNumber()
      {
        string nextWord = this.NextWord;
        if (nextWord.IndexOf('.') == -1)
        {
          long result;
          long.TryParse(nextWord, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result);
          return (object) result;
        }
        double result1;
        double.TryParse(nextWord, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result1);
        return (object) result1;
      }

      private void EatWhitespace()
      {
        while (char.IsWhiteSpace(this.PeekChar))
        {
          this.json.Read();
          if (this.json.Peek() == -1)
            break;
        }
      }

      private char PeekChar
      {
        get
        {
          return Convert.ToChar(this.json.Peek());
        }
      }

      private char NextChar
      {
        get
        {
          return Convert.ToChar(this.json.Read());
        }
      }

      private string NextWord
      {
        get
        {
          StringBuilder stringBuilder = new StringBuilder();
          while (!MiniJSON.Parser.IsWordBreak(this.PeekChar))
          {
            stringBuilder.Append(this.NextChar);
            if (this.json.Peek() == -1)
              break;
          }
          return stringBuilder.ToString();
        }
      }

      private MiniJSON.Parser.TOKEN NextToken
      {
        get
        {
          this.EatWhitespace();
          if (this.json.Peek() == -1)
            return MiniJSON.Parser.TOKEN.NONE;
          char peekChar = this.PeekChar;
          switch (peekChar)
          {
            case '"':
              return MiniJSON.Parser.TOKEN.STRING;
            case ',':
              this.json.Read();
              return MiniJSON.Parser.TOKEN.COMMA;
            case '-':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
              return MiniJSON.Parser.TOKEN.NUMBER;
            case ':':
              return MiniJSON.Parser.TOKEN.COLON;
            default:
              switch (peekChar)
              {
                case '[':
                  return MiniJSON.Parser.TOKEN.SQUARED_OPEN;
                case ']':
                  this.json.Read();
                  return MiniJSON.Parser.TOKEN.SQUARED_CLOSE;
                default:
                  switch (peekChar)
                  {
                    case '{':
                      return MiniJSON.Parser.TOKEN.CURLY_OPEN;
                    case '}':
                      this.json.Read();
                      return MiniJSON.Parser.TOKEN.CURLY_CLOSE;
                    default:
                      string nextWord = this.NextWord;
                      if (nextWord != null)
                      {
                        if (MiniJSON.Parser.\u003C\u003Ef__switch\u0024map0 == null)
                          MiniJSON.Parser.\u003C\u003Ef__switch\u0024map0 = new Dictionary<string, int>(5)
                          {
                            {
                              "false",
                              0
                            },
                            {
                              "true",
                              1
                            },
                            {
                              "null",
                              2
                            },
                            {
                              "Infinity",
                              3
                            },
                            {
                              "-Infinity",
                              4
                            }
                          };
                        int num;
                        if (MiniJSON.Parser.\u003C\u003Ef__switch\u0024map0.TryGetValue(nextWord, out num))
                        {
                          switch (num)
                          {
                            case 0:
                              return MiniJSON.Parser.TOKEN.FALSE;
                            case 1:
                              return MiniJSON.Parser.TOKEN.TRUE;
                            case 2:
                              return MiniJSON.Parser.TOKEN.NULL;
                            case 3:
                              return MiniJSON.Parser.TOKEN.INFINITY;
                            case 4:
                              return MiniJSON.Parser.TOKEN.NEGINFINITY;
                          }
                        }
                      }
                      return MiniJSON.Parser.TOKEN.NONE;
                  }
              }
          }
        }
      }

      private enum TOKEN
      {
        NONE,
        CURLY_OPEN,
        CURLY_CLOSE,
        SQUARED_OPEN,
        SQUARED_CLOSE,
        COLON,
        COMMA,
        STRING,
        NUMBER,
        INFINITY,
        NEGINFINITY,
        TRUE,
        FALSE,
        NULL,
      }
    }

    private sealed class Serializer
    {
      private StringBuilder builder;

      private Serializer()
      {
        this.builder = new StringBuilder();
      }

      public static string Serialize(object obj)
      {
        MiniJSON.Serializer serializer = new MiniJSON.Serializer();
        serializer.SerializeValue(obj, 1);
        return serializer.builder.ToString();
      }

      private void SerializeValue(object value, int indentationLevel)
      {
        if (value == null)
          this.builder.Append("null");
        else if (value is string str)
          this.SerializeString(str);
        else if (value is bool)
          this.builder.Append(!(bool) value ? "false" : "true");
        else if (value is IList anArray)
          this.SerializeArray(anArray, indentationLevel);
        else if (value is IDictionary dictionary)
          this.SerializeObject(dictionary, indentationLevel);
        else if (value is char)
          this.SerializeString(new string((char) value, 1));
        else
          this.SerializeOther(value);
      }

      private void SerializeObject(IDictionary obj, int indentationLevel)
      {
        bool flag = true;
        this.builder.Append('{');
        this.builder.Append('\n');
        for (int index = 0; index < indentationLevel; ++index)
          this.builder.Append('\t');
        foreach (object key in (IEnumerable) obj.Keys)
        {
          if (!flag)
          {
            this.builder.Append(',');
            this.builder.Append('\n');
            for (int index = 0; index < indentationLevel; ++index)
              this.builder.Append('\t');
          }
          this.SerializeString(key.ToString());
          this.builder.Append(':');
          ++indentationLevel;
          this.SerializeValue(obj[key], indentationLevel);
          --indentationLevel;
          flag = false;
        }
        this.builder.Append('\n');
        for (int index = 0; index < indentationLevel - 1; ++index)
          this.builder.Append('\t');
        this.builder.Append('}');
      }

      private void SerializeArray(IList anArray, int indentationLevel)
      {
        this.builder.Append('[');
        bool flag = true;
        for (int index = 0; index < anArray.Count; ++index)
        {
          object an = anArray[index];
          if (!flag)
            this.builder.Append(',');
          this.SerializeValue(an, indentationLevel);
          flag = false;
        }
        this.builder.Append(']');
      }

      private void SerializeString(string str)
      {
        this.builder.Append('"');
        foreach (char ch1 in str.ToCharArray())
        {
          char ch2 = ch1;
          switch (ch2)
          {
            case '\b':
              this.builder.Append("\\b");
              break;
            case '\t':
              this.builder.Append("\\t");
              break;
            case '\n':
              this.builder.Append("\\n");
              break;
            case '\f':
              this.builder.Append("\\f");
              break;
            case '\r':
              this.builder.Append("\\r");
              break;
            default:
              switch (ch2)
              {
                case '"':
                  this.builder.Append("\\\"");
                  continue;
                case '\\':
                  this.builder.Append("\\\\");
                  continue;
                default:
                  int int32 = Convert.ToInt32(ch1);
                  if (int32 >= 32 && int32 <= 126)
                  {
                    this.builder.Append(ch1);
                    continue;
                  }
                  this.builder.Append("\\u");
                  this.builder.Append(int32.ToString("x4"));
                  continue;
              }
          }
        }
        this.builder.Append('"');
      }

      private void SerializeOther(object value)
      {
        if (value is float)
          this.builder.Append(((float) value).ToString("R", (IFormatProvider) CultureInfo.InvariantCulture));
        else if (value is int || value is uint || (value is long || value is sbyte) || (value is byte || value is short || (value is ushort || value is ulong)))
          this.builder.Append(value);
        else if (value is double || value is Decimal)
          this.builder.Append(Convert.ToDouble(value).ToString("R", (IFormatProvider) CultureInfo.InvariantCulture));
        else if (value is Vector2)
        {
          Vector2 vector2 = (Vector2) value;
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          this.builder.Append("\"(" + __nonvirtual (vector2.x.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (vector2.y.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + ")\"");
        }
        else if (value is Vector3)
        {
          Vector3 vector3 = (Vector3) value;
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          this.builder.Append("\"(" + __nonvirtual (vector3.x.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (vector3.y.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (vector3.z.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + ")\"");
        }
        else if (value is Vector4)
        {
          Vector4 vector4 = (Vector4) value;
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          this.builder.Append("\"(" + __nonvirtual (vector4.x.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (vector4.y.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (vector4.z.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (vector4.w.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + ")\"");
        }
        else if (value is Quaternion)
        {
          Quaternion quaternion = (Quaternion) value;
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          this.builder.Append("\"(" + __nonvirtual (quaternion.x.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (quaternion.y.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (quaternion.z.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + "," + __nonvirtual (quaternion.w.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)) + ")\"");
        }
        else
          this.SerializeString(value.ToString());
      }
    }
  }
}
