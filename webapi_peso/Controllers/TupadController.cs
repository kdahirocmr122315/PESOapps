using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using webapi_peso.Dbcontext;
using static webapi_peso.Model.Tbl_tupadbeneficiary;

namespace webapi_peso.Controllers
{
    [ApiController]
    [Route("Api/[controller]")]
    [Authorize]
        public class TupadController : ControllerBase
        {
                private readonly TupadRepository _TupadRepository;

                   public TupadController(TupadRepository tupadRepository)
                    {
                          _TupadRepository = tupadRepository;

                    }

                    [HttpGet("{id}")]

                    public ActionResult<Beneficiary> Get(int id)
                    {
                        var user = _TupadRepository.GetbeneficiaryById(id);
                        if (user == null)
                        {
                            return NotFound();
                        }
                        return Ok(user);
                    }


                        [HttpGet("getall/")]
                       public IEnumerable<Beneficiary> Get()
                        {
                        var list = _TupadRepository.GetAllBeneficiary();
                        return list;
                        }


                    [HttpPost]
                    public ActionResult Post([FromBody] Beneficiary Beneficiary)
                    {
                        _TupadRepository.Addbeneficiary(Beneficiary);
                        return CreatedAtAction(nameof(Get), new { id = Beneficiary.ID }, Beneficiary);
                    }

                    [HttpPut("{id}")]
                    public ActionResult Put(int id, [FromBody] Beneficiary beneficiary)
                        {
                             beneficiary.ID = id;
                            _TupadRepository.UpdateUser(beneficiary);
                            return NoContent();
                        }

                    [HttpDelete("{id}")]
                    public ActionResult Delete(int id)
                             {
                            _TupadRepository.Deletebeneficiary(id);
                            return NoContent();
                            }

                    [HttpGet("verified/")]
                    public IEnumerable<Beneficiary> GetVerifiedBeneficiaries()
                    {
                        var list = _TupadRepository.VerifiedBeneficiary();
                        return list;
                    }

                    [HttpGet("unverified/")]
                    public IEnumerable<Beneficiary> GetUnverifiedBeneficiaries()
                    {
                        var list = _TupadRepository.UnverifiedBeneficiary();
                        return list;
                    }

    }

}

