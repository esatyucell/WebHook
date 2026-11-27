import { useState } from 'react';
import {
    X,
    Send,
    Plus,
    Sparkles,
    CheckCircle2,
    XCircle,
    Copy,
    Check,
} from 'lucide-react';
import { webhookApi } from '../services/api';

function formatJson(raw) {
    if (!raw) return '{}';
    if (typeof raw === 'object') return JSON.stringify(raw, null, 2);
    try {
        const parsed = JSON.parse(raw);
        if (typeof parsed === 'string') {
            return JSON.stringify(JSON.parse(parsed), null, 2);
        }
        return JSON.stringify(parsed, null, 2);
    } catch {
        return String(raw);
    }
}

/* 1. Yeni Abonelik Oluşturma Modalı */
export function CreateSubscriptionModal({ isOpen, onClose, onCreated }) {
    const [name, setName] = useState('');
    const [endpointUrl, setEndpointUrl] = useState('');
    const [eventType, setEventType] = useState('order.created');
    const [secretKey, setSecretKey] = useState('whsec_gizli_anahtar_2026');
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState('');

    if (!isOpen) return null;

    const generateSecret = () => {
        const random = crypto.randomUUID().replace(/-/g, '').slice(0, 24);
        setSecretKey(`whsec_${random}`);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSubmitting(true);
        try {
            await webhookApi.createSubscription({
                name,
                endpointUrl,
                eventType,
                secretKey,
            });
            setName('');
            setEndpointUrl('');
            onCreated();
            onClose();
        } catch (err) {
            setError(err.message);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-xs p-4">
            <div className="w-full max-w-md rounded-xl border border-zinc-800 bg-zinc-900 shadow-2xl">
                <div className="flex items-center justify-between border-b border-zinc-800 px-5 py-4">
                    <h3 className="text-sm font-semibold text-zinc-100">
                        Yeni Webhook Aboneliği
                    </h3>
                    <button
                        onClick={onClose}
                        className="rounded-lg p-1 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200 cursor-pointer"
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-5 space-y-4 text-xs">
                    {error && (
                        <div className="rounded-lg border border-rose-500/30 bg-rose-500/10 p-3 text-rose-300">
                            {error}
                        </div>
                    )}

                    <div>
                        <label className="block font-medium text-zinc-300 mb-1.5">
                            Abonelik Adı *
                        </label>
                        <input
                            type="text"
                            required
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            placeholder="Örn: Muhasebe Servisi"
                            className="w-full rounded-lg border border-zinc-800 bg-zinc-950 px-3 py-2 text-zinc-100 placeholder-zinc-600 focus:border-indigo-500 focus:outline-none"
                        />
                    </div>

                    <div>
                        <label className="block font-medium text-zinc-300 mb-1.5">
                            Hedef Endpoint URL *
                        </label>
                        <input
                            type="url"
                            required
                            value={endpointUrl}
                            onChange={(e) => setEndpointUrl(e.target.value)}
                            placeholder="https://webhook.site/..."
                            className="w-full rounded-lg border border-zinc-800 bg-zinc-950 px-3 py-2 font-mono text-zinc-100 placeholder-zinc-600 focus:border-indigo-500 focus:outline-none"
                        />
                    </div>

                    <div>
                        <label className="block font-medium text-zinc-300 mb-1.5">
                            Dinlenecek Olay Tipi (Event Type) *
                        </label>
                        <input
                            type="text"
                            required
                            value={eventType}
                            onChange={(e) => setEventType(e.target.value)}
                            placeholder="order.created"
                            className="w-full rounded-lg border border-zinc-800 bg-zinc-950 px-3 py-2 font-mono text-zinc-100 placeholder-zinc-600 focus:border-indigo-500 focus:outline-none"
                        />
                    </div>

                    <div>
                        <div className="flex items-center justify-between mb-1.5">
                            <label className="font-medium text-zinc-300">
                                HMAC Secret Key *
                            </label>
                            <button
                                type="button"
                                onClick={generateSecret}
                                className="inline-flex items-center gap-1 text-[11px] text-indigo-400 hover:text-indigo-300 cursor-pointer"
                            >
                                <Sparkles className="h-3 w-3" />
                                Rastgele Üret
                            </button>
                        </div>
                        <input
                            type="text"
                            required
                            value={secretKey}
                            onChange={(e) => setSecretKey(e.target.value)}
                            className="w-full rounded-lg border border-zinc-800 bg-zinc-950 px-3 py-2 font-mono text-zinc-100 focus:border-indigo-500 focus:outline-none"
                        />
                    </div>

                    <div className="flex justify-end gap-2 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="rounded-lg border border-zinc-800 px-4 py-2 font-medium text-zinc-300 hover:bg-zinc-800 cursor-pointer"
                        >
                            İptal
                        </button>
                        <button
                            type="submit"
                            disabled={submitting}
                            className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-4 py-2 font-medium text-white hover:bg-indigo-500 disabled:opacity-50 cursor-pointer"
                        >
                            <Plus className="h-3.5 w-3.5" />
                            {submitting ? 'Kaydediliyor...' : 'Aboneliği Kaydet'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

/* 2. Canlı Olay Tetikleme Modalı (Event Simulator) */
export function TriggerEventModal({ isOpen, onClose, onTriggered }) {
    const [eventType, setEventType] = useState('order.created');
    const [payloadText, setPayloadText] = useState(
        JSON.stringify(
            {
                orderId: 2048,
                customer: 'Esat',
                amount: 1490.5,
                currency: 'TRY',
            },
            null,
            2
        )
    );
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState('');

    if (!isOpen) return null;

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        let parsedPayload;
        try {
            parsedPayload = JSON.parse(payloadText);
        } catch {
            setError('Geçersiz JSON formatı. Lütfen süslü parantez ve tırnakları kontrol et.');
            return;
        }

        setSubmitting(true);
        try {
            await webhookApi.triggerEvent({
                eventType,
                payload: parsedPayload,
            });
            onTriggered();
            onClose();
        } catch (err) {
            setError(err.message);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-xs p-4">
            <div className="w-full max-w-lg rounded-xl border border-zinc-800 bg-zinc-900 shadow-2xl">
                <div className="flex items-center justify-between border-b border-zinc-800 px-5 py-4">
                    <h3 className="text-sm font-semibold text-zinc-100">
                        Outbox Olayı Tetikle (Simülatör)
                    </h3>
                    <button
                        onClick={onClose}
                        className="rounded-lg p-1 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200 cursor-pointer"
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-5 space-y-4 text-xs">
                    {error && (
                        <div className="rounded-lg border border-rose-500/30 bg-rose-500/10 p-3 text-rose-300">
                            {error}
                        </div>
                    )}

                    <div>
                        <label className="block font-medium text-zinc-300 mb-1.5">
                            Olay Tipi (Event Type)
                        </label>
                        <input
                            type="text"
                            required
                            value={eventType}
                            onChange={(e) => setEventType(e.target.value)}
                            className="w-full rounded-lg border border-zinc-800 bg-zinc-950 px-3 py-2 font-mono text-zinc-100 focus:border-indigo-500 focus:outline-none"
                        />
                    </div>

                    <div>
                        <label className="block font-medium text-zinc-300 mb-1.5">
                            JSON Payload Gövdesi
                        </label>
                        <textarea
                            rows={7}
                            required
                            value={payloadText}
                            onChange={(e) => setPayloadText(e.target.value)}
                            className="w-full rounded-lg border border-zinc-800 bg-zinc-950 p-3 font-mono text-xs text-emerald-300 focus:border-indigo-500 focus:outline-none"
                        />
                    </div>

                    <div className="flex justify-end gap-2 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="rounded-lg border border-zinc-800 px-4 py-2 font-medium text-zinc-300 hover:bg-zinc-800 cursor-pointer"
                        >
                            İptal
                        </button>
                        <button
                            type="submit"
                            disabled={submitting}
                            className="inline-flex items-center gap-1.5 rounded-lg bg-emerald-600 px-4 py-2 font-medium text-white hover:bg-emerald-500 disabled:opacity-50 cursor-pointer"
                        >
                            <Send className="h-3.5 w-3.5" />
                            {submitting ? 'Kuyruğa Alınıyor...' : 'Kuyruğa Gönder'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

/* 3. Teslimat Log Detay İnceleyici (Sağ Yan Çekmece) */
export function LogDetailDrawer({ log, onClose }) {
    const [copied, setCopied] = useState(false);

    if (!log) return null;

    const formattedPayload = formatJson(log.payload);

    const copyPayload = () => {
        navigator.clipboard.writeText(formattedPayload);
        setCopied(true);
        setTimeout(() => setCopied(false), 1500);
    };

    return (
        <div className="fixed inset-0 z-50 flex justify-end bg-black/60 backdrop-blur-xs">
            <div className="flex h-full w-full max-w-lg flex-col border-l border-zinc-800 bg-zinc-900 shadow-2xl">
                {/* Başlık */}
                <div className="flex items-center justify-between border-b border-zinc-800 px-6 py-4">
                    <div className="flex items-center gap-2.5">
                        {log.isSuccessful ? (
                            <CheckCircle2 className="h-4 w-4 text-emerald-400" />
                        ) : (
                            <XCircle className="h-4 w-4 text-rose-400" />
                        )}
                        <h3 className="text-sm font-semibold text-zinc-100">
                            Teslimat Kaydı Detayı
                        </h3>
                    </div>
                    <button
                        onClick={onClose}
                        className="rounded-lg p-1 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200 cursor-pointer"
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>

                {/* İçerik */}
                <div className="flex-1 overflow-y-auto p-6 space-y-5 text-xs">
                    <div className="grid grid-cols-2 gap-3 rounded-xl border border-zinc-800 bg-zinc-950/60 p-4">
                        <div>
                            <span className="text-zinc-500 block">HTTP Durumu</span>
                            <span
                                className={`mt-1 inline-block font-mono font-semibold ${log.isSuccessful ? 'text-emerald-400' : 'text-rose-400'
                                    }`}
                            >
                                {log.httpStatusCode ?? 'Bağlantı Hatası'}
                            </span>
                        </div>
                        <div>
                            <span className="text-zinc-500 block">Deneme Sayısı</span>
                            <span className="mt-1 inline-block font-mono text-zinc-200">
                                {log.attemptCount} / 3
                            </span>
                        </div>
                        <div className="col-span-2">
                            <span className="text-zinc-500 block">Olay Tipi</span>
                            <span className="mt-1 inline-block font-mono text-indigo-300">
                                {log.eventType}
                            </span>
                        </div>
                        <div className="col-span-2">
                            <span className="text-zinc-500 block">Hedef URL</span>
                            <span className="mt-1 block break-all font-mono text-zinc-300">
                                {log.endpointUrl}
                            </span>
                        </div>
                    </div>

                    {log.errorMessage && (
                        <div className="rounded-xl border border-rose-500/30 bg-rose-500/10 p-4">
                            <span className="font-semibold text-rose-300 block mb-1">
                                Hata Mesajı
                            </span>
                            <p className="font-mono text-rose-200 break-words">
                                {log.errorMessage}
                            </p>
                        </div>
                    )}

                    <div>
                        <div className="flex items-center justify-between mb-2">
                            <span className="font-medium text-zinc-300">
                                Gönderilen JSON Payload
                            </span>
                            <button
                                onClick={copyPayload}
                                className="inline-flex items-center gap-1 rounded border border-zinc-800 bg-zinc-950 px-2 py-1 text-[11px] text-zinc-400 hover:text-zinc-200 cursor-pointer"
                            >
                                {copied ? (
                                    <>
                                        <Check className="h-3 w-3 text-emerald-400" />
                                        Kopyalandı
                                    </>
                                ) : (
                                    <>
                                        <Copy className="h-3 w-3" />
                                        Kopyala
                                    </>
                                )}
                            </button>
                        </div>
                        <pre className="overflow-x-auto rounded-xl border border-zinc-800 bg-zinc-950 p-4 font-mono text-xs leading-relaxed text-emerald-300">
                            {formattedPayload}
                        </pre>
                    </div>

                    <div className="space-y-1.5 border-t border-zinc-800 pt-4 font-mono text-[11px] text-zinc-500">
                        <div>Log ID: {log.id}</div>
                        <div>Outbox ID: {log.outboxEventId}</div>
                        <div>Subscription ID: {log.subscriptionId}</div>
                    </div>
                </div>
            </div>
        </div>
    );
}