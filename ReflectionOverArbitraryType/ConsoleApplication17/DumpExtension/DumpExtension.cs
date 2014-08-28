using System;
using System.Linq;
using System.Text;

namespace ConsoleApplication17
{
    public static class DumpExtension
    {
        public static T Dump<T>(this T obj)
        {
            if (obj == null)
            {
                Console.WriteLine("null");
            }
            else
            {
                if (obj.IsValue())
                {
                    Console.WriteLine(obj);
                }
                else
                {
                    var sb = new StringBuilder();

                    sb.AppendFormat("{0}: ", obj.GetType().Name).AppendLine();
                    obj.DumpToStringBuilder(sb, 1);

                    Console.WriteLine(sb.ToString());
                }
            }

            return obj;
        }

        private static void DumpToStringBuilder(this object obj, StringBuilder sb, int indentLevel)
        {
            foreach (var property in obj.GetType().GetProperties().Where(p => p.CanRead))
            {
                sb.AppendFormat("{0}{1} {2}: ", indentLevel.GetIndentString(), property.PropertyType.Name, property.Name);

                var propertyValue = property.GetValue(obj);

                if (propertyValue == null)
                {
                    sb.AppendLine("null");
                }
                else
                {
                    if (propertyValue.IsValue())
                    {
                        sb.Append(propertyValue).AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        propertyValue.DumpToStringBuilder(sb, indentLevel + 1);
                    }
                }
            }
        }

        private static bool IsValue(this object obj)
        {
            var type = obj.GetType();
            return type.IsPrimitive || type == typeof(string);
        }

        private static string GetIndentString(this int indentLevel)
        {
            return string.Concat(Enumerable.Repeat("  ", indentLevel));
        }
    }
}
