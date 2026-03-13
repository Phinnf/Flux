'use client';

import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';

import { API_URL, getAuthToken } from '@/lib/api';
import { useCurrentUser } from '@/features/auth/api/use-current-user';

type SocketContextType = {
  socket: signalR.HubConnection | null;
  isConnected: boolean;
};

const SocketContext = createContext<SocketContextType>({
  socket: null,
  isConnected: false,
});

export const useSocket = () => useContext(SocketContext);

export const SocketProvider = ({ children }: { children: ReactNode }) => {
  const [socket, setSocket] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const { data: userData } = useCurrentUser();
  const userId = userData?.value?.id;

  useEffect(() => {
    if (!userId) return;

    const token = getAuthToken();
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/chatHub?userId=${userId}`, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    connection
      .start()
      .then(() => {
        setIsConnected(true);
        setSocket(connection);
      })
      .catch((err) => console.error('SignalR Connection Error: ', err));

    return () => {
      connection.stop();
    };
  }, [userId]);

  return (
    <SocketContext.Provider value={{ socket, isConnected }}>
      {children}
    </SocketContext.Provider>
  );
};
