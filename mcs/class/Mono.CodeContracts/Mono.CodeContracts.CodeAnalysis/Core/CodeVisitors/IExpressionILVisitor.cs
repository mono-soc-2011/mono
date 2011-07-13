using Mono.CodeContracts.CodeAnalysis.Core.AST;

namespace Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors
{
  public interface IExpressionILVisitor<Label, Type, Source, Dest, Data, Result>
  {
    Result Binary(Label pc, BinaryOperator op, Dest dest, Source operand1, Source operand2, Data data);
    Result Isinst(Label pc, Type type, Dest dest, Source obj, Data data);

    Result LoadNull(Label pc, Dest dest, Data data);
    Result LoadConst(Label pc, Type type, object constant, Dest dest, Data data);

    Result Sizeof(Label pc, Type type, Dest dest, Data data);

    Result Unary(Label pc, UnaryOperator op, bool unsigned, Dest dest, Source source, Data data);
  }
}