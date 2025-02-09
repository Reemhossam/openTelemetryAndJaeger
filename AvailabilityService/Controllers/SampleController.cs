using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityService.Controllers
{
  [Route("api/sample")]
  [ApiController]
  public class SampleController(MyService _myService) : ControllerBase
  {
    [HttpPost]
    public async Task TestLog()
    {
      _myService.Execute();
    }
  }
}
