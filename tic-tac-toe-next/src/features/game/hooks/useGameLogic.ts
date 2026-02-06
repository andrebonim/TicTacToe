import { useState, useCallback } from 'react';
import { PlayerSymbol } from '../types';
import { calculateWinner, isBoardFull } from '@/lib/utils';

export const useGameLogic = (initialSquares: PlayerSymbol[] = Array(9).fill(null)) => {
  const [squares, setSquares] = useState<PlayerSymbol[]>(initialSquares);
  const [isFinished, setIsFinished] = useState(false);

  const currentTurn: PlayerSymbol = 
    squares.filter(s => s !== null).length % 2 === 0 ? 'X' : 'O';

  const makeMove = useCallback((index: number, symbol: PlayerSymbol) => {
    if (squares[index] || isFinished || calculateWinner(squares).winner) {
      return false;
    }

    const newSquares = [...squares];
    newSquares[index] = symbol;
    setSquares(newSquares);

    const { winner } = calculateWinner(newSquares);
    const draw = isBoardFull(newSquares) && !winner;

    if (winner || draw) {
      setIsFinished(true);
    }
    return true;
  }, [squares, isFinished]);

  // Função só para o modo rede (SignalR) – o modo local NUNCA chama ela
  const syncBoardFromServer = useCallback((serverBoard: (string | '')[]) => {
    const newBoard = serverBoard.map(cell => 
      cell === '' ? null : (cell as PlayerSymbol)
    );

    setSquares(newBoard);

    const { winner } = calculateWinner(newBoard);
    const draw = isBoardFull(newBoard) && !winner;
    if (winner || draw) {
      setIsFinished(true);
    }
  }, []);

  const resetGame = useCallback(() => {
    setSquares(Array(9).fill(null));
    setIsFinished(false);
  }, []);

  const winnerInfo = calculateWinner(squares);

  return {
    squares,
    currentTurn,
    winner: winnerInfo.winner,
    winningLine: winnerInfo.line,
    isDraw: isBoardFull(squares) && !winnerInfo.winner,
    isFinished,
    makeMove,
    syncBoardFromServer,   // só usado no modo network
    resetGame,
  };
};