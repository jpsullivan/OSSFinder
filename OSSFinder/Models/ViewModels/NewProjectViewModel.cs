using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using OSSFinder.Infrastructure.Attributes;

namespace OSSFinder.Models.ViewModels
{
    public class NewProjectViewModel
    {

        public NewProjectViewModel() { }


    }

    public class NewProjectStep1
    {
        [Required]
        [Display(Name = "Repository URL")]
        [Hint("Enter in the repository URL for this project")]
        public Uri RepositoryUrl { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Project Description")]
        public string ProjectDesscription { get; set; }

        public NewProjectStep1() { }
    }
}