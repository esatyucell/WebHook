import { useState } from 'react';
import {
    CheckCircle2,
    XCircle,
    Search,
    ChevronRight,
    RotateCw,
    Terminal,
} from 'lucide-react';

function formatTime(isoString) {
    if (!isoString) return '-';
    const date = new Date(isoString.endsWith('Z') ? isoString : `${isoString}Z`);
    return date.toLocaleTimeString('tr-TR', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
    });
}

export default function DeliveryFeed({ logs, loading, onSelectLog }) {
    const [filter, setFilter] = useState('all'); // 'all' | 'success' | 'failed'
    const [search, setSearch] = useState('');

    const filteredLogs = logs.filter((log) => {
        if (filter === 'success' && !log.isSuccessful) return false;
        if (filter === 'failed' && log.isSuccessful) return false;

        if (search.trim()) {
            const q = search.toLowerCase();
            const matchEvent = log.eventType?.toLowerCase().includes(q);
            const matchUrl = log.endpointUrl?.toLowerCase().includes(q);
            return matchEvent || matchUrl;
        }
        return true;
    });

    return (
        <div className="rounded-xl border border-zinc-800/90 bg-zinc-900/50 overflow-hidden">
            {/* Üst Araç Çubuğu */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-zinc-800/80 px-5 py-4">
                <div className="flex items-center gap-2">
                    <Terminal className="h-4 w-4 text-zinc-400" />
                    <h2 className="text-sm font-semibold text-zinc-100">
                        Canlı Teslimat Akışı
                    </h2>
                    <span className="rounded-full bg-zinc-800 px-2 py-0.5 text-xs font-mono text-zinc-400">
                        {filteredLogs.length}
                    </span>
                </div>

                <div className="flex flex-wrap items-center gap-2.5">
                    {/* Arama */}
                    <div className="relative">
                        <Search className="pointer-events-none absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-zinc-500" />
                        <input
                            type="text"
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            placeholder="Event veya URL ara..."
                            className="h-8 w-48 rounded-lg border border-zinc-800 bg-zinc-950 pl-8 pr-3 text-xs text-zinc-200 placeholder-zinc-500 focus:border-indigo-500/60 focus:outline-none"
                        />
                    </div>

                    {/* Filtre Sekmeleri */}
                    <div className="flex rounded-lg border border-zinc-800 bg-zinc-950 p-0.5 text-xs">
                        {[
                            { id: 'all', label: 'Tümü' },
                            { id: 'success', label: 'Başarılı' },
                            { id: 'failed', label: 'Hatalı' },
                        ].map((tab) => (
                            <button
                                key={tab.id}
                                onClick={() => setFilter(tab.id)}
                                className={`rounded-md px-2.5 py-1 font-medium transition-colors cursor-pointer ${filter === tab.id
                                        ? 'bg-zinc-800 text-zinc-100'
                                        : 'text-zinc-400 hover:text-zinc-200'
                                    }`}
                            >
                                {tab.label}
                            </button>
                        ))}
                    </div>
                </div>
            </div>

            {/* Tablo İçeriği */}
            {loading ? (
                <div className="p-8 space-y-3">
                    {[...Array(5)].map((_, i) => (
                        <div key={i} className="h-11 animate-pulse rounded-lg bg-zinc-800/50" />
                    ))}
                </div>
            ) : filteredLogs.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16 text-center px-4">
                    <div className="rounded-full border border-zinc-800 bg-zinc-900 p-3 text-zinc-500">
                        <Terminal className="h-5 w-5" />
                    </div>
                    <p className="mt-3 text-sm font-medium text-zinc-300">
                        Henüz gösterilecek teslimat kaydı yok
                    </p>
                    <p className="mt-1 text-xs text-zinc-500 max-w-sm">
                        Sağ üstteki &ldquo;Test Olayı Tetikle&rdquo; butonunu kullanarak ilk webhook teslimatını başlatabilirsin.
                    </p>
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="border-b border-zinc-800/80 bg-zinc-950/40 text-[11px] font-medium uppercase tracking-wider text-zinc-500">
                                <th className="py-3 pl-5 pr-3">Durum</th>
                                <th className="py-3 px-3">Olay Tipi</th>
                                <th className="py-3 px-3">Hedef Endpoint</th>
                                <th className="py-3 px-3">Deneme</th>
                                <th className="py-3 px-3">Zaman</th>
                                <th className="py-3 pl-3 pr-5 text-right">Detay</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-zinc-800/60 text-xs">
                            {filteredLogs.map((log) => (
                                <tr
                                    key={log.id}
                                    onClick={() => onSelectLog(log)}
                                    className="group transition-colors hover:bg-zinc-800/40 cursor-pointer"
                                >
                                    {/* HTTP Durum Rozeti */}
                                    <td className="py-3.5 pl-5 pr-3 whitespace-nowrap">
                                        {log.isSuccessful ? (
                                            <span className="inline-flex items-center gap-1.5 rounded-md border border-emerald-500/25 bg-emerald-500/10 px-2 py-0.5 font-mono text-[11px] font-medium text-emerald-400">
                                                <CheckCircle2 className="h-3 w-3" />
                                                {log.httpStatusCode ?? 200} OK
                                            </span>
                                        ) : (
                                            <span className="inline-flex items-center gap-1.5 rounded-md border border-rose-500/25 bg-rose-500/10 px-2 py-0.5 font-mono text-[11px] font-medium text-rose-400">
                                                <XCircle className="h-3 w-3" />
                                                {log.httpStatusCode ? `HTTP ${log.httpStatusCode}` : 'ERR'}
                                            </span>
                                        )}
                                    </td>

                                    {/* Event Type */}
                                    <td className="py-3.5 px-3 whitespace-nowrap">
                                        <span className="rounded bg-zinc-800/90 px-2 py-0.5 font-mono text-zinc-200 border border-zinc-700/60">
                                            {log.eventType || 'unknown'}
                                        </span>
                                    </td>

                                    {/* Endpoint URL */}
                                    <td className="py-3.5 px-3 max-w-xs truncate font-mono text-zinc-400">
                                        {log.endpointUrl || '-'}
                                    </td>

                                    {/* Attempt Count */}
                                    <td className="py-3.5 px-3 whitespace-nowrap">
                                        <span className="inline-flex items-center gap-1 font-mono text-zinc-400">
                                            <RotateCw className="h-3 w-3 text-zinc-500" />
                                            {log.attemptCount} / 3
                                        </span>
                                    </td>

                                    {/* Zaman */}
                                    <td className="py-3.5 px-3 whitespace-nowrap font-mono text-zinc-500">
                                        {formatTime(log.createdAt)}
                                    </td>

                                    {/* İncele İkonu */}
                                    <td className="py-3.5 pl-3 pr-5 text-right whitespace-nowrap">
                                        <ChevronRight className="inline-block h-4 w-4 text-zinc-600 transition-transform group-hover:translate-x-0.5 group-hover:text-zinc-300" />
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}