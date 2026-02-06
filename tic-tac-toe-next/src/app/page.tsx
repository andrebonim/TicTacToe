'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { config } from '@/config/env';
import RankingModal from '@/components/RankingModal';

export default function Home() {
  const router = useRouter();

  const [playerXName, setPlayerXName] = useState('');
  const [playerOName, setPlayerOName] = useState('');
  const [roomCode, setRoomCode] = useState('');
  const [error, setError] = useState('');

  const startLocalGame = () => {
    if (!playerXName.trim() || !playerOName.trim()) {
      setError('Informe os nomes dos dois jogadores');
      return;
    }
    router.push(`/game?mode=local&px=${encodeURIComponent(playerXName)}&po=${encodeURIComponent(playerOName)}`);
  };

  const createNetworkGame = async () => {
    if (!playerXName.trim()) {
      setError('Informe o nome do Jogador X');
      return;
    }

    try {
      const res = await fetch(`${config.apiBaseUrl}/api/games`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ playerXName: playerXName.trim() }),
      });

      if (!res.ok) throw new Error('Falha ao criar partida');

      const data = await res.json();
      const gameId = data.id;

      // Se o jogador O já informou nome, podemos enviar join (opcional)
      if (playerOName.trim()) {
        await fetch(`${config.apiBaseUrl}/api/games/${gameId}/join`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ name: playerOName.trim() }),
        });
      }

      router.push(`/game?mode=network&gameId=${gameId}&px=${encodeURIComponent(playerXName)}`);
    } catch (err) {
      setError('Erro ao criar partida. Verifique o backend.');
    }
  };

  const joinNetworkGame = () => {
    if (!roomCode.trim()) {
      setError('Informe o código da sala');
      return;
    }
    router.push(`/game?mode=network&tipoPessoa=convidado&gameId=${roomCode.trim()}`);
  };

  const startGame = async (isNetwork: boolean) => {
    if (!playerXName.trim()) {
      setError('Informe o nome do Jogador X');
      return;
    }
  
    try {
      //setError(null);
      const res = await fetch(`${config.apiBaseUrl}/api/games`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          playerXName: playerXName.trim(),
          playerOName: playerOName.trim() || undefined,
          isNetwork: isNetwork
        }),
      });
  
      if (!res.ok) {
        const errText = await res.text();
        throw new Error(errText || 'Falha ao criar partida');
      }
  
      const data = await res.json();
      const newGameId = data.id;  // <--- pegue o ID retornado
  
      // Redireciona com gameId na URL
      router.push(
        `/game?mode=${isNetwork ? 'network' : 'local'}&tipoPessoa=criador&gameId=${newGameId}&px=${encodeURIComponent(playerXName.trim())}&po=${encodeURIComponent(playerOName.trim() || 'Jogador O')}`
      );
    } catch (err) {
      setError('Erro ao iniciar: ' + (err as Error).message);
    }
  };

  return (
    <main className="min-h-screen flex flex-col items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <RankingModal />
      <h1 className="text-5xl md:text-6xl font-extrabold text-indigo-900 mb-10">Jogo da Velha</h1>

      {error && <p className="text-red-600 bg-red-100 px-6 py-3 rounded-lg mb-6">{error}</p>}

      <div className="w-full max-w-lg bg-white rounded-2xl shadow-xl p-8 space-y-8">
        <div className="space-y-4">
          <input
            type="text"
            placeholder="Nome do Jogador X"
            value={playerXName}
            onChange={(e) => setPlayerXName(e.target.value)}
            className="w-full p-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-black placeholder:text-gray-400"
          />
          <input
            type="text"
            placeholder="Nome do Jogador O (opcional no modo rede)"
            value={playerOName}
            onChange={(e) => setPlayerOName(e.target.value)}
            className="w-full p-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-black placeholder:text-gray-400"
          />
        </div>

        <div className="grid gap-4">
          <button
            onClick={() => startGame(false)}  // local
            className="w-full py-4 bg-indigo-600 text-white rounded-lg text-lg font-semibold hover:bg-indigo-700 transition"
          >
            Iniciar Jogo Local
          </button>

          <button
            onClick={() => startGame(true)}   // rede
            className="w-full py-4 bg-green-600 text-white rounded-lg text-lg font-semibold hover:bg-green-700 transition"
          >
            Iniciar Jogo em Rede
          </button>

          <div className="flex gap-3">
            <input
              type="text"
              placeholder="Código da sala"
              value={roomCode}
              onChange={(e) => setRoomCode(e.target.value.toUpperCase())}
              className="flex-1 p-4 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 text-black placeholder:text-gray-400"
            />
            <button
              onClick={joinNetworkGame}
              className="px-8 py-4 bg-purple-600 text-white rounded-lg text-lg font-semibold hover:bg-purple-700 transition"
            >
              Entrar
            </button>
          </div>
          <p className="text-sm text-gray-500 text-center mt-4">
            • Jogo Local: joga sozinho ou com alguém no mesmo aparelho (salva no banco)  <br />
            • Jogo em Rede: joga com outra pessoa na mesma rede Wi-Fi (real-time)
          </p>
        </div>
      </div>
    </main>
  );
}