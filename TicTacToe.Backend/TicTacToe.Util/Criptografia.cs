using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Util
{
    public class Criptografia
    {

        private readonly string _chave;

        public Criptografia(string chave)
        {
            _chave = chave;
        }

        /// <summary>
        /// Criptografa usando AES e transforma para base 64
        /// </summary>
        public string Criptografar(string texto)
        {

            if (string.IsNullOrEmpty(texto) || string.IsNullOrEmpty(_chave))
            {
                throw new InvalidOperationException($"Um dos parâmetros ({nameof(texto)} ou {nameof(_chave)}) para criptografia não foi informado.");
            }

            var bytesTexto = Encoding.ASCII.GetBytes(texto);

            var md5 = System.Security.Cryptography.MD5.Create();

            var keyHash = md5.ComputeHash(Encoding.ASCII.GetBytes(_chave));
            var ivHash = md5.ComputeHash(Encoding.ASCII.GetBytes(_chave + _chave.ToUpper()));

            var aesCript = System.Security.Cryptography.Aes.Create();
            var encriptor = aesCript.CreateEncryptor(keyHash, ivHash);
            var bytesCripto = encriptor.TransformFinalBlock(bytesTexto, 0, bytesTexto.Length);

            var base64 = Convert.ToBase64String(bytesCripto);
            var result = Uri.EscapeDataString(base64);
            return result;

        }

        /// <summary>
        /// Decodifica o texto em base 64 e depois descriptografa
        /// </summary>
        public string Descriptografar(string textoCriptoBase64)
        {

            if (string.IsNullOrEmpty(textoCriptoBase64) || string.IsNullOrEmpty(_chave))
            {
                throw new InvalidOperationException($"Um dos parâmetros ({nameof(textoCriptoBase64)} ou {nameof(_chave)}) para descriptografia não foi informado.");
            }

            var md5 = System.Security.Cryptography.MD5.Create();

            var keyHash = md5.ComputeHash(Encoding.ASCII.GetBytes(_chave));
            var ivHash = md5.ComputeHash(Encoding.ASCII.GetBytes(_chave + _chave.ToUpper()));

            var aesCript = System.Security.Cryptography.Aes.Create();
            var decriptor = aesCript.CreateDecryptor(keyHash, ivHash);

            var decoded = Uri.UnescapeDataString(textoCriptoBase64);
            byte[] bytesTextoCripto;
            try
            {
                bytesTextoCripto = Convert.FromBase64String(decoded);
            }
            catch (Exception)
            {
                return decoded;
            }            

            var bytesDecripto = decriptor.TransformFinalBlock(bytesTextoCripto, 0, bytesTextoCripto.Length);

            return Encoding.ASCII.GetString(bytesDecripto);

        }
        /// <summary>
        /// Criptografa usando AES e transforma para base 64
        /// </summary>
        public string CriptografarUTF8(string texto)
        {

            if (string.IsNullOrEmpty(texto) || string.IsNullOrEmpty(_chave))
            {
                throw new InvalidOperationException($"Um dos parâmetros ({nameof(texto)} ou {nameof(_chave)}) para criptografia não foi informado.");
            }

            var bytesTexto = Encoding.UTF8.GetBytes(texto);

            var md5 = System.Security.Cryptography.MD5.Create();

            var keyHash = md5.ComputeHash(Encoding.UTF8.GetBytes(_chave));
            var ivHash = md5.ComputeHash(Encoding.UTF8.GetBytes(_chave + _chave.ToUpper()));

            var aesCript = System.Security.Cryptography.Aes.Create();
            var encriptor = aesCript.CreateEncryptor(keyHash, ivHash);
            var bytesCripto = encriptor.TransformFinalBlock(bytesTexto, 0, bytesTexto.Length);

            var base64 = Convert.ToBase64String(bytesCripto);


            int limit = 2000;

            StringBuilder sb = new StringBuilder();
            int loops = base64.Length / limit;

            for (int i = 0; i <= loops; i++)
            {
                if (i < loops)
                {
                    sb.Append(Uri.EscapeDataString(base64.Substring(limit * i, limit)));
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(base64.Substring(limit * i)));
                }
            }

            var result = sb.ToString();// Uri.EscapeDataString(base64);
            return result;

        }

        /// <summary>
        /// Decodifica o texto em base 64 e depois descriptografa
        /// </summary>
        public string DescriptografarUTF8(string textoCriptoBase64)
        {

            if (string.IsNullOrEmpty(textoCriptoBase64) || string.IsNullOrEmpty(_chave))
            {
                throw new InvalidOperationException($"Um dos parâmetros ({nameof(textoCriptoBase64)} ou {nameof(_chave)}) para descriptografia não foi informado.");
            }

            var md5 = System.Security.Cryptography.MD5.Create();

            var keyHash = md5.ComputeHash(Encoding.UTF8.GetBytes(_chave));
            var ivHash = md5.ComputeHash(Encoding.UTF8.GetBytes(_chave + _chave.ToUpper()));

            var aesCript = System.Security.Cryptography.Aes.Create();
            var decriptor = aesCript.CreateDecryptor(keyHash, ivHash);

            var decoded = Uri.UnescapeDataString(textoCriptoBase64);
            var bytesTextoCripto = Convert.FromBase64String(decoded);

            var bytesDecripto = decriptor.TransformFinalBlock(bytesTextoCripto, 0, bytesTextoCripto.Length);

            return Encoding.UTF8.GetString(bytesDecripto);

        }
    }
}
