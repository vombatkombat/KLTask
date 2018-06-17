using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KES
{
    class Program
    {
        static void Main(string[] args)
        {
            RequestWorker requestworker = new RequestWorker();
            requestworker.Start(3, 3);
            Thread.Sleep(30000);
            requestworker.Stop();

            Console.ReadLine();
        }
    }

    class Request
    {

    }

    class RequestWorker
    {
        List<Task> tasks;
        ConcurrentQueue<Request> requests;
        CancellationTokenSource cancelationTokenSource;
        CancellationToken cancellationToken;

        int _cnt;
        object _lockObj = new object();
        private int Count
        {
            get { return _cnt; }
            set
            {
                lock (_lockObj)
                {
                    _cnt = value;
                }
            }
        }

        public void Start(int requestProcessorCount, int requestGetterCount)
        {
            cancelationTokenSource = new CancellationTokenSource();
            cancellationToken = cancelationTokenSource.Token;
            requests = new ConcurrentQueue<Request>();
            tasks = new List<Task>();
            Count = 0;

            for (int i = 0; i < requestProcessorCount; ++i)
                tasks.Add(Task.Factory.StartNew(ProcessRequestWorker));

            for (int i = 0; i < requestGetterCount; ++i)
                tasks.Add(Task.Factory.StartNew(GetRequestWorker));
        }

        public void Stop()
        {
            cancelationTokenSource.Cancel();
            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Count of unprocessed requests = {requests.Count}");
            Console.WriteLine($"Count of all requests = {Count}");

            while (requests.TryDequeue(out var request))
            {
                DeleteRequest(request);
            }

            Console.WriteLine($"Size of queue after delete = {requests.Count}");
        }
        private void DeleteRequest(Request request)
        {

        }
        private Request GetRequest(CancellationToken cancelationToken)
        {
            if (!cancelationToken.IsCancellationRequested)
            {
                Thread.Sleep((new Random()).Next(1000, 3000));
                ++Count;
                return new Request();
            }
            else
                return null;
        }
        private void ProcessRequest(Request request, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                Thread.Sleep((new Random()).Next(1000, 2000));
        }
        private void ProcessRequestWorker()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(400);
                if (requests.TryDequeue(out var request))
                {
                    ProcessRequest(request, cancellationToken);
                    Console.WriteLine($"Processworker {Thread.CurrentThread.ManagedThreadId}");
                }
            }
        }
        private void GetRequestWorker()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = GetRequest(cancellationToken);
                if (request != null)
                {
                    requests.Enqueue(request);
                    Console.WriteLine($"Getworker {Thread.CurrentThread.ManagedThreadId}");
                }
            }
        }
    }
}