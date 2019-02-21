using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public interface ISessionPatchService
    {
        string Patch(string sessionJson, SessionPatch sessionPatch);
    }
}
