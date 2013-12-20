using System;
using System.Web.Mvc;
using OSSFinder.Configuration;
using OSSFinder.Controllers;
using OSSFinder.Core.Entities;

namespace OSSFinder.Views
{
    public abstract class ViewBase : WebViewPage
    {
        private readonly Lazy<OSSFinderContext> _nugetContext;

        public OSSFinderContext OssFinderContext
        {
            get { return _nugetContext.Value; }
        }

        public ConfigurationService Config
        {
            get { return OssFinderContext.Config; }
        }

        public User CurrentUser
        {
            get { return OssFinderContext.CurrentUser; }
        }

        protected ViewBase()
        {
            _nugetContext = new Lazy<OSSFinderContext>(GetContextThunk(this));
        }

        internal static Func<OSSFinderContext> GetContextThunk(WebViewPage self)
        {
            return () =>
            {
                var ctrl = self.ViewContext.Controller as AppController;
                if (ctrl == null)
                {
                    throw new InvalidOperationException("Viewbase should only be used on views for actions on AppControllers");
                }
                return ctrl.OssFinderContext;
            };
        }
    }

    public abstract class ViewBase<T> : WebViewPage<T>
    {
        private Lazy<OSSFinderContext> _nugetContext;

        public OSSFinderContext OssFinderContext
        {
            get { return _nugetContext.Value; }
        }

        public ConfigurationService Config
        {
            get { return OssFinderContext.Config; }
        }

        public User CurrentUser
        {
            get { return OssFinderContext.CurrentUser; }
        }

        protected ViewBase()
        {
            _nugetContext = new Lazy<OSSFinderContext>(ViewBase.GetContextThunk(this));
        }
    }
}