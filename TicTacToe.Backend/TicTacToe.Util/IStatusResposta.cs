using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Util
{
    public interface IStatusResposta
    {
        bool Sucesso { get; }
    }

    public interface IStatusResposta<T>: IStatusResposta
    {
    }
}
