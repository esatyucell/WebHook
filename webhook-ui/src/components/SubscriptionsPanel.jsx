import { useState } from 'react';
import { Globe, KeyRound, Plus, Radio, Trash2 } from 'lucide-react';

export default function SubscriptionsPanel({
    subscriptions,
    loading,
    onOpenNewSub,
    onDeleteSub,
}) {
    const [deletingId, setDeletingId] = useState(null);

    const handleDelete = async (sub) => {
        const confirmed = window.confirm(
            `"${sub.name || sub.endpointUrl}" aboneliğini silmek istediğine emin misin?`
        );
        if (!confirmed) return;

        setDeletingId(sub.id);
        try {
            await onDeleteSub(sub.id);
        } finally {
            setDeletingId(null);
        }
    };

    return (
        <div className="rounded-xl border border-zinc-800/90 bg-zinc-900/50 overflow-hidden">
            <div className="flex items-center justify-between border-b border-zinc-800/80 px-5 py-4">
                <div className="flex items-center gap-2">
                    <Radio className="h-4 w-4 text-indigo-400" />
                    <h2 className="text-sm font-semibold text-zinc-100">
                        Kayıtlı Abonelikler
                    </h2>
                    <span className="rounded-full bg-zinc-800 px-2 py-0.5 text-xs font-mono text-zinc-400">
                        {subscriptions.length}
                    </span>
                </div>

                <button
                    onClick={onOpenNewSub}
                    className="inline-flex items-center gap-1.5 rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-1.5 text-xs font-medium text-zinc-100 hover:bg-zinc-700 transition-colors cursor-pointer"
                >
                    <Plus className="h-3.5 w-3.5" />
                    Yeni Ekle
                </button>
            </div>

            {loading ? (
                <div className="p-5 space-y-2.5">
                    {[...Array(3)].map((_, i) => (
                        <div key={i} className="h-14 animate-pulse rounded-lg bg-zinc-800/50" />
                    ))}
                </div>
            ) : subscriptions.length === 0 ? (
                <div className="py-12 text-center px-4">
                    <p className="text-xs text-zinc-400">Henüz tanımlı bir abonelik bulunmuyor.</p>
                </div>
            ) : (
                <div className="divide-y divide-zinc-800/60">
                    {subscriptions.map((sub) => (
                        <div
                            key={sub.id}
                            className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 px-5 py-3.5 hover:bg-zinc-800/30 transition-colors"
                        >
                            <div className="min-w-0 space-y-1">
                                <div className="flex items-center gap-2">
                                    <span className="h-2 w-2 rounded-full bg-emerald-400 shrink-0" />
                                    <span className="text-xs font-semibold text-zinc-100 truncate">
                                        {sub.name || 'İsimsiz Abonelik'}
                                    </span>
                                    <span className="rounded bg-indigo-500/10 border border-indigo-500/20 px-2 py-0.5 font-mono text-[11px] text-indigo-300 shrink-0">
                                        {sub.eventType}
                                    </span>
                                </div>
                                <div className="flex items-center gap-1.5 text-xs font-mono text-zinc-400 truncate">
                                    <Globe className="h-3 w-3 shrink-0 text-zinc-500" />
                                    <span className="truncate">{sub.endpointUrl}</span>
                                </div>
                            </div>

                            <div className="flex items-center gap-2 self-start sm:self-center shrink-0">
                                <div className="flex items-center gap-1.5 rounded-md border border-zinc-800 bg-zinc-950 px-2.5 py-1 font-mono text-[11px] text-zinc-500">
                                    <KeyRound className="h-3 w-3 text-zinc-600" />
                                    <span>HMAC Aktif</span>
                                </div>

                                <button
                                    onClick={() => handleDelete(sub)}
                                    disabled={deletingId === sub.id}
                                    title="Aboneliği Sil"
                                    className="rounded-md border border-zinc-800 bg-zinc-950 p-1.5 text-zinc-400 hover:border-rose-500/40 hover:bg-rose-500/10 hover:text-rose-400 disabled:opacity-50 transition-colors cursor-pointer"
                                >
                                    <Trash2 className="h-3.5 w-3.5" />
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}