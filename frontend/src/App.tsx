import { useEffect, useState } from 'react';
import { habitApi, periodNames } from './api/habits';
import type { Habit, PeriodType } from './api/habits';
import './App.css';

function App() {
    const [habits, setHabits] = useState<Habit[]>([]);
    const [name, setName] = useState('');
    const [period, setPeriod] = useState<PeriodType>(7);
    const [target, setTarget] = useState(7);

    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [completingId, setCompletingId] = useState<string | null>(null);

    const loadHabits = async () => {
        try {
            setLoading(true);
            const data = await habitApi.getAll();
            setHabits(data || []);
            setError(null);
        } catch {
            setError('Не удалось загрузить данные');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        // Начальная загрузка списка при монтировании: setLoading(true) здесь
        // выполняется ровно один раз и не порождает каскадных рендеров.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadHabits();
    }, []);

    const handleAdd = async () => {
        if (!name.trim()) return;
        try {
            await habitApi.create({ name: name.trim(), period, targetCount: target });
            setName('');
            setTarget(period);
            await loadHabits();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Ошибка при создании привычки');
        }
    };

    const handleComplete = async (id: string) => {
        setCompletingId(id);
        try {
            await habitApi.complete(id);
            setError(null);
            await loadHabits();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Ошибка при отметке выполнения');
        } finally {
            setCompletingId(null);
        }
    };

    const handleDelete = async (id: string) => {
        if (!window.confirm('Удалить эту привычку?')) return;
        try {
            await habitApi.delete(id);
            await loadHabits();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Ошибка при удалении');
        }
    };

    const getProgressPercent = (completed: number, target: number) => Math.min((completed / target) * 100, 100);
    const isGoalReached = (completed: number, target: number) => completed >= target;

    return (
        <div className="app">
            <div className="container">
                <header className="header">
                    <h1>Трекер привычек</h1>
                </header>

                {error && (
                    <div className="error-banner">
                        {error}
                        <button onClick={() => setError(null)} className="close-error">×</button>
                    </div>
                )}

                <div className="add-form">
                    <input
                        value={name}
                        onChange={e => setName(e.target.value)}
                        onKeyDown={e => e.key === 'Enter' && handleAdd()}
                        placeholder="Например: Читать 30 минут"
                        className="input-main"
                        disabled={loading}
                    />
                    <div className="form-controls">
                        <select
                            value={period}
                            onChange={e => {
                                const p = Number(e.target.value) as PeriodType;
                                setPeriod(p);
                                setTarget(p);
                            }}
                            className="input-select"
                            disabled={loading}
                        >
                            {Object.entries(periodNames).map(([value, label]) => (
                                <option key={value} value={value}>{label}</option>
                            ))}
                        </select>
                        <input
                            type="number"
                            value={target}
                            onChange={e => setTarget(Math.max(1, Number(e.target.value)))}
                            min="1"
                            className="input-number"
                            disabled={loading}
                        />
                        <button onClick={handleAdd} className="btn-primary" disabled={loading || !name.trim()}>
                            {loading ? '...' : 'Добавить'}
                        </button>
                    </div>
                </div>

                {loading && habits.length === 0 ? (
                    <div className="loading-state">Загрузка...</div>
                ) : habits.length === 0 ? (
                    <div className="empty-state">Список пуст. Добавьте первую привычку выше.</div>
                ) : (
                    <div className="habits-list">
                        {[...habits]
                            .sort((a, b) => {
                                const aReached = a.completedInPeriod >= a.targetCount;
                                const bReached = b.completedInPeriod >= b.targetCount;
                                if (aReached === bReached) return 0;
                                return aReached ? 1 : -1;
                            })
                            .map(h => {
                                const progress = getProgressPercent(h.completedInPeriod, h.targetCount);
                                const reached = isGoalReached(h.completedInPeriod, h.targetCount);

                                return (
                                    <div key={h.id} className={`habit-card ${reached ? 'goal-reached-card' : ''}`}>
                                        <div className="card-header">
                                            <div>
                                                <h3 className="habit-name">{h.name}</h3>
                                                <span className="habit-meta">
                                                    {h.completedInPeriod} из {h.targetCount} дней
                                                </span>
                                            </div>
                                            <button
                                                onClick={() => handleDelete(h.id)}
                                                className="btn-icon btn-delete"
                                                title="Удалить"
                                                disabled={completingId === h.id}
                                            >
                                                ✕
                                            </button>
                                        </div>

                                        <div className="progress-container">
                                            <div className="progress-bar-bg">
                                                <div
                                                    className={`progress-bar-fill ${reached ? 'fill-success' : 'fill-primary'}`}
                                                    style={{ width: `${progress}%` }}
                                                />
                                            </div>
                                            <span className="progress-text">{Math.round(progress)}%</span>
                                        </div>

                                        <button
                                            onClick={() => handleComplete(h.id)}
                                            className={`btn-action ${reached ? 'btn-disabled' : 'btn-success'}`}
                                            disabled={reached || completingId === h.id}
                                        >
                                            {completingId === h.id ? 'Сохранение...' : (reached ? 'Цель достигнута' : 'Отметить выполнение')}
                                        </button>
                                    </div>
                                );
                            })}
                    </div>
                )}
            </div>
        </div>
    );
}

export default App;
