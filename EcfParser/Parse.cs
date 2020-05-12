using System;
using System.Collections.Generic;
using System.Globalization;

namespace EcfParser
{

    public static class Parse
    {
        public static EcfFile Deserialize(params string[] lines)
        {
            var result = new EcfFile();
            var i = -1;

            do
            {
                string currentLine = ReadNextLine();

                if (!string.IsNullOrEmpty(currentLine))
                {
                    if (currentLine.StartsWith("{"))
                    {
                        var block = ReadBlock(false, currentLine, ReadNextLine);

                        if (result.Blocks == null) result.Blocks = new List<EcfBlock>();
                        result.Blocks.Add(block);
                    }
                    else
                    {
                        var attr = ReadAttribute(currentLine);
                        if (attr != null && attr.Name.Equals("VERSION", StringComparison.InvariantCultureIgnoreCase)) result.Version = (int)attr.Value;
                    }
                }
            } while (i < lines.Length - 1);

            return result;

            string ReadNextLine()
            {
                var line = lines[++i].Trim();
                var commentPos = line.IndexOf('#');
                line = (commentPos >= 0 ? line.Substring(0, commentPos) : line).Trim();
                commentPos = line.IndexOf("/*");
                if(commentPos >= 0)
                {
                    var commentEnd = line.IndexOf("*/");
                    if (commentEnd >= 0) return 
                            (line.Substring(0, commentPos) + 
                            (line.Length == commentEnd + 2 ? string.Empty : line.Substring(commentEnd + 2))).Trim();

                    while (line.IndexOf("*/") == -1) line = lines[++i].Trim();
                    return string.Empty;
                }
                return line;
            }
        }

        private static EcfBlock ReadBlock(bool isChild, string line, Func<string> nextLine)
        {
            var currentLine = line.Substring(1).Trim();
            var nameDelimiterPos = isChild ? currentLine.Length : currentLine.IndexOf(' ');
            var block = new EcfBlock()
            {
                Name = nameDelimiterPos > 0 ? currentLine.Substring(0, nameDelimiterPos).Trim() : null
            };
            if (nameDelimiterPos == currentLine.Length) currentLine = string.Empty;
            else if (nameDelimiterPos > 0) currentLine = currentLine.Substring(nameDelimiterPos).Trim();

            int unnamedChild = 0;

            do{
                if (currentLine.StartsWith("{"))
                {
                    var childBlock = ReadBlock(true, currentLine, nextLine);
                    if (block.Childs == null) block.Childs = new Dictionary<string, EcfBlock>();
                    block.Childs.Add(childBlock.Name ?? unnamedChild++.ToString(), childBlock);
                }
                else
                {
                    var attr = ReadAttribute(currentLine);
                    if(attr != null)
                    {
                        if (block.Attr == null) block.Attr = new List<EcfAttribute>();
                        block.Attr.Add(attr);
                    }
                }

                currentLine = nextLine().Trim();
            } while (!currentLine.StartsWith("}"));

            return block;
        }

        public static EcfAttribute ReadAttribute(string lineData)
        {
            var line = lineData.Trim();
            var delimiterPos = line.IndexOfAny(new[] { ':', ' ' });
            if (delimiterPos == -1) return null;

            EcfAttribute result = null;

            do{
                delimiterPos = line.IndexOfAny(new[] { ':', ' ' });
                var name = line.Substring(0, delimiterPos);
                line = line.Substring(delimiterPos + 1).Trim();

                var nextPayload = line.IndexOfAny(new[] { ',', '"' });

                if (nextPayload == -1)
                {
                    if (result == null) result = new EcfAttribute()
                    {
                        Name = name,
                        Value = ParseValue(line.Trim()),
                    };
                    else
                    {
                        if (result.AddOns == null) result.AddOns = new Dictionary<string, object>();
                        result.AddOns.Add(name, ParseValue(line.Trim()));
                    }
                    line = null;
                }
                else if (line[nextPayload] == '"')
                {
                    var payloadEnd = line.IndexOf('"', nextPayload + 1);
                    if (result == null) result = new EcfAttribute()
                    {
                        Name = name,
                        Value = ParseValue(line.Substring(nextPayload + 1, payloadEnd - nextPayload - 1).Trim()),
                    };
                    else
                    {
                        if (result.AddOns == null) result.AddOns = new Dictionary<string, object>();
                        result.AddOns.Add(name, ParseValue(line.Substring(nextPayload + 1, payloadEnd - nextPayload - 1).Trim()));
                    }

                    if (payloadEnd + 1 >= line.Length) line = null;
                    else
                    {
                        payloadEnd = line.IndexOf(',', payloadEnd + 1);
                        line = line.Substring(payloadEnd + 1).Trim();
                    }
                }
                else if(nextPayload > 0) {
                    if (result == null) result = new EcfAttribute()
                    {
                        Name = name,
                        Value = ParseValue(line.Substring(0, nextPayload).Trim()),
                    };
                    else
                    {
                        if (result.AddOns == null) result.AddOns = new Dictionary<string, object>();
                        result.AddOns.Add(name, ParseValue(line.Substring(0, nextPayload).Trim()));
                    }
                    line = line.Substring(nextPayload + 1).Trim();
                }
                        
            } while (!string.IsNullOrEmpty(line));

            return result;
        }

        private static object ParseValue(string v)
        {
            if (bool  .TryParse(v, out var b)) return b;
            if (int   .TryParse(v, NumberStyles.Integer, null, out var i)) return i;
            if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return f;

            return v;
        }
    }
}
