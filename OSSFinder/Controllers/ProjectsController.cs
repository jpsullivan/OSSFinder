using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSSFinder.Configuration;
using OSSFinder.Entities;
using OSSFinder.Infrastructure.Attributes;
using OSSFinder.Models.ViewModels;
using OSSFinder.Services.Interfaces;

namespace OSSFinder.Controllers
{
    public partial class ProjectsController : AppController
    {
        private readonly IAppConfiguration _config;
        private readonly IMessageService _messageService;
        private readonly IProjectService _projectService;
        private readonly IEntitiesContext _entitiesContext;
        private readonly ICacheService _cacheService;

        public ProjectsController(
            IProjectService projectService,
            IMessageService messageService,
            IEntitiesContext entitiesContext,
            IAppConfiguration config,
            ICacheService cacheService) 
        {
            _projectService = projectService;
            _messageService = messageService;
            _entitiesContext = entitiesContext;
            _config = config;
            _cacheService = cacheService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [Route("project/new")]
        public virtual ActionResult New() 
        {
            return View("New_Step1", new NewProjectStep1());
        }

        [Authorize]
        [Route("project/process_step_1")]
        public virtual ActionResult ProcessStep1(NewProjectStep1 model)
        {
            return new EmptyResult();
        }
    }
}
