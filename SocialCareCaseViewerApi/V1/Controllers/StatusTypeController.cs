using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]

    public class StatusTypeController : BaseController
    {
        private readonly ICaseStatusesUseCase _caseStatusesUseCase;

        public StatusTypeController(ICaseStatusesUseCase caseStatusUseCase)
        {
            _caseStatusesUseCase = caseStatusUseCase;
        }

        /// <summary>
        /// Get a list of case statuses by person id
        /// </summary>
        /// <response code="200">Successful request. Case statuses returned</response>
        /// <response code="404">Case status not found</response>
        [ProducesResponseType(typeof(ListRelationshipsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId:long}/casestatuses")]
        public IActionResult ListCaseStatuses(long personId,
            [FromQuery(Name = "start_date")] string? startDate,
            [FromQuery(Name = "end_date")] string? endDate,
            [FromQuery(Name = "date")] string? statusDate)
        {
            try
            {
                if (startDate == null && endDate == null)
                {
                    startDate = statusDate;
                    endDate = statusDate;
                }

                return Ok(_caseStatusesUseCase.ExecuteGet(personId, startDate, endDate));
            }
            catch (GetCaseStatusesException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
