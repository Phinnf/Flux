import React, { useState, useEffect } from 'react';
import { 
  Cloud, 
  Sun, 
  CloudRain, 
  CloudLightning, 
  Wind, 
  Droplets, 
  MapPin,
  ChevronRight,
  ChevronLeft,
  AlertTriangle
} from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { WeatherData } from '../../types';
import { cn } from '../../lib/utils';
import { format } from 'date-fns';

const WEATHER_CODES: Record<number, { condition: string; icon: React.ReactNode; color: string }> = {
  0: { condition: 'Clear Sky', icon: <Sun className="text-yellow-400" />, color: 'from-blue-400 to-blue-200' },
  1: { condition: 'Mainly Clear', icon: <Sun className="text-yellow-300" />, color: 'from-blue-400 to-blue-100' },
  2: { condition: 'Partly Cloudy', icon: <Cloud className="text-zinc-400" />, color: 'from-blue-300 to-zinc-200' },
  3: { condition: 'Overcast', icon: <Cloud className="text-zinc-500" />, color: 'from-zinc-400 to-zinc-300' },
  45: { condition: 'Fog', icon: <Wind className="text-zinc-300" />, color: 'from-zinc-300 to-zinc-100' },
  48: { condition: 'Fog', icon: <Wind className="text-zinc-300" />, color: 'from-zinc-300 to-zinc-100' },
  51: { condition: 'Drizzle', icon: <CloudRain className="text-blue-300" />, color: 'from-blue-300 to-zinc-200' },
  61: { condition: 'Rain', icon: <CloudRain className="text-blue-500" />, color: 'from-blue-500 to-blue-300' },
  80: { condition: 'Showers', icon: <CloudRain className="text-blue-600" />, color: 'from-blue-600 to-blue-400' },
  95: { condition: 'Thunderstorm', icon: <CloudLightning className="text-purple-500" />, color: 'from-purple-600 to-zinc-800' },
};

