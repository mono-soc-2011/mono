using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Property : Member
  {
    private readonly PropertyDefinition property_definition;

    public Property(PropertyDefinition propertyDefinition) : base (NodeType.Property)
    {
      this.property_definition = propertyDefinition;
    }

    #region Overrides of Member
    public override bool IsStatic
    {
      get { return false; }
    }
    #endregion
  }
}