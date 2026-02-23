namespace ContractForge.Core;

public interface ICodeGen
{
    public CodeGenResult Build(DefinitionState state);
}