using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Next_V4.Util
{
    public static class Enumeradores
    {

        public static KeyValuePair<string, string>[] GetListEnumDescription<TEnum>()
        {

            var values = Enum.GetValues(typeof(TEnum));
            var dic = new Dictionary<string, string>();

            for (int i = 0; i < values.Length; i++)
            {

                var value = values.GetValue(i);
                if (value == null) continue;

                var text = GetEnumDescription((Enum)value);
                dic.TryAdd(value.ToString(), text);

            }

            return dic.ToArray();

        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

    }


}
