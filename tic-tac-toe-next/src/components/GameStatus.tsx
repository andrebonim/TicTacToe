'use client';

import { GameStatus as GameStatusType, PlayerSymbol } from '@/features/game/types';  // ← renomeado como tipo

interface GameStatusProps {
  gameStatus: GameStatusType | null;           
  mySymbol?: PlayerSymbol;
  isMyTurn?: boolean;
}

export default function GameStatusDisplay({ gameStatus, mySymbol, isMyTurn }: GameStatusProps) {
  if (!gameStatus) {
    return (
      <div className="text-center text-xl font-medium text-gray-600">
        Carregando status do jogo...
      </div>
    );
  }

  const { winner, isDraw, currentPlayerSymbol, status } = gameStatus;

  let message = '';
  let className = 'text-xl md:text-2xl font-semibold text-center p-4 rounded-lg';

  if (status === 'finished') {
    if (winner) {
      message = winner === mySymbol ? 'Você venceu! 🎉' : `Jogador ${winner} venceu!`;
      if(winner === mySymbol){
        className += ' bg-green-100 text-green-800';
      }
      else{
        className += ' bg-red-100 text-red-800';
      }
      
    } else if (isDraw) {
      message = 'Empate!';
      className += ' bg-yellow-100 text-yellow-800';
    }
  } else if (status === 'ongoing') {
    if (isMyTurn) {
      message = `Sua vez (${mySymbol || currentPlayerSymbol})`;
      className += ' bg-blue-100 text-blue-800 animate-pulse';
    } else {
      message = `Vez do ${currentPlayerSymbol}`;
      className += ' bg-gray-100 text-gray-800';
    }
  } else if (status === 'abandoned') {
    message = 'Partida abandonada';
    className += ' bg-red-100 text-red-800';
  }

  return (
    <div className={className}>
      {message}
      {mySymbol && (
        <div className="text-base mt-2">
          Você joga como <strong>{mySymbol}</strong>
        </div>
      )}
    </div>
  );
}