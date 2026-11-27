import * as signalR from '@microsoft/signalr';

const API_BASE_URL = 'http://localhost:5195/api';
export const HUB_URL = 'http://localhost:5195/hubs/delivery';

async function request(endpoint, options = {}) {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
        ...options,
    });

    if (!response.ok) {
        let errorMessage = `HTTP ${response.status}`;
        try {
            const errorData = await response.json();
            if (errorData.errors) {
                errorMessage = Object.values(errorData.errors).flat().join(' ');
            } else if (errorData.title || errorData.message) {
                errorMessage = errorData.title || errorData.message;
            }
        } catch {
            // JSON gövdesi yoksa varsayılan durum mesajını koru
        }
        throw new Error(errorMessage);
    }

    if (response.status === 204) return null;
    const text = await response.text();
    return text ? JSON.parse(text) : null;
}

export const webhookApi = {
    getStats: () => request('/dashboard/stats'),
    getRecentLogs: (count = 50) => request(`/delivery-logs?count=${count}`),
    getSubscriptions: () => request('/subscriptions'),
    createSubscription: (data) =>
        request('/subscriptions', {
            method: 'POST',
            body: JSON.stringify(data),
        }),
    deleteSubscription: (id) =>
        request(`/subscriptions/${id}`, {
            method: 'DELETE',
        }),
    triggerEvent: (data) =>
        request('/events', {
            method: 'POST',
            body: JSON.stringify(data),
        }),
};

export function createDeliveryHubConnection() {
    return new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, {
            withCredentials: true,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();
}