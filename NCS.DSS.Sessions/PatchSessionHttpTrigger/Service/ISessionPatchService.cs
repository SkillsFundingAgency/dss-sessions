﻿using System;
using System.Collections.Generic;
using System.Text;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.PatchSessionHttpTrigger.Service
{
    public interface ISessionPatchService
    {
        Session Patch(string sessionJson, SessionPatch sessionPatch);
    }
}
