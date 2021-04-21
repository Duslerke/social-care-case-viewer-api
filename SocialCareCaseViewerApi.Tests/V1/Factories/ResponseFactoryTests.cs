using System;
using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "true");
        }

        [Test]
        public void CanMapResidentAndAddressFromDomainToResponse()
        {
            var caseNote = TestHelpers.CreateCaseNote();

            var person = TestHelpers.CreatePerson();
            var address = TestHelpers.CreateAddress(person.Id);

            var phoneNumber1 = TestHelpers.CreatePhoneNumber(person.Id);
            var phoneNumber2 = TestHelpers.CreatePhoneNumber(person.Id);

            var otherName1 = TestHelpers.CreatePersonOtherName(person.Id);
            var otherName2 = TestHelpers.CreatePersonOtherName(person.Id);

            var names = new List<PersonOtherName>
            {
                otherName1,
                otherName2
            };

            var phoneNumbers = new List<PhoneNumber>()
            {
                phoneNumber1,
                phoneNumber2
            };

            var residentResponse = TestHelpers.CreateAddNewResidentResponse(person.Id);

            var expectedResponse = new AddNewResidentResponse
            {
                PersonId = person.Id,
                AddressId = address.AddressId,
                OtherNameIds = new List<int>() { otherName1.Id, otherName2.Id },
                PhoneNumberIds = new List<int> { phoneNumber1.Id, phoneNumber2.Id },
                CaseNoteId = caseNote.CaseNoteId,
                CaseNoteErrorMessage = residentResponse.CaseNoteErrorMessage
            };

            person.ToResponse(address, names, phoneNumbers, caseNote.CaseNoteId, residentResponse.CaseNoteErrorMessage).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsCaseNoteMappedToDomain()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", historicalCaseNote.CaseNoteId),
                    new BsonElement("mosaic_id", historicalCaseNote.MosaicId),
                    new BsonElement("worker_email", historicalCaseNote.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Case_Note"),
                    new BsonElement("form_name", historicalCaseNote.NoteType),
                    new BsonElement("title", historicalCaseNote.CaseNoteTitle),
                    new BsonElement("timestamp", historicalCaseNote.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsNoteTypeForFormNameIfFeatureFlagIsOn()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "true");
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.NoteType);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsTitleForFormNameIfFeatureFlagIsFalse()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "false");
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.CaseNoteTitle);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsTitleForFormNameIfFeatureFlagIsAnEmptyString()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "");
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.CaseNoteTitle);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsTitleForFormNameIfFeatureFlagIsNull()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", null);
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.CaseNoteTitle);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsCaseNoteAsAStringWhenNoteTypeIsNull()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = null;

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case note");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsCaseNoteAsAStringWhenNoteTypeIsAnEmptyString()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case note");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsFormNameWithoutBracketsAndWhitespaceWhenNoteTypeHasASCInBrackets()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Case Summary (ASC)");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case Summary");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsFormNameWithoutBracketsAndWhitespaceWhenNoteTypeHasYOTInBrackets()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Home Visit (YOT)");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Home Visit");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsFormNameWithoutBracketsAndWhitespaceWhenNoteTypeHasYHInBrackets()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Manager's Decisions (YH)");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Manager's Decisions");
        }

        [Test]
        public void CanMapVisitToBsonDocument()
        {
            var visit = TestHelpers.CreateVisit();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", visit.VisitId.ToString()),
                    new BsonElement("mosaic_id", visit.PersonId.ToString()),
                    new BsonElement("worker_email", visit.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Visit"),
                    new BsonElement("form_name", $"Historical Visit - {visit.VisitType}"),
                    new BsonElement("timestamp", visit.ActualDateTime?.ToString("dd/MM/yyyy H:mm:ss") ??
                                                 visit.PlannedDateTime?.ToString("dd/MM/yyyy H:mm:ss")),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalVisitsToDomain(visit);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void ToResponseForCaseNoteReturnsCaseNoteMappedToCaseNoteResponse()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(createdOn: new DateTime(2021, 3, 1, 15, 30, 0));
            var expectedCaseNoteResponse = TestHelpers.CreateCaseNoteResponse(historicalCaseNote);

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedCaseNoteResponse);
        }

        [Test]
        public void ToResponseForCaseNoteReturnsCaseNoteAsAStringForFormNameWhenNoteTypeIsNull()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = null;

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.FormName.Should().Be("Case note");
        }

        [Test]
        public void ToResponseForCaseNoteReturnsCaseNoteAsAStringForFormNameWhenNoteTypeIsAnEmptyString()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "");

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.FormName.Should().Be("Case note");
        }

        [Test]
        public void ToResponseForCaseNoteReturnsNoteTypeWithoutBracketsAndWhitespaceForFormName()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Manager's Decisions (YH)");

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.FormName.Should().Be("Manager's Decisions");
        }
    }
}
