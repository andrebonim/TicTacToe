'use client';

import Square from './Square';
import { PlayerSymbol } from '@/features/game/types';

interface BoardProps {
  squares: PlayerSymbol[];
  onSquareClick: (index: number) => void;
  winningLine?: number[];
  disabled?: boolean;
}

export default function Board({
  squares,
  onSquareClick,
  winningLine = [],
  disabled = false,
}: BoardProps) {
  const renderSquare = (index: number) => (
    <Square
      key={index}
      value={squares[index]}
      onClick={() => onSquareClick(index)}
      isWinning={winningLine.includes(index)}
      disabled={disabled || squares[index] !== null}
    />
  );

  return (
    <div className="grid grid-cols-3 gap-2 md:gap-3 mx-auto my-8">
      {Array.from({ length: 9 }).map((_, i) => (
        <div key={i} className="aspect-square">
          {renderSquare(i)}
        </div>
      ))}
    </div>
  );
}