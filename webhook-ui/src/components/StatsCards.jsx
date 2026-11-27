import { Activity, CheckCircle2, Clock, Radio } from 'lucide-react';

export default function StatsCards({ stats, loading }) {
    const cards = [
        {
            title: 'Aktif Abonelik',
            value: stats?.activeSubscriptions ?? 0,
            subtitle: 'Dinleyen hedef uç nokta',
            icon: Radio,
            accent: 'text-indigo-400',
            bg: 'bg-indigo-500/10 border-indigo-500/20',
        },
        {
            title: 'Kuyrukta Bekleyen',
            value: stats?.pendingEvents ?? 0,
            subtitle: 'Outbox işlem sırası',
            icon: Clock,
            accent: 'text-amber-400',
            bg: 'bg-amber-500/10 border-amber-500/20',
        },
        {
            title: 'Toplam Teslimat',
            value: stats?.totalDeliveries ?? 0,
            subtitle: 'Tüm deneme kayıtları',
            icon: Activity,
            accent: 'text-sky-400',
            bg: 'bg-sky-500/10 border-sky-500/20',
        },
        {
            title: 'Başarı Oranı',
            value: `%${stats?.successRate ?? 0}`,
            subtitle: '2xx HTTP yanıt oranı',
            icon: CheckCircle2,
            accent: 'text-emerald-400',
            bg: 'bg-emerald-500/10 border-emerald-500/20',
        },
    ];

    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {cards.map((card) => {
                const Icon = card.icon;
                return (
                    <div
                        key={card.title}
                        className="rounded-xl border border-zinc-800/90 bg-zinc-900/60 p-5 transition-colors hover:border-zinc-700/80"
                    >
                        <div className="flex items-center justify-between">
                            <span className="text-xs font-medium uppercase tracking-wider text-zinc-400">
                                {card.title}
                            </span>
                            <div className={`rounded-lg border p-2 ${card.bg}`}>
                                <Icon className={`h-4 w-4 ${card.accent}`} />
                            </div>
                        </div>
                        <div className="mt-3 flex items-baseline gap-2">
                            {loading ? (
                                <div className="h-8 w-20 animate-pulse rounded bg-zinc-800" />
                            ) : (
                                <span className="text-2xl font-semibold tracking-tight text-zinc-100 font-mono">
                                    {card.value}
                                </span>
                            )}
                        </div>
                        <p className="mt-1 text-xs text-zinc-500">{card.subtitle}</p>
                    </div>
                );
            })}
        </div>
    );
}