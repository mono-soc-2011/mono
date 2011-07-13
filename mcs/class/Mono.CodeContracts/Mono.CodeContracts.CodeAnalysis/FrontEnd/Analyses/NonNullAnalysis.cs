using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Lattices;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd.Analyses
{
  public class NonNullAnalysis : IMethodAnalysis
  {
    #region IMethodAnalysis Members
    public string Name
    {
      get { return "Non-null"; }
    }

    public IMethodResult<Variable> Analyze<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>
      (string fullMethodName,
       IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> methodDriver)
      where Expression : IEquatable<Expression> where Variable : IEquatable<Variable> where Type : IEquatable<Type>
    {
      return null;
    }
    #endregion

    #region Nested type: TypeBindings
    public static class TypeBindings<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>
      where Type : IEquatable<Type>
      where Expression : IEquatable<Expression>
      where Variable : IEquatable<Variable>
    {
      #region Nested type: Analysis
      private class Analysis :
        DefaultILVisitor<APC, Local, Parameter, Method, Field, Type, Variable, Variable, Domain, Domain>,
        IAnalysis<APC, Domain, IILVisitor<APC, Local, Parameter, Method, Field, Type, Variable, Variable, Domain, Domain>, IImmutableMap<Variable, List<Variable>>>
      {
        private IFixPointInfo<APC, Domain> fix_point_info;
        private IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> method_driver;

        public Analysis(IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> mdriver)
        {
          this.method_driver = mdriver;
        }

        #region IAnalysis<APC,TypeBindings<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,Expression,Variable>.Domain,IILVisitor<APC,Local,Parameter,Method,Field,Type,Variable,Variable,TypeBindings<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,Expression,Variable>.Domain,TypeBindings<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,Expression,Variable>.Domain>,IImmutableMap<Variable,List<Variable>>> Members
        public IILVisitor<APC, Local, Parameter, Method, Field, Type, Variable, Variable, Domain, Domain> GetVisitor()
        {
          return this;
        }

        public Domain Join(Pair<APC, APC> edge, Domain newstate, Domain prevstate, out bool weaker, bool widen)
        {
          bool nonNullWeaker;
          SetDomain<Variable> nonNulls = prevstate.NonNulls.Join (newstate.NonNulls, widen, out nonNullWeaker);
          bool nullWeaker;
          SetDomain<Variable> nulls = prevstate.Nulls.Join (newstate.Nulls, widen, out nullWeaker);
          
          weaker = nonNullWeaker || nullWeaker;
          return new Domain (nonNulls, nulls);
        }

        public Domain ImmutableVersion(Domain state)
        {
          return state;
        }

        public Domain MutableVersion(Domain state)
        {
          return state;
        }

        public Domain EdgeConversion(APC from, APC to, bool isJoinPoint, IImmutableMap<Variable, List<Variable>> data, Domain state)
        {
          if (data == null)
            return state;
          SetDomain<Variable> setDomain1 = state.NonNulls;
          SetDomain<Variable> nonNulls = SetDomain<Variable>.TopValue;

          SetDomain<Variable> setDomain2 = state.Nulls;
          SetDomain<Variable> nulls = SetDomain<Variable>.TopValue;
          foreach (Variable variable in data.Keys) {
            bool nonNullContains = setDomain1.Contains (variable);
            bool nullContains = setDomain2.Contains (variable);

            if (nonNullContains || nullContains) {
              foreach (Variable anotherVariable in data[variable]) {
                if (nonNullContains)
                  nonNulls = nonNulls.Add (anotherVariable);
                if (nullContains)
                  nulls = nulls.Add (anotherVariable);
              }
            }
          }

          return new Domain (nonNulls, nulls);
        }

        public bool IsBottom(APC pc, Domain state)
        {
          return state.NonNulls.IsBottom;
        }
        #endregion

        #region Implementation of DefaultILVisitor<APC,Type,Variable,Variable,TypeBindings<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,Expression,Variable>.Domain,TypeBindings<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly,Expression,Variable>.Domain>
        public override Domain DefaultVisit(APC pc, Domain data)
        {
          return data;
        }

        private static Domain AssumeNonNull(Variable dest, Domain before)
        {
          if (!before.NonNulls.Contains (dest))
            return new Domain (before.NonNulls.Add (dest), before.Nulls);
          return before;
        }

        private static Domain AssumeNull(Variable dest, Domain before)
        {
          if (!before.Nulls.Contains (dest))
            return new Domain (before.NonNulls, before.Nulls.Add (dest));
          return before;
        }

        public override Domain Binary(APC pc, BinaryOperator op, Variable dest, Variable operand1, Variable operand2, Domain data)
        {
          switch (op) {
            case BinaryOperator.Add:
            case BinaryOperator.Add_Ovf:
            case BinaryOperator.Add_Ovf_Un:
            case BinaryOperator.Sub:
            case BinaryOperator.Sub_Ovf:
            case BinaryOperator.Sub_Ovf_Un:
              throw new NotImplementedException ();
          }
          return data;
        }

        public override Domain Unary(APC pc, UnaryOperator op, bool unsigned, Variable dest, Variable source, Domain data)
        {
          switch (op) {
            case UnaryOperator.Conv_i:
            case UnaryOperator.Conv_u:
              if (data.IsNonNull (source))
                return AssumeNonNull (dest, data);
              break;
          }
          return data;
        }
        #endregion
      }
      #endregion

      #region Nested type: Domain
      public struct Domain
      {
        public readonly SetDomain<Variable> NonNulls;
        public readonly SetDomain<Variable> Nulls;

        public Domain(SetDomain<Variable> nonNulls, SetDomain<Variable> nulls)
        {
          this.NonNulls = nonNulls;
          this.Nulls = nulls;
        }

        public bool IsNonNull(Variable v)
        {
          return this.NonNulls.Contains (v);
        }

        public bool IsNull(Variable v)
        {
          return this.Nulls.Contains (v);
        }
      }
      #endregion
    }
    #endregion
  }
}