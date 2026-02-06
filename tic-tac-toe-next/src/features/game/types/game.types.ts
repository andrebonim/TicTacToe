/**
 * Tipos principais do domínio do jogo da velha
 */

export type PlayerSymbol = 'X' | 'O' | null;

export interface BoardState {
  squares: PlayerSymbol[]; // array flat de 9 posições
}

export interface GameStatus {
  id?: number;                     // ID da partida no backend
  board: string[];                 // flat array ["X", "", "O", ...] – "" = vazio
  currentPlayerSymbol: PlayerSymbol;
  winner?: PlayerSymbol | null;
  isDraw: boolean;
  status: 'ongoing' | 'finished' | 'abandoned';
  mySymbol?: PlayerSymbol;         // símbolo do jogador atual no client
  roomCode?: string;               // código da sala (para modo rede)
  playerXName?: string;
  playerOName?: string;
  moveCount?: number;
}