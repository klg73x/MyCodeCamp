using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Produces("application/json")]
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    public class SpeakersController : BaseController
    {
        private ICampRepository _repo;
        private ILogger<SpeakersController> _logger;
        private IMapper _mapper;

        public SpeakersController(ICampRepository repo, ILogger<SpeakersController> logger,
            IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(string moniker)
        {
            var speakers = _repo.GetSpeakersByMoniker(moniker);
            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id)
        {
            var speaker = _repo.GetSpeaker(id);
            if (speaker == null) return NotFound();
            if (speaker.Camp.Moniker != moniker) return BadRequest($"Speaker is not from the camp {moniker}");
            return Ok(_mapper.Map<SpeakerModel>(speaker));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = _repo.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest("Could not find camp");
                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;
                _repo.Add(speaker);

                if(await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
                    return Created(newUri, _mapper.Map<SpeakerModel>(speaker));
                }
                else
                {
                    _logger.LogWarning("Could not save Camp to the database.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Exception thrown while adding speaker: {ex}");
            }
            return BadRequest("Could not add new speaker.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerModel model)
        {
            try
            {
                _logger.LogInformation($"Updating the Speaker Record {model.Name}");
                var oldspeaker = _repo.GetSpeaker(id);
                if (oldspeaker == null)
                {
                    return NotFound($"Could not find a speaker with the record id of {id} for camp {moniker}");
                }
                if (oldspeaker.Camp.Moniker != moniker)
                {
                    return BadRequest("That speaker is not from the camp supplied");
                }
                _mapper.Map(model, oldspeaker);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(oldspeaker));
                }
                else
                {
                    _logger.LogWarning("Could not save speaker to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw an exception while saving speaker {ex}");
            }

            return BadRequest("Could not update speaker");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var oldspeaker = _repo.GetSpeaker(id);
                if (oldspeaker == null)
                {
                    return NotFound($"Could not find a speaker with the record id of {id} for camp {moniker}");
                }
                if (oldspeaker.Camp.Moniker != moniker)
                {
                    return BadRequest("That speaker is not from the camp supplied");
                }

                _repo.Delete(oldspeaker);
                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldspeaker);
                }
                else
                {
                    _logger.LogWarning("Could not Delete Speaker from the database.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw an exception while deleting camp {ex}");
            }

            return BadRequest("Could not delete camp.");
        }
    }
}