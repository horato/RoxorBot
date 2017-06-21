using System;

namespace RoxorBot.Data.Attributes
{
    public class ChangescriptAttribute : Attribute
    {
        public int SourceVersion { get; }
        public int TargetVersion { get; }
        public string Description { get; }

        public ChangescriptAttribute(int sourceVersion, int targetVersion, string description)
        {
            SourceVersion = sourceVersion;
            TargetVersion = targetVersion;
            Description = description;
        }
    }
}
