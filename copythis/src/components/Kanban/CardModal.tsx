import React, { useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { 
  X, 
  Trash2, 
  Archive, 
  Calendar, 
  Tag, 
  Type, 
  CheckSquare,
  Plus,
  Trash
} from 'lucide-react';
import { KanbanCard, Subtask, Priority } from '../../types';
import { cn } from '../../lib/utils';
import ReactMarkdown from 'react-markdown';

interface CardModalProps {
  card: KanbanCard;
  onClose: () => void;
  onUpdate: (updates: Partial<KanbanCard>) => void;
  onArchive: () => void;
  onDelete: () => void;
}

export default function CardModal({ card, onClose, onUpdate, onArchive, onDelete }: CardModalProps) {
  const [isEditingDesc, setIsEditingDesc] = useState(false);
  const [newSubtask, setNewSubtask] = useState('');

  const toggleSubtask = (id: string) => {
    const newSubtasks = card.subtasks.map(st => 
      st.id === id ? { ...st, completed: !st.completed } : st
    );
    onUpdate({ subtasks: newSubtasks });
  };

  const addSubtask = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newSubtask.trim()) return;
    const st: Subtask = {
      id: `st-${Date.now()}`,
      title: newSubtask,
      completed: false
    };
    onUpdate({ subtasks: [...card.subtasks, st] });
    setNewSubtask('');
  };

  const removeSubtask = (id: string) => {
    onUpdate({ subtasks: card.subtasks.filter(st => st.id !== id) });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <motion.div 
        initial={{ scale: 0.95, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        className="bg-white dark:bg-zinc-900 w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]"
      >
        {/* Header */}
        <div className="p-6 flex items-start justify-between border-b border-zinc-100 dark:border-zinc-800">
          <div className="flex-1">
            <input 
              type="text"
              value={card.title}
              onChange={(e) => onUpdate({ title: e.target.value })}
              className="text-xl font-bold bg-transparent border-none outline-none w-full focus:ring-2 focus:ring-blue-500/20 rounded px-1"
            />
            <div className="flex items-center gap-2 mt-2">
              <select 
                value={card.priority}
                onChange={(e) => onUpdate({ priority: e.target.value as Priority })}
                className="text-xs font-bold uppercase bg-zinc-100 dark:bg-zinc-800 px-2 py-1 rounded outline-none"
              >
                <option value="low">Low</option>
                <option value="medium">Medium</option>
                <option value="high">High</option>
                <option value="urgent">Urgent</option>
              </select>
              <input 
                type="color"
                value={card.color}
                onChange={(e) => onUpdate({ color: e.target.value })}
                className="w-6 h-6 rounded cursor-pointer border-none p-0 bg-transparent"
              />
            </div>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-zinc-100 dark:hover:bg-zinc-800 rounded-full transition-colors">
            <X size={20} />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-6 space-y-8">
          {/* Description */}
          <section className="space-y-3">
            <div className="flex items-center gap-2 text-zinc-400 font-semibold text-sm">
              <Type size={16} />
              Description
            </div>
            {isEditingDesc ? (
              <div className="space-y-2">
                <textarea 
                  autoFocus
                  value={card.description}
                  onChange={(e) => onUpdate({ description: e.target.value })}
                  onBlur={() => setIsEditingDesc(false)}
                  className="w-full h-32 p-3 text-sm bg-zinc-50 dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/20"
                  placeholder="Add a detailed description... (Markdown supported)"
                />
                <p className="text-[10px] text-zinc-400">Click outside to save</p>
              </div>
            ) : (
              <div 
                onClick={() => setIsEditingDesc(true)}
                className="p-4 bg-zinc-50 dark:bg-zinc-800 rounded-xl cursor-pointer hover:bg-zinc-100 dark:hover:bg-zinc-700 transition-colors min-h-[80px]"
              >
                {card.description ? (
                  <div className="prose prose-sm dark:prose-invert max-w-none">
                    <ReactMarkdown>{card.description}</ReactMarkdown>
                  </div>
                ) : (
                  <span className="text-sm text-zinc-400">Add a description...</span>
                )}
              </div>
            )}
          </section>

          {/* Subtasks */}
          <section className="space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-zinc-400 font-semibold text-sm">
                <CheckSquare size={16} />
                Subtasks
              </div>
              <span className="text-xs text-zinc-400">
                {card.subtasks.filter(s => s.completed).length}/{card.subtasks.length}
              </span>
            </div>
            
            <div className="space-y-2">
              {card.subtasks.map(st => (
                <div key={st.id} className="flex items-center gap-3 group">
                  <input 
                    type="checkbox"
                    checked={st.completed}
                    onChange={() => toggleSubtask(st.id)}
                    className="w-4 h-4 rounded border-zinc-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className={cn("flex-1 text-sm", st.completed && "line-through text-zinc-400")}>
                    {st.title}
                  </span>
                  <button 
                    onClick={() => removeSubtask(st.id)}
                    className="opacity-0 group-hover:opacity-100 p-1 text-zinc-400 hover:text-red-500 transition-all"
                  >
                    <Trash size={14} />
                  </button>
                </div>
              ))}
              <form onSubmit={addSubtask} className="flex items-center gap-2 mt-2">
                <Plus size={14} className="text-zinc-400" />
                <input 
                  type="text"
                  value={newSubtask}
                  onChange={(e) => setNewSubtask(e.target.value)}
                  placeholder="Add a subtask..."
                  className="flex-1 text-sm bg-transparent border-none outline-none focus:ring-0"
                />
              </form>
            </div>
          </section>

          {/* Due Date */}
          <section className="space-y-3">
            <div className="flex items-center gap-2 text-zinc-400 font-semibold text-sm">
              <Calendar size={16} />
              Due Date
            </div>
            <input 
              type="date"
              value={card.dueDate || ''}
              onChange={(e) => onUpdate({ dueDate: e.target.value })}
              className="bg-zinc-50 dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-lg px-3 py-2 text-sm outline-none"
            />
          </section>
        </div>

        {/* Footer */}
        <div className="p-6 bg-zinc-50 dark:bg-zinc-800/50 flex items-center justify-between border-t border-zinc-100 dark:border-zinc-800">
          <div className="flex items-center gap-2">
            <button 
              onClick={() => { onArchive(); onClose(); }}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-zinc-600 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700 rounded-xl transition-colors"
            >
              <Archive size={16} />
              Archive
            </button>
          </div>
          <button 
            onClick={() => { if(confirm('Are you sure?')) { onDelete(); onClose(); } }}
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-xl transition-colors"
          >
            <Trash2 size={16} />
            Delete
          </button>
        </div>
      </motion.div>
    </div>
  );
}
