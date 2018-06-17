using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        RequestWorker requestworker = new RequestWorker(new RequestRepository());
        requestworker.Start(3, 3);
        Thread.Sleep(30000);
        requestworker.Stop();
    }
}

public class Request
{

}

public interface IRequestRepository
{
    Request Get(CancellationToken cancellationToken);
    void Process(Request request, CancellationToken cancellationToken);
    void DeleteRequest(Request request);
}

public class RequestRepository : IRequestRepository
{
    public Request Get(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep((new Random()).Next(1000, 3000));
            return new Request();
        }
        else
            return null;
    }

    public void Process(Request request, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            Thread.Sleep((new Random()).Next(1000, 2000));
    }

    public void DeleteRequest(Request request)
    {
    }
}

public class RequestWorker
{
    private readonly IRequestRepository _requestRepository;
    private List<Task> _tasks;
    private ConcurrentQueue<Request> _requests;
    private CancellationTokenSource _cancelationTokenSource;

    public RequestWorker(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public void Start(int requestProcessorCount, int requestGetterCount)
    {
        _cancelationTokenSource = new CancellationTokenSource();
        _requests = new ConcurrentQueue<Request>();
        _tasks = new List<Task>();

        for (int i = 0; i < requestProcessorCount; ++i)
            _tasks.Add(Task.Factory.StartNew(() => ProcessRequestWorker(_cancelationTokenSource.Token)));

        for (int i = 0; i < requestGetterCount; ++i)
            _tasks.Add(Task.Factory.StartNew(() => GetRequestWorker(_cancelationTokenSource.Token)));
    }

    public void Stop()
    {
        _cancelationTokenSource.Cancel();
        Task.WaitAll(_tasks.ToArray());
        while (_requests.TryDequeue(out var request))
        {
            _requestRepository.DeleteRequest(request);
        }

        _cancelationTokenSource.Dispose();
    }

    private void ProcessRequestWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Thread.Sleep(400);
            if (_requests.TryDequeue(out var request))
            {
                _requestRepository.Process(request, token);
            }
        }
    }

    private void GetRequestWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var request = _requestRepository.Get(token);
            if (request != null)
            {
                _requests.Enqueue(request);
            }
        }
    }
}