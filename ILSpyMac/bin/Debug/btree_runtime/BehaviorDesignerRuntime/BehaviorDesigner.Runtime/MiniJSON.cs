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
		private sealed class Parser : IDisposable
		{
			private enum TOKEN
			{
				NONE = 0,
				CURLY_OPEN = 1,
				CURLY_CLOSE = 2,
				SQUARED_OPEN = 3,
				SQUARED_CLOSE = 4,
				COLON = 5,
				COMMA = 6,
				STRING = 7,
				NUMBER = 8,
				INFINITY = 9,
				NEGINFINITY = 10,
				TRUE = 11,
				FALSE = 12,
				NULL = 13
			}

			private const string WORD_BREAK = "{}[],:\"";

			private StringReader json;

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
						{
							break;
						}
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
					{
						return MiniJSON.Parser.TOKEN.NONE;
					}
					char peekChar = this.PeekChar;
					switch (peekChar)
					{
					case '"':
						return MiniJSON.Parser.TOKEN.STRING;
					case '#':
					case '$':
					case '%':
					case '&':
					case '\'':
					case '(':
					case ')':
					case '*':
					case '+':
					case '.':
					case '/':
						IL_8D:
						switch (peekChar)
						{
						case '[':
							return MiniJSON.Parser.TOKEN.SQUARED_OPEN;
						case '\\':
						{
							IL_A2:
							switch (peekChar)
							{
							case '{':
								return MiniJSON.Parser.TOKEN.CURLY_OPEN;
							case '}':
								this.json.Read();
								return MiniJSON.Parser.TOKEN.CURLY_CLOSE;
							}
							string nextWord = this.NextWord;
							if (nextWord != null)
							{
								//if (MiniJSON.Parser.<>f__switch$map0 == null)
								if (MiniJSON.Parser.fswitchMap0 == null)
								{
									Dictionary<string, int> dictionary = new Dictionary<string, int>(5);
									dictionary.Add("false", 0);
									dictionary.Add("true", 1);
									dictionary.Add("null", 2);
									dictionary.Add("Infinity", 3);
									dictionary.Add("-Infinity", 4);
									MiniJSON.Parser.fswitchMap0 = dictionary;
								}
								int num;
								if (MiniJSON.Parser.fswitchMap0.TryGetValue(nextWord, out num))
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
						case ']':
							this.json.Read();
							return MiniJSON.Parser.TOKEN.SQUARED_CLOSE;
						}
						goto IL_A2;
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
					}
					goto IL_8D;
				}
			}

			private Parser(string jsonString)
			{
				this.json = new StringReader(jsonString);
			}

			public static bool IsWordBreak(char c)
			{
				return char.IsWhiteSpace(c) || "{}[],:\"".IndexOf(c) != -1;
			}

			public static object Parse(string jsonString)
			{
				object result;
				using (MiniJSON.Parser parser = new MiniJSON.Parser(jsonString))
				{
					result = parser.ParseValue();
				}
				return result;
			}

			public void Dispose()
			{
				this.json.Dispose();
				this.json = null;
			}

			private Dictionary<string, object> ParseObject()
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				this.json.Read();
				while (true)
				{
					MiniJSON.Parser.TOKEN nextToken = this.NextToken;
					switch (nextToken)
					{
					case MiniJSON.Parser.TOKEN.NONE:
						goto IL_37;
					case MiniJSON.Parser.TOKEN.CURLY_OPEN:
					{
						IL_2B:
						if (nextToken == MiniJSON.Parser.TOKEN.COMMA)
						{
							continue;
						}
						string text = this.ParseString();
						if (text == null)
						{
							goto Block_2;
						}
						if (this.NextToken != MiniJSON.Parser.TOKEN.COLON)
						{
							goto Block_3;
						}
						this.json.Read();
						dictionary.set_Item(text, this.ParseValue());
						continue;
					}
					case MiniJSON.Parser.TOKEN.CURLY_CLOSE:
						return dictionary;
					}
					goto IL_2B;
				}
				IL_37:
				return null;
				Block_2:
				return null;
				Block_3:
				return null;
			}

			private List<object> ParseArray()
			{
				List<object> list = new List<object>();
				this.json.Read();
				bool flag = true;
				while (flag)
				{
					MiniJSON.Parser.TOKEN nextToken = this.NextToken;
					MiniJSON.Parser.TOKEN tOKEN = nextToken;
					switch (tOKEN)
					{
					case MiniJSON.Parser.TOKEN.SQUARED_CLOSE:
						flag = false;
						continue;
					case MiniJSON.Parser.TOKEN.COLON:
						IL_38:
						if (tOKEN != MiniJSON.Parser.TOKEN.NONE)
						{
							object obj = this.ParseByToken(nextToken);
							list.Add(obj);
							continue;
						}
						return null;
					case MiniJSON.Parser.TOKEN.COMMA:
						continue;
					}
					goto IL_38;
				}
				return list;
			}

			private object ParseValue()
			{
				MiniJSON.Parser.TOKEN nextToken = this.NextToken;
				return this.ParseByToken(nextToken);
			}

			private object ParseByToken(MiniJSON.Parser.TOKEN token)
			{
				switch (token)
				{
				case MiniJSON.Parser.TOKEN.CURLY_OPEN:
					return this.ParseObject();
				case MiniJSON.Parser.TOKEN.SQUARED_OPEN:
					return this.ParseArray();
				case MiniJSON.Parser.TOKEN.STRING:
					return this.ParseString();
				case MiniJSON.Parser.TOKEN.NUMBER:
					return this.ParseNumber();
				case MiniJSON.Parser.TOKEN.INFINITY:
					return float.PositiveInfinity;
				case MiniJSON.Parser.TOKEN.NEGINFINITY:
					return float.NegativeInfinity;
				case MiniJSON.Parser.TOKEN.TRUE:
					return true;
				case MiniJSON.Parser.TOKEN.FALSE:
					return false;
				case MiniJSON.Parser.TOKEN.NULL:
					return null;
				}
				return null;
			}

			private string ParseString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				this.json.Read();
				bool flag = true;
				while (flag)
				{
					if (this.json.Peek() == -1)
					{
						break;
					}
					char nextChar = this.NextChar;
					char c = nextChar;
					if (c != '"')
					{
						if (c != '\\')
						{
							stringBuilder.Append(nextChar);
						}
						else
						{
							if (this.json.Peek() != -1)
							{
								nextChar = this.NextChar;
								char c2 = nextChar;
								switch (c2)
								{
								case 'n':
									stringBuilder.Append('\n');
									continue;
								case 'o':
								case 'p':
								case 'q':
								case 's':
									IL_A5:
									if (c2 == '"' || c2 == '/' || c2 == '\\')
									{
										stringBuilder.Append(nextChar);
										continue;
									}
									if (c2 == 'b')
									{
										stringBuilder.Append('\b');
										continue;
									}
									if (c2 != 'f')
									{
										continue;
									}
									stringBuilder.Append('\f');
									continue;
								case 'r':
									stringBuilder.Append('\r');
									continue;
								case 't':
									stringBuilder.Append('\t');
									continue;
								case 'u':
								{
									char[] array = new char[4];
									for (int i = 0; i < 4; i++)
									{
										array[i] = this.NextChar;
									}
									stringBuilder.Append((char)Convert.ToInt32(new string(array), 16));
									continue;
								}
								}
								goto IL_A5;
							}
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				return stringBuilder.ToString();
			}

			private object ParseNumber()
			{
				string nextWord = this.NextWord;
				if (nextWord.IndexOf('.') == -1)
				{
					long num;
					long.TryParse(nextWord, 511, CultureInfo.get_InvariantCulture(), ref num);
					return num;
				}
				double num2;
				double.TryParse(nextWord, 511, CultureInfo.get_InvariantCulture(), ref num2);
				return num2;
			}

			private void EatWhitespace()
			{
				while (char.IsWhiteSpace(this.PeekChar))
				{
					this.json.Read();
					if (this.json.Peek() == -1)
					{
						break;
					}
				}
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
				string str;
				IList anArray;
				IDictionary obj;
				if (value == null)
				{
					this.builder.Append("null");
				}
				else if ((str = (value as string)) != null)
				{
					this.SerializeString(str);
				}
				else if (value is bool)
				{
					this.builder.Append((!(bool)value) ? "false" : "true");
				}
				else if ((anArray = (value as IList)) != null)
				{
					this.SerializeArray(anArray, indentationLevel);
				}
				else if ((obj = (value as IDictionary)) != null)
				{
					this.SerializeObject(obj, indentationLevel);
				}
				else if (value is char)
				{
					this.SerializeString(new string((char)value, 1));
				}
				else
				{
					this.SerializeOther(value);
				}
			}

			private void SerializeObject(IDictionary obj, int indentationLevel)
			{
				bool flag = true;
				this.builder.Append('{');
				this.builder.Append('\n');
				for (int i = 0; i < indentationLevel; i++)
				{
					this.builder.Append('\t');
				}
				IEnumerator enumerator = obj.get_Keys().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object current = enumerator.get_Current();
						if (!flag)
						{
							this.builder.Append(',');
							this.builder.Append('\n');
							for (int j = 0; j < indentationLevel; j++)
							{
								this.builder.Append('\t');
							}
						}
						this.SerializeString(current.ToString());
						this.builder.Append(':');
						indentationLevel++;
						this.SerializeValue(obj.get_Item(current), indentationLevel);
						indentationLevel--;
						flag = false;
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				this.builder.Append('\n');
				for (int k = 0; k < indentationLevel - 1; k++)
				{
					this.builder.Append('\t');
				}
				this.builder.Append('}');
			}

			private void SerializeArray(IList anArray, int indentationLevel)
			{
				this.builder.Append('[');
				bool flag = true;
				for (int i = 0; i < anArray.get_Count(); i++)
				{
					object value = anArray.get_Item(i);
					if (!flag)
					{
						this.builder.Append(',');
					}
					this.SerializeValue(value, indentationLevel);
					flag = false;
				}
				this.builder.Append(']');
			}

			private void SerializeString(string str)
			{
				this.builder.Append('"');
				char[] array = str.ToCharArray();
				for (int i = 0; i < array.Length; i++)
				{
					char c = array[i];
					char c2 = c;
					switch (c2)
					{
					case '\b':
						this.builder.Append("\\b");
						goto IL_14C;
					case '\t':
						this.builder.Append("\\t");
						goto IL_14C;
					case '\n':
						this.builder.Append("\\n");
						goto IL_14C;
					case '\v':
						IL_44:
						if (c2 == '"')
						{
							this.builder.Append("\\\"");
							goto IL_14C;
						}
						if (c2 != '\\')
						{
							int num = Convert.ToInt32(c);
							if (num >= 32 && num <= 126)
							{
								this.builder.Append(c);
							}
							else
							{
								this.builder.Append("\\u");
								this.builder.Append(num.ToString("x4"));
							}
							goto IL_14C;
						}
						this.builder.Append("\\\\");
						goto IL_14C;
					case '\f':
						this.builder.Append("\\f");
						goto IL_14C;
					case '\r':
						this.builder.Append("\\r");
						goto IL_14C;
					}
					goto IL_44;
					IL_14C:;
				}
				this.builder.Append('"');
			}

			private void SerializeOther(object value)
			{
				if (value is float)
				{
					this.builder.Append(((float)value).ToString("R", CultureInfo.get_InvariantCulture()));
				}
				else if (value is int || value is uint || value is long || value is sbyte || value is byte || value is short || value is ushort || value is ulong)
				{
					this.builder.Append(value);
				}
				else if (value is double || value is decimal)
				{
					this.builder.Append(Convert.ToDouble(value).ToString("R", CultureInfo.get_InvariantCulture()));
				}
				else if (value is Vector2)
				{
					Vector2 vector = (Vector2)value;
					this.builder.Append(string.Concat(new string[]
					{
						"\"(",
						vector.x.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						vector.y.ToString("R", CultureInfo.get_InvariantCulture()),
						")\""
					}));
				}
				else if (value is Vector3)
				{
					Vector3 vector2 = (Vector3)value;
					this.builder.Append(string.Concat(new string[]
					{
						"\"(",
						vector2.x.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						vector2.y.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						vector2.z.ToString("R", CultureInfo.get_InvariantCulture()),
						")\""
					}));
				}
				else if (value is Vector4)
				{
					Vector4 vector3 = (Vector4)value;
					this.builder.Append(string.Concat(new string[]
					{
						"\"(",
						vector3.x.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						vector3.y.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						vector3.z.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						vector3.w.ToString("R", CultureInfo.get_InvariantCulture()),
						")\""
					}));
				}
				else if (value is Quaternion)
				{
					Quaternion quaternion = (Quaternion)value;
					this.builder.Append(string.Concat(new string[]
					{
						"\"(",
						quaternion.x.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						quaternion.y.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						quaternion.z.ToString("R", CultureInfo.get_InvariantCulture()),
						",",
						quaternion.w.ToString("R", CultureInfo.get_InvariantCulture()),
						")\""
					}));
				}
				else
				{
					this.SerializeString(value.ToString());
				}
			}
		}

		public static object Deserialize(string json)
		{
			if (json == null)
			{
				return null;
			}
			return MiniJSON.Parser.Parse(json);
		}

		public static string Serialize(object obj)
		{
			return MiniJSON.Serializer.Serialize(obj);
		}
	}
}
