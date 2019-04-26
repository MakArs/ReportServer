using System.Configuration;
using Nancy;

namespace ReportService.Nancy.Modules
{
    public enum ApiUserRole
    {
        Viewer,
        Editor,
        StopRunner,//executor?
        NoRole
    }

    public abstract class NancyBaseModule : NancyModule
    {
        protected string PermissionsType = "permissions";
        protected string ViewPermission = ConfigurationManager.AppSettings["Permissions_View"];
        protected string EditPermission = ConfigurationManager.AppSettings["Permissions_Edit"];
        protected string StopRunPermission = ConfigurationManager.AppSettings["Permissions_StopRun"];
    }
}