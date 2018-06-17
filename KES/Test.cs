using System.Threading;

namespace TestProject1
{
    public class Tests
    {
        [Fact]
        public void Test()
        {
            var repositoryStub = new RequestRepositoryStub();
            var worker = new RequestWorker(repositoryStub);
            worker.Start(12, 56);
            Thread.Sleep(5000);
            worker.Stop();
            Assert.Equal(repositoryStub.Requested, repositoryStub.Processed + repositoryStub.Deleted);
        }

        private class RequestRepositoryStub : IRequestRepository
        {
            public Request Get(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                Requested++;
                return new Request();
            }

            public void Process(Request request, CancellationToken cancellationToken)
            {
                if (!cancellationToken.IsCancellationRequested) Processed++;
            }

            public void DeleteRequest(Request request)
            {
                Deleted++;
            }

            public int Requested { get; private set; }

            public int Processed { get; private set; }

            public int Deleted { get; private set; }
        }
    }
}