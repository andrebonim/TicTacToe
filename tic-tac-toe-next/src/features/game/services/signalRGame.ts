import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { config } from '@/config/env';
import { GameStatus } from '../types';

type MoveCallback = (updatedGame: GameStatus) => void;
type ErrorCallback = (message: string) => void;
type GameEndedCallback = (result: any) => void;
type PlayerJoinedCallback = (player: string) => void;
type PlayerSetCallback = (player: number) => void;
type CloseCallback = () => void;

let connection: HubConnection | null = null;

export const getOrCreateConnection  = () => {
  
  if (connection?.state === 'Connected') return connection;

  // if (connection?.state === 'Connecting' || connection?.state === 'Reconnecting') {
  //   return connection; // evita race condition
  // }

  connection = new HubConnectionBuilder()
    .withUrl(`${config.apiBaseUrl}${config.signalrHubPath}`, {withCredentials: false})
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();


  return connection;
};

export const connectToGame = async (  
  gameId: number,
  quemEntrou: string,
  onMove: MoveCallback,
  onError: ErrorCallback,
  onGameEnded: GameEndedCallback,
  onPlayerJoined: PlayerJoinedCallback,
  onPlayerSetted: PlayerSetCallback
) => {
  const conn = getOrCreateConnection();

   // Registra listeners **sempre** (mas idealmente remover no cleanup)
  conn.on('ReceiveMove', onMove);
  conn.on('MoveError', onError);
  conn.on('GameEnded', onGameEnded);
  //conn.on('PlayerJoined', onPlayerJoined);
  conn.on('PlayerJoined', onPlayerJoined);  
  conn.on('SetPlayerId', onPlayerSetted);  
  // conn.on('meuteste', (msg) => console.log(quemEntrou + ': ' + msg));
  // conn.onreconnected(async (connectionId) => {
  //   console.log('Conexão restabelecida. Reentrando no jogo...', connectionId);
  //   // IMPORTANTE: Você precisa ter o gameId e o papel (criador/convidado) salvos em algum lugar (state ou ref)
  //   if (gameId && quemEntrou) { 
  //       await conn.invoke('JoinGame', gameId, quemEntrou);
  //   }
  // });

  // Se já está conectado → só adiciona listeners e faz join (se necessário)
  if (conn.state === 'Connected') {
    console.log('SignalR já conectado, só registrando listeners e join');
    // Join idempotente (backend deve ignorar duplicados)
    try {
      await conn.invoke('JoinGame', gameId, quemEntrou);
    } catch (err) {
      console.warn('JoinGame já feito ou falhou (ignorado)', err);
    }
  } else if (conn.state === 'Connecting' || conn.state === 'Reconnecting') {
    console.log('SignalR em transição, aguardando...');
    // Pode aguardar aqui se quiser ser mais preciso, mas geralmente não precisa
    // await conn.start(); // NÃO chame novamente!
  } else {
    // Apenas Disconnected → tenta conectar
    
    try {
      await conn.start();
      try {
        await conn.invoke('JoinGame', gameId, quemEntrou);
      } catch (err) {
        console.warn('JoinGame já feito ou falhou (ignorado)', err);
      }
    } catch (err) {
      console.error('[CONVIDADO DEBUG] FALHA TOTAL no start:', err);
      return null;
    }
  
  }

 
  return conn;
};

export const sendMove = async (gameId: number, position: number, playerId: number) => {
  if (connection?.state !== 'Connected') {
    console.warn('SignalR não conectado');
    return;
  }

  try {
    console.log('gameId: ' + gameId);
    console.log('playerId: ' + playerId);
    console.log('position: ' + position);
    await connection.invoke('SendMove', gameId, playerId, position);
  } catch (err) {
    console.error('Erro ao enviar jogada:', err);
  }
};

export const disconnectSignalR = () => {
  if (connection?.state === 'Connected') {
    connection.stop();
    connection = null;
  }
};