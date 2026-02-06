namespace TicTacToe.Util
{
    public class RetornoPadrao<TDados> : IStatusResposta<TDados>
    {
        public bool Sucesso { get; set; }

        public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

        public string MensagemErro { get; set; }

        public TDados Dados { get; set; }

    }

    // Helper para facilitar criação de falhas (opcional, mas deixa o código mais limpo)
    public static class RetornoPadraoExtensions
    {
        public static RetornoPadrao<T> Falha<T>(string mensagemErro) where T : class
        {
            return new RetornoPadrao<T>
            {
                Sucesso = false,
                MensagemErro = mensagemErro,
                Dados = null
            };
        }
    }
}