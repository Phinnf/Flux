/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import Sidebar from './components/Sidebar';
import { Layout, Zap, Globe } from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { cn } from './lib/utils';
import KanbanBoard from './components/Kanban/KanbanBoard';
import WeatherWidget from './components/Weather/WeatherWidget';
import WikipediaSearch from './components/Wikipedia/WikipediaSearch';

export type ViewType = 'dashboard' | 'kanban' | 'weather' | 'wikipedia';

export default function App() {
  const [activeView, setActiveView] = useState<ViewType>('kanban');

  const renderContent = () => {
    switch (activeView) {
      case 'kanban':
        return (
          <motion.div 
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="space-y-6"
          >
            <div className="flex items-center gap-3 mb-8">
              <div className="p-3 bg-blue-500/10 rounded-2xl text-blue-500">
                <Layout size={32} />
              </div>
              <div>
                <h1 className="text-3xl font-black tracking-tight">Kanban Board</h1>
                <p className="text-zinc-500 text-sm">Manage your tasks and workflows.</p>
              </div>
            </div>
            <KanbanBoard />
          </motion.div>
        );
      case 'weather':
        return (
          <motion.div 
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="max-w-2xl mx-auto"
          >
            <div className="flex items-center gap-3 mb-8">
              <div className="p-3 bg-yellow-500/10 rounded-2xl text-yellow-500">
                <Zap size={32} />
              </div>
              <div>
                <h1 className="text-3xl font-black tracking-tight">Weather Forecast</h1>
                <p className="text-zinc-500 text-sm">Real-time updates and forecasts.</p>
              </div>
            </div>
            <WeatherWidget />
          </motion.div>
        );
      case 'wikipedia':
        return (
          <motion.div 
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="max-w-3xl mx-auto"
          >
            <div className="flex items-center gap-3 mb-8">
              <div className="p-3 bg-purple-500/10 rounded-2xl text-purple-500">
                <Globe size={32} />
              </div>
              <div>
                <h1 className="text-3xl font-black tracking-tight">Wikipedia Search</h1>
                <p className="text-zinc-500 text-sm">Explore knowledge instantly.</p>
              </div>
            </div>
            <WikipediaSearch />
          </motion.div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="flex h-screen overflow-hidden bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-50">
      <Sidebar activeView={activeView} onViewChange={setActiveView} />
      
      <main className="flex-1 overflow-y-auto p-8 lg:p-12 custom-scrollbar">
        <div className={cn(
          "mx-auto transition-all duration-500",
          activeView === 'kanban' ? "max-w-[1600px]" : "max-w-5xl"
        )}>
          <AnimatePresence mode="wait">
            {renderContent()}
          </AnimatePresence>
        </div>
      </main>
    </div>
  );
}
