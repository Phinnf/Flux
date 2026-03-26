import React, { useState, useEffect, useCallback } from 'react';
import { 
  Search, 
  X, 
  ExternalLink, 
  History, 
  Star, 
  BookOpen,
  ArrowRight,
  Loader2
} from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { WikiResult } from '../../types';
import { cn } from '../../lib/utils';

export default function WikipediaSearch() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<WikiResult[]>([]);
  const [selectedResult, setSelectedResult] = useState<WikiResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [history, setHistory] = useState<WikiResult[]>([]);
  const [featured, setFeatured] = useState<WikiResult | null>(null);

  const fetchFeatured = async () => {
    try {
      const res = await fetch('https://en.wikipedia.org/api/rest_v1/page/random/summary');
      const data = await res.json();
      setFeatured({
        title: data.title,
        extract: data.extract,
        thumbnail: data.thumbnail?.source,
        pageid: data.pageid
      });
    } catch (err) {
      console.error('Failed to fetch featured article');
    }
  };

  useEffect(() => {
    fetchFeatured();
  }, []);

  const searchWiki = useCallback(async (q: string) => {
    if (!q.trim()) {
      setResults([]);
      return;
    }
    setLoading(true);
    try {
      const res = await fetch(
        `https://en.wikipedia.org/w/api.php?action=query&format=json&origin=*&prop=extracts|pageimages&generator=search&exsentences=3&exintro=1&explaintext=1&gsrsearch=${encodeURIComponent(q)}&gsrlimit=5&piprop=thumbnail&pithumbsize=400`
      );
      const data = await res.json();
      if (data.query && data.query.pages) {
        const pages = Object.values(data.query.pages).map((p: any) => ({
          title: p.title,
          extract: p.extract,
          thumbnail: p.thumbnail?.source,
          pageid: p.pageid
        }));
        setResults(pages);
      } else {
        setResults([]);
      }
    } catch (err) {
      console.error('Search failed');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (query) searchWiki(query);
    }, 500);
    return () => clearTimeout(timer);
  }, [query, searchWiki]);

  const selectArticle = (article: WikiResult) => {
    setSelectedResult(article);
    setHistory(prev => {
      const filtered = prev.filter(h => h.pageid !== article.pageid);
      return [article, ...filtered].slice(0, 5);
    });
    setQuery('');
    setResults([]);
  };

  return (
    <div className="flex flex-col gap-4">
      {/* Search Input */}
      <div className="relative">
        <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none text-zinc-400">
          {loading ? <Loader2 size={16} className="animate-spin" /> : <Search size={16} />}
        </div>
        <input 
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search Wikipedia..."
          className="w-full pl-10 pr-10 py-2.5 text-sm bg-zinc-100 dark:bg-zinc-800 border-none rounded-xl outline-none focus:ring-2 focus:ring-blue-500/20 transition-all"
        />
        {query && (
          <button 
            onClick={() => setQuery('')}
            className="absolute inset-y-0 right-3 flex items-center text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200"
          >
            <X size={16} />
          </button>
        )}

        {/* Autocomplete Dropdown */}
        <AnimatePresence>
          {results.length > 0 && (
            <motion.div 
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="absolute top-full left-0 right-0 mt-2 z-20 bg-white dark:bg-zinc-900 border border-zinc-100 dark:border-zinc-800 rounded-xl shadow-xl overflow-hidden"
            >
              {results.map((r) => (
                <button
                  key={r.pageid}
                  onClick={() => selectArticle(r)}
                  className="w-full flex items-center gap-3 p-3 hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors text-left"
                >
                  {r.thumbnail ? (
                    <img src={r.thumbnail} alt={r.title} className="w-10 h-10 rounded-lg object-cover flex-shrink-0" referrerPolicy="no-referrer" />
                  ) : (
                    <div className="w-10 h-10 rounded-lg bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center text-zinc-400 flex-shrink-0">
                      <BookOpen size={16} />
                    </div>
                  )}
                  <div className="flex flex-col min-w-0">
                    <span className="text-sm font-semibold truncate">{r.title}</span>
                    <span className="text-[10px] text-zinc-400 truncate">{r.extract}</span>
                  </div>
                </button>
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Selected Article View */}
      <AnimatePresence mode="wait">
        {selectedResult ? (
          <motion.div 
            key={selectedResult.pageid}
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.95 }}
            className="flex flex-col gap-4 p-4 rounded-2xl bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-100 dark:border-zinc-800"
          >
            <div className="flex items-start justify-between">
              <h3 className="font-bold text-lg leading-tight">{selectedResult.title}</h3>
              <button 
                onClick={() => setSelectedResult(null)}
                className="p-1 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200"
              >
                <X size={16} />
              </button>
            </div>
            
            {selectedResult.thumbnail && (
              <div className="relative group overflow-hidden rounded-xl shadow-sm">
                <img 
                  src={selectedResult.thumbnail} 
                  alt={selectedResult.title} 
                  className="w-full h-48 object-cover transition-transform duration-500 group-hover:scale-105"
                  referrerPolicy="no-referrer"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
              </div>
            )}

            <div className="space-y-3">
              <div className="flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest text-blue-500 bg-blue-500/10 w-fit px-2 py-1 rounded-full">
                <BookOpen size={10} />
                Brief Overview
              </div>
              <p className="text-sm text-zinc-600 dark:text-zinc-300 leading-relaxed italic border-l-2 border-blue-500/30 pl-4">
                {selectedResult.extract}
              </p>
            </div>

            <a 
              href={`https://en.wikipedia.org/?curid=${selectedResult.pageid}`}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center justify-center gap-2 w-full py-2.5 text-xs font-bold uppercase tracking-wider bg-blue-500 text-white rounded-xl hover:bg-blue-600 transition-colors"
            >
              Read Full Article
              <ExternalLink size={14} />
            </a>
          </motion.div>
        ) : (
          <motion.div 
            key="empty"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="space-y-6"
          >
            {/* Featured Article */}
            {featured && (
              <div className="space-y-3">
                <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-zinc-400">
                  <Star size={12} className="text-yellow-500" />
                  Featured Today
                </div>
                <button 
                  onClick={() => selectArticle(featured)}
                  className="w-full p-4 rounded-2xl bg-gradient-to-br from-blue-500/10 to-purple-500/10 border border-blue-100 dark:border-blue-900/30 text-left group hover:border-blue-300 transition-all"
                >
                  <h4 className="font-bold text-sm group-hover:text-blue-600 transition-colors">{featured.title}</h4>
                  <p className="text-xs text-zinc-500 mt-1 line-clamp-2">{featured.extract}</p>
                  <div className="flex items-center gap-1 mt-3 text-[10px] font-bold text-blue-500 uppercase tracking-wider">
                    Learn More <ArrowRight size={10} />
                  </div>
                </button>
              </div>
            )}

            {/* History */}
            {history.length > 0 && (
              <div className="space-y-3">
                <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-zinc-400">
                  <History size={12} />
                  Recent Searches
                </div>
                <div className="flex flex-wrap gap-2">
                  {history.map(h => (
                    <button 
                      key={h.pageid}
                      onClick={() => selectArticle(h)}
                      className="px-3 py-1.5 text-xs font-medium bg-zinc-100 dark:bg-zinc-800 hover:bg-zinc-200 dark:hover:bg-zinc-700 rounded-full transition-colors"
                    >
                      {h.title}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
