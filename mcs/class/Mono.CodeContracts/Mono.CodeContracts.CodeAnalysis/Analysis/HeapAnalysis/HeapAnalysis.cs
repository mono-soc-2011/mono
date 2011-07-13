using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;
using Mono.CodeContracts.CodeAnalysis.Lattices;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public class HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
    : IAnalysis<APC, HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>.Domain,
        IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>.Domain,
        HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>.Domain>,
        Dummy>
    where Type : IEquatable<Type>
  {
    #region Nested type: Domain
    public class Domain : IAbstractDomain<Domain>
    {
      private static Domain BottomValue;
      private readonly Dictionary<APC, Domain> BeginOldSavedStates;
      private readonly WrapTable Constructors;
      private readonly EGraph<Constructor, AbstractType> egraph;
      private readonly HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> parent;
      private APC beginOldPC;
      private IImmutableMap<SymValue, Constructor> constructorLookup;
      private Domain oldDomain;
      private IImmutableSet<SymValue> unmodifiedFieldsSinceEntry;
      private IImmutableSet<SymValue> unmodifiedSinceEntry;

      private void CopyValueToOldState(APC pc, Type type, int dest, int source, Domain targetDomain)
      {
        throw new NotImplementedException ();
      }

      private void AssignValue(int dest, FlatDomain<Type> type)
      {
        throw new NotImplementedException ();
      }

      private SymValue Address(Local local)
      {
        throw new NotImplementedException ();
      }

      private void AssignConst(int dest, Type type, object constant)
      {
        throw new NotImplementedException ();
      }

      private void AssignNull(int pos)
      {
        throw new NotImplementedException ();
      }

      private void CopyValue(SymValue dest, SymValue source, FlatDomain<Type> resultType)
      {
        throw new NotImplementedException ();
      }

      private SymValue OldValueAddress(Parameter argument)
      {
        throw new NotImplementedException ();
      }

      private SymValue Address(Parameter argument)
      {
        throw new NotImplementedException ();
      }

      private SymValue Address(int pos)
      {
        throw new NotImplementedException ();
      }

      private void Copy(int dest, int source)
      {
        throw new NotImplementedException ();
      }

      private void Havoc(int i)
      {
        throw new NotImplementedException ();
      }

      private void AssignSpecialUnary(int dest, Constructor constructor, int operand, FlatDomain<Type> resultType)
      {
        throw new NotImplementedException ();
      }

      private void AssignPureBinary(int dest, BinaryOperator op, FlatDomain<Type> resultType, int op1, int op2)
      {
        throw new NotImplementedException ();
      }

      private bool IsZero(SymValue symValue)
      {
        throw new NotImplementedException ();
      }

      private SymValue Value(int stackPos)
      {
        throw new NotImplementedException ();
      }

      private AbstractType CurrentType(int stackPos)
      {
        throw new NotImplementedException ();
      }

      private FlatDomain<Type> BinaryResultType(BinaryOperator op, AbstractType op1, AbstractType op2)
      {
        throw new NotImplementedException ();
      }

      #region Nested type: AbstractType
      private class AbstractType : IAbstractDomainForEGraph<AbstractType>, IEquatable<AbstractType>
      {
        #region Implementation of IAbstractDomain<HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain.AbstractType>
        public AbstractType Top
        {
          get { throw new NotImplementedException (); }
        }

        public AbstractType Bottom
        {
          get { throw new NotImplementedException (); }
        }

        public bool IsTop
        {
          get { throw new NotImplementedException (); }
        }

        public bool IsBottom
        {
          get { throw new NotImplementedException (); }
        }

        public AbstractType Join(AbstractType that, bool widening, out bool weaker)
        {
          throw new NotImplementedException ();
        }

        public AbstractType Meet(AbstractType that)
        {
          throw new NotImplementedException ();
        }

        public bool LessEqual(AbstractType that)
        {
          throw new NotImplementedException ();
        }

        public AbstractType ImmutableVersion()
        {
          throw new NotImplementedException ();
        }
        #endregion

        #region Implementation of IAbstractDomainForEGraph<HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain.AbstractType>
        public bool HasAllBottomFields
        {
          get { throw new NotImplementedException (); }
        }

        public AbstractType ForManifestedField()
        {
          throw new NotImplementedException ();
        }
        #endregion

        #region Implementation of IEquatable<HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain.AbstractType>
        public bool Equals(AbstractType other)
        {
          throw new NotImplementedException ();
        }
        #endregion
      }
      #endregion

      #region Nested type: AnalysisDecoder
      public struct AnalysisDecoder : IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, Domain, Domain>
      {
        private readonly HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> parent;

        public AnalysisDecoder(HeapAnalysis<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> parent)
        {
          this.parent = parent;
        }

        #region Helper Methods
        private Domain BinaryEffect(APC pc, BinaryOperator op, int dest, int op1, int op2, Domain data)
        {
          FlatDomain<Type> resultType = data.BinaryResultType (op, data.CurrentType (op1), data.CurrentType (op2));
          switch (op) {
            case BinaryOperator.Ceq:
            case BinaryOperator.Cobjeq: {
              SymValue val1 = data.Value (op1);
              if (data.IsZero (val1)) {
                data.AssignSpecialUnary (dest, data.Constructors.UnaryNot, op2, resultType);
                break;
              }
              SymValue val2 = data.Value (op2);
              if (data.IsZero (val2)) {
                data.AssignSpecialUnary (dest, data.Constructors.UnaryNot, op1, resultType);
                break;
              }
              goto default;
            }
            case BinaryOperator.Cne_Un: {
              SymValue val1 = data.Value (op1);
              if (data.IsZero (val1)) {
                data.AssignSpecialUnary (dest, data.Constructors.NeZero, op2, resultType);
                break;
              }
              SymValue val2 = data.Value (op2);
              if (data.IsZero (val2)) {
                data.AssignSpecialUnary (dest, data.Constructors.NeZero, op1, resultType);
                break;
              }
              goto default;
            }
            default:
              data.AssignPureBinary (dest, op, resultType, op1, op2);
              break;
          }

          data.Havoc (2);
          return data;
        }

        private void LoadArgEffect(Parameter argument, bool isOld, int dest, Domain data)
        {
          SymValue address = isOld ? data.OldValueAddress (argument) : data.Address (argument);
          IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder = MetadataDecoder;
          data.CopyValue (data.Address (dest), address, metadataDecoder.ManagedPointer (metadataDecoder.ParameterType (argument)));
        }

        private void StoreLocalEffect(Local local, int source, Domain data)
        {
          data.CopyValue (data.Address (local), data.Address (source), MetadataDecoder.ManagedPointer (MetadataDecoder.LocalType (local)));
          data.Havoc (source);
        }

        private void IsinstEffect(Type type, int dest, Domain data)
        {
          data.AssignValue (dest, type);
        }

        private void LoadLocalEffect(Local local, int dest, Domain data)
        {
          data.CopyValue (data.Address (dest), data.Address (local), MetadataDecoder.ManagedPointer (MetadataDecoder.LocalType (local)));
        }
        #endregion

        #region Implementation of IExpressionILVisitor<APC,Type,int,int,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain>
        public Domain Binary(APC pc, BinaryOperator op, int dest, int operand1, int operand2, Domain data)
        {
          data = BinaryEffect (pc, op, dest, operand1, operand2, data);
          if (data.oldDomain != null)
            data.oldDomain = BinaryEffect (pc, op, dest, operand1, operand2, data.oldDomain);
          return data;
        }

        public Domain Isinst(APC pc, Type type, int dest, int obj, Domain data)
        {
          IsinstEffect (type, dest, data);
          if (data.oldDomain != null)
            IsinstEffect (type, dest, data.oldDomain);

          return data;
        }

        public Domain LoadNull(APC pc, int dest, Domain data)
        {
          data.AssignNull (dest);
          if (data.oldDomain != null)
            data.oldDomain.AssignNull (dest);
          return data;
        }

        public Domain LoadConst(APC pc, Type type, object constant, int dest, Domain data)
        {
          data.AssignConst (dest, type, constant);
          if (data.oldDomain != null)
            data.oldDomain.AssignConst (dest, type, constant);

          return data;
        }

        public Domain Sizeof(APC pc, Type type, int dest, Domain data)
        {
          data.AssignValue (dest, MetadataDecoder.System_Int32);
          if (data.oldDomain != null)
            data.oldDomain.AssignValue (dest, MetadataDecoder.System_Int32);

          return data;
        }

        public Domain Unary(APC pc, UnaryOperator op, bool unsigned, int dest, int source, Domain data)
        {
          throw new NotImplementedException ();
        }
        #endregion

        #region Implementation of ISyntheticILVisitor<APC,Method,Field,Type,int,int,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain>
        public Domain Entry(APC pc, Method method, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Assume(APC pc, EdgeTag tag, int condition, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Assert(APC pc, EdgeTag tag, int condition, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain BeginOld(APC pc, APC matchingEnd, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain EndOld(APC pc, APC matchingBegin, Type type, int dest, int source, Domain data)
        {
          throw new NotImplementedException ();
        }
        #endregion

        #region Implementation of IILVisitor<APC,Local,Parameter,Method,Field,Type,int,int,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain>
        public Domain Arglist(APC pc, int dest, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Branch(APC pc, APC target, bool leavesExceptionBlock, Domain data)
        {
          throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
        }

        public Domain BranchCond(APC pc, APC target, BranchOperator bop, int value1, int value2, Domain data)
        {
          throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
        }

        public Domain BranchTrue(APC pc, APC target, int cond, Domain data)
        {
          throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
        }

        public Domain BranchFalse(APC pc, APC target, int cond, Domain data)
        {
          throw new InvalidOperationException ("Should not see branches, should see assumes. See APCDecoder");
        }

        public Domain Break(APC pc, Domain data)
        {
          return data;
        }

        public Domain Call<TypeList, ArgList>(APC pc, Method method, bool virt, TypeList extraVarargs, int dest, ArgList args, Domain data) where TypeList : IIndexable<Type>
          where ArgList : IIndexable<int>
        {
          throw new NotImplementedException ();
        }

        public Domain Calli<TypeList, ArgList>(APC pc, Type returnType, TypeList argTypes, bool instance, int dest, int functionPointer, ArgList args, Domain data) where TypeList : IIndexable<Type>
          where ArgList : IIndexable<int>
        {
          throw new NotImplementedException ();
        }

        public Domain CheckFinite(APC pc, int dest, int source, Domain data)
        {
          data.Copy (dest, source);
          if (data.oldDomain != null)
            data.oldDomain.Copy (dest, source);

          return data;
        }

        public Domain CopyBlock(APC pc, int destAddress, int srcAddress, int len, Domain data)
        {
          data.Havoc (destAddress);
          data.Havoc (srcAddress);
          data.Havoc (len);

          return data;
        }

        public Domain EndFilter(APC pc, int decision, Domain data)
        {
          data.Havoc (decision);

          return data;
        }

        public Domain EndFinally(APC pc, Domain data)
        {
          return data;
        }

        public Domain Jmp(APC pc, Method method, Domain data)
        {
          return data;
        }

        public Domain LoadArg(APC pc, Parameter argument, bool isOld, int dest, Domain data)
        {
          LoadArgEffect (argument, isOld, dest, data);
          if (data.oldDomain != null)
            LoadArgEffect (argument, isOld, dest, data.oldDomain);

          return data;
        }

        public Domain LoadLocal(APC pc, Local local, int dest, Domain data)
        {
          LoadLocalEffect (local, dest, data);
          if (data.oldDomain != null)
            data.CopyValueToOldState (data.oldDomain.beginOldPC, MetadataDecoder.LocalType (local), dest, dest, data.oldDomain);

          return data;
        }


        public Domain Nop(APC pc, Domain data)
        {
          return data;
        }

        public Domain Pop(APC pc, int source, Domain data)
        {
          data.Havoc (source);
          if (data.oldDomain != null)
            data.oldDomain.Havoc (source);

          return data;
        }

        public Domain Return(APC pc, int source, Domain data)
        {
          data.Havoc (source);

          return data;
        }

        public Domain StoreArg(APC pc, Parameter argument, int source, Domain data)
        {
          data.CopyValue (data.Address (argument), data.Address (source), MetadataDecoder.ManagedPointer (MetadataDecoder.ParameterType (argument)));
          data.Havoc (source);

          return data;
        }

        public Domain StoreLocal(APC pc, Local local, int source, Domain data)
        {
          StoreLocalEffect (local, source, data);
          if (data.oldDomain != null)
            StoreLocalEffect (local, source, data);

          return data;
        }

        public Domain Switch(APC pc, Type type, IEnumerable<Pair<object, APC>> cases, int value, Domain data)
        {
          throw new InvalidOperationException ("Should only see assumes");
        }

        public Domain Box(APC pc, Type type, int dest, int source, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain ConstrainedCallvirt<TypeList, ArgList>(APC pc, Method method, Type constraint, TypeList extraVarargs, int dest, ArgList args, Domain data) where TypeList : IIndexable<Type>
          where ArgList : IIndexable<int>
        {
          throw new NotImplementedException ();
        }

        public Domain CastClass(APC pc, Type type, int dest, int obj, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain CopyObj(APC pc, Type type, int destPtr, int sourcePtr, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Initobj(APC pc, Type type, int ptr, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadElement(APC pc, Type type, int dest, int array, int index, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadField(APC pc, Field field, int dest, int obj, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadLength(APC pc, int dest, int array, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadStaticField(APC pc, Field field, int dest, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadTypeToken(APC pc, Type type, int dest, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadFieldToken(APC pc, Field type, int dest, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain LoadMethodToken(APC pc, Method type, int dest, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain NewArray<ArgList>(APC pc, Type type, int dest, ArgList list, Domain data) where ArgList : IIndexable<int>
        {
          throw new NotImplementedException ();
        }

        public Domain NewObj<ArgList>(APC pc, Method ctor, int dest, ArgList args, Domain data) where ArgList : IIndexable<int>
        {
          throw new NotImplementedException ();
        }

        public Domain MkRefAny(APC pc, Type type, int dest, int obj, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain RefAnyType(APC pc, int dest, int source, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain RefAnyVal(APC pc, Type type, int dest, int source, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Rethrow(APC pc, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain StoreElement(APC pc, Type type, int array, int index, int value, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain StoreField(APC pc, Field field, int obj, int value, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain StoreStaticField(APC pc, Field field, int value, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Throw(APC pc, int exception, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain Unbox(APC pc, Type type, int dest, int obj, Domain data)
        {
          throw new NotImplementedException ();
        }

        public Domain UnboxAny(APC pc, Type type, int dest, int obj, Domain data)
        {
          throw new NotImplementedException ();
        }
        #endregion

        public IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
        {
          get { throw new NotImplementedException (); }
        }
      }
      #endregion

      #region Nested type: Constructor
      private abstract class Constructor : IConstantInfo, IEquatable<Constructor>
      {
        private readonly int id;
        protected readonly IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadata_decoder;

        protected Constructor(ref int idGen, IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
        {
          this.metadata_decoder = metadataDecoder;
          this.id = idGen++;
        }

        public abstract bool ActsAsField { get; }
        public abstract bool IsVirtualMethod { get; }
        public abstract bool IsStatic { get; }
        public abstract bool IfRootIsParameter { get; }

        #region IConstantInfo Members
        public abstract bool KeepAsBottomField { get; }
        public abstract bool ManifestField { get; }
        #endregion

        public abstract Type FieldAddressType();
        public abstract PathElementBase ToPathElement(bool tryCompact);

        public static Wrapper<T> For<T>(T value, ref int idGen, IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
        {
          if (value is Parameter) return (Wrapper<T>) (object) new ParameterConstructor ((Parameter) (object) value, ref idGen, metadataDecoder);
          if (value is Method)
            return (Wrapper<T>) (object) new PureMethodConstructor ((Method) (object) value, ref idGen, metadataDecoder);

          return new Wrapper<T> (value, ref idGen, metadataDecoder);
        }

        #region Nested type: ParameterConstructor
        public class ParameterConstructor : Wrapper<Parameter>
        {
          private readonly int argumentIndex;

          public ParameterConstructor(Parameter parameter, ref int idGen, IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
            : base (parameter, ref idGen, metadataDecoder)
          {
            this.argumentIndex = metadataDecoder.ArgumentIndex (parameter);
          }

          public override bool ActsAsField
          {
            get { return false; }
          }

          public override Type FieldAddressType()
          {
            throw new InvalidOperationException ();
          }

          public override string ToString()
          {
            return this.metadata_decoder.Name (this.Value);
          }
        }
        #endregion

        #region Nested type: PureMethodConstructor
        public class PureMethodConstructor : Wrapper<Method>
        {
          public PureMethodConstructor(Method value, ref int idGen, IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
            : base (value, ref idGen, metadataDecoder)
          {
          }

          public override bool ActsAsField
          {
            get { return true; }
          }

          public override Type FieldAddressType()
          {
            return this.metadata_decoder.ManagedPointer (this.metadata_decoder.ReturnType (this.Value));
          }
        }
        #endregion

        #region Nested type: Wrapper
        public class Wrapper<T> : Constructor
        {
          public readonly T Value;
          protected PathElementBase PathElement;

          public Wrapper(T value, ref int idGen, IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
            : base (ref idGen, metadataDecoder)
          {
            this.Value = value;
          }

          #region Overrides of Constructor
          public override bool ActsAsField
          {
            get { return this.Value is Field; }
          }

          public override bool IsVirtualMethod
          {
            get { return this.Value is Method && this.metadata_decoder.IsVirtual ((Method) (object) this.Value); }
          }

          public override bool KeepAsBottomField
          {
            get
            {
              var str = this.Value as string;
              if (str == null)
                return true;

              return str != "$UnaryNot" && str != "$NeZero";
            }
          }

          public override bool ManifestField
          {
            get
            {
              var str = this.Value as string;
              if (str != null)
                return str == "$Value" || str == "$Length";

              return (this.Value is Field || this.Value is Method);
            }
          }

          public override bool IsStatic
          {
            get
            {
              if (this.Value is Field)
                return this.metadata_decoder.IsStatic ((Field) (object) this.Value);
              if (this.Value is Method)
                return this.metadata_decoder.IsStatic ((Method) (object) this.Value);

              return false;
            }
          }

          public override bool IfRootIsParameter
          {
            get
            {
              if (this.Value is Field)
                return !this.metadata_decoder.IsStatic ((Field) (object) this.Value);
              if (this.Value is Method)
                return !this.metadata_decoder.IsStatic ((Method) (object) this.Value);
              if (this.Value is Parameter)
                return true;
              if (this.Value is Local)
                return false;

              return true;
            }
          }

          public override Type FieldAddressType()
          {
            if (this.Value is Field)
              return this.metadata_decoder.ManagedPointer (this.metadata_decoder.FieldType ((Field) (object) this.Value));

            throw new InvalidOperationException ();
          }

          public override PathElementBase ToPathElement(bool tryCompact)
          {
            throw new NotImplementedException ();
          }
          #endregion

          public Type Type { get; set; }

          public override string ToString()
          {
            if (typeof (T).Equals (typeof (int)))
              return string.Format ("s{0}", this.Value);
            if (this.Value is Field) {
              var field = (Field) (object) this.Value;
              if (this.metadata_decoder.IsStatic (field))
                return string.Format ("{0}.{1}", this.metadata_decoder.FullName (this.metadata_decoder.DeclaringType (field)), this.metadata_decoder.Name (field));

              return this.metadata_decoder.Name (field);
            }
            if (this.Value is Method) {
              var method = (Method) (object) this.Value;
              if (this.metadata_decoder.IsStatic (method))
                return string.Format ("{0}.{1}", this.metadata_decoder.FullName (this.metadata_decoder.DeclaringType (method)), this.metadata_decoder.Name (method));

              return this.metadata_decoder.Name (method);
            }

            return this.Value.ToString ();
          }
        }
        #endregion

        #region Implementation of IEquatable<HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain.Constructor>
        public bool Equals(Constructor other)
        {
          throw new NotImplementedException ();
        }
        #endregion
      }
      #endregion

      #region Nested type: PathElementBase
      private class PathElementBase
      {
      }
      #endregion

      #region Nested type: WrapTable
      private class WrapTable
      {
        public Constructor UnaryNot { get; private set; }
        public Constructor NeZero { get; private set; }
      }
      #endregion

      #region Implementation of IAbstractDomain<HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain>
      public Domain Top
      {
        get { throw new NotImplementedException (); }
      }

      public Domain Bottom
      {
        get { throw new NotImplementedException (); }
      }

      public bool IsTop
      {
        get { throw new NotImplementedException (); }
      }

      public bool IsBottom
      {
        get { throw new NotImplementedException (); }
      }

      public Domain Join(Domain that, bool widening, out bool weaker)
      {
        throw new NotImplementedException ();
      }

      public Domain Meet(Domain that)
      {
        throw new NotImplementedException ();
      }

      public bool LessEqual(Domain that)
      {
        throw new NotImplementedException ();
      }

      public Domain ImmutableVersion()
      {
        throw new NotImplementedException ();
      }
      #endregion
    }
    #endregion

    #region Implementation of IAnalysis<APC,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain,IILVisitor<APC,Local,Parameter,Method,Field,Type,int,int,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain,HeapAnalysis<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>.Domain>,Dummy>
    IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, Domain, Domain> IAnalysis<APC, Domain, IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, Domain, Domain>, Dummy>.
      GetVisitor()
    {
      return GetVisitor ();
    }

    public Domain Join(Pair<APC, APC> edge, Domain newstate, Domain prevstate, out bool weaker, bool widen)
    {
      throw new NotImplementedException ();
    }

    public Domain ImmutableVersion(Domain arg)
    {
      throw new NotImplementedException ();
    }

    public Domain MutableVersion(Domain arg)
    {
      throw new NotImplementedException ();
    }

    public Domain EdgeConversion(APC @from, APC to, bool isJoinPoint, Dummy data, Domain state)
    {
      throw new NotImplementedException ();
    }

    public bool IsBottom(APC pc, Domain state)
    {
      throw new NotImplementedException ();
    }

    private IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, Domain, Domain> GetVisitor()
    {
      return new Domain.AnalysisDecoder (this);
    }
    #endregion
  }
}