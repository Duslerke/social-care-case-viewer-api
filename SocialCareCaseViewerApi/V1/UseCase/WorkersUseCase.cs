using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class WorkersUseCase : IWorkersUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public WorkersUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public WorkerResponse ExecutePost(CreateWorkerRequest createWorkerRequest)
        {
            return _databaseGateway.CreateWorker(createWorkerRequest).ToDomain(true).ToResponse();
        }

        public void ExecutePatch(UpdateWorkerRequest updateWorkerRequest)
        {
            _databaseGateway.UpdateWorker(newWorker);
        }
    }
}
