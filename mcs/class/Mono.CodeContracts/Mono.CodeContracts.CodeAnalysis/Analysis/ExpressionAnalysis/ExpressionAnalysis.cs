using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Lattices;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ExpressionAnalysis
{
  public class ExpressionAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, SymbolicValue, Context, EdgeData>
    where Type : IEquatable<Type>
    where SymbolicValue : IEquatable<SymbolicValue>
    where Context : IValueContext<Local, Parameter, Method, Field, Type, SymbolicValue>
    where EdgeData : IImmutableMap<SymbolicValue, LispList<SymbolicValue>>
  {
    private readonly ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, SymbolicValue, SymbolicValue, Context, EdgeData> value_layer;

    public ExpressionAnalysis(ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, SymbolicValue, SymbolicValue, Context, EdgeData> valueLayer)
    {
      value_layer = valueLayer;
    }

    public class Domain : IGraph<SymbolicValue, Dummy>
    {
      private EnvironmentDomain<SymbolicValue, FlatDomain<Expression>> expressions;

      private Domain(EnvironmentDomain<SymbolicValue, FlatDomain<Expression>> expressions)
      {
        this.expressions = expressions;
      }

      #region Implementation of IGraph<SymbolicValue,Dummy>
      IEnumerable<SymbolicValue> IGraph<SymbolicValue, Dummy>.Nodes
      {
        get { return this.expressions.Keys; }
      }

      public IEnumerable<Pair<Dummy, SymbolicValue>> Successors(SymbolicValue node)
      {
        var expr = this.expressions[node];
        if (expr.IsNormal) {
          foreach (SymbolicValue sv in expr.Value.Variables) {
            yield return new Pair<Dummy, SymbolicValue> (Dummy.Value, sv);
          }
        }
      }
      #endregion

      public Domain Join(Domain that, bool widening, out bool weaker)
      {
        return new Domain(this.expressions.Join(that.expressions, widening, out weaker));
      }
    }

    public abstract class Expression : IEquatable<Expression>
    {
      public static readonly Expression NullValue = new Null ();
      public abstract IEnumerable<SymbolicValue> Variables { get; }

      public abstract Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        where Visitor : IExpressionILVisitor<APC, Type, SymbolicValue, SymbolicValue, Data, Result>;

      public abstract Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions);

      /// <summary>
      /// Specifies that current expression is partially contained in candidates
      /// </summary>
      public abstract bool IsContained(IImmutableSet<SymbolicValue> candidates);
      public abstract bool Contains(SymbolicValue symbol);

      public abstract override string ToString();

      public abstract bool Equals(Expression other);

      public class Unary : Expression
      {
        public readonly SymbolicValue Source;
        public readonly UnaryOperator Operator;
        public readonly bool Unsigned;

        public Unary(SymbolicValue source, UnaryOperator op, bool unsigned)
        {
          this.Source = source;
          this.Operator = op;
          this.Unsigned = unsigned;
        }

        #region Overrides of Expression
        public override IEnumerable<SymbolicValue> Variables
        {
          get { yield return Source; }
        }

        public override Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        {
          return visitor.Unary (pc, this.Operator, this.Unsigned, dest, this.Source, data);
        }

        public override Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions)
        {
          return substitutions.ContainsKey (this.Source) ? new Unary (substitutions[this.Source].Head, this.Operator, this.Unsigned) : null;
        }

        public override bool IsContained(IImmutableSet<SymbolicValue> candidates)
        {
          return candidates.Contains (this.Source);
        }

        public override bool Contains(SymbolicValue symbol)
        {
          return symbol.Equals (Source);
        }

        public override string ToString()
        {
          return string.Format ("Unary({0} {1})", this.Operator, this.Source);
        }

        public override bool Equals(Expression other)
        {
          Unary unary = other as Unary;
          if (unary == null || unary.Operator != this.Operator || unary.Unsigned != this.Unsigned)
            return false;

          return unary.Source.Equals (this.Source);
        }
        #endregion
      }

      public class Binary : Expression
      {
        public readonly SymbolicValue Left;
        public readonly SymbolicValue Right;
        public readonly BinaryOperator Operator;

        public Binary(SymbolicValue left, SymbolicValue right, BinaryOperator op )
        {
          this.Left = left;
          this.Right = right;
          this.Operator = op;
        }

        #region Overrides of Expression
        public override IEnumerable<SymbolicValue> Variables
        {
          get
          {
            yield return Left;
            yield return Right;
          }
        }

        public override Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        {
          return visitor.Binary (pc, Operator, dest, Left, Right, data);
        }

        public override Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions)
        {
          if (substitutions.ContainsKey (Left) && substitutions.ContainsKey (Right))
            return new Binary(substitutions[Left].Head, substitutions[Right].Head, Operator);

          return null;
        }

        public override bool IsContained(IImmutableSet<SymbolicValue> candidates)
        {
          return candidates.Contains (Left) || candidates.Contains (Right);
        }

        public override bool Contains(SymbolicValue symbol)
        {
          return Left.Equals (symbol) || Right.Equals (symbol);
        }

        public override string ToString()
        {
          return string.Format ("Binary({0} {1} {2})", Left, Operator, Right);
        }

        public override bool Equals(Expression other)
        {
          var binary = other as Binary;
          if (binary == null || binary.Operator != this.Operator)
            return false;

          return binary.Left.Equals (this.Left) && binary.Right.Equals (this.Right);
        }
        #endregion
      }

      public class IsInst : Expression
      {
        public readonly Type Type;
        public readonly SymbolicValue Argument;

        public IsInst(SymbolicValue argument, Type type)
        {
          this.Argument = argument;
          this.Type = type;
        }

        #region Overrides of Expression
        public override IEnumerable<SymbolicValue> Variables
        {
          get { yield return Argument; }
        }

        public override Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        {
          return visitor.Isinst (pc, Type, dest, Argument, data);
        }

        public override Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions)
        {
          if (substitutions.ContainsKey (Argument))
            return new IsInst (substitutions[Argument].Head, Type);
          return null;
        }

        public override bool IsContained(IImmutableSet<SymbolicValue> candidates)
        {
          return candidates.Contains (Argument);
        }

        public override bool Contains(SymbolicValue symbol)
        {
          return Argument.Equals (symbol);
        }

        public override string ToString()
        {
          return string.Format ("IsInst({0} {1})", Type, Argument);
        }

        public override bool Equals(Expression other)
        {
          var inst = other as IsInst;

          return (inst != null && inst.Type.Equals (this.Type) && inst.Argument.Equals (this.Argument));
        }
        #endregion
      }

      public class Sizeof : Expression
      {
        public readonly Type Type;

        public Sizeof(Type type)
        {
          this.Type = type;
        }

        #region Overrides of Expression
        public override IEnumerable<SymbolicValue> Variables
        {
          get { return Enumerable.Empty<SymbolicValue> (); }
        }

        public override Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        {
          return visitor.Sizeof (pc, Type, dest, data);
        }

        public override Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions)
        {
          return this;
        }

        public override bool IsContained(IImmutableSet<SymbolicValue> candidates)
        {
          return false;
        }

        public override bool Contains(SymbolicValue symbol)
        {
          return false;
        }

        public override string ToString()
        {
          return string.Format ("Sizeof({0})", Type);
        }

        public override bool Equals(Expression other)
        {
          var @sizeof = other as Sizeof;
          
          return (@sizeof != null && @sizeof.Type.Equals (this.Type));
        }
        #endregion
      }

      public class Constant : Expression
      {
        public readonly Type Type;
        public readonly object value;

        public Constant(Type type, object value)
        {
          this.Type = type;
          this.value = value;
        }

        #region Overrides of Expression
        public override IEnumerable<SymbolicValue> Variables
        {
          get { return Enumerable.Empty<SymbolicValue> (); }
        }

        public override Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        {
          return visitor.LoadConst (pc, Type, value, dest, data);
        }

        public override Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions)
        {
          return this;
        }

        public override bool IsContained(IImmutableSet<SymbolicValue> candidates)
        {
          return false;
        }

        public override bool Contains(SymbolicValue symbol)
        {
          return false;
        }

        public override string ToString()
        {
          return string.Format ("Const({0} {1})", Type, value);
        }

        public override bool Equals(Expression other)
        {
          var constant = other as Constant;
          
          return constant != null && constant.Type.Equals (this.Type) && constant.value.Equals (this.value);
        }
        #endregion
      }

      public class Null : Expression
      {
        #region Overrides of Expression
        public override IEnumerable<SymbolicValue> Variables { get { return Enumerable.Empty<SymbolicValue>(); } }

        public override Result Decode<Data, Result, Visitor>(APC pc, SymbolicValue dest, Visitor visitor, Data data)
        {
          return visitor.LoadNull (pc, dest, data);
        }

        public override Expression Substitute(IImmutableMap<SymbolicValue, LispList<SymbolicValue>> substitutions)
        {
          return this;
        }

        public override bool IsContained(IImmutableSet<SymbolicValue> candidates)
        {
          return false;
        }

        public override bool Contains(SymbolicValue symbol)
        {
          return false;
        }

        public override string ToString()
        {
          return "Null()";
        }

        public override bool Equals(Expression other)
        {
          return other == this;
        }
        #endregion
      }
    }
  }
}