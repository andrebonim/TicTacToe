'use client';

import { PlayerSymbol } from '@/features/game/types';

interface SquareProps {
  value: PlayerSymbol;
  onClick: () => void;
  isWinning?: boolean;
  disabled?: boolean;
}

export default function Square({ value, onClick, isWinning = false, disabled = false }: SquareProps) {
  return (
    <button
      className={`
        w-24 h-24 md:w-32 md:h-32 text-5xl md:text-7xl font-bold
        flex items-center justify-center border-4 border-gray-700
        transition-all duration-200 text-black
        ${disabled 
          ? 'cursor-not-allowed opacity-70 bg-gray-100' 
          : 'cursor-pointer hover:bg-gray-50 active:scale-95'}
        ${isWinning 
          ? 'bg-yellow-200 border-yellow-500 animate-pulse' 
          : 'bg-white'}
      `}
      onClick={onClick}
      disabled={disabled}
      aria-label={`Casa ${value || 'vazia'}`}
    >
      {value}
    </button>
  );
}