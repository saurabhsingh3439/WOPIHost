﻿/*
Copyright Mitratech Holdings Inc, 2021
This software is provided under the terms of a License Agreement and may
only be used and/or copied in accordance with the terms of such agreement.
Neither this software nor any copy thereof may be provided or otherwise
made available to any other person. No title or ownership of this software
is hereby transferred.
*/

using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Response;
using System.Net;

namespace MS_WOPI.Interfaces
{
    public interface IStatusValidator
    {
        void ReturnStatus(HttpListenerResponse response, int code, string description);
    }
}
