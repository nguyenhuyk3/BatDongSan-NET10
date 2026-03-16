namespace VTTGROUP.Infrastructure.Services
{
    public class LoadingService
    {
        private int _counter = 0;
        public event Action? OnChanged;

        public bool IsLoading => _counter > 0;

        public void Show()
        {
            Interlocked.Increment(ref _counter);
            OnChanged?.Invoke();
        }

        public void Hide()
        {
            if (Interlocked.Decrement(ref _counter) < 0)
                Interlocked.Exchange(ref _counter, 0);
            OnChanged?.Invoke();
        }

        public async Task RunAsync(Func<Task> action)
        {
            Show();
            try { await action(); }
            finally { Hide(); }
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> action)
        {
            Show();
            try { return await action(); }
            finally { Hide(); }
        }
    }
}
