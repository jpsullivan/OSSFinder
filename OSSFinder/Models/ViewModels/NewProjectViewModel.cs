using System.ComponentModel.DataAnnotations;
using OSSFinder.Infrastructure.Attributes;

namespace OSSFinder.Models.ViewModels
{
    public class NewProjectViewModel
    {
        public NewProjectStep1 Step1 { get; set; }

        public NewProjectViewModel() { }
    }

    public class NewProjectStep1
    {
        [Required]
        [Display(Name = "Repository URL")]
        [Hint("Enter in the repository URL for this project")]
        public string RepositoryUrl { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        [Display(Name = "Project Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Language")]
        public string Language { get; set; }

        public NewProjectStep1() { }

        public NewProjectStep1(string repositoryUrl, string name, string desc)
        {
            RepositoryUrl = repositoryUrl;
            Name = name;
            Description = desc;
        }
    }

    public class NewProjectStep2
    {
        
    }
}