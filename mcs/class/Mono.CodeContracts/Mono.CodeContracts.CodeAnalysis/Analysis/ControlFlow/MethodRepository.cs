using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.Consumers;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
    : IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Dummy, Subroutine>
  {
    #region Subroutine Factories 

    #region Nested type: SubroutineFactory
    private abstract class SubroutineFactory<Key, Data> : ICodeConsumer<Local, Parameter, Method, Field, Type, Data, Subroutine>
    {
      private readonly Dictionary<Key, Subroutine> cache = new Dictionary<Key, Subroutine>();
      private readonly MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> method_repository;

      protected SubroutineFactory(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository)
      {
        this.method_repository = methodRepository;
      }

      #region Implementation of ICodeConsumer<Local,Parameter,Method,Field,Type,Data,Subroutine>
      public Subroutine Accept<Label>(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider, Label entryPoint, Data data)
      {
        return Factory(new SimpleSubroutineBuilder<Label>(codeProvider, this.method_repository, entryPoint), entryPoint, data);
      }

      public Subroutine Get(Key key)
      {
        if (this.cache.ContainsKey(key))
          return this.cache[key];

        Subroutine sub = BuildNewSubroutine(key);
        this.cache.Add(key, sub);
        if (sub != null)
          sub.Initialize();
        return sub;
      }

      protected abstract Subroutine BuildNewSubroutine(Key key);
      protected abstract Subroutine Factory<Label>(SimpleSubroutineBuilder<Label> builder, Label entry, Data data);
      #endregion

      protected MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodRepository
      {
        get { return this.method_repository; }
      }

      protected IContractProvider<Local, Parameter, Method, Field, Type> ContractDecoder
      {
        get { return this.method_repository.contract_decoder; }
      }

      protected IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
      {
        get { return this.method_repository.metadata_decoder; }
      }
    }
    #endregion

    #region Nested type: RequiresCache
    private class RequiresCache : SubroutineFactory<Method, Pair<Method, IImmutableSet<Subroutine>>>
    {
      public RequiresCache(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository)
        : base(methodRepository)
      {
      }

      #region Overrides of SubroutineFactory<Method,Pair<Method,IImmutableSet<Subroutine>>>
      protected override Subroutine BuildNewSubroutine(Method method)
      {
        if (this.ContractDecoder != null)
        {
          IImmutableSet<Subroutine> inheritedRequires = GetInheritedRequires(method);
          if (this.ContractDecoder.HasRequires(method))
            return this.ContractDecoder.AccessRequires(method, this, new Pair<Method, IImmutableSet<Subroutine>>(method, inheritedRequires));
          if (inheritedRequires.Count > 0)
          {
            if (inheritedRequires.Count == 1)
              return inheritedRequires.Any;

            return new RequiresSubroutine<Dummy>(MethodRepository, method, inheritedRequires);
          }
        }
        return null;
      }

      private IImmutableSet<Subroutine> GetInheritedRequires(Method method)
      {
        IImmutableSet<Subroutine> result = ImmutableSet<Subroutine>.Empty();

        if (MetadataDecoder.IsVirtual(method) && this.ContractDecoder.CanInheritContracts(method))
        {
          Method rootMethod;
          if (MetadataDecoder.TryGetRootMethod(method, out rootMethod))
          {
            Subroutine sub = Get(MetadataDecoder.Unspecialized(method));
            if (sub != null)
              result = result.Add(sub);
          }
          foreach (Method implMethod in MetadataDecoder.ImplementedMethods(method))
          {
            Subroutine sub = Get(MetadataDecoder.Unspecialized(implMethod));
            if (sub != null)
              result = result.Add(sub);
          }
        }

        return result;
      }

      protected override Subroutine Factory<Label>(SimpleSubroutineBuilder<Label> builder, Label entry, Pair<Method, IImmutableSet<Subroutine>> data)
      {
        return new RequiresSubroutine<Label> (this.MethodRepository, data.Key, builder, entry, data.Value);
      }
      #endregion
    }
    #endregion

    #region Nested type: EnsuresCache
    private class EnsuresCache : SubroutineFactory<Method, Pair<Method, IImmutableSet<Subroutine>>>
    {
      public EnsuresCache(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository) : base (methodRepository)
      {
      }

      #region Overrides of SubroutineFactory<Method,Pair<Method,IImmutableSet<Subroutine>>>
      protected override Subroutine BuildNewSubroutine(Method method)
      {
        if (this.ContractDecoder != null) {
          IImmutableSet<Subroutine> inheritedEnsures = GetInheritedEnsures (method);
          if (this.ContractDecoder.HasEnsures (method))
            return this.ContractDecoder.AccessEnsures (method, this, new Pair<Method, IImmutableSet<Subroutine>> (method, inheritedEnsures));
          if (inheritedEnsures.Count > 0) {
            if (inheritedEnsures.Count > 1)
              return new EnsuresSubroutine<Dummy> (MethodRepository, method, inheritedEnsures);
            return inheritedEnsures.Any;
          }
        }
        return new EnsuresSubroutine<Dummy> (MethodRepository, method, null);
      }

      private IImmutableSet<Subroutine> GetInheritedEnsures(Method method)
      {
        IImmutableSet<Subroutine> result = ImmutableSet<Subroutine>.Empty ();
        if (MetadataDecoder.IsVirtual (method) && this.ContractDecoder.CanInheritContracts (method)) {
          foreach (Method baseMethod in MetadataDecoder.OverridenAndImplementedMethods (method)) {
            Subroutine baseEnsures = Get (MetadataDecoder.Unspecialized (method));
            if (baseEnsures != null)
              result = result.Add (baseEnsures);
          }
        }
        return result;
      }

      protected override Subroutine Factory<Label>(SimpleSubroutineBuilder<Label> builder, Label entry, Pair<Method, IImmutableSet<Subroutine>> data)
      {
        return new EnsuresSubroutine<Label> (MethodRepository, data.Key, builder, entry, data.Value);
      }
      #endregion
    }

    

    #endregion

    #endregion

    private readonly IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadata_decoder;

    private readonly Dictionary<Method, ICFG> method_cache = new Dictionary<Method, ICFG> ();
    private readonly IContractProvider<Local, Parameter, Method, Field, Type> contract_decoder;
    private readonly RequiresCache requires_cache;
    private readonly EnsuresCache ensures_cache;

    public MethodRepository(IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder,
      IContractProvider<Local, Parameter, Method, Field, Type> contractDecoder)
    {
      this.metadata_decoder = metadataDecoder;
      this.contract_decoder = contractDecoder;
      this.requires_cache = new RequiresCache (this);
      this.ensures_cache = new EnsuresCache (this);
    }

    public Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy,Data, Result>
    {
      var block = pc.Block as BlockBase;
      if (block != null) 
        return block.ForwardDecode<Data, Result, Visitor> (pc, visitor, data);

      return visitor.Nop (pc, data);
    }

    #region IMethodCodeConsumer<Local,Parameter,Method,Field,Type,Dummy,Subroutine> Members
    Subroutine IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Dummy, Subroutine>.Accept<Label, Handler>(
      IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> codeProvider,
      Label entry,
      Method method,
      Dummy data)
    {
      var builder = new SubroutineWithHandlersBuilder<Label, Handler> (codeProvider, this, method, entry);
      return new MethodSubroutine<Label, Handler> (this, method, entry, builder);
    }
    #endregion

    private Subroutine GetRequires(Method method)
    {
      method = this.metadata_decoder.Unspecialized (method);
      return this.requires_cache.Get (method);
    }

    private Subroutine GetEnsures(Method method)
    {
      //todo: implement this
      return null;
    }

    private Subroutine GetInvariant(Type type)
    {
      //todo: implement this
      return null;
    }

    private Subroutine GetRedundantInvariant(Subroutine invariant, Type type)
    {
      //todo: implement this
      return null;
    }

    public ICFG GetControlFlowGraph(Method method)
    {
      if (this.method_cache.ContainsKey (method))
        return this.method_cache[method];

      if (!this.metadata_decoder.HasBody (method))
        throw new InvalidOperationException ("Method has no body");

      return new ControlFlowGraph<Method, Type> (this.metadata_decoder.AccessMethodBody (method, this, Dummy.Value), this);
    }

    #region Subroutines

    #region Nested type: MethodContractSubroutine
    private abstract class MethodContractSubroutine<Label> : SubroutineBase<Label>, IMethodInfo<Method>
    {
      private readonly Method method;
      protected new SimpleSubroutineBuilder<Label> Builder { get { return (SimpleSubroutineBuilder<Label>)base.Builder; } set { base.Builder = value; } } 

      public Method Method
      {
        get { return this.method; }
      }

      protected MethodContractSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                          Method method) : base (methodRepository)
      {
        this.method = method;
      }

      protected MethodContractSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                          Method method,
                                          SimpleSubroutineBuilder<Label> builder,
                                          Label startLabel) : base (methodRepository, startLabel, builder)
      {
        this.method = method;
      }
    }
    #endregion

    #region Nested type: RequiresSubroutine
    private sealed class RequiresSubroutine<Label> : MethodContractSubroutine<Label>, IEquatable<RequiresSubroutine<Label>>
    {
      public override SubroutineKind Kind { get { return SubroutineKind.Requires; } }

      public override bool IsContract { get { return true; } }

      public override bool IsRequires { get { return true; } }

      public RequiresSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                Method method,
                                IImmutableSet<Subroutine> inherited)
        : base(methodRepository, method)
      {
        AddSuccessor(Entry, EdgeTag.Entry, Exit);
        AddBaseRequires(Exit, inherited);
        Commit();
      }

      public RequiresSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                Method method,
                                SimpleSubroutineBuilder<Label> builder,
                                Label entryLabel,
                                IImmutableSet<Subroutine> inheritedRequires)
        : base(methodRepository, method, builder, entryLabel)
      {
        AddBaseRequires(this.GetTargetBlock(entryLabel), inheritedRequires);
      }

      public override void Initialize()
      {
        if (this.Builder == null)
          return;

        this.Builder.BuildBlocks(this.StartLabel, this);
        this.Commit();

        this.Builder = null;
      }

      private void AddBaseRequires(CFGBlock targetOfEntry, IImmutableSet<Subroutine> inherited)
      {
        foreach (var subroutine in inherited.Elements)
          this.AddEdgeSubroutine(this.Entry, targetOfEntry, subroutine, EdgeTag.Inherited);
      }

      public bool Equals(RequiresSubroutine<Label> that)
      {
        return this.Id == that.Id;
      }
    }
    #endregion

    #region Nested type: EnsuresSubroutine
    private sealed class EnsuresSubroutine<Label> : MethodContractSubroutine<Label>, IEquatable<EnsuresSubroutine<Label>>
    {
      public EnsuresSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                               Method method, IImmutableSet<Subroutine> inherited) : base (methodRepository, method)
      {
        AddSuccessor (Entry, EdgeTag.Entry, Exit);
        AddBaseEnsures (Entry, Exit, inherited);
        Commit ();
      }

      public EnsuresSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                               Method method, SimpleSubroutineBuilder<Label> builder, Label startLabel, IImmutableSet<Subroutine> inherited)
        : base (methodRepository, method, builder, startLabel)
      {
        AddBaseEnsures (Entry, GetTargetBlock (startLabel), inherited);
      }

      public override bool IsEnsures
      {
        get { return true; }
      }

      public override bool IsContract
      {
        get { return true; }
      }

      private void AddBaseEnsures(CFGBlock from, CFGBlock to, IImmutableSet<Subroutine> inherited)
      {
        if (inherited == null)
          return;
        foreach (Subroutine subroutine in inherited.Elements) AddEdgeSubroutine (from, to, subroutine, EdgeTag.Inherited);
      }

      #region Overrides of SubroutineBase<Label>
      public override void Initialize()
      {
        if (this.Builder == null)
          return;
        this.Builder.BuildBlocks (this.StartLabel, this);
        Commit ();
        this.Builder = null;
      }

      public override void Commit()
      {
//        base.Commit ();
//        var visitor = new CommitScanState (this);
//        EnsuresBlock<Label> priorBlock = null;
//        foreach (CFGBlock block in Blocks) {
//          var ensuresBlock = block as EnsuresBlock<Label>;
//          if (ensuresBlock != null) {
//            priorBlock = ensuresBlock;
//            int count = ensuresBlock.Count;
//            visitor.StartBlock (ensuresBlock);
//            for (int index = 0; index < count; index++) if (ensuresBlock.OriginalForwardDecode ()) 
//          }
//        }
      }

      private class CommitScanState : DefaultILVisitor<Label, Local, Parameter, Method, Field, Type, Dummy, Dummy, int, bool>,
                                      IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, int, bool>
      {
        private readonly Dictionary<CFGBlock, ScanState> block_start_states = new Dictionary<CFGBlock, ScanState>();
        private readonly EnsuresSubroutine<Label> parent;
        private EnsuresBlock<Label> current_block;
        private Type next_end_old_type;
        private ScanState state;

        public CommitScanState(EnsuresSubroutine<Label> ensuresSubroutine)
        {
          this.parent = ensuresSubroutine;
          this.state = ScanState.OutsideOld;
        }

        #region Overrides of DefaultILVisitor<Label,Local,Parameter,Method,Field,Type,Dummy,Dummy,int,bool>
        public override bool DefaultVisit(Label pc, int index)
        {
          if (this.state != ScanState.InsertingOld)
            return this.current_block.UsesOverriding;
          this.state = ScanState.OutsideOld;
          this.current_block.EndOldWithoutInstruction(this.parent.method_repository.metadata_decoder.ManagedPointer(this.next_end_old_type));
          return true;
        }
        #endregion

        #region Implementation of ICodeQuery<Label,Local,Parameter,Method,Field,Type,int,bool>
        public bool Aggregate(Label pc, Label aggregateStart, bool canBeTargetOfBranch, int data)
        {
          return Nop(pc, data);
        }
        #endregion

        #region ICodeQuery<Label,Local,Parameter,Method,Field,Type,int,bool> Members
        public override bool Nop(Label pc, int data)
        {
          return this.current_block.UsesOverriding;
        }

        public override bool BeginOld(Label pc, Label matchingEnd, int data)
        {
          this.state = ScanState.InsideOld;
          return this.current_block.UsesOverriding;
        }
        #endregion

        public override bool EndOld(Label pc, Label matchingEnd, Type type, Dummy dest, Dummy source, int data)
        {
          this.state = ScanState.OutsideOld;
          return this.current_block.UsesOverriding;
        }

        public void StartBlock(EnsuresBlock<Label> ensuresBlock)
        {
          throw new NotImplementedException();
        }

        #region Nested type: ScanState
        private enum ScanState
        {
          OutsideOld,
          InsideOld,
          InsertingOld,
          InsertingOldAfterCall
        }
        #endregion
      }
      #endregion

      #region Implementation of IEquatable<MethodRepository<Local,Parameter,Type,Method,Field,Property,Event,Attribute,Assembly>.EnsuresSubroutine<Label>>
      public bool Equals(EnsuresSubroutine<Label> other)
      {
        return Id == other.Id;
      }

      #endregion
    }
    #endregion

    #region Nested type: MethodSubroutine
    private class MethodSubroutine<Label, Handler> : SubroutineWithHandlers<Label, Handler>, IMethodInfo<Method>
    {
      public override SubroutineKind Kind
      {
        get
        {
          return SubroutineKind.Method;
        }
      }
      private readonly Method method;
      private HashSet<BlockWithLabels<Label>> blocks_ending_in_return_point;

      public MethodSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository, Method method)
        : base (methodRepository)
      {
        this.method = method;
      }

      public override bool HasReturnValue
      {
        get { return !this.method_repository.metadata_decoder.IsVoidMethod (this.method); }
      }

      public MethodSubroutine(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                              Method method, Label startLabel,
                              SubroutineWithHandlersBuilder<Label, Handler> builder) : base (methodRepository, startLabel, builder)
      {
        this.method = method;
        IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder = this.method_repository.metadata_decoder;
        builder.BuildBlocks (startLabel, this);
        BlockWithLabels<Label> targetBlock = GetTargetBlock (startLabel);
        Commit ();

        Type type = metadataDecoder.DeclaringType (method);
        Subroutine invariant = this.method_repository.GetInvariant (type);
        if (invariant != null && !metadataDecoder.IsConstructor (method) && !metadataDecoder.IsStatic (method)) {
          AddEdgeSubroutine (Entry, targetBlock, invariant, EdgeTag.Entry);
          Subroutine requires = this.method_repository.GetRequires (method);
          if (requires != null) {
            AddEdgeSubroutine (Entry, targetBlock, requires, EdgeTag.Entry);
            AddEdgeSubroutine (Entry, targetBlock, this.method_repository.GetRedundantInvariant (invariant, type), EdgeTag.Entry);
          }
        } else
          AddEdgeSubroutine (Entry, targetBlock, this.method_repository.GetRequires (method), EdgeTag.Entry);

        if (this.blocks_ending_in_return_point == null)
          return;

        Subroutine ensures = this.method_repository.GetEnsures (method);
        bool putInvariantAfterExit = !metadataDecoder.IsStatic (method) && !metadataDecoder.IsFinalizer (method) && !metadataDecoder.IsDispose (method);
        foreach (var block in this.blocks_ending_in_return_point) {
          if (putInvariantAfterExit)
            AddEdgeSubroutine (block, Exit, invariant, EdgeTag.Exit);
          AddEdgeSubroutine (block, Exit, ensures, EdgeTag.Exit);
        }

//        if (ensures != null) {
//          foreach (Subroutine oldEval in ensures.UsedSubroutines ()) {
//            if (oldEval.IsOldValue)
//              this.AddEdgeSubroutine (this.Entry, targetBlock, );
//          }
//        }
        this.blocks_ending_in_return_point = null;
      }

      #region Overrides of Subroutine
      public override void Initialize()
      {
      }
      #endregion

      #region Overrides of SubroutineBase<Label>
      public override void AddReturnBlock(BlockWithLabels<Label> block)
      {
        if (this.blocks_ending_in_return_point == null)
          this.blocks_ending_in_return_point = new HashSet<BlockWithLabels<Label>> ();

        this.blocks_ending_in_return_point.Add (block);

        base.AddReturnBlock (block);
      }
      #endregion

      #region Implementation of IMethodInfo<Method>
      public Method Method
      {
        get { return this.method; }
      }
      #endregion

      public override bool IsMethod
      {
        get { return true; }
      }

      public override bool IsConstructor
      {
        get { return this.method_repository.metadata_decoder.IsConstructor (this.method); }
      }

      public override string Name
      {
        get { return this.method_repository.metadata_decoder.FullName (this.method); }
      }
    }
    #endregion

    #region Nested type: SubroutineBase
    private abstract class SubroutineBase<Label> : Subroutine, IGraph<CFGBlock, Dummy>, IStackInfo, IEdgeSubroutineAdaptor
    {
      private const int UnusedBlockIndex = int.MaxValue - 1;
      protected int BlockIdGenerator;

      public override bool HasContextDependentStackDepth
      {
        get { return true; }
      }

      public override bool HasReturnValue
      {
        get { return false; }
      }
      private readonly Dictionary<Pair<CFGBlock, CFGBlock>, LispList<Pair<EdgeTag, Subroutine>>> edge_subroutines
        = new Dictionary<Pair<CFGBlock, CFGBlock>, LispList<Pair<EdgeTag, Subroutine>>>();

      private readonly BlockWithLabels<Label> entry;
      private readonly BlockWithLabels<Label> entry_after_requires;
      private readonly BlockWithLabels<Label> exception_exit;
      private readonly BlockWithLabels<Label> exit;

      protected readonly MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> method_repository;
      protected readonly Label StartLabel;
      private readonly List<Edge<CFGBlock, EdgeTag>> successors = new List<Edge<CFGBlock, EdgeTag>>();
      protected Dictionary<Label, BlockWithLabels<Label>> LabelsThatStartBlocks = new Dictionary<Label, BlockWithLabels<Label>> ();
      
      private CFGBlock[] blocks;
      private DepthFirst.Visitor<CFGBlock, Dummy> edge_info;
      private EdgeMap<EdgeTag> successor_edges;
      private EdgeMap<EdgeTag> predecessor_edges;

      protected SubroutineBase(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository)
      {
        this.method_repository = methodRepository;
        this.entry = new EntryBlock<Label> (this, ref this.BlockIdGenerator);
        this.exit = new EntryExitBlock<Label> (this, ref this.BlockIdGenerator);
        this.exception_exit = new CatchFilterEntryBlock<Label> (this, ref this.BlockIdGenerator);
      }

      protected SubroutineBase(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                               Label startLabel, SubroutineBuilder<Label> builder)
        : this (methodRepository)
      {
        this.StartLabel = startLabel;
        Builder = builder;
        CodeProvider = builder.CodeProvider;
        this.entry_after_requires = GetTargetBlock (startLabel);
        
        AddSuccessor (this.entry, EdgeTag.Entry, this.entry_after_requires);
      }

      public override int StackDelta
      {
        get { return 0; }
      }

      #region Main Blocks
      public override CFGBlock Entry { get { return this.entry; } }
      public override CFGBlock EntryAfterRequires {get { return this.entry_after_requires; } }
      public override CFGBlock Exit { get { return this.exit; } } 
      public override CFGBlock ExceptionExit { get { return this.exception_exit; } }
      #endregion

      public IEnumerable<CFGBlock> Nodes
      {
        get { return this.blocks; }
      }

      public virtual IEnumerable<Pair<Dummy, CFGBlock>> Successors(CFGBlock node)
      {
        foreach (var pair in this.successor_edges[node]) 
          yield return new Pair<Dummy, CFGBlock> (Dummy.Value, pair.Value);
        
        if (node != this.exception_exit) 
          yield return new Pair<Dummy, CFGBlock> (Dummy.Value, this.exception_exit);
      }

      protected SubroutineBuilder<Label> Builder { get; set; }

      public override DepthFirst.Visitor<CFGBlock, Dummy> EdgeInfo
      {
        get { return this.edge_info; }
      }

      public ICodeProvider<Label, Local, Parameter, Method, Field, Type> CodeProvider { get; private set; }

      public override int BlockCount
      {
        get { return this.blocks.Length; }
      }

      public override IEnumerable<CFGBlock> Blocks
      {
        get { return this.blocks; }
      }

      public override string Name
      {
        get { return "SR" + Id; }
      }

      public override EdgeMap<EdgeTag> SuccessorEdges
      {
        get { return this.successor_edges; }
      }

      public override EdgeMap<EdgeTag> PredecessorEdges
      {
        get
        {
          if (this.predecessor_edges == null)
            this.predecessor_edges = this.successor_edges.Reverse ();
          return this.predecessor_edges;
        }
      }

      #region IStackInfo Members
      bool IStackInfo.IsCallOnThis(APC apc)
      {
        return false;
      }
      #endregion

      public override IEnumerable<CFGBlock> SuccessorBlocks(CFGBlock block)
      {
        return SuccessorEdges[block].Select (it => it.Value);
      }

      public override bool HasSingleSuccessor(APC point, out APC ifFound)
      {
        if (point.Index < point.Block.Count) {
          ifFound = new APC (point.Block, point.Index + 1, point.SubroutineContext);
          return true;
        }

        if (IsSubroutineEnd (point.Block)) {
          if (point.SubroutineContext == null) {
            ifFound = APC.Dummy;
            return false;
          }

          ifFound = ComputeSubroutineContinuation (point);
          return true;
        }

        BlockWithLabels<Label> onlyOne = null;
        foreach (BlockWithLabels<Label> successor in point.Block.Subroutine.SuccessorBlocks (point.Block)) {
          if (onlyOne == null)
            onlyOne = successor;
          else {
            ifFound = APC.Dummy;
            return false;
          }
        }

        if (onlyOne != null) {
          ifFound = ComputeTargetFinallyContext (point, onlyOne);
          return true;
        }

        ifFound = APC.Dummy;
        return false;
      }

      public override bool HasSinglePredecessor(APC point, out APC ifFound)
      {
        if (point.Index > 0) {
          ifFound = new APC (point.Block, point.Index - 1, point.SubroutineContext);
          return true;
        }

        if (IsSubroutineStart (point.Block)) {
          if (point.SubroutineContext == null) {
            ifFound = APC.Dummy;
            return false;
          }

          bool hasSinglePredecessor;
          ifFound = ComputeSubroutinePreContinuation (point, out hasSinglePredecessor);
          return hasSinglePredecessor;
        }


        CFGBlock onlyOne = null;
        foreach (CFGBlock predecessor in point.Block.Subroutine.PredecessorBlocks (point.Block)) {
          if (onlyOne != null) {
            ifFound = APC.Dummy;
            return false;
          }
          onlyOne = predecessor;
        }
        if (onlyOne == null) {
          ifFound = APC.Dummy;
          return false;
        }

        LispList<Pair<EdgeTag, Subroutine>> list = EdgeSubroutinesOuterToInner(onlyOne, point.Block, point.SubroutineContext);
        if (list.IsEmpty ()) {
          ifFound = APC.ForEnd (onlyOne, point.SubroutineContext);
          return true;
        }

        var edge = new Edge<CFGBlock, EdgeTag>(onlyOne, point.Block, list.Head.Key);
        Subroutine sub = list.Head.Value;
        ifFound = APC.ForEnd (sub.Exit, point.SubroutineContext.Cons (edge));
        return true;
      }

      private APC ComputeSubroutinePreContinuation(APC point, out bool hasSinglePredecessor)
      {
        Edge<CFGBlock, EdgeTag> head = point.SubroutineContext.Head;
        bool isExceptionHandlerEdge;
        LispList<Edge<CFGBlock, EdgeTag>> tail = point.SubroutineContext.Tail;
        LispList<Pair<EdgeTag, Subroutine>> flist = EdgeSubroutinesOuterToInner(head.From, head.To, out isExceptionHandlerEdge, tail);
        while (flist.Head.Value != this)
          flist = flist.Tail;
        if (flist.Tail.IsEmpty ()) {
          if (isExceptionHandlerEdge && head.From.Count > 1) {
            hasSinglePredecessor = false;
            return APC.Dummy;
          }

          hasSinglePredecessor = true;
          return APC.ForEnd (head.From, tail);
        }
        Pair<EdgeTag, Subroutine> first = flist.Tail.Head;
        Subroutine sub = first.Value;
        hasSinglePredecessor = true;

        return APC.ForEnd(sub.Exit, point.SubroutineContext.Cons(new Edge<CFGBlock, EdgeTag>(head.From, head.To, first.Key)));
      }

      private APC ComputeSubroutineContinuation(APC point)
      {
        Edge<CFGBlock, EdgeTag> head = point.SubroutineContext.Head;
        LispList<Edge<CFGBlock, EdgeTag>> tail = point.SubroutineContext.Tail;
        LispList<Pair<EdgeTag, Subroutine>> outerToInner = EdgeSubroutinesOuterToInner(head.From, head.To, tail);
        if (outerToInner.Head.Value == this)
          return new APC (head.To, 0, tail);

        while (outerToInner.Tail.Head.Value != this)
          outerToInner = outerToInner.Tail;

        return new APC(outerToInner.Head.Value.Entry, 0, tail.Cons(new Edge<CFGBlock, EdgeTag>(head.From, head.To, outerToInner.Head.Key)));
      }

      public override IEnumerable<CFGBlock> PredecessorBlocks(CFGBlock block)
      {
        return PredecessorEdges[block].Select (it => it.Value);
      }

      public override bool IsJoinPoint(CFGBlock block)
      {
        if (IsCatchFilterHeader (block) || IsSubroutineStart (block) || IsSubroutineEnd (block))
          return true;

        return PredecessorEdges[block].Count > 1;
      }

      public override bool IsSubroutineEnd(CFGBlock block)
      {
        return block == this.exit || block == this.exception_exit;
      }

      public override bool IsSubroutineStart(CFGBlock block)
      {
        return block == this.entry;
      }

      public override bool IsSplitPoint(CFGBlock block)
      {
        if (IsSubroutineStart (block) || IsSubroutineEnd (block))
          return true;

        return SuccessorEdges[block].Count > 1;
      }

      public override bool IsCatchFilterHeader(CFGBlock block)
      {
        return block is CatchFilterEntryBlock<Label>;
      }

      public void AddSuccessor(CFGBlock from, EdgeTag tag, CFGBlock to)
      {
        AddNormalControlFlowEdge (this.successors, from, tag, to);
      }

      private void AddNormalControlFlowEdge(List<Edge<CFGBlock, EdgeTag>> succs, CFGBlock from, EdgeTag tag, CFGBlock to)
      {
        succs.Add(new Edge<CFGBlock, EdgeTag>(from, to, tag));
      }

      public virtual void AddReturnBlock(BlockWithLabels<Label> block)
      {
      }

      public BlockWithLabels<Label> GetTargetBlock(Label label)
      {
        return this.GetBlock (label);
      }

      public BlockWithLabels<Label> GetBlock(Label label)
      {
        var metadataDecoder = this.method_repository.metadata_decoder;

        BlockWithLabels<Label> block;
        if (!this.LabelsThatStartBlocks.TryGetValue(label, out block))
        {
          Pair<Method, bool> methodVirtualPair;
          Method constructor;

          if (Builder == null)
            throw new InvalidOperationException ("Builder must be not null");
          
          if (Builder.IsMethodCallSite(label, out methodVirtualPair))
          {
            var parametersCount = metadataDecoder.Parameters(methodVirtualPair.Key).Count;
            block = new MethodCallBlock<Label>(methodVirtualPair.Key, this, ref this.BlockIdGenerator, parametersCount, methodVirtualPair.Value);
          }
          else if (Builder.IsNewObjSite(label, out constructor))
          {
            var parametersCount = metadataDecoder.Parameters(constructor).Count;
            block = new NewObjCallBlock<Label>(constructor, parametersCount, this, ref this.BlockIdGenerator);
          }
          else
            block = this.NewBlock();
          
          if (this.Builder.IsTargetLabel (label))
            this.LabelsThatStartBlocks.Add (label, block);
        }
        return block;
      }

      public BlockWithLabels<Label> NewBlock()
      {
        return new BlockWithLabels<Label> (this, ref this.BlockIdGenerator);
      }

      public AssumeBlock<Label> NewAssumeBlock(Label pc, EdgeTag tag)
      {
        return new AssumeBlock<Label> (this, pc, tag, ref this.BlockIdGenerator);
      }

      public override sealed void AddEdgeSubroutine(CFGBlock from, CFGBlock to, Subroutine subroutine, EdgeTag tag)
      {
        if (subroutine == null)
          return;

        var key = new Pair<CFGBlock, CFGBlock> (from, to);
        LispList<Pair<EdgeTag, Subroutine>> list;
        var item = new Pair<EdgeTag, Subroutine> (tag, subroutine);

        this.edge_subroutines.TryGetValue (key, out list);
        this.edge_subroutines[key] = list.Cons (item);
      }

      public override IEnumerable<APC> Successors(APC pc)
      {
        APC singleNext;
        if (HasSingleSuccessor (pc, out singleNext)) 
          yield return singleNext;
        else
          foreach (CFGBlock block in pc.Block.Subroutine.SuccessorBlocks (pc.Block)) 
            yield return pc.Block.Subroutine.ComputeTargetFinallyContext (pc, block);
      }

      public override IEnumerable<APC> Predecessors(APC pc)
      {
        if (pc.Index > 0)
          yield return new APC (pc.Block, pc.Index - 1, pc.SubroutineContext);

        else if (IsSubroutineStart (pc.Block)) {
          if (!pc.SubroutineContext.IsEmpty ()) {
            foreach (APC apc in ComputeSubroutinePreContinuation (pc))
              yield return apc;
          }
        } else {
          foreach (CFGBlock block in pc.Block.Subroutine.PredecessorBlocks (pc.Block)) {
            LispList<Pair<EdgeTag, Subroutine>> diffs = EdgeSubroutinesOuterToInner(block, pc.Block, pc.SubroutineContext);
            if (diffs.IsEmpty ())
              yield return APC.ForEnd (block, pc.SubroutineContext);
            else {
              Subroutine sub = diffs.Head.Value;
              var edge = new Edge<CFGBlock, EdgeTag> (block, pc.Block, diffs.Head.Key);
              yield return APC.ForEnd (sub.Exit, pc.SubroutineContext.Cons (edge));
            }
          }
        }
      }

      private IEnumerable<APC> ComputeSubroutinePreContinuation(APC point)
      {
        Edge<CFGBlock, EdgeTag> edge = point.SubroutineContext.Head;
        LispList<Edge<CFGBlock, EdgeTag>> tail = point.SubroutineContext.Tail;

        bool isHandlerEdge;
        LispList<Pair<EdgeTag, Subroutine>> diffs = EdgeSubroutinesOuterToInner(edge.From, edge.To, out isHandlerEdge, tail);
        while (diffs.Head.Value != this)
          diffs = diffs.Tail;

        if (diffs.Tail == null) {
          if (isHandlerEdge) {
            for (int i = 0; i < edge.From.Count; i++)
              yield return new APC (edge.From, i, tail);
          } else yield return APC.ForEnd (edge.From, tail);
        } else {
          Pair<EdgeTag, Subroutine> first = diffs.Tail.Head;
          Subroutine nextSubroutine = first.Value;
          yield return APC.ForEnd(nextSubroutine.Exit, point.SubroutineContext.Cons(new Edge<CFGBlock, EdgeTag>(edge.From, edge.To, first.Key)));
        }
      }

      public override APC ComputeTargetFinallyContext(APC pc, CFGBlock succ)
      {
        LispList<Pair<EdgeTag, Subroutine>> list = EdgeSubroutinesOuterToInner(pc.Block, succ, pc.SubroutineContext);
        if (list.IsEmpty ())
          return new APC (succ, 0, pc.SubroutineContext);

        var last = list.Last ();
        return new APC (last.Value.Entry, 0, pc.SubroutineContext.Cons (new Edge<CFGBlock, EdgeTag> (pc.Block, succ, last.Key)));
      }

      private LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner(CFGBlock from, CFGBlock succ, LispList<Edge<CFGBlock, EdgeTag>> subroutineContext)
      {
        bool isExceptionHandlerEdge;
        return EdgeSubroutinesOuterToInner (from, succ, out isExceptionHandlerEdge, subroutineContext);
      }

      public override LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner(CFGBlock from, CFGBlock succ, out bool isExceptionHandlerEdge, LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        if (from.Subroutine != this)
          return from.Subroutine.EdgeSubroutinesOuterToInner (from, succ, out isExceptionHandlerEdge, context);

        isExceptionHandlerEdge = IsCatchFilterHeader (succ);
        return GetOrdinaryEdgeSubroutines (from, succ, context);
      }

      public override LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutines(CFGBlock from, CFGBlock to, LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        var metadataDecoder = this.method_repository.metadata_decoder;
        var apc = new APC (to, 0, context);

        DecoratorHelper.Push<SubroutineBase<Label>> (this);
        try {
          LispList<Pair<EdgeTag, Subroutine>> list = DecoratorHelper.Dispatch<IEdgeSubroutineAdaptor>(this).GetOrdinaryEdgeSubroutinesInternal(from, to, context);
          if (apc.InsideContract) {
            if (context != null && !list.IsEmpty ()) {
              Method calledMethod;
              bool isNewObj;
              bool isVirtual;
              if (@from.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual) && isVirtual && ((IStackInfo) this).IsCallOnThis (new APC (@from, 0, null))) {
                Type type = metadataDecoder.DeclaringType (calledMethod);
                do {
                  if (context.Head.Tag.StartsWith (EdgeTag.InheritedMask) || context.Head.Tag.StartsWith (EdgeTag.ExtraMask) || context.Head.Tag.StartsWith (EdgeTag.OldMask))
                    context = context.Tail;
                  else {
                    Method calledMethod2;
                    bool isNewObj2;
                    bool isVirtual2;
                    if (context.Head.Tag.StartsWith (EdgeTag.AfterMask) && context.Head.From.IsMethodCallBlock (out calledMethod2, out isNewObj2, out isVirtual2)) {
                      Type sub = metadataDecoder.DeclaringType (calledMethod2);
                      if (metadataDecoder.DerivesFrom (sub, type))
                        type = sub;
                      if (!DecoratorHelper.Dispatch<IStackInfo> (this).IsCallOnThis (new APC (context.Head.From, 0, null)))
                        break;
                    } else if (context.Head.Tag.StartsWith (EdgeTag.BeforeMask) && context.Head.To.IsMethodCallBlock (out calledMethod2, out isNewObj2, out isVirtual2)) {
                      Type sub = metadataDecoder.DeclaringType (calledMethod2);
                      if (metadataDecoder.DerivesFrom (sub, type))
                        type = sub;
                      if (!DecoratorHelper.Dispatch<IStackInfo>(this).IsCallOnThis(new APC(context.Head.To, 0, null)))
                        break;
                    } else if (context.Head.Tag == EdgeTag.Exit) {
                      var methodInfo = context.Head.From.Subroutine as IMethodInfo<Method>;
                      if (methodInfo != null) {
                        Type sub = metadataDecoder.DeclaringType (methodInfo.Method);
                        if (metadataDecoder.DerivesFrom (sub, type))
                          type = sub;
                      }
                      break;

                    } else {
                      if (context.Head.Tag != EdgeTag.Entry)
                        return list;
                      var methodInfo = context.Head.From.Subroutine as IMethodInfo<Method>;
                      if (methodInfo != null) {
                        Type sub = metadataDecoder.DeclaringType (methodInfo.Method);
                        if (metadataDecoder.DerivesFrom (sub, type))
                          type = sub;
                      } 
                      break;
                    }
                    context = context.Tail;
                  }
                } while (!context.IsEmpty ());
                Method implementingMethod;
                if (!metadataDecoder.Equal (type, metadataDecoder.DeclaringType (calledMethod)) &&
                    metadataDecoder.TryGetImplementingMethod (type, calledMethod, out implementingMethod))
                  list = SpecializedEnsures (list, this.method_repository.GetEnsures (calledMethod), this.method_repository.GetEnsures (implementingMethod));
              }
            }
          } else {
            Method calledMethod;
            bool isNewObj;
            bool isVirtual;
            if (@from.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual)) {
              if (DecoratorHelper.Dispatch<IStackInfo>(this).IsCallOnThis(new APC(@from, 0, null)))
              {
                var methodInfo = @from.Subroutine as IMethodInfo<Method>;
                if (methodInfo != null) {
                  Type bestType = metadataDecoder.DeclaringType (methodInfo.Method);
                  Method implementingMethod;
                  if (isVirtual && metadataDecoder.TryGetImplementingMethod (bestType, calledMethod, out implementingMethod))
                    list = SpecializedEnsures (list, this.method_repository.GetEnsures (calledMethod), this.method_repository.GetEnsures (implementingMethod));
                  list = InsertInvariant (@from, list, calledMethod, ref bestType, context);
                }
              } else {
                //todo: do nothing for now
              }
            }
          }
          return list;
        } finally {
          DecoratorHelper.Pop ();
        }

      }

      private LispList<Pair<EdgeTag, Subroutine>> InsertInvariant(CFGBlock from, LispList<Pair<EdgeTag, Subroutine>> list, Method calledMethod, ref Type type,
                                                               LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        var metadataDecoder = this.method_repository.metadata_decoder;

        Property property;
        if (metadataDecoder.IsPropertySetter (calledMethod, out property) && (metadataDecoder.IsAutoPropertyMember (calledMethod) || WithinConstructor (from, context)))
          return list;

        if (metadataDecoder.IsConstructor (calledMethod))
          type = metadataDecoder.DeclaringType (calledMethod);

        Subroutine invariant = this.method_repository.GetInvariant (type);
        if (invariant != null) {
          var methodCallBlock = from as MethodCallBlock<Label>;
          if (methodCallBlock != null) {
            EdgeTag first = methodCallBlock.IsNewObj ? EdgeTag.AfterNewObj : EdgeTag.AfterCall;
            return list.Cons (new Pair<EdgeTag, Subroutine> (first, invariant));
          }
        }

        return list;
      }

      private bool WithinConstructor(CFGBlock current, LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        return new APC (current, 0, context).InsideConstructor;
      }

      private LispList<Pair<EdgeTag, Subroutine>> SpecializedEnsures(LispList<Pair<EdgeTag, Subroutine>> subroutines, Subroutine toReplace, Subroutine specializedEnsures)
      {
        return subroutines.Select (pair => new Pair<EdgeTag, Subroutine> (pair.Key, pair.Value == toReplace ? specializedEnsures : pair.Value));
      }

      public LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutinesInternal(CFGBlock from, CFGBlock to, LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        LispList<Pair<EdgeTag, Subroutine>> list;
        this.edge_subroutines.TryGetValue (new Pair<CFGBlock, CFGBlock> (from, to), out list);
        if (list != null && context != null)
          list = list.Where (FilterRecursiveContracts (to, context));
        return list;
      }

      private static Predicate<Pair<EdgeTag, Subroutine>> FilterRecursiveContracts(CFGBlock from, LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        return (candidate) => {
                 Subroutine sub = candidate.Value;
                 if (!sub.IsContract)
                   return true;
                 if (sub == from.Subroutine)
                   return false;
                 if (context.Any (ctx => sub == ctx.From.Subroutine)) return false;
                 return true;
               };
      }

      public abstract override void Initialize();

      public virtual void Commit()
      {
        PostProcessBlocks ();
      }

      protected void PostProcessBlocks()
      {
        var blockStack = new Stack<CFGBlock> ();
        this.successor_edges = new EdgeMap<EdgeTag> (this.successors);
        this.edge_info = new DepthFirst.Visitor<CFGBlock, Dummy> (this, null, (block) => blockStack.Push (block), null);
        this.edge_info.VisitSubGraphNonRecursive (this.exception_exit);
        this.edge_info.VisitSubGraphNonRecursive (this.exit);
        this.edge_info.VisitSubGraphNonRecursive (this.entry);

        foreach (var successorEdge in this.successor_edges) {
          int idGen = UnusedBlockIndex;
          successorEdge.From.Renumber (ref idGen);
        }
        int idGen1 = 0;
        foreach (CFGBlock cfgBlock in blockStack) cfgBlock.Renumber (ref idGen1);

        SuccessorEdges.Filter ((e) => e.From.Index != UnusedBlockIndex);
        this.predecessor_edges = this.successor_edges.Reverse ();
        int finishTime = 0;
        var visitor = new DepthFirst.Visitor<CFGBlock, EdgeTag> (this.predecessor_edges, null, block => block.ReversePostOrderIndex = finishTime++, null);
        visitor.VisitSubGraphNonRecursive (this.exit);
        foreach (CFGBlock node in blockStack) 
          visitor.VisitSubGraphNonRecursive (node);

        SuccessorEdges.Resort ();
        this.blocks = blockStack.ToArray ();
        this.LabelsThatStartBlocks = null;
        Builder = null;
      }

      public override IEnumerable<Subroutine> UsedSubroutines(HashSet<int> alreadySeen)
      {
        foreach (var list in this.edge_subroutines.Values) {
          foreach (var pair in list.AsEnumerable ()) {
            Subroutine sub = pair.Value;
            if (!alreadySeen.Contains (sub.Id)) {
              alreadySeen.Add (sub.Id);
              yield return sub;
            }
          }
        }
      }

      public override IEnumerable<CFGBlock> ExceptionHandlers<Data,TType>(CFGBlock block, Subroutine innerSubroutine, Data data, IHandlerFilter<TType, Data> handlerPredicate)
      {
        yield return this.exception_exit;
      }

      public CFGBlock InferredBeginEndBijection(APC pc)
      {
        throw new NotImplementedException ();
      }

      public CFGBlock InferredBeginEndBijection(APC pc, out Type endOldType)
      {
        throw new NotImplementedException ();
      }

      public void AddInferredOldMap(int index, int count, CFGBlock beginBlock, Type nextEndOldType)
      {
        throw new NotImplementedException ();
      }

      public override void Print(System.IO.TextWriter tw)
      {
        LispList<Edge<CFGBlock, EdgeTag>> context = null;
        HashSet<Subroutine> subs = new HashSet<Subroutine> ();
        var methodInfo = this as IMethodInfo<Method>;
        string method = (methodInfo != null) ? string.Format ("({0})", this.method_repository.metadata_decoder.FullName (methodInfo.Method)) : null;
        
        tw.WriteLine ("Subroutine SR{0} {1} {2}", this.Id, this.Kind, method);
        tw.WriteLine("-------------");
        foreach (BlockWithLabels<Label> block in this.blocks) {
          tw.Write("Block {0} ({1})", block.Index, block.ReversePostOrderIndex);
          if (this.edge_info.DepthFirstInfo(block).TargetOfBackEdge)
            tw.WriteLine(" (target of backedge)");
          else if (this.IsJoinPoint (block))
            tw.WriteLine(" (join point)");
          else 
            tw.WriteLine();

          tw.Write("  Predecessors: ");
          foreach (var edge in block.Subroutine.PredecessorEdges[block]) {
            tw.Write ("({0}, {1}) ", edge.Key, edge.Value.Index);
          }
          tw.WriteLine();

          tw.Write("  Successors: ");
          foreach (var edge in block.Subroutine.SuccessorEdges[block]) {
            tw.Write ("({0}, {1}", edge.Key, edge.Value.Index);
            if (this.edge_info.IsBackEdge (block, Dummy.Value, edge.Value)) 
              tw.Write (" BE");

            for (LispList<Pair<EdgeTag, Subroutine>> list = this.GetOrdinaryEdgeSubroutines(block, edge.Value, context); list != null; list = list.Tail)
            {
              subs.Add (list.Head.Value);
              tw.Write (" SR{0}({1})", list.Head.Value.Id, list.Head.Key);
            }
            tw.Write (") ");
          }
          tw.WriteLine();
        }
      }

      public int GetILOffset(Label label)
      {
        return this.CodeProvider.GetILOffset (label);
      }
    }
    #endregion

    #region Nested type: SubroutineWithHandlers
    private abstract class SubroutineWithHandlers<Label, Handler> : SubroutineBase<Label>
    {
      private readonly Dictionary<Handler, BlockWithLabels<Label>> CatchFilterHeaders = new Dictionary<Handler, BlockWithLabels<Label>> ();
      private readonly Dictionary<Handler, Subroutine> FaultFinallySubroutines = new Dictionary<Handler, Subroutine> ();
      private readonly Dictionary<Handler, BlockWithLabels<Label>> FilterCodeBlocks = new Dictionary<Handler, BlockWithLabels<Label>> ();
      private readonly Dictionary<CFGBlock, LispList<Handler>> ProtectingHandlers = new Dictionary<CFGBlock, LispList<Handler>>();

      protected SubroutineWithHandlers(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository)
        : base (methodRepository)
      {
      }

      protected SubroutineWithHandlers(MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                       Label startLabel,
                                       SubroutineBuilder<Label> builder)
        : base (methodRepository, startLabel, builder)
      {
      }

      protected new IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> CodeProvider
      {
        get { return (IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler>) base.CodeProvider; }
      }

      private bool IsFault(Handler handler)
      {
        return CodeProvider.IsFaultHandler (handler);
      }

      public BlockWithLabels<Label> CreateCatchFilterHeader(Handler handler, Label label)
      {
        BlockWithLabels<Label> block;
        if (!this.LabelsThatStartBlocks.TryGetValue (label, out block)) {
          block = new CatchFilterEntryBlock<Label> (this, ref this.BlockIdGenerator);
          this.CatchFilterHeaders.Add (handler, block);
          this.LabelsThatStartBlocks.Add (label, block);
          if (CodeProvider.IsFilterHandler (handler)) {
            BlockWithLabels<Label> targetBlock = GetTargetBlock (CodeProvider.FilterExpressionStart (handler));
            this.FilterCodeBlocks.Add (handler, targetBlock);
          }
        }
        return block;
      }

      public override IEnumerable<Subroutine> UsedSubroutines(HashSet<int> alreadySeen)
      {
        return this.FaultFinallySubroutines.Values.Concat (base.UsedSubroutines (alreadySeen));
      }

      public override LispList<Pair<EdgeTag, Subroutine>> EdgeSubroutinesOuterToInner(CFGBlock current, CFGBlock succ, out bool isExceptionHandlerEdge, LispList<Edge<CFGBlock, EdgeTag>> context)
      {
        if (current.Subroutine != this)
          return current.Subroutine.EdgeSubroutinesOuterToInner (current, succ, out isExceptionHandlerEdge, context);

        LispList<Handler> l1 = ProtectingHandlerList(current);
        LispList<Handler> l2 = ProtectingHandlerList(succ);
        isExceptionHandlerEdge = IsCatchFilterHeader (succ);

        LispList<Pair<EdgeTag, Subroutine>> result = GetOrdinaryEdgeSubroutines(current, succ, context);

        while (l1 != l2) {
          if (l1.Length () >= l2.Length ()) {
            Handler head = l1.Head;
            if (IsFaultOrFinally (head) && (!IsFault (head)) || isExceptionHandlerEdge)
              result = result.Cons (new Pair<EdgeTag, Subroutine> (EdgeTag.Finally, this.FaultFinallySubroutines[head]));
            l1 = l1.Tail;
          } else
            l2 = l2.Tail;
        }

        return result;
      }

      private bool IsFaultOrFinally(Handler handler)
      {
        return CodeProvider.IsFaultHandler (handler) || CodeProvider.IsFinallyHandler (handler);
      }

      private LispList<Handler> ProtectingHandlerList(CFGBlock block)
      {
        LispList<Handler> list;
        this.ProtectingHandlers.TryGetValue (block, out list);
        return list;
      }

      public override IEnumerable<Pair<Dummy, CFGBlock>> Successors(CFGBlock node)
      {
        foreach (var pair in SuccessorEdges[node])
          yield return new Pair<Dummy, CFGBlock> (Dummy.Value, pair.Value);

        foreach (Handler handler in ProtectingHandlerList (node).AsEnumerable ()) {
          if (!IsFaultOrFinally (handler))
            yield return new Pair<Dummy, CFGBlock> (Dummy.Value, this.CatchFilterHeaders[handler]);
        }
        if (node != ExceptionExit)
          yield return new Pair<Dummy, CFGBlock> (Dummy.Value, ExceptionExit);
      }

      public override IEnumerable<CFGBlock> ExceptionHandlers<Data,TType>(CFGBlock block, Subroutine innerSubroutine, Data data, IHandlerFilter<TType, Data> handlerPredicate)
      {
        var handleFilter = (IHandlerFilter<Type, Data>) handlerPredicate;
        LispList<Handler> list = ProtectingHandlerList(block);
        if (innerSubroutine != null && innerSubroutine.IsFaultFinally) {
          for (; list != null; list = list.Tail) {
            Handler handler = list.Head;
            if (IsFaultOrFinally (handler) && this.FaultFinallySubroutines[handler] == innerSubroutine) {
              list = list.Tail;
              break;
            }
          }
        }
        for (; list != null; list = list.Tail) {
          Handler handler = list.Head;
          if (!IsFaultOrFinally (handler)) {
            if (handleFilter != null) {
              bool stopPropagation = false;
              if (CodeProvider.IsCatchHandler (handler)) {
                if (handleFilter.Catch (data, CodeProvider.CatchType (handler), out stopPropagation))
                  yield return this.CatchFilterHeaders[handler];
              } else if (handleFilter.Filter (data, new APC (this.FilterCodeBlocks[handler], 0, null), out stopPropagation))
                yield return this.CatchFilterHeaders[handler];
              if (stopPropagation)
                yield break;
            } else
              yield return this.CatchFilterHeaders[handler];

            if (CodeProvider.IsCatchAllHandler (handler))
              yield break;
          }
        }
        yield return ExceptionExit;
      }
    }
    #endregion

    #endregion

    #region Subroutine Builders

    #region Nested type: SimpleSubroutineBuilder
    private class SimpleSubroutineBuilder<Label> : SubroutineBuilder<Label>
    {
      private SubroutineBase<Label> current_subroutine; 

      public SimpleSubroutineBuilder(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider,
                                     MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                     Label entry) : base (codeProvider, methodRepository, entry)
      {
        this.Initialize (entry);
      }

      #region Overrides of SubroutineBuilder<Label>
      protected override SubroutineBase<Label> CurrentSubroutine
      {
        get { return this.current_subroutine; }
      }
      #endregion

      public BlockWithLabels<Label> BuildBlocks(Label entry, SubroutineBase<Label> subroutine)
      {
        this.current_subroutine = subroutine;

        return base.BuildBlocks (entry);
      }

      protected override BlockWithLabels<Label> RecordInformationForNewBlock(Label currentLabel, BlockWithLabels<Label> previousBlock)
      {
        var block = this.CurrentSubroutine.GetBlock (currentLabel);
        if (previousBlock != null) {
          var newBlock = block;
          var prevBlock = previousBlock;
          if (block is MethodCallBlock<Label> && previousBlock is MethodCallBlock<Label>) {
            var ab = this.CurrentSubroutine.NewBlock ();
            this.RecordInformationSameAsOtherBlock (ab, previousBlock);
            newBlock = ab;
            prevBlock = ab;
            this.CurrentSubroutine.AddSuccessor (previousBlock, EdgeTag.FallThrough, ab);
            this.CurrentSubroutine.AddSuccessor (ab, EdgeTag.FallThrough, block);
          } else 
            this.CurrentSubroutine.AddSuccessor (previousBlock, EdgeTag.FallThrough, block);

          this.InsertPostConditionEdges (previousBlock, newBlock);
          this.InsertPreConditionEdges (prevBlock, block);
        }

        return block;
      }

    }
    #endregion

    #region Nested type: SubroutineBuilder
    private abstract class SubroutineBuilder<Label>
    {
      private readonly ICodeProvider<Label, Local, Parameter, Method, Field, Type> code_provider;
      private readonly Dictionary<Label, Pair<Method, bool>> labels_for_call_sites = new Dictionary<Label, Pair<Method, bool>> ();
      private readonly Dictionary<Label, Method> labels_for_new_obj_sites = new Dictionary<Label, Method> ();
      private readonly HashSet<Label> labels_starting_blocks = new HashSet<Label> ();
      private readonly MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> method_repository;
      private readonly HashSet<Label> target_labels = new HashSet<Label> ();

      protected SubroutineBuilder(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider,
                                  MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                  Label entry)
      {
        this.code_provider = codeProvider;
        this.method_repository = methodRepository;
        AddTargetLabel (entry);
      }

      public ICodeProvider<Label, Local, Parameter, Method, Field, Type> CodeProvider
      {
        get { return this.code_provider; }
      }

      protected abstract SubroutineBase<Label> CurrentSubroutine { get; }

      private void AddTargetLabel(Label target)
      {
        AddBlockStart (target);
        this.target_labels.Add (target);
      }

      private void AddBlockStart(Label target)
      {
        this.labels_starting_blocks.Add (target);
      }

      public void Initialize(Label entry)
      {
        new BlockStartGatherer (this).TraceAggregateSequentally (entry);
      }

      public bool IsBlockStart(Label label)
      {
        return this.labels_starting_blocks.Contains (label);
      }

      public bool IsTargetLabel(Label label)
      {
        return this.target_labels.Contains (label);
      }

      protected internal BlockWithLabels<Label> BuildBlocks(Label entry)
      {
        return BlockBuilder.BuildBlocks (entry, this);
      }

      protected virtual void RecordInformationSameAsOtherBlock(BlockWithLabels<Label> newBlock, BlockWithLabels<Label> currentBlock)
      {
      }

      protected virtual BlockWithLabels<Label> RecordInformationForNewBlock(Label currentLabel, BlockWithLabels<Label> previousBlock)
      {
        BlockWithLabels<Label> block = this.CurrentSubroutine.GetBlock (currentLabel);
        if (previousBlock != null) {
          var newBlock = block;
          var prevBlock = previousBlock;
          if (block is MethodCallBlock<Label> && previousBlock is MethodCallBlock<Label>) {
            var ab = this.CurrentSubroutine.NewBlock ();
            this.RecordInformationSameAsOtherBlock (ab, previousBlock);
            newBlock = ab;
            prevBlock = ab;
            this.CurrentSubroutine.AddSuccessor (previousBlock, EdgeTag.FallThrough, ab);
            this.CurrentSubroutine.AddSuccessor (ab, EdgeTag.FallThrough, block);
          } else
            this.CurrentSubroutine.AddSuccessor (previousBlock, EdgeTag.FallThrough, block);

          this.InsertPostConditionEdges (previousBlock, newBlock);
          this.InsertPreConditionEdges (prevBlock, block);
        }
        return block;
      }

      protected void InsertPreConditionEdges(BlockWithLabels<Label> previousBlock, BlockWithLabels<Label> newBlock)
      {
        var methodCallBlock = newBlock as MethodCallBlock<Label>;
        if (methodCallBlock == null || this.CurrentSubroutine.IsContract || this.CurrentSubroutine.IsOldValue)
          return;

        if (this.CurrentSubroutine.IsMethod) {
          IMethodInfo<Method> methodInfo = this.CurrentSubroutine as IMethodInfo<Method>;
          Property property;
          var mdDecoder = this.method_repository.metadata_decoder;
          if (methodInfo != null && mdDecoder.IsConstructor(methodInfo.Method) && mdDecoder.IsPropertySetter(methodCallBlock.CalledMethod, out property) && mdDecoder.IsAutoPropertyMember(methodCallBlock.CalledMethod))
            return;
        }

        EdgeTag callTag = methodCallBlock.IsNewObj ? EdgeTag.BeforeNewObj : EdgeTag.BeforeCall;
        Subroutine requires = this.method_repository.GetRequires(methodCallBlock.CalledMethod);

        this.CurrentSubroutine.AddEdgeSubroutine(previousBlock, newBlock, requires, callTag);
      }

      protected void InsertPostConditionEdges(BlockWithLabels<Label> previousBlock, BlockWithLabels<Label> newBlock)
      {
        var methodCallBlock = previousBlock as MethodCallBlock<Label>;
        if (methodCallBlock == null)
          return;
        if (this.CurrentSubroutine.IsMethod) {
          IMethodInfo<Method> methodInfo = this.CurrentSubroutine as IMethodInfo<Method>;
          Property property;
          var mdDecoder = this.method_repository.metadata_decoder;
          if (methodInfo != null && mdDecoder.IsConstructor(methodInfo.Method) && mdDecoder.IsPropertyGetter(methodCallBlock.CalledMethod, out property) && mdDecoder.IsAutoPropertyMember(methodCallBlock.CalledMethod))
            return;
        }

        EdgeTag callTag = methodCallBlock.IsNewObj ? EdgeTag.AfterNewObj : EdgeTag.AfterCall;
        Subroutine ensures = this.method_repository.GetEnsures (methodCallBlock.CalledMethod);
       
        this.CurrentSubroutine.AddEdgeSubroutine (previousBlock, newBlock, ensures, callTag);
      }

      public bool IsMethodCallSite(Label label, out Pair<Method, bool> methodVirtPair)
      {
        return this.labels_for_call_sites.TryGetValue (label, out methodVirtPair);
      }

      public bool IsNewObjSite(Label label, out Method constructor)
      {
        return this.labels_for_new_obj_sites.TryGetValue (label, out constructor);
      }

      #region Nested type: BlockBuilder
      private class BlockBuilder :
        DefaultILVisitor<Label, Local, Parameter, Method, Field, Type, Dummy, Dummy, BlockWithLabels<Label>, bool>,
        IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, BlockWithLabels<Label>, bool>
      {
        private readonly SubroutineBuilder<Label> parent;
        private BlockWithLabels<Label> current_block;

        private BlockBuilder(SubroutineBuilder<Label> parent)
        {
          this.parent = parent;
        }

        private SubroutineBase<Label> CurrentSubroutine
        {
          get { return this.parent.CurrentSubroutine; }
        }

        public static BlockWithLabels<Label> BuildBlocks(Label entry, SubroutineBuilder<Label> parent)
        {
          var builder = new BlockBuilder (parent);
          builder.TraceAggregateSequentally (entry);
          if (builder.current_block == null)
            return null;

          var subroutine = builder.CurrentSubroutine;

          subroutine.AddSuccessor (builder.current_block, EdgeTag.FallThroughReturn, subroutine.Exit);
          subroutine.AddReturnBlock (builder.current_block);

          return builder.current_block;
        }

        private void TraceAggregateSequentally(Label current)
        {
          do {
            if (this.parent.IsBlockStart (current))
              this.current_block = this.parent.RecordInformationForNewBlock (current, this.current_block);
            if (this.parent.code_provider.Decode<BlockBuilder, BlockWithLabels<Label>, bool> (current, this, this.current_block))
              this.current_block = null;
          } while (this.parent.code_provider.Next (current, out current));
        }

        #region Overrides of DefaultILVisitor<Label,Local,Parameter,Method,Field,Type,Dummy,Dummy,MethodRepository<Local,Parameter,Type,Method,Field,Property,Event,Attribute,Assembly>.BlockWithLabels<Label>,bool>
        public override bool Branch(Label pc, Label target, bool leavesExceptionBlock, BlockWithLabels<Label> data)
        {
          this.current_block.AddLabel (pc);
          CurrentSubroutine.AddSuccessor (this.current_block, EdgeTag.Branch, CurrentSubroutine.GetTargetBlock (target));

          return true;
        }

        public override bool BranchCond(Label pc, Label target, BranchOperator bop, Dummy value1, Dummy value2, BlockWithLabels<Label> data)
        {
          return HandleConditionalBranch (pc, target, true);
        }

        public override bool BranchFalse(Label pc, Label target, Dummy cond, BlockWithLabels<Label> data)
        {
          return HandleConditionalBranch (pc, target, false);
        }

        public override bool BranchTrue(Label pc, Label target, Dummy cond, BlockWithLabels<Label> data)
        {
          return HandleConditionalBranch (pc, target, true);
        }

        public override bool Throw(Label pc, Dummy exception, BlockWithLabels<Label> data)
        {
          this.current_block.AddLabel (pc);
          return true;
        }

        public override bool Rethrow(Label pc, BlockWithLabels<Label> data)
        {
          this.current_block.AddLabel (pc);
          return true;
        }

        public override bool EndFinally(Label pc, BlockWithLabels<Label> data)
        {
          this.current_block.AddLabel (pc);
          CurrentSubroutine.AddSuccessor (this.current_block, EdgeTag.EndSubroutine, CurrentSubroutine.Exit);
          return true;
        }

        public override bool Return(Label pc, Dummy source, BlockWithLabels<Label> data)
        {
          this.current_block.AddLabel (pc);
          CurrentSubroutine.AddSuccessor (this.current_block, EdgeTag.Return, CurrentSubroutine.Exit);
          CurrentSubroutine.AddReturnBlock (this.current_block);

          return true;
        }

        public override bool Nop(Label pc, BlockWithLabels<Label> data)
        {
          return false;
        }

        public override bool DefaultVisit(Label pc, BlockWithLabels<Label> data)
        {
          this.current_block.AddLabel (pc);
          return false;
        }

        private bool HandleConditionalBranch(Label pc, Label target, bool isTrueBranch)
        {
          this.current_block.AddLabel (pc);
          EdgeTag trueTag = isTrueBranch ? EdgeTag.True : EdgeTag.False;
          EdgeTag falseTag = isTrueBranch ? EdgeTag.False : EdgeTag.True;

          AssumeBlock<Label> trueBlock = CurrentSubroutine.NewAssumeBlock (pc, trueTag);
          this.parent.RecordInformationSameAsOtherBlock (trueBlock, this.current_block);
          CurrentSubroutine.AddSuccessor (this.current_block, trueTag, trueBlock);
          CurrentSubroutine.AddSuccessor (trueBlock, EdgeTag.FallThrough, CurrentSubroutine.GetTargetBlock (target));

          AssumeBlock<Label> falseBlock = CurrentSubroutine.NewAssumeBlock (pc, falseTag);
          this.parent.RecordInformationSameAsOtherBlock (falseBlock, this.current_block);
          CurrentSubroutine.AddSuccessor (this.current_block, falseTag, falseBlock);
          this.current_block = falseBlock;

          return false;
        }
        #endregion

        #region Implementation of ICodeQuery<Label,Local,Parameter,Method,Field,Type,MethodRepository<Local,Parameter,Type,Method,Field,Property,Event,Attribute,Assembly>.BlockWithLabels<Label>,bool>
        public bool Aggregate(Label pc, Label aggregateStart, bool canBeTargetOfBranch, BlockWithLabels<Label> data)
        {
          TraceAggregateSequentally (aggregateStart);
          return false;
        }
        #endregion
      }
      #endregion

      #region Nested type: BlockStartGatherer
      private class BlockStartGatherer : DefaultILVisitor<Label, Local, Parameter, Method, Field, Type, Dummy, Dummy, Dummy, bool>,
                                         IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, Dummy, bool>
      {
        private readonly SubroutineBuilder<Label> parent;

        public BlockStartGatherer(SubroutineBuilder<Label> parent)
        {
          this.parent = parent;
        }

        #region Overrides of DefaultILVisitor<Label,Local,Parameter,Method,Field,Type,Dummy,Dummy,Dummy,bool>
        public override bool Branch(Label pc, Label target, bool leavesExceptionBlock, Dummy data)
        {
          AddTargetLabel (target);
          return true;
        }

        public override bool BranchCond(Label pc, Label target, BranchOperator bop, Dummy value1, Dummy value2, Dummy data)
        {
          AddTargetLabel (target);
          return true;
        }

        public override bool BranchFalse(Label pc, Label target, Dummy cond, Dummy data)
        {
          AddTargetLabel (target);
          return true;
        }

        public override bool BranchTrue(Label pc, Label target, Dummy cond, Dummy data)
        {
          AddTargetLabel (target);
          return true;
        }

        public override bool EndFinally(Label pc, Dummy data)
        {
          return true;
        }

        public override bool Return(Label pc, Dummy source, Dummy data)
        {
          return true;
        }

        public override bool Rethrow(Label pc, Dummy data)
        {
          return true;
        }

        public override bool Throw(Label pc, Dummy exception, Dummy data)
        {
          return true;
        }

        public override bool DefaultVisit(Label pc, Dummy data)
        {
          return false;
        }
        #endregion

        #region Implementation of ICodeQuery<Label,Local,Parameter,Method,Field,Type,Dummy,bool>
        public bool Aggregate(Label pc, Label aggregateStart, bool canBeTargetOfBranch, Dummy data)
        {
          return TraceAggregateSequentally (aggregateStart);
        }
        #endregion

        public bool TraceAggregateSequentally(Label entry)
        {
          bool isCurrentBranches;
          bool isCurrentHasSuccessor;
          do {
            isCurrentBranches = this.parent.code_provider.Decode<BlockStartGatherer, Dummy, bool> (entry, this, Dummy.Value);
            isCurrentHasSuccessor = this.parent.code_provider.Next (entry, out entry);
            if (isCurrentBranches && isCurrentHasSuccessor)
              AddBlockStart (entry);
          } while (isCurrentHasSuccessor);

          return isCurrentBranches;
        }

        private void AddBlockStart(Label target)
        {
          this.parent.AddBlockStart (target);
        }

        private void AddTargetLabel(Label target)
        {
          this.parent.AddTargetLabel (target);
        }
      }
      #endregion
    }
    #endregion

    #region Nested type: SubroutineWithHandlersBuilder
    private class SubroutineWithHandlersBuilder<Label, Handler> : SubroutineBuilder<Label>
    {
      private readonly Method method;
      private LispList<SubroutineWithHandlers<Label, Handler>> subroutineStack;

      protected new IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> CodeProvider
      {
        get { return (IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler>) base.CodeProvider; }
      }

      public SubroutineWithHandlersBuilder(IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> codeProvider,
                                           MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository,
                                           Method method,
                                           Label entry)
        : base (codeProvider, methodRepository, entry)
      {
        this.method = method;
        this.ComputeTryBlockStartAndEndInfo (method);
        this.Initialize (entry);
      }

      private void ComputeTryBlockStartAndEndInfo(Method method)
      {
        foreach (var handler in this.CodeProvider.GetTryBlocks(method)) {
          throw new NotImplementedException();
        }
      }

      #region Overrides of SubroutineBuilder<Label>
      protected override SubroutineBase<Label> CurrentSubroutine
      {
        get { return this.subroutineStack.Head; }
      }
      #endregion

      public CFGBlock BuildBlocks(Label entry, SubroutineWithHandlers<Label, Handler> subroutine)
      {
        this.subroutineStack = LispList<SubroutineWithHandlers<Label, Handler>>.Cons(subroutine, null);

        return base.BuildBlocks (entry);
      }
    }
    #endregion

    #endregion
  }
}