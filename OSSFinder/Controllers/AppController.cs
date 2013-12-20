﻿using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Ninject;
using OSSFinder.App_Start;
using OSSFinder.Authentication;
using System.Net;
using OSSFinder.Configuration;
using OSSFinder.Core.Entities;
using OSSFinder.Infrastructure;
using OSSFinder.Infrastructure.Extensions;

namespace OSSFinder.Controllers
{
    public abstract partial class AppController : Controller
    {
        private IOwinContext _overrideContext;

        public IOwinContext OwinContext
        {
            get { return _overrideContext ?? HttpContext.GetOwinContext(); }
            set { _overrideContext = value; }
        }

        public OSSFinderContext OssFinderContext { get; private set; }

        public new ClaimsPrincipal User
        {
            get { return base.User as ClaimsPrincipal; }
        }

        protected AppController()
        {
            OssFinderContext = new OSSFinderContext(this);
        }

        protected internal virtual T GetService<T>()
        {
            return DependencyResolver.Current.GetService<T>();
        }

        protected internal User GetCurrentUser()
        {
            return OwinContext.GetCurrentUser();
        }

        protected internal virtual ActionResult SafeRedirect(string returnUrl)
        {
            return new SafeRedirectResult(returnUrl, Url.Home());
        }
    }

    public class OSSFinderContext
    {
        private Lazy<User> _currentUser;

        public ConfigurationService Config { get; private set; }
        public User CurrentUser { get { return _currentUser.Value; } }

        public OSSFinderContext(AppController ctrl)
        {
            Config = Container.Kernel.TryGet<ConfigurationService>();

            _currentUser = new Lazy<User>(() =>
                ctrl.OwinContext.GetCurrentUser());
        }
    }
}