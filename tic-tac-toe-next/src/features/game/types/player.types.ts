/**
 * Tipos relacionados a jogadores
 */

export interface PlayerSummary {
    id: number;
    name: string;
  }
  
  export interface PlayerStats {
    id: number;
    name: string;
    gamesPlayed: number;
    wins: number;
    draws: number;
    winRate: number; // porcentagem calculada
  }