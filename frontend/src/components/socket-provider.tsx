'use client';

import { createContext, useContext, useEffect, useState, ReactNode, useRef } from 'react';
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
  
  // Dùng ref để đảm bảo không tạo nhiều connection dư thừa
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    // Nếu không có userId hoặc đang có connection rồi thì không làm gì
    if (!userId || connectionRef.current) return;

    const token = getAuthToken();
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/chatHub?userId=${userId}`, {
        accessTokenFactory: () => token || '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning) // Chỉ hiện cảnh báo, bớt rác console
      .build();

    const startConnection = async () => {
        try {
            await connection.start();
            connectionRef.current = connection;
            setSocket(connection);
            setIsConnected(true);
            console.log('SignalR Connected.');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            // Thử lại sau 5s nếu lỗi
            setTimeout(startConnection, 5000);
        }
    };

    startConnection();

    // Cleanup khi component unmount hoặc userId thay đổi
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop().then(() => {
            console.log('SignalR Disconnected.');
            connectionRef.current = null;
            setSocket(null);
            setIsConnected(false);
        });
      }
    };
  }, [userId]);

  return (
    <SocketContext.Provider value={{ socket, isConnected }}>
      {children}
    </SocketContext.Provider>
  );
};
