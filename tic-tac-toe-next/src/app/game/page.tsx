'use client';

import { useSearchParams, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import Board from '@/components/ui/Board';
import GameStatusDisplay from '@/components/GameStatus';
import { calculateWinner, isBoardFull } from '@/lib/utils';
import { GameStatus, PlayerSymbol } from '@/features/game/types';
import { config } from '@/config/env';
import { connectToGame, sendMove, disconnectSignalR } from '@/features/game/services/signalRGame';
import { useGameLogic } from '@/features/game/hooks/useGameLogic';

export default function GamePage() {
  const searchParams = useSearchParams();
  const router = useRouter();

  const mode = searchParams.get('mode') || 'local';
  const tipoPessoa = searchParams.get('tipoPessoa') || 'criador';
  const gameIdFromUrl = searchParams.get('gameId') ? Number(searchParams.get('gameId')) : null;
  const playerXNameFromUrl = decodeURIComponent(searchParams.get('px') || 'Jogador X');
  const playerONameFromUrl = decodeURIComponent(searchParams.get('po') || 'Aguardando...');

  const isNetworkMode = mode === 'network';
  
  const [gameId, setGameId] = useState<number | null>(gameIdFromUrl);
  const [playerId, setPlayerId] = useState<number | null>(null);
  const [mySymbol, setMySymbol] = useState<PlayerSymbol>(null);
  const [statusMessage, setStatusMessage] = useState('Carregando...');
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [isFinished, setIsFinished] = useState(false);
  const [waitingForPlayer, setWaitingForPlayer] = useState(isNetworkMode);

  const {
    squares,
    currentTurn,
    winner,
    winningLine,
    isDraw,
    isFinished: gameLogicFinished,
    makeMove,
    syncBoardFromServer,   // ← só existe no retorno, mas modo local ignora
  } = useGameLogic();
  // Inicialização do SignalR
  useEffect(() => {

    if(!isNetworkMode && gameId){
      setMySymbol('X');
    }    

    if (!isNetworkMode || !gameId) return;
    let isCurrent = true;
    
    connectToGame(
      gameId,
      tipoPessoa,
      (updatedGame) => {
        // Atualiza squares usando makeMove (seguro)
        console.log(updatedGame); 

        syncBoardFromServer(updatedGame.board);

        setStatusMessage(
          updatedGame.winner
            ? `Vencedor: ${updatedGame.winner}`
            : updatedGame.isDraw ? 'Empate!' : `Vez do ${updatedGame.currentPlayerSymbol}`
        );

        if (updatedGame.winner || updatedGame.isDraw) {
          setIsFinished(true);
        }
      },
      (msg) => setError(msg),
      (result) => {
        setIsFinished(true);
        setStatusMessage(result.isDraw ? 'Empate!' : `Vencedor: ${result.winner}`);
      },
      (message) => {        
        setWaitingForPlayer(false);
        setStatusMessage('Segundo jogador entrou! Jogo iniciado.'); 
        console.log('Player recebido: ' + message);
        setMySymbol(message as PlayerSymbol); // quem entra depois
      },
      (newIdPlayer) => {        
        setPlayerId(newIdPlayer as number);
      }
    );
    
    return () => {
      isCurrent = false;
      // Opcional: só desconectar se for realmente sair da página
      // disconnectSignalR();   ← muitas vezes NÃO desconectamos aqui
    };
  }, [isNetworkMode, gameId]);

  // Salvamento automático
  useEffect(() => {
    if (isFinished || !gameId) return;

    const checkAndSave = async () => {
      const { winner } = calculateWinner(squares);
      const draw = isBoardFull(squares) && !winner;

      if (winner || draw) {
        
        if(!isNetworkMode){
          setMySymbol(mySymbol == "O" ? "X" : "O");
        }
        
        setIsFinished(true);
        setIsSaving(true);
        setStatusMessage('Salvando resultado...');
        console.log('salvando...');
        try {
          const res = await fetch(`${config.apiBaseUrl}/api/games/${gameId}/finish`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              winnerSymbol: winner || null,
              isDraw: draw,
            }),
          });

          if (!res.ok) throw new Error(await res.text());

          setStatusMessage(
            winner
              ? winner === mySymbol ? 'Você venceu! Partida salva 🎉' : `Jogador ${winner} venceu! Partida salva`
              : 'Empate! Partida salva'
          );
          setError(null);
        } catch (err) {
          setError('Falha ao salvar resultado: ' + (err as Error).message);
          setStatusMessage('Resultado não salvo.');
        } finally {
          setIsSaving(false);
        }
      }
    };

    checkAndSave();
  }, [squares, gameId, mySymbol, isFinished]);

  const handleClick = (index: number) => {
    if (waitingForPlayer) {
      setError('Aguarde o segundo jogador entrar.');
      return;
    }

    if (squares[index] || winner || isDraw || isFinished) return;

    if (currentTurn !== mySymbol) {
      setError('Não é sua vez!');
      return;
    }

    makeMove(index, currentTurn);

    if(!isNetworkMode && gameId){
      if(!isFinished)
      {
        const newSymbol = mySymbol == "O" ? "X" : "O";
        setMySymbol(newSymbol);
      }
    }

    if (isNetworkMode && gameId && playerId) {
      sendMove(gameId, index, playerId);
    }
  };

  return (
    <main className="min-h-screen flex flex-col items-center justify-center bg-gradient-to-br from-gray-50 to-gray-200 p-4">
      <h1 className="text-5xl font-bold mb-8 text-gray-800">Jogo da Velha</h1>

      <h2 className="text-2xl font-bold text-black mb-4">
        {playerXNameFromUrl} (X) vs {playerONameFromUrl} (O)
      </h2>

      {gameId && <p className="mb-4 text-black">Sala: <strong>{gameId}</strong></p>}

      {error && <div className="bg-red-100 text-red-700 px-4 py-3 rounded mb-6">{error}</div>}

      {isSaving && <div className="text-blue-600 animate-pulse mb-4">Salvando...</div>}

      {waitingForPlayer && (
        <div className="text-center py-10 bg-yellow-100 p-8 rounded-xl shadow-lg max-w-md">
          <h3 className="text-2xl font-bold mb-4">Aguardando segundo jogador...</h3>
          <p className="text-lg mb-6">Compartilhe este código da sala:</p>
          <div className="text-4xl font-mono bg-white p-6 rounded-lg shadow-inner mb-6">
            {gameId || 'Gerando...'}
          </div>
          <div className="animate-pulse text-blue-600 text-xl">Esperando...</div>
        </div>
      )}

      {!waitingForPlayer && (
        <>
          <GameStatusDisplay
            gameStatus={{
              board: squares.map(s => s || ''),
              currentPlayerSymbol: currentTurn,
              winner,
              isDraw,
              status: isFinished ? 'finished' : 'ongoing',
            }}
            mySymbol={mySymbol}
            isMyTurn={currentTurn === mySymbol}
          />

          <Board
            squares={squares}
            onSquareClick={handleClick}
            winningLine={calculateWinner(squares).line}
            disabled={waitingForPlayer || isFinished || isSaving}
          />
        </>
      )}

      <button
        onClick={() => router.push('/')}
        className="mt-10 px-8 py-4 bg-gray-600 text-white rounded-xl hover:bg-gray-700 transition"
      >
        Voltar ao Menu
      </button>
    </main>
  );
}