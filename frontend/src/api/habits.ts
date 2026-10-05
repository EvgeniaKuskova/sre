// Фактор 3: адрес API приходит только из переменной окружения VITE_API_URL
// (шаблон — .env.example, локальные значения — в .env, который не коммитится).
const API_URL = import.meta.env.VITE_API_URL;

if (!API_URL) {
    throw new Error('Переменная окружения VITE_API_URL не задана. Скопируйте frontend/.env.example в frontend/.env');
}

export type PeriodType = 1 | 7 | 30 | 90 | 365;

export const periodNames: Record<PeriodType, string> = {
    1: 'День',
    7: 'Неделя',
    30: 'Месяц',
    90: 'Квартал',
    365: 'Год'
};

export interface Habit {
    id: string;
    name: string;
    period: PeriodType;
    targetCount: number;
    completedInPeriod: number;
    completedDates: string[];
}

export const habitApi = {
    getAll: async () => {
        const res = await fetch(`${API_URL}/habits`);
        return res.json();
    },
    create: async (data: { name: string; period: PeriodType; targetCount: number }) => {
        await fetch(`${API_URL}/habits`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
    },
    complete: async (id: string) => {
        const res = await fetch(`${API_URL}/habits/${id}/complete`, { method: 'POST' });
        if (!res.ok) {
            const error = await res.json();
            throw new Error(error.message || 'Ошибка');
        }
    },
    delete: async (id: string) => {
        await fetch(`${API_URL}/habits/${id}`, { method: 'DELETE' });
    }
};
