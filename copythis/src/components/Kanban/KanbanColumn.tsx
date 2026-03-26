import React, { useState } from 'react';
import { useDroppable } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Plus, MoreHorizontal, GripVertical } from 'lucide-react';
import { KanbanColumn, KanbanCard } from '../../types';
import Card from './KanbanCard';
import { cn } from '../../lib/utils';

interface ColumnProps {
  column: KanbanColumn;
  cards: KanbanCard[];
  onAddCard: (title: string) => void;
  onCardClick: (id: string) => void;
  onArchiveCard?: (cardId: string) => void;
  onDeleteCard?: (cardId: string) => void;
  isOverlay?: boolean;
}

export default function Column({ 
  column, 
  cards, 
  onAddCard, 
  onCardClick, 
  onArchiveCard,
  onDeleteCard,
  isOverlay 
}: ColumnProps) {
  const [isAdding, setIsAdding] = useState(false);
  const [newTitle, setNewTitle] = useState('');

  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging
  } = useSortable({ 
    id: column.id,
    data: {
      type: 'Column',
      column
    },
    disabled: isOverlay
  });

  const style = {
    transform: CSS.Translate.toString(transform),
    transition,
  };

  const { setNodeRef: setDroppableRef } = useDroppable({
    id: column.id,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (newTitle.trim()) {
      onAddCard(newTitle);
      setNewTitle('');
      setIsAdding(false);
    }
  };

  return (
    <div 
      ref={setNodeRef}
      style={style}
      className={cn(
        "flex flex-col gap-3 min-h-[100px] bg-zinc-50/50 dark:bg-zinc-900/30 p-4 rounded-2xl border border-transparent transition-all",
        isDragging && "opacity-30",
        isOverlay && "border-blue-500 ring-2 ring-blue-500/20 shadow-xl bg-white dark:bg-zinc-900"
      )}
    >
      <div className="flex items-center justify-between px-1">
        <div className="flex items-center gap-2 group/header">
          <div {...attributes} {...listeners} className="cursor-grab active:cursor-grabbing text-zinc-300 hover:text-zinc-500 transition-colors">
            <GripVertical size={14} />
          </div>
          <h3 className="text-sm font-bold text-zinc-700 dark:text-zinc-300">{column.title}</h3>
          <span className="text-[10px] font-bold bg-zinc-200 dark:bg-zinc-800 text-zinc-500 px-1.5 py-0.5 rounded-full">
            {cards.length}
          </span>
        </div>
        <button className="text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 transition-colors">
          <MoreHorizontal size={14} />
        </button>
      </div>

      <div ref={setDroppableRef} className="flex flex-col gap-2 min-h-[50px]">
        <SortableContext items={cards.map(c => c.id)} strategy={verticalListSortingStrategy}>
          {cards.map(card => (
            <Card 
              key={card.id} 
              card={card} 
              onClick={() => onCardClick(card.id)} 
              onArchive={() => onArchiveCard?.(card.id)}
              onDelete={() => onDeleteCard?.(card.id)}
            />
          ))}
        </SortableContext>
      </div>

      {isAdding ? (
        <form onSubmit={handleSubmit} className="mt-1">
          <input
            autoFocus
            type="text"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            onBlur={() => !newTitle && setIsAdding(false)}
            placeholder="What needs to be done?"
            className="w-full p-2 text-sm bg-white dark:bg-zinc-800 border border-blue-500 rounded-lg outline-none shadow-sm"
          />
        </form>
      ) : (
        <button 
          onClick={() => setIsAdding(true)}
          className="flex items-center gap-2 p-2 text-xs font-medium text-zinc-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-all"
        >
          <Plus size={14} />
          Quick Add
        </button>
      )}
    </div>
  );
}
