import React, { useState } from 'react';
import { 
  DndContext, 
  DragOverlay, 
  closestCorners, 
  KeyboardSensor, 
  PointerSensor, 
  useSensor, 
  useSensors,
  DragStartEvent,
  DragOverEvent,
  DragEndEvent,
  defaultDropAnimationSideEffects
} from '@dnd-kit/core';
import { 
  arrayMove, 
  SortableContext, 
  sortableKeyboardCoordinates, 
  verticalListSortingStrategy,
  horizontalListSortingStrategy
} from '@dnd-kit/sortable';
import { Plus, Archive } from 'lucide-react';
import { KanbanData, KanbanCard, KanbanColumn, Priority } from '../../types';
import Column from './KanbanColumn';
import Card from './KanbanCard';
import CardModal from './CardModal';
import { cn } from '../../lib/utils';

const initialData: KanbanData = {
  cards: {
    'card-1': {
      id: 'card-1',
      title: 'Design Sidebar',
      description: 'Create a responsive sidebar with collapsible sections.',
      priority: 'high',
      color: '#ef4444',
      subtasks: [
        { id: 'st-1', title: 'Layout', completed: true },
        { id: 'st-2', title: 'Animations', completed: false }
      ],
      archived: false
    },
    'card-2': {
      id: 'card-2',
      title: 'Weather API',
      description: 'Integrate Open-Meteo API for real-time weather data.',
      priority: 'medium',
      color: '#3b82f6',
      subtasks: [],
      archived: false
    },
    'card-3': {
      id: 'card-3',
      title: 'User Research',
      description: 'Conduct interviews with potential users to gather feedback.',
      priority: 'medium',
      color: '#8b5cf6',
      subtasks: [],
      archived: false
    },
    'card-4': {
      id: 'card-4',
      title: 'Code Review',
      description: 'Review the latest PR for the authentication module.',
      priority: 'high',
      color: '#f59e0b',
      subtasks: [],
      archived: false
    }
  },
  columns: {
    'col-1': { id: 'col-1', title: 'Research', cardIds: ['card-3'] },
    'col-2': { id: 'col-2', title: 'To Do', cardIds: ['card-1', 'card-2'] },
    'col-3': { id: 'col-3', title: 'In Progress', cardIds: [] },
    'col-4': { id: 'col-4', title: 'Review', cardIds: ['card-4'] },
    'col-5': { id: 'col-5', title: 'Done', cardIds: [] }
  },
  columnOrder: ['col-1', 'col-2', 'col-3', 'col-4', 'col-5']
};

