using Nancy;
using ReportService.Entities.ServiceSettings;

namespace ReportService.Nancy.Modules
{
    public enum ApiUserRole
    {
        Viewer,
        Editor,
        StopRunner, //executor?
        NoRole
    }

    public abstract class NancyBaseModule : NancyModule
    {
        protected NancyBaseModule(ServiceConfiguration config)
        {
            ViewPermission = config.PermissionsSettings.Permissions_View;
            EditPermission = config.PermissionsSettings.Permissions_Edit;
            StopRunPermission = config.PermissionsSettings.Permissions_StopRun;
        }

        protected string PermissionsType = "permissions";
        protected string ViewPermission;
        protected string EditPermission;
        protected string StopRunPermission;
    }
}