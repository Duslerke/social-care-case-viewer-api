using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class UpdateWorkerDetails : IntegrationTestSetup<Startup>
    {
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _allocationWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _existingDbTeam;
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _differentDbTeam;
        private SocialCareCaseViewerApi.V1.Infrastructure.Person _resident;

        [SetUp]
        public void Setup()
        {
            // Clear test database of any rows in the database
            DatabaseContext.Teams.RemoveRange(DatabaseContext.Teams);
            DatabaseContext.Workers.RemoveRange(DatabaseContext.Workers);
            DatabaseContext.WorkerTeams.RemoveRange(DatabaseContext.WorkerTeams);
            DatabaseContext.Persons.RemoveRange(DatabaseContext.Persons);
            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);

            // Create existing workers with teams
            var (existingDbWorker, existingDbTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
            var (allocationWorker, _) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);

            var differentDbTeam = IntegrationTestHelpers.CreateTeam(DatabaseContext, existingDbWorker.ContextFlag);

            // Create an existing resident that shares the same age context as existingDbWorker
            var resident = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext, ageContext: existingDbWorker.ContextFlag);

            _existingDbWorker = existingDbWorker;
            _existingDbTeam = existingDbTeam;
            _allocationWorker = allocationWorker;
            _differentDbTeam = differentDbTeam;
            _resident = resident;
        }


        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            // Patch request to update team of existingDbWorker
            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var newTeamRequest = new WorkerTeamRequest { Id = _differentDbTeam.Id, Name = _differentDbTeam.Name };
            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(_existingDbWorker, newTeamRequest);
            var serializedRequest = JsonSerializer.Serialize(patchRequest);

            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            var patchWorkerResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated
            var getUri = new Uri($"/api/v1/workers?email={_existingDbWorker.Email}", UriKind.Relative);
            var getUpdatedWorkersResponse = await Client.GetAsync(getUri).ConfigureAwait(true);
            getUpdatedWorkersResponse.StatusCode.Should().Be(200);

            var updatedContent = await getUpdatedWorkersResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedWorkerResponse = JsonConvert.DeserializeObject<List<WorkerResponse>>(updatedContent).ToList();
            updatedWorkerResponse.Count.Should().Be(1);

            // Worker's initial team should be replaced with the new team
            updatedWorkerResponse.Single().Teams.Count.Should().Be(1);
            updatedWorkerResponse.Single().Teams.Single().Id.Should().Be(newTeamRequest.Id);
            updatedWorkerResponse.Single().Teams.Single().Name.Should().Be(newTeamRequest.Name);

            // Check the db state as well
            var persistedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId.Equals(_existingDbWorker.Id)).ToList();
            persistedWorkerTeams.Count.Should().Be(1);
            persistedWorkerTeams.Single().Team.Id.Should().Be(newTeamRequest.Id);
            persistedWorkerTeams.Single().Team.Name.Should().Be(newTeamRequest.Name);
        }

        [Test]
        public async Task UpdateWorkerWithNewTeamUpdatesAnyAllocationsAssociated()
        {
            // Create an allocation request for existingDbWorker
            var createAllocationUri = new Uri("/api/v1/allocations", UriKind.Relative);

            var allocationRequest = IntegrationTestHelpers.CreateAllocationRequest(_resident.Id, _existingDbTeam.Id, _existingDbWorker.Id, _allocationWorker);
            var serializedRequest = JsonSerializer.Serialize(allocationRequest);

            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var allocationResponse = await Client.PostAsync(createAllocationUri, requestContent).ConfigureAwait(true);
            allocationResponse.StatusCode.Should().Be(201);

            // Create another allocation request for existingDbWorker
            var secondAllocationRequest = IntegrationTestHelpers.CreateAllocationRequest(_resident.Id, _existingDbTeam.Id, _existingDbWorker.Id, _allocationWorker);
            var secondSerializedRequest = JsonSerializer.Serialize(secondAllocationRequest);

            var secondRequestContent = new StringContent(secondSerializedRequest, Encoding.UTF8, "application/json");

            var allocationTwoResponse = await Client.PostAsync(createAllocationUri, secondRequestContent).ConfigureAwait(true);
            allocationTwoResponse.StatusCode.Should().Be(201);

            // Patch request to update team of existingDbWorker
            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var newTeamRequest = new WorkerTeamRequest { Id = _differentDbTeam.Id, Name = _differentDbTeam.Name };
            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(_existingDbWorker, newTeamRequest);
            var patchTeamSerializedRequest = JsonSerializer.Serialize(patchRequest);

            var patchRequestContent = new StringContent(patchTeamSerializedRequest, Encoding.UTF8, "application/json");
            var patchWorkerResponse = await Client.PatchAsync(patchUri, patchRequestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated on existingDbWorker's allocations
            var getAllocationsUri = new Uri($"/api/v1/allocations?mosaic_id={_resident.Id}", UriKind.Relative);
            var getAllocationsResponse = await Client.GetAsync(getAllocationsUri).ConfigureAwait(true);
            getAllocationsResponse.StatusCode.Should().Be(200);

            var allocationsContent = await getAllocationsResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedAllocationResponse = JsonConvert.DeserializeObject<AllocationList>(allocationsContent);

            updatedAllocationResponse.Allocations.Count.Should().Be(2);

            var firstAllocation = updatedAllocationResponse.Allocations.ElementAtOrDefault(0);

            firstAllocation?.AllocatedWorkerTeam.Should().Be(newTeamRequest.Name);
            firstAllocation?.PersonId.Should().Be(_resident.Id);
            firstAllocation?.AllocatedWorker.Should().Be($"{_existingDbWorker.FirstName} {_existingDbWorker.LastName}");

            var secondAllocation = updatedAllocationResponse.Allocations.ElementAtOrDefault(1);

            secondAllocation?.AllocatedWorkerTeam.Should().Be(newTeamRequest.Name);
            secondAllocation?.PersonId.Should().Be(_resident.Id);
            secondAllocation?.AllocatedWorker.Should().Be($"{_existingDbWorker.FirstName} {_existingDbWorker.LastName}");
        }
    }
}
