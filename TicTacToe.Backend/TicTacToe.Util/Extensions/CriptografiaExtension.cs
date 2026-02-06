using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TicTacToe.Util.Extensions
{
    public static class CriptografiaExtension
    {
        public static Criptografia Current { get; set; }

        public static string Criptografar(this string texto)
        {

            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            if (Current == null)
            {
                var msg = $"Operação inválida. A propriedade {nameof(Current)} da classe " +
                          $"estática {nameof(CriptografiaExtension)} não foi configurada.";
                throw new InvalidOperationException(msg);
            }

            return Current.Criptografar(texto);

        }

        public static string Descriptografar(this string textoCriptoBase64)
        {
            if (string.IsNullOrWhiteSpace(textoCriptoBase64)) return string.Empty;

            if (Current == null)
            {
                var msg = $"Operação inválida. A propriedade {nameof(Current)} da classe " +
                          $"estática {nameof(CriptografiaExtension)} não foi configurada.";
                throw new InvalidOperationException(msg);
            }

            if (textoCriptoBase64.Length < 5)
            {
                textoCriptoBase64 = $"0{textoCriptoBase64}";
            }

            return Current.Descriptografar(textoCriptoBase64);
        }

        public static string CriptografarUTF8(this string texto)
        {

            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            if (Current == null)
            {
                var msg = $"Operação inválida. A propriedade {nameof(Current)} da classe " +
                          $"estática {nameof(CriptografiaExtension)} não foi configurada.";
                throw new InvalidOperationException(msg);
            }

            return Current.CriptografarUTF8(texto);

        }

        public static string DescriptografarUTF8(this string textoCriptoBase64)
        {
            if (string.IsNullOrWhiteSpace(textoCriptoBase64)) return string.Empty;

            if (Current == null)
            {
                var msg = $"Operação inválida. A propriedade {nameof(Current)} da classe " +
                          $"estática {nameof(CriptografiaExtension)} não foi configurada.";
                throw new InvalidOperationException(msg);
            }

            return Current.DescriptografarUTF8(textoCriptoBase64);
        }

        public static string UriEncode(this string texto)
        {
            return Uri.EscapeDataString(texto);
        }

        public static string SerializarParaJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static TObjeto JsonParaObjeto<TObjeto>(this string obj)
        {
            return JsonConvert.DeserializeObject<TObjeto>(obj);
        }
    }
}
