import React, { useState, useEffect } from 'react';
import { 
  LayoutDashboard, 
  CloudSun, 
  Search, 
  ChevronLeft, 
  ChevronRight,
  GripVertical,
  Moon,
  Sun,
  Home
} from 'lucide-react';
import { motion } from 'motion/react';
import { cn } from '../lib/utils';
import { ViewType } from '../App';

interface SidebarProps {
  activeView: ViewType;
  onViewChange: (view: ViewType) => void;
}

export default function Sidebar({ activeView, onViewChange }: SidebarProps) {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [width, setWidth] = useState(260);
  const [isResizing, setIsResizing] = useState(false);
  const [isDark, setIsDark] = useState(false);

  useEffect(() => {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [isDark]);

  const startResizing = (e: React.MouseEvent) => {
    setIsResizing(true);
    e.preventDefault();
  };

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isResizing) return;
      const newWidth = e.clientX;
      if (newWidth > 200 && newWidth < 450) {
        setWidth(newWidth);
      }
    };

    const handleMouseUp = () => {
      setIsResizing(false);
    };

    if (isResizing) {
      window.addEventListener('mousemove', handleMouseMove);
      window.addEventListener('mouseup', handleMouseUp);
    }

    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', handleMouseUp);
    };
  }, [isResizing]);

  const navItems = [
    { id: 'dashboard', label: 'Dashboard', icon: <Home size={20} /> },
    { id: 'kanban', label: 'Kanban Board', icon: <LayoutDashboard size={20} /> },
    { id: 'weather', label: 'Weather', icon: <CloudSun size={20} /> },
    { id: 'wikipedia', label: 'Wikipedia', icon: <Search size={20} /> },
  ] as const;

  return (
    <div 
      className={cn(
        "relative h-screen flex flex-col border-r border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 transition-all duration-300 ease-in-out",
        isCollapsed ? "w-20" : ""
      )}
      style={{ width: isCollapsed ? undefined : width }}
    >
      {/* Header */}
      <div className="p-6 flex items-center justify-end">
        <button 
          onClick={() => setIsCollapsed(!isCollapsed)}
          className={cn(
            "p-2 rounded-xl hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors",
            isCollapsed ? "mx-auto" : ""
          )}
        >
          {isCollapsed ? <ChevronRight size={20} /> : <ChevronLeft size={20} />}
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-4 space-y-2 mt-4">
        {navItems.map((item) => (
          <button
            key={item.id}
            onClick={() => onViewChange(item.id as ViewType)}
            className={cn(
              "w-full flex items-center gap-3 p-3 rounded-xl transition-all group",
              activeView === item.id 
                ? "bg-blue-500 text-white shadow-lg shadow-blue-500/20" 
                : "text-zinc-500 hover:bg-zinc-100 dark:hover:bg-zinc-800",
              isCollapsed ? "justify-center px-0" : ""
            )}
            title={isCollapsed ? item.label : undefined}
          >
            <span className={cn(
              "transition-transform group-hover:scale-110",
              activeView === item.id ? "text-white" : "text-zinc-400 group-hover:text-blue-500"
            )}>
              {item.icon}
            </span>
            {!isCollapsed && (
              <span className="font-bold text-sm tracking-tight">{item.label}</span>
            )}
          </button>
        ))}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-zinc-100 dark:border-zinc-800">
        <button 
          onClick={() => setIsDark(!isDark)}
          className={cn(
            "w-full flex items-center gap-3 p-3 rounded-xl hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors text-zinc-500",
            isCollapsed ? "justify-center px-0" : ""
          )}
        >
          {isDark ? <Sun size={20} /> : <Moon size={20} />}
          {!isCollapsed && (
            <span className="font-bold text-sm tracking-tight">
              {isDark ? 'Light Mode' : 'Dark Mode'}
            </span>
          )}
        </button>
      </div>

      {/* Resize Handle */}
      {!isCollapsed && (
        <div 
          onMouseDown={startResizing}
          className="absolute right-0 top-0 bottom-0 w-1 cursor-col-resize hover:bg-blue-500/30 transition-colors group"
        >
          <div className="absolute top-1/2 right-0 -translate-y-1/2 opacity-0 group-hover:opacity-100">
            <GripVertical size={12} className="text-blue-500" />
          </div>
        </div>
      )}
    </div>
  );
}
