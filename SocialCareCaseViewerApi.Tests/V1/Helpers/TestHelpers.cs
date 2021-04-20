using System;
using Bogus;
using Bogus.DataSets;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using InfrastructurePerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class TestHelpers
    {
        public static Visit CreateVisit()
        {
            return new Faker<Visit>()
                .RuleFor(v => v.VisitId, f => f.UniqueIndex)
                .RuleFor(v => v.PersonId, f => f.UniqueIndex)
                .RuleFor(v => v.VisitType, f => f.Random.String2(1, 20))
                .RuleFor(v => v.PlannedDateTime, f => f.Date.Past())
                .RuleFor(v => v.ActualDateTime, f => f.Date.Past())
                .RuleFor(v => v.ReasonNotPlanned, f => f.Random.String2(1, 16))
                .RuleFor(v => v.ReasonVisitNotMade, f => f.Random.String2(1, 16))
                .RuleFor(v => v.SeenAloneFlag, f => f.Random.Bool())
                .RuleFor(v => v.CompletedFlag, f => f.Random.Bool())
                .RuleFor(v => v.CpRegistrationId, f => f.UniqueIndex)
                .RuleFor(v => v.CpVisitScheduleStepId, f => f.UniqueIndex)
                .RuleFor(v => v.CpVisitScheduleDays, f => f.Random.Number(999))
                .RuleFor(v => v.CpVisitOnTime, f => f.Random.Bool())
                .RuleFor(v => v.CreatedByEmail, f => f.Person.Email)
                .RuleFor(v => v.CreatedByName, f => f.Person.FullName);
        }

        public static CaseNote CreateCaseNote()
        {
            return new Faker<CaseNote>()
                .RuleFor(c => c.CaseNoteId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.MosaicId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.CreatedOn, f => f.Date.Past())
                .RuleFor(c => c.NoteType, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteContent, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(c => c.CreatedByEmail, f => f.Person.Email)
                .RuleFor(c => c.CreatedByName, f => f.Person.FirstName);
        }

        public static ResidentHistoricRecord CreateResidentHistoricRecord(long? personId = null)
        {
            return new Faker<ResidentHistoricRecord>()
                .RuleFor(r => r.RecordId, f => f.UniqueIndex)
                .RuleFor(r => r.FormName, f => f.Random.String2(50))
                .RuleFor(r => r.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(r => r.FirstName, f => f.Person.FirstName)
                .RuleFor(r => r.LastName, f => f.Person.LastName)
                .RuleFor(r => r.DateOfBirth, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.OfficerEmail, f => f.Person.Email)
                .RuleFor(r => r.CaseFormUrl, f => f.Internet.Url())
                .RuleFor(r => r.CaseFormTimeStamp, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.DateOfEvent, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(r => r.RecordType, f => f.PickRandom<RecordType>())
                .RuleFor(r => r.IsHistoric, true);
        }

        public static ResidentHistoricRecordCaseNote CreateResidentHistoricRecordCaseNote(long? personId = null)
        {
            var caseNote = CreateCaseNote();

            return new Faker<ResidentHistoricRecordCaseNote>()
                .RuleFor(r => r.RecordId, f => f.UniqueIndex)
                .RuleFor(r => r.FormName, f => f.Random.String2(50))
                .RuleFor(r => r.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(r => r.FirstName, f => f.Person.FirstName)
                .RuleFor(r => r.LastName, f => f.Person.LastName)
                .RuleFor(r => r.DateOfBirth, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.OfficerEmail, f => f.Person.Email)
                .RuleFor(r => r.CaseFormUrl, f => f.Internet.Url())
                .RuleFor(r => r.CaseFormTimeStamp, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.DateOfEvent, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.CaseNoteTitle, "Historical Case Note Title")
                .RuleFor(r => r.RecordType, f => f.PickRandom<RecordType>())
                .RuleFor(r => r.IsHistoric, true)
                .RuleFor(r => r.CaseNote, caseNote);
        }

        public static ResidentHistoricRecordVisit CreateResidentHistoricRecordVisit(long? personId = null)
        {
            var visit = CreateVisit();

            return new Faker<ResidentHistoricRecordVisit>()
                .RuleFor(r => r.RecordId, f => f.UniqueIndex)
                .RuleFor(r => r.FormName, f => f.Random.String2(50))
                .RuleFor(r => r.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(r => r.FirstName, f => f.Person.FirstName)
                .RuleFor(r => r.LastName, f => f.Person.LastName)
                .RuleFor(r => r.DateOfBirth, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.OfficerEmail, f => f.Person.Email)
                .RuleFor(r => r.CaseFormUrl, f => f.Internet.Url())
                .RuleFor(r => r.CaseFormTimeStamp, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.DateOfEvent, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(r => r.RecordType, f => f.PickRandom<RecordType>())
                .RuleFor(r => r.IsHistoric, true)
                .RuleFor(r => r.Visit, visit);
        }

        public static (CreateAllocationRequest, Worker, Worker, InfrastructurePerson, Team) CreateAllocationRequest(
            int? mosaicId = null,
            int? teamId = null,
            int? workerId = null,
            string? createdBy = null
            )
        {
            var worker = CreateWorker();
            var createdByWorker = CreateWorker();
            var person = CreatePerson();
            var team = CreateTeam();

            var createAllocationRequest = new Faker<CreateAllocationRequest>()
                .RuleFor(c => c.MosaicId, f => mosaicId ?? person.Id)
                .RuleFor(c => c.AllocatedTeamId, f => teamId ?? team.Id)
                .RuleFor(c => c.AllocatedWorkerId, f => workerId ?? worker.Id)
                .RuleFor(c => c.CreatedBy, f => createdBy ?? createdByWorker.Email)
                .RuleFor(c => c.AllocationStartDate, DateTime.Now);

            return (createAllocationRequest, worker, createdByWorker, person, team);
        }

        public static (UpdateAllocationRequest, Worker, Worker, InfrastructurePerson, Team) CreateUpdateAllocationRequest(
            int? id = null,
            string? deallocationReason = null,
            string? createdBy = null,
            DateTime? deallocationDate = null
            )
        {
            var worker = CreateWorker();
            var updatedByWorker = CreateWorker();
            var person = CreatePerson();
            var team = CreateTeam();

            var updateAllocationRequest = new Faker<UpdateAllocationRequest>()
                .RuleFor(u => u.Id, f => id ?? f.UniqueIndex + 1)
                .RuleFor(u => u.DeallocationReason, f => deallocationReason ?? f.Random.String2(200))
                .RuleFor(u => u.CreatedBy, createdBy ?? updatedByWorker.Email)
                .RuleFor(u => u.DeallocationDate, f => deallocationDate ?? f.Date.Recent());

            return (updateAllocationRequest, worker, updatedByWorker, person, team);
        }

        private static Worker CreateWorker(int? workerId = null)
        {
            return new Faker<Worker>()
                .RuleFor(w => w.Id, f => workerId ?? f.UniqueIndex + 1)
                .RuleFor(w => w.Email, f => f.Person.Email)
                .RuleFor(w => w.FirstName, f => f.Person.FirstName)
                .RuleFor(w => w.LastName, f => f.Person.LastName);
        }

        private static InfrastructurePerson CreatePerson(int? personId = null)
        {
            return new Faker<InfrastructurePerson>()
                .RuleFor(p => p.Id, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.FirstName)
                .RuleFor(p => p.FullName, f => f.Person.FullName)
                .RuleFor(p => p.EmailAddress, f => f.Person.Email);
        }

        private static Team CreateTeam(int? teamId = null)
        {
            return new Faker<Team>()
                .RuleFor(t => t.Id, f => teamId ?? f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => f.Random.String2(1))
                .RuleFor(t => t.Name, f => f.Random.String2(1, 200));
        }
    }
}
