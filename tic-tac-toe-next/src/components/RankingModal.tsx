'use client';

import { useState, useEffect } from 'react';
import { config } from '@/config/env';

interface PlayerRanking {
  playerName: string;
  wins: number;
  // Opcional: empates?: number; derrotas?: number; totalGames?: number;
}

export default function RankingModal() {
  const [isOpen, setIsOpen] = useState(false);
  const [ranking, setRanking] = useState<PlayerRanking[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchRanking = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`${config.apiBaseUrl}/api/games/top10`);
      
      if (!res.ok) {
        throw new Error('Falha ao carregar o ranking');
      }

      const data = await res.json();
      setRanking(data);
    } catch (err) {
      setError((err as Error).message || 'Erro ao carregar ranking');
    } finally {
      setLoading(false);
    }
  };

  // Carrega automaticamente quando o modal abre
  useEffect(() => {
    if (isOpen && ranking.length === 0) {
      fetchRanking();
    }
  }, [isOpen]);

  if (!isOpen) {
    return (
      <button
        onClick={() => setIsOpen(true)}
        className="fixed bottom-6 right-6 z-50 px-6 py-3 bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-full shadow-lg hover:shadow-xl transform hover:scale-105 transition-all font-semibold flex items-center gap-2"
      >
        <span>🏆 Ranking</span>
      </button>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full max-h-[85vh] overflow-hidden flex flex-col">
        {/* Cabeçalho */}
        <div className="bg-gradient-to-r from-indigo-600 to-purple-600 p-6 text-white">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-bold">Top 10 Maiores Vencedores</h2>
            <button
              onClick={() => setIsOpen(false)}
              className="text-white hover:text-gray-200 text-2xl font-bold"
            >
              ×
            </button>
          </div>
          <p className="text-indigo-100 mt-1">Baseado em vitórias registradas</p>
        </div>

        {/* Conteúdo */}
        <div className="p-6 overflow-y-auto flex-1">
          {loading ? (
            <div className="flex justify-center items-center h-40">
              <div className="animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-indigo-600"></div>
            </div>
          ) : error ? (
            <div className="text-center text-red-600 py-8">
              {error}
              <button
                onClick={fetchRanking}
                className="mt-4 px-4 py-2 bg-red-100 text-red-700 rounded-lg hover:bg-red-200"
              >
                Tentar novamente
              </button>
            </div>
          ) : ranking.length === 0 ? (
            <div className="text-center text-gray-500 py-10">
              Ainda não há vitórias registradas...
            </div>
          ) : (
            <div className="space-y-3">
              {ranking.map((player, index) => (
                <div
                  key={player.playerName}
                  className={`flex items-center justify-between p-4 rounded-lg ${
                    index === 0
                      ? 'bg-yellow-50 border-2 border-yellow-400'
                      : index === 1
                      ? 'bg-gray-100'
                      : index === 2
                      ? 'bg-amber-100'
                      : 'bg-gray-50'
                  }`}
                >
                  <div className="flex items-center gap-4">
                    <span className="text-2xl font-bold w-10 text-center text-black">
                      {index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : `${index + 1}º`}
                    </span>
                    <span className="font-semibold text-lg truncate max-w-[220px] text-black">
                      {player.playerName}
                    </span>
                  </div>
                  <span className="text-xl font-bold text-indigo-700">
                    {player.wins} {player.wins === 1 ? 'vitória' : 'vitórias'}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Rodapé */}
        <div className="p-4 border-t border-gray-200 text-center text-sm text-gray-500">
          Atualizado em tempo real • Jogue mais para aparecer aqui!
        </div>
      </div>
    </div>
  );
}