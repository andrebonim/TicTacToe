using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{

    public static string ApenasNumeros(this string valor)
    {
        if (valor == null) return null;
        var result = Regex.Replace(valor, "[^0-9]", string.Empty);
        return result;
    }

    public static string FormatCnpjCpf(this string valor)
    {

        if (string.IsNullOrWhiteSpace(valor) || string.IsNullOrEmpty(valor))
        {
            return "";
        }

        var novoValor = valor.Trim().Replace(".", "").Replace("-", "")
                        .Replace("\\", "").Replace("/", "")
                        .Replace(" ", "");

        bool isCnpj;
        string retorno;

        if (novoValor.Length <= 11)
        {
            novoValor = novoValor.PadLeft(11, '0');
            isCnpj = false;
        } else
        {
            novoValor = novoValor.PadLeft(14, '0');
            isCnpj = true;  
        }

        if (isCnpj)
        {
            var n = novoValor;
            retorno = $"{n[0..2]}.{n[2..5]}.{n[5..8]}/{n[8..12]}-{n[12..14]}";
        } else
        {
            var n = novoValor;
            retorno = $"{n[0..3]}.{n[3..6]}.{n[6..9]}-{n[9..11]}";
        }

        return retorno;
    }

    public static string PrimeiraLetraMaiuscula(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

    public static string SubstituirSeVazio(this string valor, string novoValor = "-")
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return novoValor;
        }
        return valor;
    }

    public static string SubstituirSeIgual(this string valor,string igual, string novoValor = "")
    {
        return valor == igual ? novoValor : valor;
    }

    public static bool TemConteudo(this string valor)
    {
        return string.IsNullOrWhiteSpace(valor) == false;
    }

    /// <summary>
    /// Agrupa um número seguindo o padrão X-XXXX-XXXX
    /// </summary>
    /// <returns></returns>
    public static string AgruparNumeros(this string numero)
    {

        var novoNumero = numero.ApenasNumeros();
        if (string.IsNullOrWhiteSpace(novoNumero)) return novoNumero;

        var invertido = novoNumero.Reverse().ToArray();
        var stb = new StringBuilder();
        var cont = 0;

        for (int i = 0; i < invertido.Length; i++)
        {
            stb.Append(invertido[i]);
            cont++;

            if (cont == 4 && (i + 1) < invertido.Length)
            {
                stb.Append("-");
                cont = 0;
            }
        }

        novoNumero = string.Join(string.Empty, stb.ToString().Reverse());

        return novoNumero;

    }

    public static string IncluirSufixo(this string texto, string sufixo, bool somenteSeNaoExistir = true)
    {
        
        if (string.IsNullOrWhiteSpace(texto)) return sufixo;

        if (texto.EndsWith(sufixo) == false || somenteSeNaoExistir == false)
        {
            return texto + sufixo;
        }

        return texto;

    }

    public static string FormataNomeEmpresa(this string nome)
    {
        return nome.ToUpper()
            .Replace(".", "")
            .Replace("/", "")
            .Replace("SA", "")
            .Replace("LTDA", "")
            .Replace(" - ME", "")
            .Replace(" ME", "")
            .Replace(" S/C", "")
            .Replace(" S/S", "")
            .Replace(" S.A", "")
            .Replace(" S.A.", "")
            .Replace(" S/A.", "")
            .Replace(" S/A", "")
            .Replace(" S A", "")
            .Replace(" SA", "")
            .Replace(" EPP", "")
            .Replace(" LTDA- ME", "")
            .Replace(" LTDA -ME", "")
            .Replace(" LTDA - ME", "")
            .Replace(" LTDA", "")
            .Replace(" LTDA.", "");

        // 
    }

    public static string FormataSite(this string input)
    {
        return input.Replace("www.", "")
            .Replace(".com", "")
            .Replace(".br", "")
            .Replace(".org", "")
            .Replace(".us", "")
            .Replace(".net", "")
            .Replace(".", "")
            .Replace(".", "")
            .Replace(".", "")
            .Replace(".io", "")
            .Replace("https://", "")
            .Replace("http://", "")
            .Replace("/", "");
    }

    
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}

