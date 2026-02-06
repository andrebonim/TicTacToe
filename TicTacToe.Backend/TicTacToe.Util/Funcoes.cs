using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Next_V4.Util
{
    public static class Funcoes
    {

        public static string FormatNumberSepMilharPorCulture(int valor)
        {            
            return string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:0,0}", valor);
        }
        public static string FormatNumberSepMilhar(int valor)
        {
            return valor.ToString("#,0");
        }

        public static string FormatNumber(decimal? num)
        {
            if (!num.HasValue)
                return "0";
            return FormatNumber(Convert.ToInt64(num));
        }
        
        public static string FormatNumber(double? num)
        {
            if (!num.HasValue)
                return "0";
            return FormatNumber(Convert.ToInt64(num));
        }
        
        public static string FormatNumber(int? num)
        {
            if (!num.HasValue)
                return "0";
            return FormatNumber(Convert.ToInt64(num));
        }
        public static string FormatNumber(long? num)
        {
            if (!num.HasValue)
                return "0";
            if (num >= 1000000)
                return (num / 1000000) + "MM";
            if (num >= 10000)
            {
                return (num / 1000D)?.ToString("0") + "M";
                //return (num / 1000D)?.ToString("0.#") + "M";
            }
            return num?.ToString("#,0");
        }
        public static string NormalizaBairro(string entrada)
        {
            entrada = entrada?.ToUpper();
            if (entrada?.IndexOf("PARQUE") == 0)
            {
                var regex = new Regex(Regex.Escape("PARQUE"));
                entrada = regex.Replace(entrada, "PRQ", 1);
            }
            if (entrada?.IndexOf("VILA") == 0)
            {
                var regex = new Regex(Regex.Escape("VILA"));
                entrada = regex.Replace(entrada, "VL", 1);
            }
            return entrada;
        }
        public static string FormatNumber(string num)
        {
            return FormatNumber(Convert.ToInt64(num));
        }
        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        public static string RemoveAccents(this string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return text;
                StringBuilder sbReturn = new StringBuilder();
                var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
                foreach (char letter in arrayText)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                        sbReturn.Append(letter);
                }
                return sbReturn.ToString();
            }
            catch (Exception)
            {
                return text;
            }
            
        }

        /// <summary>
        /// Remove os caracteres que não podem ser usados na nomeação de uma 
        /// arquivo substituindo-os por _ (sublinhado/underline/undescore)
        /// </summary>
        public static string RemoverCaracteresInvalidosNomeArquivo(this string text)
        {
            if (text == null) return null;

            char[] invalidPathChars = Path.GetInvalidPathChars();

            var aux = text
                .Replace(" ", "_").Replace('\\', '_')
                .Replace('/', '_').Replace(",", "_");

            foreach (var item in invalidPathChars)
            {
                aux = aux.Replace(item, '_');
            }

            return aux.Replace("__", "_");

        }

        public static string ToStringOrEmpty(this string text)
        {
            if (!string.IsNullOrEmpty(text))
                return text.ToString();
            return string.Empty;
        }


        /// <summary>
        /// Salva um arquivo em uma conta de armazenamento. Se a operação tiver sucesso, executa Dispose no stream
        /// </summary>
        public static string CreateBlob(Stream stream, string nomeContainer, string name, string contentType, string connectionBlobAzure)
        {
           
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            CloudBlockBlob blob = ObterCloudBlockBlob(nomeContainer, connectionBlobAzure, name);
            blob.Properties.ContentType = contentType;

            try
            {
                var task = blob.UploadFromStreamAsync(stream);
                task.GetAwaiter().GetResult();
                try { stream.Dispose(); } catch { /* Não há necessidade de tratamento */ }
            }
            catch (Exception e)
            {

                if (!e.Message.Contains("Bad Request"))
                {
                    throw e;
                }

                const string msg = "Solicitação inválida. Pode ser que o nome do seu arquivo contenha caracteres inválidos.";
                throw new Exception(msg, e);
                
            }

            return blob.StorageUri.PrimaryUri.AbsoluteUri;

        }

        public static async Task<Stream> DownloadFromBlob(string nomeArquivo, string nomeContainer, string connectionBlobAzure)
        {
            CloudBlockBlob blob = ObterCloudBlockBlob(nomeContainer, connectionBlobAzure, nomeArquivo);

            MemoryStream memStream = new MemoryStream();

            await blob.DownloadToStreamAsync(memStream);

            return memStream;
        }

        public static async Task<bool> RemoveFromBlob(string nomeArquivo, string nomeContainer, string connectionBlobAzure)
        {
            CloudBlockBlob blob = ObterCloudBlockBlob(nomeContainer, connectionBlobAzure, nomeArquivo);

            return await blob.DeleteIfExistsAsync();
        }

        private static CloudBlockBlob ObterCloudBlockBlob(string nomeContainer, string connectionBlobAzure, string nomeArquivo)
        {
            string connectionString;
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;

            connectionString = connectionBlobAzure;
            storageAccount = CloudStorageAccount.Parse(connectionString);

            client = storageAccount.CreateCloudBlobClient();

            container = client.GetContainerReference(nomeContainer);

            container.CreateIfNotExistsAsync();

            return container.GetBlockBlobReference(nomeArquivo);
        }

        public static bool IsCnpj(string cnpj)
        {
            try
            {
                if (string.IsNullOrEmpty(cnpj))
                {
                    return false;
                }

                int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int soma;
                int resto;
                string digito;
                string tempCnpj;
                cnpj = cnpj.Trim();
                cnpj = cnpj.ApenasNumeros();
                if (cnpj.Length > 14)
                    cnpj = cnpj.Substring(cnpj.Length - 14);
                else
                    cnpj = cnpj.PadLeft(14, '0');
                tempCnpj = cnpj.Substring(0, 12);
                soma = 0;
                for (int i = 0; i < 12; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
                resto = (soma % 11);
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = resto.ToString();
                tempCnpj = tempCnpj + digito;
                soma = 0;
                for (int i = 0; i < 13; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
                resto = (soma % 11);
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = digito + resto.ToString();
                return cnpj.EndsWith(digito);
            }
            catch (Exception e)
            {
            }
            return false;
        }

        public static bool IsInt(this string value)
        {
            int aux = 0;
            return int.TryParse(value, out aux);
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static Dictionary<string, string> GetInfoDictionary(this IDisplayInfo instance)
        {
            Dictionary<string, string> dados = new Dictionary<string, string>();
            var prop = instance.GetType().GetProperties();

            foreach (var p in prop)
            {

                if (p.CustomAttributes.Any(c => c.AttributeType == typeof(DisplayAttribute)))
                {
                    object objValue;
                    var attr = p.GetCustomAttributes(typeof(DisplayAttribute), false)?.Cast<DisplayAttribute>()?.Single();
                    string display = attr.Description ?? string.Empty;
                    string value = string.Empty;
                    objValue = p.GetValue(instance);
                    if (objValue != null)
                    {
                        if (objValue.GetType() == typeof(bool?) || objValue.GetType() == typeof(bool))
                            value = (Convert.ToBoolean(objValue)) ? "Sim" : "Não";
                        else if (objValue.GetType() == typeof(List<string>))
                        {
                            value = string.Join(",", (objValue as List<string>));
                        }
                        else if (objValue.GetType() == typeof(string[]))
                        {
                            value = string.Join(",", (objValue as string[]));
                        }
                        else if (objValue.GetType() == typeof(decimal?) || objValue.GetType() == typeof(decimal))
                        {
                            if (Convert.ToDecimal(objValue) != 0)
                                if (attr.GroupName?.ToLower() == "money")
                                    value = FormatNumber(Convert.ToDecimal(objValue));
                                else
                                    value = string.Format("{0:#,0}", Convert.ToInt64(objValue));

                        }
                        else if (objValue.GetType() == typeof(int?) || objValue.GetType() == typeof(int))
                        {
                            if (Convert.ToInt32(objValue) != 0)
                                if (attr.GroupName?.ToLower() == "money")
                                    value = FormatNumber(Convert.ToInt32(objValue));
                                else
                                    value = string.Format("{0:#,0}", Convert.ToInt64(objValue));
                        }
                        else if (objValue.GetType() == typeof(string))
                        {
                            if (objValue.ToString() == "true")
                                value = (Convert.ToBoolean(objValue)) ? "Sim" : "Não";
                            else if (objValue.ToString() == "S" || objValue.ToString() == "N")
                                value = (objValue.ToString() == "S") ? "Sim" : "Não";
                            else if (objValue.ToString() == "on" || objValue.ToString() == "off")
                                value = (objValue.ToString() == "on") ? "Sim" : "Não";
                            else
                                value = objValue.ToString();
                        }
                        else
                            value = objValue.ToString();

                        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(display))
                        {
                            dados.Add(display, value);
                        }
                    }
                }
            }
            return dados;
        }

        public static List<string> LerArquivo(string[] linhas)
        {
            List<string> cnpjs = new List<string>();
            foreach (var item in linhas)
            {
                var cnpjPossivel = item.ApenasNumeros();

                if (cnpjPossivel.Length < 14)
                {
                    int quantidadeDeZeros = 14 - cnpjPossivel.Length;

                    for (int i = 0; i < quantidadeDeZeros; i++)
                    {
                        cnpjPossivel = "0" + cnpjPossivel;
                    }
                }


                if (Funcoes.IsCnpj(cnpjPossivel))
                    cnpjs.Add(cnpjPossivel);
            }

            return cnpjs;

        }

        public static string AjustarFiltroBairro(string entrada)
        {
            try
            {
                var entradaDados = entrada.Split(" ");

                if (entradaDados.Length > 1)
                {
                    var campo = entradaDados[0];
                    string retorno = "";

                    switch (campo)
                    {
                        case "JD":
                            retorno = "JARDIM";
                            break;
                        case "PRQ":
                            retorno = "PARQUE";
                            break;
                        case "PC":
                            retorno = "PRACA";
                            break;
                        case "FAZ":
                            retorno = "FAZENDA";
                            break;
                        case "SIT":
                            retorno = "SITIO";
                            break;
                        case "CH":
                            retorno = "CHACARA";
                            break;
                        case "VL":
                            retorno = "VILA";
                            break;
                        case "AT":
                            retorno = "ALTO";
                            break;
                        case "CID":
                            retorno = "CIDADE";
                            break;
                        case "CJ":
                            retorno = "CONJUNTO";
                            break;
                        case "S":
                            retorno = "SAO";
                            break;
                    }

                    return retorno + " " + String.Join(" ", entradaDados.Skip(1));
                }
                else
                {
                    return entrada;
                }
            }
            catch (Exception ex)
            {
                return entrada;
            }
        }

        public static string AjustarFiltroBairroLocalidade(string entrada)
        {
            try
            {
                var entradaDados = entrada.Split(" ");

                var campo = entradaDados[0];

                switch (campo)
                {
                    case "JARDIM":
                        entradaDados[0] = "JD";
                        break;
                    case "PARQUE":
                        entradaDados[0] = "PRQ";
                        break;
                    case "PRACA":
                        entradaDados[0] = "PC";
                        break;
                    case "FAZENDA":
                        entradaDados[0] = "FAZ";
                        break;
                    case "SITIO":
                        entradaDados[0] = "SIT";
                        break;
                    case "CHACARA":
                        entradaDados[0] = "CH";
                        break;
                    case "VILA":
                        entradaDados[0] = "VL";
                        break;
                    case "ALTO":
                        entradaDados[0] = "AT";
                        break;
                    case "CIDADE":
                        entradaDados[0] = "CID";
                        break;
                    case "CONJUNTO":
                        entradaDados[0] = "CJ";
                        break;
                    case "SAO":
                        entradaDados[0] = "S";
                        break;
                }

                return String.Join(" ", entradaDados);
            }
            catch (Exception ex)
            {
                return entrada;
            }
        }

        public static DateTime DateTimeBr()
        {
            return global::DateTimeBr.NowBr();
        }

        public static void CopiarPropriedades<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);

                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }

        public static string StripHTML(string HTMLCode)
        {
            // Remove new lines since they are not visible in HTML
            HTMLCode = HTMLCode.Replace("\n", " ");

            // Remove tab spaces
            HTMLCode = HTMLCode.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

            // Remove HEAD tag
            HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", ""
                                , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", ""
              , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(HTMLCode);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = {"&nbsp;", "&amp;", "&quot;", "&lt;",
   "&gt;", "&reg;", "&copy;", "&bull;", "&trade;","&#39;"};
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "\'" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("<p ", "<p ");
            sbHTML.Replace("</p>", "</p> ");
            sbHTML.Replace(";", "/");

            // Finally, remove all HTML tags and return plain text
            return System.Text.RegularExpressions.Regex.Replace(
              sbHTML.ToString(), "<[^>]*>", "");
        }

        public static Uri ConverterParaUri(this string site)
        {
            if (string.IsNullOrWhiteSpace(site)) return null;

            var aux = site.Trim().ToLower();

            if (aux.StartsWith("http://") == false && aux.StartsWith("https://") == false)
            {
                aux = aux.Replace("http:", string.Empty)
                    .Replace("http:/", string.Empty)
                    .Replace("https:", string.Empty)
                    .Replace("https:/", string.Empty);
                aux = "http://" + aux;
            }

            return new Uri(aux);
        }

    }
}