export default function WeatherWidget() {
  const [weather, setWeather] = useState<WeatherData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [view, setView] = useState<'current' | 'hourly' | 'daily'>('current');

  const fetchWeather = async (lat: number, lon: number) => {
    try {
      setLoading(true);
      
      // Fetch weather data and location name in parallel
      const [weatherRes, locationRes] = await Promise.all([
        fetch(`https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,precipitation&hourly=temperature_2m,weather_code&daily=weather_code,temperature_2m_max,temperature_2m_min&timezone=auto`),
        fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}&zoom=10`)
      ]);

      const data = await weatherRes.json();
      const locationData = await locationRes.json();
      
      const cityName = locationData.address?.city || 
                        locationData.address?.town || 
                        locationData.address?.village || 
                        locationData.address?.suburb || 
                        'Unknown Location';

      const currentCode = data.current.weather_code;
      const weatherInfo = WEATHER_CODES[currentCode] || { condition: 'Unknown', icon: <Cloud />, color: 'from-zinc-400 to-zinc-200' };

      setWeather({
        temp: Math.round(data.current.temperature_2m),
        condition: weatherInfo.condition,
        location: cityName,
        feelsLike: Math.round(data.current.apparent_temperature),
        humidity: data.current.relative_humidity_2m,
        precipProb: data.current.precipitation,
        hourly: data.hourly.time.slice(0, 12).map((time: string, i: number) => ({
          time: format(new Date(time), 'ha'),
          temp: Math.round(data.hourly.temperature_2m[i]),
          condition: WEATHER_CODES[data.hourly.weather_code[i]]?.condition || 'Cloudy'
        })),
        daily: data.daily.time.slice(0, 5).map((date: string, i: number) => ({
          date: format(new Date(date), 'EEE'),
          maxTemp: Math.round(data.daily.temperature_2m_max[i]),
          minTemp: Math.round(data.daily.temperature_2m_min[i]),
          condition: WEATHER_CODES[data.daily.weather_code[i]]?.condition || 'Cloudy'
        }))
      });
      setError(null);
    } catch (err) {
      setError('Failed to fetch weather data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => fetchWeather(pos.coords.latitude, pos.coords.longitude),
        () => fetchWeather(40.7128, -74.0060) // Default to NYC
      );
    } else {
      fetchWeather(40.7128, -74.0060);
    }
  }, []);

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center py-8 gap-3">
        <div className="w-8 h-8 border-4 border-blue-500/20 border-t-blue-500 rounded-full animate-spin" />
        <p className="text-xs text-zinc-400 font-medium">Checking the sky...</p>
      </div>
    );
  }

  if (error || !weather) {
    return (
      <div className="flex flex-col items-center justify-center py-8 gap-3 text-center">
        <AlertTriangle className="text-orange-400" size={24} />
        <p className="text-xs text-zinc-500 px-4">{error || 'Weather unavailable'}</p>
        <button 
          onClick={() => window.location.reload()}
          className="text-[10px] font-bold uppercase tracking-wider text-blue-500 hover:underline"
        >
          Retry
        </button>
      </div>
    );
  }

  const currentCondition = Object.values(WEATHER_CODES).find(c => c.condition === weather.condition) || WEATHER_CODES[0];

  return (
    <div className="flex flex-col gap-4">
      {/* Main Card */}
      <div className={cn(
        "relative p-5 rounded-2xl overflow-hidden bg-gradient-to-br shadow-lg text-white transition-all duration-500",
        "dark:shadow-blue-900/20",
        currentCondition.color
      )}>
        <div className="relative z-10 flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-1.5 text-xs font-semibold opacity-90 bg-white/10 px-2 py-1 rounded-full backdrop-blur-md">
              <MapPin size={12} />
              {weather.location}
            </div>
            <div className="text-[10px] font-bold uppercase tracking-widest opacity-70">
              {format(new Date(), 'EEEE, MMM d')}
            </div>
          </div>

          <div className="flex items-center justify-between">
            <div className="flex flex-col">
              <span className="text-5xl font-bold tracking-tighter leading-none">
                {weather.temp}°
              </span>
              <span className="text-sm font-medium opacity-90 mt-1">
                {weather.condition}
              </span>
            </div>
            <div className="scale-[2.5] opacity-90 mr-4">
              {currentCondition.icon}
            </div>
          </div>

          <div className="grid grid-cols-3 gap-2 mt-2 pt-4 border-t border-white/20">
            <div className="flex flex-col items-center gap-1">
              <span className="text-[10px] uppercase font-bold opacity-60">Feels Like</span>
              <span className="text-sm font-bold">{weather.feelsLike}°</span>
            </div>
            <div className="flex flex-col items-center gap-1">
              <span className="text-[10px] uppercase font-bold opacity-60">Humidity</span>
              <span className="text-sm font-bold">{weather.humidity}%</span>
            </div>
            <div className="flex flex-col items-center gap-1">
              <span className="text-[10px] uppercase font-bold opacity-60">Precip</span>
              <span className="text-sm font-bold">{weather.precipProb}%</span>
            </div>
          </div>
        </div>

        {/* Decorative Circles */}
        <div className="absolute -top-10 -right-10 w-40 h-40 bg-white/10 rounded-full blur-3xl" />
        <div className="absolute -bottom-10 -left-10 w-40 h-40 bg-white/10 rounded-full blur-3xl" />
      </div>

      {/* Forecast Controls */}
      <div className="flex items-center gap-2 p-1 bg-zinc-100 dark:bg-zinc-800 rounded-lg">
        {(['current', 'hourly', 'daily'] as const).map((v) => (
          <button
            key={v}
            onClick={() => setView(v)}
            className={cn(
              "flex-1 py-1.5 text-[10px] font-bold uppercase tracking-wider rounded-md transition-all",
              view === v 
                ? "bg-white dark:bg-zinc-700 text-blue-600 shadow-sm" 
                : "text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-300"
            )}
          >
            {v}
          </button>
        ))}
      </div>

      {/* Forecast Content */}
      <div className="min-h-[100px]">
        <AnimatePresence mode="wait">
          {view === 'hourly' && (
            <motion.div 
              key="hourly"
              initial={{ opacity: 0, x: 10 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -10 }}
              className="flex gap-4 overflow-x-auto pb-2 custom-scrollbar"
            >
              {weather.hourly.map((h, i) => (
                <div key={i} className="flex flex-col items-center gap-2 min-w-[45px]">
                  <span className="text-[10px] font-bold text-zinc-400">{h.time}</span>
                  <div className="text-zinc-600 dark:text-zinc-300">
                    {Object.values(WEATHER_CODES).find(c => c.condition === h.condition)?.icon || <Cloud size={16} />}
                  </div>
                  <span className="text-xs font-bold">{h.temp}°</span>
                </div>
              ))}
            </motion.div>
          )}

          {view === 'daily' && (
            <motion.div 
              key="daily"
              initial={{ opacity: 0, x: 10 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -10 }}
              className="flex flex-col gap-3"
            >
              {weather.daily.map((d, i) => (
                <div key={i} className="flex items-center justify-between p-2 rounded-xl bg-zinc-50 dark:bg-zinc-800/50">
                  <span className="text-xs font-bold w-10">{d.date}</span>
                  <div className="flex items-center gap-2 flex-1 justify-center text-zinc-500">
                    {Object.values(WEATHER_CODES).find(c => c.condition === d.condition)?.icon || <Cloud size={14} />}
                    <span className="text-[10px] font-medium">{d.condition}</span>
                  </div>
                  <div className="flex items-center gap-2 w-16 justify-end">
                    <span className="text-xs font-bold">{d.maxTemp}°</span>
                    <span className="text-xs font-medium text-zinc-400">{d.minTemp}°</span>
                  </div>
                </div>
              ))}
            </motion.div>
          )}

          {view === 'current' && (
            <motion.div 
              key="current"
              initial={{ opacity: 0, y: 5 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -5 }}
              className="p-4 rounded-2xl border border-zinc-100 dark:border-zinc-800 bg-zinc-50/50 dark:bg-zinc-900/50 flex items-center justify-between"
            >
              <div className="flex flex-col gap-1">
                <span className="text-[10px] font-bold text-zinc-400 uppercase tracking-wider">Next Hour</span>
                <p className="text-xs font-medium text-zinc-600 dark:text-zinc-300">
                  {weather.precipProb > 0 ? `Expect ${weather.precipProb}mm of rain.` : 'Clear skies expected for the next hour.'}
                </p>
              </div>
              <div className="p-2 bg-blue-50 dark:bg-blue-900/20 rounded-xl">
                <Wind size={18} className="text-blue-500" />
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}
