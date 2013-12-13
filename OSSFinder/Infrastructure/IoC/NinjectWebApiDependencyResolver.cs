using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Ninject;
using Ninject.Syntax;

namespace OSSFinder.Infrastructure.IoC
{
    public class NinjectWebApiDependencyResolver : NinjectWebApiDependencyScope, IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectWebApiDependencyResolver(IKernel kernel)
            : base(kernel)
        {
            _kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectWebApiDependencyScope(_kernel);
        }
    }

    public class NinjectWebApiDependencyScope : IDependencyScope
    {
        private readonly IResolutionRoot _root;

        public NinjectWebApiDependencyScope(IResolutionRoot root)
        {
            _root = root;
        }

        public object GetService(Type serviceType)
        {
            return _root.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _root.GetAll(serviceType);
        }

        public void Dispose() { }
    }
}