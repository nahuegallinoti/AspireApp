namespace AspireApp.Tools.Generator.Generator.Mutators;

internal interface IFileMutator
{
    string TargetPath { get; }
    MutationResult Mutate(string source, EntitySpec entity);
}

internal sealed record MutationResult(string NewContent, bool Changed, string Description);
