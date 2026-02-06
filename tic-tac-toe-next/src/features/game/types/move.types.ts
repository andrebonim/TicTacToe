/**
 * Tipos relacionados a jogadas e respostas do backend/SignalR
 */

import { GameStatus, PlayerSymbol } from "./game.types";

export interface MakeMoveRequest {
    gameId: number;
    playerId: number;
    position: number; // 0 a 8
  }
  
  export interface MakeMoveResponse {
    success: boolean;
    message?: string;
    updatedGame: GameStatus;
    winner?: PlayerSymbol | null;
    isDraw: boolean;
  }
  
  /**
   * Payload recebido via SignalR quando uma jogada é feita
   */
  export interface MoveUpdatePayload {
    updatedGame: GameStatus;
    // pode ter mais campos se o backend enviar (ex: timestamp, player que jogou)
  }