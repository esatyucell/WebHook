import { useCallback, useEffect, useState } from 'react';
import {
    Webhook,
    Plus,
    Send,
    RefreshCw,
    Wifi,
    WifiOff,
} from 'lucide-react';
import {
    webhookApi,
    createDeliveryHubConnection,
} from './services/api';
import StatsCards from './components/StatsCards';
import DeliveryFeed from './components/DeliveryFeed';
import SubscriptionsPanel from './components/SubscriptionsPanel';
import {
    CreateSubscriptionModal,
    TriggerEventModal,
    LogDetailDrawer,
} from './components/ActionModals';

export default function App() {
    const [stats, setStats] = useState(null);
    const [logs, setLogs] = useState([]);
    const [subscriptions, setSubscriptions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [hubStatus, setHubStatus] = useState('connecting');

    const [isSubModalOpen, setIsSubModalOpen] = useState(false);
    const [isEventModalOpen, setIsEventModalOpen] = useState(false);
    const [selectedLog, setSelectedLog] = useState(null);

    const loadAllData = useCallback(async () => {
        try {
            const [statsData, logsData, subsData] = await Promise.all([
                webhookApi.getStats(),
                webhookApi.getRecentLogs(50),
                webhookApi.getSubscriptions(),
            ]);
            setStats(statsData);
            setLogs(logsData || []);
            setSubscriptions(subsData || []);
        } catch (err) {
            console.error('Veri yüklenirken hata oluştu:', err);
        } finally {
            setLoading(false);
        }
    }, []);

    const refreshStatsOnly = useCallback(async () => {
        try {
            const statsData = await webhookApi.getStats();
            setStats(statsData);
        } catch (err) {
            console.error('İstatistik güncellenemedi:', err);
        }
    }, []);

    const handleDeleteSubscription = async (id) => {
        try {
            await webhookApi.deleteSubscription(id);
            setSubscriptions((prev) => prev.filter((sub) => sub.id !== id));
            refreshStatsOnly();
        } catch (err) {
            alert(`Abonelik silinemedi: ${err.message}`);
        }
    };

    useEffect(() => {
        loadAllData();
    }, [loadAllData]);

    // SignalR Canlı Akış Bağlantısı
    useEffect(() => {
        let isMounted = true;
        const connection = createDeliveryHubConnection();

        connection.on('ReceiveDeliveryLog', (incomingLog) => {
            if (!isMounted) return;
            setLogs((prev) => {
                if (prev.some((item) => item.id === incomingLog.id)) return prev;
                return [incomingLog, ...prev].slice(0, 100);
            });
            refreshStatsOnly();
        });

        connection.onreconnecting(() => {
            if (isMounted) setHubStatus('connecting');
        });

        connection.onreconnected(() => {
            if (isMounted) {
                setHubStatus('connected');
                loadAllData();
            }
        });

        connection.onclose(() => {
            if (isMounted) setHubStatus('disconnected');
        });

        connection
            .start()
            .then(() => {
                if (isMounted) setHubStatus('connected');
            })
            .catch(() => {
                if (isMounted) setHubStatus('disconnected');
            });

        return () => {
            isMounted = false;
            connection.stop();
        };
    }, [loadAllData, refreshStatsOnly]);

    return (
        <div className="min-h-screen bg-zinc-950 text-zinc-100">
            {/* Üst Navigasyon Çubuğu */}
            <header className="sticky top-0 z-30 border-b border-zinc-800/80 bg-zinc-950/85 backdrop-blur-md">
                <div className="mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-4 px-4 sm:px-6 py-3.5">
                    <div className="flex items-center gap-3">
                        <div className="flex h-9 w-9 items-center justify-center rounded-xl border border-indigo-500/30 bg-indigo-500/10 text-indigo-400">
                            <Webhook className="h-5 w-5" />
                        </div>
                        <div>
                            <div className="flex items-center gap-2.5">
                                <h1 className="text-sm font-semibold tracking-tight text-zinc-100">
                                    WebHook Engine
                                </h1>
                                {/* SignalR Bağlantı Durum Rozeti */}
                                {hubStatus === 'connected' ? (
                                    <span className="inline-flex items-center gap-1.5 rounded-full border border-emerald-500/25 bg-emerald-500/10 px-2.5 py-0.5 text-[11px] font-medium text-emerald-400">
                                        <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 animate-pulse" />
                                        SignalR Canlı
                                    </span>
                                ) : hubStatus === 'connecting' ? (
                                    <span className="inline-flex items-center gap-1.5 rounded-full border border-amber-500/25 bg-amber-500/10 px-2.5 py-0.5 text-[11px] font-medium text-amber-400">
                                        <Wifi className="h-3 w-3 animate-pulse" />
                                        Bağlanıyor...
                                    </span>
                                ) : (
                                    <span className="inline-flex items-center gap-1.5 rounded-full border border-rose-500/25 bg-rose-500/10 px-2.5 py-0.5 text-[11px] font-medium text-rose-400">
                                        <WifiOff className="h-3 w-3" />
                                        Çevrimdışı
                                    </span>
                                )}
                            </div>
                            <p className="text-xs text-zinc-500">
                                Outbox Kuyruk &amp; HMAC-SHA256 Teslimat Monitörü
                            </p>
                        </div>
                    </div>

                    {/* Aksiyon Butonları */}
                    <div className="flex items-center gap-2.5">
                        <button
                            onClick={loadAllData}
                            title="Verileri Yenile"
                            className="inline-flex items-center justify-center rounded-lg border border-zinc-800 bg-zinc-900 p-2 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200 transition-colors cursor-pointer"
                        >
                            <RefreshCw className="h-4 w-4" />
                        </button>

                        <button
                            onClick={() => setIsSubModalOpen(true)}
                            className="inline-flex items-center gap-1.5 rounded-lg border border-zinc-700 bg-zinc-900 px-3.5 py-2 text-xs font-medium text-zinc-200 hover:bg-zinc-800 transition-colors cursor-pointer"
                        >
                            <Plus className="h-3.5 w-3.5" />
                            Yeni Abonelik
                        </button>

                        <button
                            onClick={() => setIsEventModalOpen(true)}
                            className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-3.5 py-2 text-xs font-medium text-white hover:bg-indigo-500 transition-colors shadow-sm cursor-pointer"
                        >
                            <Send className="h-3.5 w-3.5" />
                            Test Olayı Tetikle
                        </button>
                    </div>
                </div>
            </header>

            {/* Ana İçerik */}
            <main className="mx-auto max-w-7xl px-4 sm:px-6 py-6 space-y-6">
                <StatsCards stats={stats} loading={loading} />

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
                    <div className="lg:col-span-2">
                        <DeliveryFeed
                            logs={logs}
                            loading={loading}
                            onSelectLog={(log) => setSelectedLog(log)}
                        />
                    </div>

                    <div className="lg:col-span-1">
                        <SubscriptionsPanel
                            subscriptions={subscriptions}
                            loading={loading}
                            onOpenNewSub={() => setIsSubModalOpen(true)}
                            onDeleteSub={handleDeleteSubscription}
                        />
                    </div>
                </div>
            </main>

            {/* Modallar ve Yan Çekmece */}
            <CreateSubscriptionModal
                isOpen={isSubModalOpen}
                onClose={() => setIsSubModalOpen(false)}
                onCreated={loadAllData}
            />

            <TriggerEventModal
                isOpen={isEventModalOpen}
                onClose={() => setIsEventModalOpen(false)}
                onTriggered={refreshStatsOnly}
            />

            <LogDetailDrawer
                log={selectedLog}
                onClose={() => setSelectedLog(null)}
            />
        </div>
    );
}