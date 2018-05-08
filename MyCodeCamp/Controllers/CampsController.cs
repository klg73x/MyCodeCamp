using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        private ICampRepository _repo;
        private ILogger<CampsController> _logger;

        public CampsController(ICampRepository repo, ILogger<CampsController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(camps);
        }

        [HttpGet("{id}", Name = "CampGet")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                {
                    camp = _repo.GetCampWithSpeakers(id);
                }
                else
                {
                    camp = _repo.GetCamp(id);
                }             

                if (camp == null) return NotFound($"Camp {id} was not found");

                return Ok(camp);
            }
            catch
            {
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Camp model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");
                _repo.Add(model);               
                if (await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { id = model.Id });
                    return Created(newUri, model);
                }
                else
                {
                    _logger.LogWarning("Could not save Camp to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw an exception while saving camp {ex}");
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Camp model)
        {
            try
            {
                _logger.LogInformation($"Updating the Code Camp Record {model.Id}");
                var oldcamp = _repo.GetCamp(id);
                if (oldcamp == null)
                {
                    return NotFound($"Could not find a camp with the record id of {id}");
                }

                //Map model to oldCamp
                oldcamp.Name = model.Name ?? oldcamp.Name;
                oldcamp.Description = model.Description ?? oldcamp.Description;
                oldcamp.Location = model.Location ?? oldcamp.Location;
                oldcamp.Length = model.Length > 0 ? model.Length : oldcamp.Length;
                oldcamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldcamp.EventDate;

                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldcamp);
                }
                else
                {
                    _logger.LogWarning("Could not save Camp to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw an exception while saving camp {ex}");
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var oldcamp = _repo.GetCamp(id);
                if (oldcamp == null)
                {
                    return NotFound($"Could not find a camp with the record id of {id}");
                }

                _repo.Delete(oldcamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldcamp);
                }
                else
                {
                    _logger.LogWarning("Could not Delete Camp to the database.");
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
