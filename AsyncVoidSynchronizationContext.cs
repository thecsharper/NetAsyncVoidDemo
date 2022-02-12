namespace AsyncVoidDemo
{
    public sealed class AsyncVoidSynchronizationContext : SynchronizationContext
    {
        private static readonly SynchronizationContext s_default = new SynchronizationContext();

        private readonly SynchronizationContext _innerSynchronizationContext;
        private readonly TaskCompletionSource _tcs = new();
        private int _startedOperationCount;

        public AsyncVoidSynchronizationContext(SynchronizationContext? innerContext)
        {
            _innerSynchronizationContext = innerContext ?? s_default;
        }

        public Task Completed => _tcs.Task;

        public override void OperationStarted()
        {
            Interlocked.Increment(ref _startedOperationCount);
        }

        public override void OperationCompleted()
        {
            if (Interlocked.Decrement(ref _startedOperationCount) == 0)
            {
                _tcs.TrySetResult();
            }
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _startedOperationCount);

            try
            {
                _innerSynchronizationContext.Post(s =>
                {
                    try
                    {
                        d(s);
                    }
                    catch (Exception ex)
                    {
                        _tcs.TrySetException(ex);
                    }
                    finally
                    {
                        OperationCompleted();
                    }
                }, state);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            try
            {
                _innerSynchronizationContext.Send(d, state);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
        }
    }
}