namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class MemberBinding : Expression
  {
    public MemberBinding()
      : base (NodeType.MemberBinding)
    {
    }

    public MemberBinding(Expression targetObject, Member boundMember) : base (NodeType.MemberBinding)
    {
      BoundMember = boundMember;
      TargetObject = targetObject;
      switch (boundMember.NodeType) {
        case NodeType.Method:
          Type = ((Method) boundMember).ReturnType;
          break;
        default:
          Type = boundMember as TypeNode;
          break;
      }
    }

    public Member BoundMember { get; set; }
    public Expression TargetObject { get; set; }

    public override string ToString()
    {
      return string.Format ("MemberBinding({0}.{1})", TargetObject == null ? "<no target>" : TargetObject.ToString (), BoundMember);
    }
  }
}