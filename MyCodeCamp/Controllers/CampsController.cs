using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [EnableCors("AnyGET")] //This allows any user from any domain to execute the GETs in this controller
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
    {
        private ICampRepository _repo;
        private ILogger<CampsController> _logger;
        private IMapper _mapper;

        public CampsController(ICampRepository repo, ILogger<CampsController> logger,
            IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                {
                    camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                }
                else
                {
                    camp = _repo.GetCampByMoniker(moniker);
                }             

                if (camp == null) return NotFound($"Camp {moniker} was not found");

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
            }
            return BadRequest();
        }

        [EnableCors("Wildermuth")] //This allows people from wildermuth.com to use this post
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CampModel model)
        {
            try
            {            
                _logger.LogInformation("Creating a new Code Camp");
                var camp = _mapper.Map<Camp>(model);
                _repo.Add(camp);               
                if (await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { moniker = camp.Moniker });
                    return Created(newUri, _mapper.Map<CampModel>(camp));
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

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody] CampModel model)
        {
            try
            {
                _logger.LogInformation($"Updating the Code Camp Record {model.Moniker}");
                var oldcamp = _repo.GetCampByMoniker(moniker);
                if (oldcamp == null)
                {
                    return NotFound($"Could not find a camp with the record id of {moniker}");
                }

                _mapper.Map(model, oldcamp);
                
                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldcamp));
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

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldcamp = _repo.GetCampByMoniker(moniker);
                if (oldcamp == null)
                {
                    return NotFound($"Could not find a camp with the record id of {moniker}");
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
