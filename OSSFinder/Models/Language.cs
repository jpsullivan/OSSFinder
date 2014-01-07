using System.ComponentModel;

namespace OSSFinder.Models
{
    public enum Language
    {
        [Description("Not Specified")]
        NotSpecified = 0,

        [Description("C")]
        C,

        [Description("C#")]
        CSharp,

        [Description("C++")]
        Cpp,

        [Description("F#")]
        FSharp,

        Java,

        JavaScript,

        [Description("Objective-C")]
        ObjectiveC,

        Perl,

        PHP,

        Python,

        Ruby,

        Scala,

        SQL
    }
}