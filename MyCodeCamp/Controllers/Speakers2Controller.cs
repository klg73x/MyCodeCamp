using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Produces("application/json")]
    [Route("api/camps/{moniker}/speakers")]
    [ApiVersion("2.0")]
    public class Speakers2Controller : SpeakersController
    {
        public Speakers2Controller(ICampRepository repo, ILogger<SpeakersController> logger,
            IMapper mapper, UserManager<CampUser> userMgr) : base(repo, logger, mapper, userMgr)
        {

        }

        public override IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);
            return Ok(new
            {
                currentTime = DateTime.UtcNow,
                count = speakers.Count(),
                results = _mapper.Map<IEnumerable<SpeakerModel>>(speakers)
            });

        }
    }
}