export default function KanbanBoard() {
  const [data, setData] = useState<KanbanData>(initialData);
  const [activeCardId, setActiveCardId] = useState<string | null>(null);
  const [activeColumnId, setActiveColumnId] = useState<string | null>(null);
  const [selectedCardId, setSelectedCardId] = useState<string | null>(null);
  const [showArchived, setShowArchived] = useState(false);
  const [isAddingColumn, setIsAddingColumn] = useState(false);
  const [newColumnTitle, setNewColumnTitle] = useState('');

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 5,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const findColumn = (cardId: string) => {
    if (cardId in data.columns) return data.columns[cardId];
    return Object.values(data.columns).find(col => col.cardIds.includes(cardId));
  };

  const handleDragStart = (event: DragStartEvent) => {
    if (event.active.data.current?.type === 'Column') {
      setActiveColumnId(event.active.id as string);
      return;
    }
    setActiveCardId(event.active.id as string);
  };

  const handleDragOver = (event: DragOverEvent) => {
    const { active, over } = event;
    if (!over) return;

    const activeId = active.id as string;
    const overId = over.id as string;

    if (activeId === overId) return;

    const isActiveAColumn = active.data.current?.type === 'Column';
    if (isActiveAColumn) return;

    const activeColumn = findColumn(activeId);
    const overColumn = findColumn(overId) || data.columns[overId];

    if (!activeColumn || !overColumn || activeColumn.id === overColumn.id) return;

    setData(prev => {
      const activeCol = prev.columns[activeColumn.id];
      const overCol = prev.columns[overColumn.id];
      
      if (!activeCol || !overCol) return prev;

      const activeCardIds = [...activeCol.cardIds];
      const overCardIds = [...overCol.cardIds];

      const activeIndex = activeCardIds.indexOf(activeId);
      const overIndex = overCardIds.indexOf(overId);

      if (activeIndex === -1) return prev;

      activeCardIds.splice(activeIndex, 1);
      
      if (overIndex >= 0) {
        overCardIds.splice(overIndex, 0, activeId);
      } else {
        overCardIds.push(activeId);
      }

      return {
        ...prev,
        columns: {
          ...prev.columns,
          [activeColumn.id]: { ...activeCol, cardIds: activeCardIds },
          [overColumn.id]: { ...overCol, cardIds: overCardIds }
        }
      };
    });
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    
    setActiveCardId(null);
    setActiveColumnId(null);

    if (!over) return;

    const activeId = active.id as string;
    const overId = over.id as string;

    if (active.data.current?.type === 'Column') {
      if (activeId !== overId) {
        setData(prev => {
          const oldIndex = prev.columnOrder.indexOf(activeId);
          const newIndex = prev.columnOrder.indexOf(overId);
          return {
            ...prev,
            columnOrder: arrayMove(prev.columnOrder, oldIndex, newIndex)
          };
        });
      }
      return;
    }

    const activeColumn = findColumn(activeId);
    const overColumn = findColumn(overId) || data.columns[overId];

    if (!activeColumn || !overColumn) return;

    if (activeId !== overId) {
      setData(prev => {
        const col = prev.columns[activeColumn.id];
        const oldIndex = col.cardIds.indexOf(activeId);
        const newIndex = col.cardIds.indexOf(overId);

        if (oldIndex === -1 || newIndex === -1) return prev;

        return {
          ...prev,
          columns: {
            ...prev.columns,
            [activeColumn.id]: {
              ...col,
              cardIds: arrayMove(col.cardIds, oldIndex, newIndex)
            }
          }
        };
      });
    }
  };

  const addCard = (columnId: string, title: string) => {
    if (!title.trim()) return;
    const newId = `card-${Date.now()}`;
    const newCard: KanbanCard = {
      id: newId,
      title,
      description: '',
      priority: 'medium',
      color: '#3b82f6',
      subtasks: [],
      archived: false
    };

    setData(prev => ({
      ...prev,
      cards: { ...prev.cards, [newId]: newCard },
      columns: {
        ...prev.columns,
        [columnId]: {
          ...prev.columns[columnId],
          cardIds: [newId, ...prev.columns[columnId].cardIds]
        }
      }
    }));
  };

  const addColumn = (e?: React.FormEvent) => {
    e?.preventDefault();
    if (!newColumnTitle.trim()) return;
    
    const newId = `col-${Date.now()}`;
    const newColumn: KanbanColumn = {
      id: newId,
      title: newColumnTitle,
      cardIds: []
    };

    setData(prev => ({
      ...prev,
      columns: { ...prev.columns, [newId]: newColumn },
      columnOrder: [...prev.columnOrder, newId]
    }));

    setNewColumnTitle('');
    setIsAddingColumn(false);
  };

  const updateCard = (cardId: string, updates: Partial<KanbanCard>) => {
    setData(prev => ({
      ...prev,
      cards: {
        ...prev.cards,
        [cardId]: { ...prev.cards[cardId], ...updates }
      }
    }));
  };

  const archiveCard = (cardId: string) => {
    updateCard(cardId, { archived: true });
  };

  const deleteCard = (cardId: string) => {
    setData(prev => {
      const newCards = { ...prev.cards };
      delete newCards[cardId];
      
      const newColumns = { ...prev.columns };
      Object.keys(newColumns).forEach(colId => {
        newColumns[colId].cardIds = newColumns[colId].cardIds.filter(id => id !== cardId);
      });

      return { ...prev, cards: newCards, columns: newColumns };
    });
  };

  const activeCard = activeCardId ? data.cards[activeCardId] : null;
  const activeColumn = activeColumnId ? data.columns[activeColumnId] : null;

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between mb-2">
        <h2 className="text-xs font-bold uppercase tracking-widest text-zinc-400">My Board</h2>
        <button 
          onClick={() => setShowArchived(!showArchived)}
          className={cn(
            "p-1.5 rounded-md transition-colors",
            showArchived ? "bg-zinc-200 dark:bg-zinc-800 text-blue-600" : "text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800"
          )}
          title="Show Archived"
        >
          <Archive size={14} />
        </button>
      </div>

      <div className="flex flex-row gap-6 overflow-x-auto pb-4 custom-scrollbar min-h-[calc(100vh-200px)]">
        <DndContext
          sensors={sensors}
          collisionDetection={closestCorners}
          onDragStart={handleDragStart}
          onDragOver={handleDragOver}
          onDragEnd={handleDragEnd}
        >
          <div className="flex gap-6">
            <SortableContext items={data.columnOrder} strategy={horizontalListSortingStrategy}>
              {data.columnOrder.map(colId => {
                const column = data.columns[colId];
                const cards = column.cardIds
                  .map(id => data.cards[id])
                  .filter((c): c is KanbanCard => !!c)
                  .filter(card => showArchived ? card.archived : !card.archived);

                return (
                  <div key={colId} className="w-64 lg:w-72 flex-shrink-0">
                    <Column 
                      column={column} 
                      cards={cards} 
                      onAddCard={(title) => addCard(colId, title)}
                      onCardClick={(id) => setSelectedCardId(id)}
                      onArchiveCard={archiveCard}
                      onDeleteCard={(id) => { if(confirm('Are you sure?')) deleteCard(id); }}
                    />
                  </div>
                );
              })}
            </SortableContext>

            {/* Add Column Button */}
            <div className="w-64 lg:w-72 flex-shrink-0">
              {isAddingColumn ? (
                <form onSubmit={addColumn} className="bg-zinc-100 dark:bg-zinc-900/50 p-4 rounded-2xl border border-blue-500 shadow-sm">
                  <input
                    autoFocus
                    type="text"
                    value={newColumnTitle}
                    onChange={(e) => setNewColumnTitle(e.target.value)}
                    onBlur={() => !newColumnTitle && setIsAddingColumn(false)}
                    placeholder="Column Title (e.g. Review)"
                    className="w-full p-2 text-sm bg-white dark:bg-zinc-800 rounded-lg outline-none mb-2"
                  />
                  <div className="flex gap-2">
                    <button type="submit" className="px-3 py-1.5 bg-blue-500 text-white text-xs font-bold rounded-lg hover:bg-blue-600 transition-colors">
                      Add Column
                    </button>
                    <button type="button" onClick={() => setIsAddingColumn(false)} className="px-3 py-1.5 text-xs font-bold text-zinc-500 hover:bg-zinc-200 dark:hover:bg-zinc-800 rounded-lg transition-colors">
                      Cancel
                    </button>
                  </div>
                </form>
              ) : (
                <button 
                  onClick={() => setIsAddingColumn(true)}
                  className="w-full h-[100px] flex flex-col items-center justify-center gap-2 border-2 border-dashed border-zinc-200 dark:border-zinc-800 rounded-2xl text-zinc-400 hover:text-blue-500 hover:border-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/10 transition-all group"
                >
                  <Plus size={24} className="group-hover:scale-110 transition-transform" />
                  <span className="text-sm font-bold">Add Column</span>
                </button>
              )}
            </div>
          </div>

          <DragOverlay dropAnimation={{
            sideEffects: defaultDropAnimationSideEffects({
              styles: {
                active: {
                  opacity: '0.5',
                },
              },
            }),
          }}>
            {activeCard ? (
              <div className="rotate-3 scale-105 pointer-events-none">
                <Card card={activeCard} isOverlay />
              </div>
            ) : activeColumn ? (
              <div className="rotate-3 scale-105 pointer-events-none">
                <Column 
                  column={activeColumn} 
                  cards={activeColumn.cardIds.map(id => data.cards[id]).filter((c): c is KanbanCard => !!c).filter(card => showArchived ? card.archived : !card.archived)}
                  onAddCard={() => {}}
                  onCardClick={() => {}}
                  isOverlay
                />
              </div>
            ) : null}
          </DragOverlay>
        </DndContext>
      </div>

      {selectedCardId && (
        <CardModal 
          card={data.cards[selectedCardId]} 
          onClose={() => setSelectedCardId(null)}
          onUpdate={(updates) => updateCard(selectedCardId, updates)}
          onArchive={() => archiveCard(selectedCardId)}
          onDelete={() => deleteCard(selectedCardId)}
        />
      )}
    </div>
  );
}
