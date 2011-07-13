using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers.Implementation
{
  public class CodeProviderImpl
    : IMethodCodeProvider<CodeProviderImpl.PC, Local, Parameter, Method, FieldReference, TypeNode, ExceptionHandler>
  {
    public static readonly CodeProviderImpl Instance = new CodeProviderImpl ();

    #region ICodeProvider<PC,Local,Parameter,MethodReference,FieldReference,TypeReference> Members
    public Result Decode<AggregateVisitor, Data, Result>(PC pc, AggregateVisitor visitor, Data data)
      where AggregateVisitor : IAggregateVisitor<PC, Local, Parameter, Method, FieldReference, TypeNode, Data, Result>
    {
      Node nested;
      Node node = Decode (pc, out nested);
      if (IsAtomicNested (nested))
        node = nested;
      else if (nested != null)
        return visitor.Aggregate (pc, new PC (nested), nested is Block, data);

      if (node == null)
        return visitor.Nop (pc, data);

      switch (node.NodeType) {
        case NodeType.Block:
        case NodeType.Nop:
          return visitor.Nop (pc, data);
        case NodeType.Clt:
        case NodeType.Lt:
          return visitor.Binary (pc, BinaryOperator.Clt, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Cgt:
        case NodeType.Gt:
          return visitor.Binary (pc, BinaryOperator.Cgt, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Ceq:
        case NodeType.Eq:
          return visitor.Binary (pc, BinaryOperator.Ceq, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Ne:
          return visitor.Binary (pc, BinaryOperator.Cne_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Ge:
          return visitor.Binary (pc, BinaryOperator.Cge, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Le:
          return visitor.Binary (pc, BinaryOperator.Cle, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Add:
          return visitor.Binary (pc, BinaryOperator.Add, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Sub:
          return visitor.Binary (pc, BinaryOperator.Sub, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Rem:
          return visitor.Binary (pc, BinaryOperator.Rem, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Rem_Un:
          return visitor.Binary (pc, BinaryOperator.Rem_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Mul:
          return visitor.Binary (pc, BinaryOperator.Mul, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Div:
          return visitor.Binary (pc, BinaryOperator.Div, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Div_Un:
          return visitor.Binary (pc, BinaryOperator.Div_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.And:
          return visitor.Binary (pc, BinaryOperator.And, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Or:
          return visitor.Binary (pc, BinaryOperator.Or, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Shr:
          return visitor.Binary (pc, BinaryOperator.Shr, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Xor:
          return visitor.Binary (pc, BinaryOperator.Xor, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Shl:
          return visitor.Binary (pc, BinaryOperator.Shl, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Shr_Un:
          return visitor.Binary (pc, BinaryOperator.Shr_Un, Dummy.Value, Dummy.Value, Dummy.Value, data);
        case NodeType.Literal:
          var literal = (Literal) node;
          if (literal.Value == null)
            return visitor.LoadNull (pc, Dummy.Value, data);
          if (literal.Type == CoreSystemTypes.Instance.TypeBoolean && (bool) literal.Value)
            return visitor.LoadConst (pc, CoreSystemTypes.Instance.TypeInt32, 1, Dummy.Value, data);
          
          return visitor.LoadConst (pc, literal.Type, literal.Value, Dummy.Value, data);
        case NodeType.Local:
          return visitor.LoadLocal (pc, (Local) node, Dummy.Value, data);
        case NodeType.Parameter:
          return visitor.LoadArg (pc, (Parameter) node, false, Dummy.Value, data);
        case NodeType.Branch:
          Branch branch = (Branch) node;
          if (branch.Condition != null)
            return visitor.BranchTrue (pc, new PC (branch.Target), Dummy.Value, data);
          return visitor.Branch (pc, new PC (branch.Target), branch.LeavesExceptionBlock, data);
        case NodeType.ExpressionStatement:
          break;
        case NodeType.Box:
          break;
        case NodeType.Return:
          return visitor.Return (pc, Dummy.Value, data);
        case NodeType.Neg:
          return visitor.Unary (pc, UnaryOperator.Neg, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Not:
        case NodeType.LogicalNot:
          return visitor.Unary (pc, UnaryOperator.Not, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Conv:
          break;
        case NodeType.Conv_I1:
          return visitor.Unary (pc, UnaryOperator.Conv_i1, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Conv_I2:
          return visitor.Unary(pc, UnaryOperator.Conv_i2, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Conv_I4:
          return visitor.Unary(pc, UnaryOperator.Conv_i4, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Conv_I8:
          return visitor.Unary(pc, UnaryOperator.Conv_i8, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Conv_R4:
          return visitor.Unary(pc, UnaryOperator.Conv_r4, false, Dummy.Value, Dummy.Value, data);
        case NodeType.Conv_R8:
          return visitor.Unary(pc, UnaryOperator.Conv_r8, false, Dummy.Value, Dummy.Value, data);

        case NodeType.MethodContract:
          return visitor.Nop (pc, data);
        case NodeType.Requires:
          Requires requires = (Requires) node;
          return visitor.Assume (pc, EdgeTag.Requires, Dummy.Value, data);
        case NodeType.Call:
          MethodCall call = (MethodCall) node;
          Method method = GetMethodFrom (call.Callee);
          if (method.HasGenericParameters) {
            throw new NotImplementedException ();
          }
          if (method.Name != null && method.DeclaringType.Name != null && method.DeclaringType.Name.EndsWith ("Contract")) {
            switch(method.Name) {
              case "Assume":
                if (method.Parameters.Count == 1)
                  return visitor.Assume(pc, EdgeTag.Assume, Dummy.Value, data);
                break;
              case "Assert":
                if (method.Parameters.Count == 1)
                  return visitor.Assert(pc, EdgeTag.Assume, Dummy.Value, data);
                break;
            }
          }
          var indexable = new Indexable<Dummy>(Enumerable.Range (0, method.Parameters.Count).Select (it=>Dummy.Value).ToList ());
          return visitor.Call (pc, method, false, new Indexable<TypeNode> (null), Dummy.Value, indexable, data);
        default:
          return visitor.Nop (pc, data);
      }

      throw new NotImplementedException ();
    }

    private Method GetMethodFrom(Expression callee)
    {
      return (Method) ((MemberBinding) callee).BoundMember;
    }

    public bool Next(PC pc, out PC nextLabel)
    {
      Node nested;
      if (Decode (pc, out nested) == null && pc.Node!=null) {
        nextLabel = new PC(pc.Node, pc.Index + 1);
        return true;
      }
      nextLabel = new PC();
      return false;
    }

    public int GetILOffset(PC current)
    {
      throw new NotImplementedException ();
    }
    #endregion

    private static bool IsAtomicNested(Node nested)
    {
      if (nested == null) return false;
      switch (nested.NodeType) {
        case NodeType.Local:
        case NodeType.Parameter:
        case NodeType.Literal:
        case NodeType.This:
          return true;
        default:
          return false;
      }
    }

    private Node Decode(PC pc, out Node nested)
    {
      Node node = DecodeInflate (pc, out nested);

      return node;
    }

    /// <summary>
    /// Decodes nodes
    /// </summary>
    /// <param name="pc"></param>
    /// <param name="nested"></param>
    /// <returns>If node has nested, returns null and (nested = child). If last child given, node equals pc.Node</returns>
    private static Node DecodeInflate(PC pc, out Node nested)
    {
      Node node = pc.Node;
      if (node == null) {
        nested = null;
        return null;
      }

      int index = pc.Index;
      switch (node.NodeType) {
        case NodeType.MethodContract:
          MethodContract methodContract = (MethodContract) node;
          if (index < methodContract.RequiresCount) {
            nested = methodContract.Requires[index];
            return null;
          }
          if (index == methodContract.RequiresCount) {
            nested = null;
            return methodContract;
          }

          //todo: aggregate ensures
          nested = null;
          return methodContract;

        case NodeType.Requires:
          Requires requires = (Requires) node;
          if (index == 0) {
            nested = requires.Assertion;
            return null;
          }
          nested = null;
          return requires;
        case NodeType.Block:
          var block = (Block) node;
          if (block.Statements == null) {
            nested = null;
            return block;
          }
          nested = index >= block.Statements.Count ? null : block.Statements[index];
          return index + 1 < block.Statements.Count ? null : block;
        case NodeType.Return:
          var ret = (Return) node;
          if (ret.Expression != null && index == 0) {
            nested = ret.Expression;
            return null;
          }
          nested = null;
          return ret;
        case NodeType.AssignmentStatement:
          var assign = (AssignmentStatement) node;
          int innerIndex = index;

          if (assign.Target is Variable) innerIndex += 2;

          if (innerIndex == 2) {
            nested = assign.Source;
            return null;
          }

          nested = null;
          return assign;
        case NodeType.ExpressionStatement:
          var expressionStatement = (ExpressionStatement) node;
          nested = expressionStatement.Expression;
          return expressionStatement;
        case NodeType.MethodCall:
        case NodeType.Call:
        case NodeType.CallVirt:
          MethodCall methodCall = (MethodCall) node;
          MemberBinding binding = (MemberBinding) methodCall.Callee;
          int idx = index;
          if (!binding.BoundMember.IsStatic) {
            if (index == 0) {
              nested = binding.TargetObject;
              return null;
            }
            idx = index - 1;
          }
          if (idx < methodCall.Arguments.Count) {
            nested = methodCall.Arguments[idx];
            return null;
          }
          nested = null;
          return methodCall;
        default:
          var binary = node as BinaryExpression;
          if (binary != null) {
            if (index == 0) {
              nested = binary.Operand1;
              return null;
            }
            if (index == 1) {
              nested = binary.Operand2;
              return null;
            }
            nested = null;
            return binary;
          }

          var unary = node as UnaryExpression;
          if (unary != null) {
            if (index == 0) {
              nested = unary.Operand;
              return null;
            }

            nested = null;
            return unary;
          }

          //todo: ternary
          nested = null;
          return node;
      }
    }

    #region Nested type: PC
    public struct PC : IEquatable<PC>
    {
      public readonly int Index;
      public readonly Node Node;

      public PC(Node Node)
        : this (Node, 0)
      {
      }

      public PC(Node node, int index)
      {
        this.Node = node;
        this.Index = index;
      }

      #region IEquatable<PC> Members
      public bool Equals(PC other)
      {
        return Equals (other.Node, this.Node) && other.Index == this.Index;
      }
      #endregion

      public override bool Equals(object obj)
      {
        if (ReferenceEquals (null, obj)) return false;
        if (obj.GetType () != typeof (PC)) return false;
        return Equals ((PC) obj);
      }

      public override int GetHashCode()
      {
        unchecked {
          return ((this.Node != null ? this.Node.GetHashCode () : 0)*397) ^ this.Index;
        }
      }
    }
    #endregion

    public PC Entry(Method method)
    {
      return new PC(method.Body);
    }

    #region Implementation of IMethodCodeProvider<PC,Local,Parameter,Method,FieldReference,TypeReference,Dummy>
    public bool IsFaultHandler(ExceptionHandler handler)
    {
      return handler.HandlerType == NodeType.FaultHandler;
    }

    public bool IsFilterHandler(ExceptionHandler handler)
    {
      return handler.HandlerType == NodeType.Filter;
    }

    public bool IsFinallyHandler(ExceptionHandler handler)
    {
      return handler.HandlerType == NodeType.Finally;
    }

    public PC FilterExpressionStart(ExceptionHandler handler)
    {
      return new PC(handler.FilterExpression);
    }

    public bool IsCatchHandler(ExceptionHandler handler)
    {
      return handler.HandlerType == NodeType.Catch;
    }

    public TypeNode CatchType(ExceptionHandler handler)
    {
      return handler.FilterType;
    }

    public bool IsCatchAllHandler(ExceptionHandler handler)
    {
      if (!this.IsCatchHandler(handler))
        return false;
      if (handler.FilterType != null)
        return false;

      return true;
    }

    public IEnumerable<ExceptionHandler> GetTryBlocks(Method method)
    {
      yield break;
    }
    #endregion
  }
}