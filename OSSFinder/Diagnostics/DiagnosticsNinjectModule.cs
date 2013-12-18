using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject.Modules;

namespace OSSFinder.Diagnostics
{
    public class DiagnosticsNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDiagnosticsService>()
                .To<DiagnosticsService>()
                .InSingletonScope();
        }
    }
}