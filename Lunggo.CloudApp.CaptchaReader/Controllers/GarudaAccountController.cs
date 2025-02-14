﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Lunggo.CloudApp.CaptchaReader.Models;

namespace Lunggo.CloudApp.CaptchaReader.Controllers
{
    public class GarudaAccountController : ApiController
    {
        [HttpGet]
        public string ChooseUserId()
        {

            return AccountGaruda.GetUserId();
        }

        [HttpGet]
        public bool LogOut(string userId)
        {
            return AccountGaruda.LogOutAck(userId);
        }
    }
}
