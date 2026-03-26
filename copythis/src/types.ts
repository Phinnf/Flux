export type Priority = 'low' | 'medium' | 'high' | 'urgent';

export interface Subtask {
  id: string;
  title: string;
  completed: boolean;
}

export interface KanbanCard {
  id: string;
  title: string;
  description: string;
  priority: Priority;
  color: string;
  dueDate?: string;
  subtasks: Subtask[];
  archived: boolean;
}

export interface KanbanColumn {
  id: string;
  title: string;
  cardIds: string[];
}

export interface KanbanData {
  cards: Record<string, KanbanCard>;
  columns: Record<string, KanbanColumn>;
  columnOrder: string[];
}

export interface WeatherData {
  temp: number;
  condition: string;
  location: string;
  feelsLike: number;
  humidity: number;
  precipProb: number;
  hourly: { time: string; temp: number; condition: string }[];
  daily: { date: string; maxTemp: number; minTemp: number; condition: string }[];
}

export interface WikiResult {
  title: string;
  extract: string;
  thumbnail?: string;
  pageid: number;
}
