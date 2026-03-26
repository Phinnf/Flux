import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { CheckCircle2, Clock, Archive, Trash2 } from 'lucide-react';
import { KanbanCard } from '../../types';
import { cn } from '../../lib/utils';
import { format } from 'date-fns';

interface CardProps {
  card: KanbanCard;
  onClick?: () => void;
  onArchive?: (e: React.MouseEvent) => void;
  onDelete?: (e: React.MouseEvent) => void;
  isOverlay?: boolean;
}

export default function Card({ card, onClick, onArchive, onDelete, isOverlay }: CardProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging
  } = useSortable({ 
    id: card.id,
    disabled: isOverlay
  });

  const style = {
    transform: CSS.Translate.toString(transform),
    transition,
  };

  const completedSubtasks = card.subtasks.filter(st => st.completed).length;
  const totalSubtasks = card.subtasks.length;
  const progress = totalSubtasks > 0 ? (completedSubtasks / totalSubtasks) * 100 : 0;

  const priorityColors = {
    low: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
    medium: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
    high: 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
    urgent: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        "group relative bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl p-3 shadow-sm hover:shadow-md transition-all cursor-grab active:cursor-grabbing",
        isDragging && "opacity-30",
        isOverlay && "shadow-xl border-blue-500 ring-2 ring-blue-500/20"
      )}
      onClick={onClick}
      {...attributes}
      {...listeners}
    >
      <div className="flex flex-col gap-2">
        <div className="flex items-start justify-between gap-2">
          <h4 className="text-sm font-semibold leading-tight text-zinc-800 dark:text-zinc-200">
            {card.title}
          </h4>
          <div className="flex items-center gap-1">
            <div className={cn("px-1.5 py-0.5 rounded text-[10px] font-bold uppercase", priorityColors[card.priority])}>
              {card.priority}
            </div>
            {!isOverlay && (
              <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <button 
                  onClick={(e) => { e.stopPropagation(); onArchive?.(e); }}
                  className="p-1 text-zinc-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded transition-colors"
                  title="Archive"
                >
                  <Archive size={12} />
                </button>
                <button 
                  onClick={(e) => { e.stopPropagation(); onDelete?.(e); }}
                  className="p-1 text-zinc-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition-colors"
                  title="Delete"
                >
                  <Trash2 size={12} />
                </button>
              </div>
            )}
          </div>
        </div>

        {card.description && (
          <p className="text-xs text-zinc-500 dark:text-zinc-400 line-clamp-2">
            {card.description}
          </p>
        )}

        {totalSubtasks > 0 && (
          <div className="flex flex-col gap-1.5 mt-1">
            <div className="flex items-center justify-between text-[10px] text-zinc-400 font-medium">
              <div className="flex items-center gap-1">
                <CheckCircle2 size={10} />
                {completedSubtasks}/{totalSubtasks} Subtasks
              </div>
              <span>{Math.round(progress)}%</span>
            </div>
            <div className="h-1 w-full bg-zinc-100 dark:bg-zinc-700 rounded-full overflow-hidden">
              <div 
                className="h-full bg-blue-500 transition-all duration-500" 
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>
        )}

        <div className="flex items-center gap-3 mt-1">
          {card.dueDate && (
            <div className="flex items-center gap-1 text-[10px] text-zinc-400">
              <Clock size={10} />
              {format(new Date(card.dueDate), 'MMM d')}
            </div>
          )}
          <div 
            className="w-2 h-2 rounded-full" 
            style={{ backgroundColor: card.color }}
          />
        </div>
      </div>
    </div>
  );
}
