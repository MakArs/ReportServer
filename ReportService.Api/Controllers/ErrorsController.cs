using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Monik.Common;

namespace ReportService.Api.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : BaseController
    {
        private readonly IMonik mMonik;

        public ErrorsController(IMonik monik)
        {
            this.mMonik = monik;
        }
        
        [Route("error")]
        public void Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;
        
            mMonik.ApplicationError(exception.Message);
            mMonik.ApplicationError(exception.StackTrace);
        }
    }
}
