using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public class EGraph<Constant, AbstractValue> :
    IEGraph<Constant, AbstractValue, EGraph<Constant, AbstractValue>>
    where Constant : IEquatable<Constant>, IConstantInfo
    where AbstractValue : IAbstractDomainForEGraph<AbstractValue>, IEquatable<AbstractValue>
  {
    private static int egraphIdGenerator;
    private static EGraph<Constant, AbstractValue> BottomValue;
    public static bool DoIncrementalJoin = false;

    private readonly SymValue const_root;
    private readonly int egraph_id;
    private readonly EGraph<Constant, AbstractValue> parent;
    private readonly AbstractValue underlying_bottom_value;
    private readonly AbstractValue underlying_top_value;
    private readonly SymValue bottom_place_holder;
    private DoubleImmutableMap<SymValue, Constant, SymValue> term_map;
    private DoubleImmutableMap<SymValue, MultiEdge, LispList<SymValue>> multi_term_map;
    private IImmutableMap<SymValue, LispList<EGraphTerm<Constant>>> eq_term_map;
    private IImmutableMap<SymValue, EGraphTerm<Constant>> eq_multi_term_map;
    private IImmutableMap<SymValue, SymValue> forw_map;
    private IImmutableMap<SymValue, AbstractValue> abstract_value_map;
    private readonly int history_size;
    private int id_generator;
    private bool is_immutable;
    private readonly EGraph<Constant, AbstractValue> root;
    private LispList<Update> updates;

    public EGraph(AbstractValue topValue, AbstractValue bottomValue)
      :this(topValue, bottomValue, false)
    {
      if (BottomValue != null)
        return;
      BottomValue = new EGraph<Constant, AbstractValue> (topValue, bottomValue, false);
    }

    private EGraph(AbstractValue topValue, AbstractValue bottomValue, bool _)
    {
      this.egraph_id = egraphIdGenerator++;
      this.const_root = FreshSymbol ();

      this.term_map = DoubleImmutableMap<SymValue, Constant, SymValue>.Empty ();
      this.multi_term_map = DoubleImmutableMap<SymValue, MultiEdge, LispList<SymValue>>.Empty();
      this.abstract_value_map = ImmutableMap<SymValue, AbstractValue>.Empty ();
      this.forw_map = ImmutableMap<SymValue, SymValue>.Empty();
      this.eq_term_map = ImmutableMap<SymValue, LispList<EGraphTerm<Constant>>>.Empty();
      this.eq_multi_term_map = ImmutableMap<SymValue, EGraphTerm<Constant>>.Empty();

      this.bottom_place_holder = FreshSymbol();
      this.abstract_value_map = this.abstract_value_map.Add (this.bottom_place_holder, bottomValue);
      this.history_size = 1;
      this.is_immutable = false;
      this.parent = null;
      this.root = this;
      this.underlying_top_value = topValue;
      this.underlying_bottom_value = bottomValue;
      this.updates = null;
    }

    private EGraph(EGraph<Constant, AbstractValue> from)
    {
      this.egraph_id = egraphIdGenerator++;
      this.const_root = from.const_root;
      this.bottom_place_holder = from.bottom_place_holder;
      this.term_map = from.term_map;
      this.multi_term_map = from.multi_term_map;
      this.id_generator = from.id_generator;
      this.abstract_value_map = from.abstract_value_map;
      this.forw_map = from.forw_map;
      this.eq_term_map = from.eq_term_map;
      this.eq_multi_term_map = from.eq_multi_term_map;
      this.underlying_top_value = from.underlying_top_value;
      this.underlying_bottom_value = from.underlying_bottom_value;
      this.updates = from.updates;
      this.parent = from;
      this.root = from.root;
      this.history_size = from.history_size + 1;
      
      from.MarkAsImmutable ();
    }

    private int LastSymbolId
    {
      get { return this.id_generator; }
    }

    private bool IsImmutable
    {
      get { return this.is_immutable; }
    }

    private IEnumerable<Pair<SymValue, EGraphTerm<Constant>>> ValidMultiTerms
    {
      get
      {
        foreach (SymValue sv in this.eq_multi_term_map.Keys) {
          EGraphTerm<Constant> term = this.eq_multi_term_map[sv];
          if (IsValidMultiTerm (term))
            yield return new Pair<SymValue, EGraphTerm<Constant>> (sv, term);
        }
      }
    }

    private bool IsValidMultiTerm(EGraphTerm<Constant> term)
    {
      return LookupWithoutManifesting (term.Args, term.Function) != null;
    }

    private bool HasAllBottomFields(SymValue sv)
    {
      if (sv == null)
        return false;

      return this[sv].HasAllBottomFields;
    }

    #region Implementation of IAbstractDomain<EGraph<Constant,AbstractValue>>
    public EGraph<Constant, AbstractValue> Top
    {
      get { return new EGraph<Constant, AbstractValue> (this.underlying_top_value, this.underlying_bottom_value); }
    }

    public EGraph<Constant, AbstractValue> Bottom
    {
      get
      {
        if (BottomValue == null) {
          BottomValue = new EGraph<Constant, AbstractValue> (this.underlying_top_value, this.underlying_bottom_value);
          BottomValue.MarkAsImmutable ();
        }
        return BottomValue;
      }
    }

    public bool IsTop
    {
      get { return this.term_map.Keys2Count (this.const_root) == 0; }
    }

    public bool IsBottom
    {
      get { return this == BottomValue; }
    }

    public EGraph<Constant, AbstractValue> Join(EGraph<Constant, AbstractValue> that, bool widening, out bool weaker)
    {
      IMergeInfo info;
      EGraph<Constant, AbstractValue> join = Join (that, out info, widening);
      weaker = info.Changed;
      return join;
    }

    public EGraph<Constant, AbstractValue> Meet(EGraph<Constant, AbstractValue> that)
    {
      if (this == that || IsBottom || that.IsTop)
        return this;
      if (that.IsBottom || IsTop)
        return that;

      return this;
    }

    public bool LessEqual(EGraph<Constant, AbstractValue> that)
    {
      IImmutableMap<SymValue, LispList<SymValue>> forwardMap;
      IImmutableMap<SymValue, SymValue> backwardMap;

      return this.LessEqual (that, out forwardMap, out backwardMap);
    }

    public EGraph<Constant, AbstractValue> ImmutableVersion()
    {
      MarkAsImmutable();
      return this;
    }

    public bool LessEqual(EGraph<Constant, AbstractValue> that, out IImmutableMap<SymValue, LispList<SymValue>> forward, out IImmutableMap<SymValue, SymValue> backward)
    {
      if (!this.IsSameEGraph (that))
        return InternalLessEqual (this, that, out forward, out backward);

      forward = null;
      backward = null;
      return true;
    }

    private static bool InternalLessEqual(EGraph<Constant, AbstractValue> thisG, EGraph<Constant, AbstractValue> thatG, out IImmutableMap<SymValue, LispList<SymValue>> forward, out IImmutableMap<SymValue, SymValue> backward)
    {
      int updateSize;
      EGraph<Constant, AbstractValue> commonTail = ComputeCommonTail (thisG, thatG, out updateSize);
      if (thisG.IsImmutable)
        thisG = thisG.Clone ();

      var workList = new WorkList<EqPair>();
      workList.Add (new EqPair (thisG.const_root, thatG.const_root));
      IImmutableSet<SymValue> backwardManifested = ImmutableSet<SymValue>.Empty ();
      IImmutableMap<SymValue, SymValue> funcMap1 = ImmutableMap<SymValue, SymValue>.Empty ();
      IImmutableMap<SymValue, LispList<SymValue>> funcMap2 = ImmutableMap<SymValue, LispList<SymValue>>.Empty();
      IImmutableMap<SymValue, int> triggers = ImmutableMap<SymValue, int>.Empty ();

      while (!workList.IsEmpty ()) {
        EqPair eqPair = workList.Pull ();
        SymValue sv1 = eqPair.v1;
        SymValue sv2 = eqPair.v2;

        SymValue s;
        if (VisitedBefore(sv2, backwardManifested, funcMap1, out s)) {
          if (s == null || s != sv1) {
            Console.WriteLine ("---LessEqual fails due to pre-existing relation: {0} <- {1}", s, sv2);
            forward = null;
            backward = null;
            return false;
          }
        } else {
          AbstractValue thisAV = sv1 == null ? thisG.underlying_top_value.ForManifestedField () : thisG[sv1];
          AbstractValue thatAV = thatG[sv2];
          if (!thisAV.LessEqual (thatAV)) {
            Console.WriteLine ("---LessEqual fails due to abstract values: !({0} <= {1})", thisAV, thatAV);
            forward = null;
            backward = null;
            return false;
          }

          if (sv1 != null)
          {
            funcMap1 = funcMap1.Add(sv2, sv1);
            funcMap2 = funcMap2.Add(sv1, funcMap2[sv1].Cons(sv2));
          }
          else
            backwardManifested = backwardManifested.Add (sv2);
          if (!thisG.HasAllBottomFields (sv1)) {
            if (thatG.HasAllBottomFields(sv2)) {
              Console.WriteLine ("---LessEqual fails due to bottom field difference");
              forward = null;
              backward = null;
              return false;
            }

            foreach (var function in thatG.Functions (sv2)) {
              SymValue v1 = thisG[function, sv1];
              SymValue v2 = thatG[function, sv2];
              Console.WriteLine ("    {0}-{1}->{2} <=? {3}-{4}->{5}", sv1, function, v1, sv2, function, v2);
              workList.Add (new EqPair (v1, v2));
            }

            foreach (var e in thatG.MultiEdges(sv2)) {
              foreach (var sv in thatG.multi_term_map[sv2, e].AsEnumerable ()) {
                if (UpdateTrigger(sv, e, ref triggers)) {
                  EGraphTerm<Constant> term = thatG.eq_multi_term_map[sv];
                  var args = new SymValue[term.Args.Length];
                  for (int i = 0; i < args.Length; i++)
                    args[i] = funcMap1[term.Args[i]];

                  SymValue v1 = thisG.LookupWithoutManifesting (args, e.Function); 
                  if (v1 == null) {
                    Console.WriteLine ("---LessEqual fails due to missing multi term {0}({1})", e.Function, string.Join (", ", term.Args.Select (it => it.ToString ())));
                    forward = null;
                    backward = null;
                    return false;
                  }
                  workList.Add (new EqPair (v1, sv));
                }
              }
            }
          }
        }
      }
      forward = funcMap2;
      backward = CompleteWithCommon (funcMap1, thisG, thatG, commonTail.id_generator);
      return true;
    }

    private static IImmutableMap<SymValue, SymValue> CompleteWithCommon(IImmutableMap<SymValue, SymValue> map, EGraph<Constant, AbstractValue> g1, EGraph<Constant, AbstractValue> g2, int lastCommonId)
    {
      foreach (var sv in g1.eq_term_map.Keys)
        if (IsCommon (sv, lastCommonId) && !map.ContainsKey (sv))
          map = map.Add (sv, sv);

      foreach (var sv in g1.eq_multi_term_map.Keys)
        if (IsCommon(sv, lastCommonId) && !map.ContainsKey(sv))
          map = map.Add(sv, sv);

      return map;
    }

    private static bool IsCommon(SymValue sv, int lastCommonId)
    {
      return sv.UniqueId <= lastCommonId;
    }

    private static bool UpdateTrigger(SymValue sv, MultiEdge edge, ref IImmutableMap<SymValue, int> triggers)
    {
      int val = triggers[sv] + 1;
      triggers = triggers.Add (sv, val);
      return (val == edge.Arity);
    }

    private IEnumerable<MultiEdge> MultiEdges(SymValue sv2)
    {
      return this.multi_term_map.Keys2 (sv2);
    }

    private static bool VisitedBefore(SymValue sv2, IImmutableSet<SymValue> backwardManifested, IImmutableMap<SymValue, SymValue> backward, out SymValue sv1)
    {
      sv1 = backward[sv2];
      return sv1 != null || backwardManifested.Contains (sv2);
    }

    private EGraph<Constant, AbstractValue> Clone()
    {
      return new EGraph<Constant, AbstractValue> (this);
    }

    private bool IsSameEGraph(EGraph<Constant, AbstractValue> that)
    {
      if (this == that)
        return true;
      if (that.parent == this)
        return that.updates == this.updates;

      return false;
    }

    private void MarkAsImmutable()
    {
      this.is_immutable = true;
    }
    #endregion

    #region Implementation of IEGraph<Constant,AbstractValue,EGraph<Constant,AbstractValue>>
    private SymValue this[SymValue arg, Constant function]
    {
      get
      {
        arg = this.Find (arg);
        SymValue sv = this.term_map[arg, function];
        SymValue key;
        if (sv == null) {
          key = FreshSymbol ();
          this.term_map = this.term_map.Add (arg, function, key);
          this.eq_term_map = this.eq_term_map.Add (key, LispList<EGraphTerm<Constant>>.Cons (new EGraphTerm<Constant> (function, arg), null));
          this.AddEdgeUpdate (arg, function);
        } else
          key = this.Find (sv);

        return key;
      }
      set { arg = this.Find (arg);
        value = this.Find (value);
        this.term_map = this.term_map.Add (arg, function, value);
        LispList<EGraphTerm<Constant>> rest = this.eq_term_map[value];
        if (rest.IsEmpty () || (!rest.Head.Function.Equals (function) || rest.Head.Args[0] != arg))
          this.eq_term_map = this.eq_term_map.Add (value, rest.Cons (new EGraphTerm<Constant> (function, arg)));

        this.AddEdgeUpdate (arg, function);
      }
    }

    private SymValue this[SymValue[] args, Constant function]
    {
      get
      {
        int len = args.Length;
        for (int i = 0; i < len; i++)
          args[i] = this.Find (args[i]);
        SymValue candidate = this.FindCandidate (args, function);
        if (candidate != null)
          return candidate;
        candidate = this.FreshSymbol ();
        for (int i = 0; i < len; i++) {
          var edge = new MultiEdge (function, i, len);
          this.multi_term_map = this.multi_term_map.Add (args[i], edge, this.multi_term_map[args[i], edge].Cons (candidate));
        }
        this.eq_multi_term_map = this.eq_multi_term_map.Add (candidate, new EGraphTerm<Constant> (function, args));
        this.AddMultiEdgeUpdate (args, function);
        return candidate;
      }
      set
      {
        int len = args.Length;
        for (int i = 0; i < len; i++)
          args[i] = this.Find (args[i]);
        bool flag = true;
        EGraphTerm<Constant> multiTerm = this.eq_multi_term_map[value];
        if (multiTerm.Args != null) {
          for (int i = 0; i < len; i++) {
            if (multiTerm.Args[i] != args[i]) {
              flag = false;
              break;
            }
          }
        }
        for (int i = 0; i < len; i++) {
          var edge = new MultiEdge(function, i, len);
          LispList<SymValue> list = this.multi_term_map[args[i], edge];
          if (flag && !LispList<SymValue>.Contains(list, value))
            flag = false;
          if (!flag)
            this.multi_term_map = this.multi_term_map.Add (args[i], edge, list.Cons (value));
        }
        if (flag)
          return;
        this.eq_multi_term_map = this.eq_multi_term_map.Add (value, new EGraphTerm<Constant> (function, args));
        this.AddMultiEdgeUpdate (args, function);
      }
    }

    public SymValue this[Constant function, SymValue arg]
    {
      get { return this[arg, function]; }
      set { this[arg, function] = value; }
    }

    public SymValue this[Constant function]
    {
      get { return this[this.const_root, function]; }
      set { this[this.const_root, function] = value; }
    }

    public AbstractValue this[SymValue symbol]
    {
      get
      {
        symbol = Find (symbol);
        if (this.abstract_value_map.ContainsKey (symbol))
          return this.abstract_value_map[symbol];

        return this.underlying_top_value;
      }
      set
      {
        SymValue newSym = Find (symbol);
        if (this[symbol].Equals (newSym))
          return;
        AddAbstractValueUpdate (newSym);
        if (value.IsTop)
          this.abstract_value_map = this.abstract_value_map.Remove (newSym);
        else
          this.abstract_value_map = this.abstract_value_map.Add (newSym, value);
      }
    }

    public IEnumerable<Constant> Constants
    {
      get { return this.term_map.Keys2 (this.const_root); }
    }

    public IEnumerable<SymValue> SymbolicValues
    {
      get { return this.term_map.Keys1; }
    }

    public SymValue TryLookup(Constant function, SymValue arg)
    {
      return LookupWithoutManifesting (arg, function);
    }

    public SymValue TryLookup(Constant function)
    {
      return LookupWithoutManifesting (this.const_root, function);
    }

    private SymValue LookupOrBottomPlaceHolder(SymValue arg, Constant function, out bool isPlaceHolder)
    {
      SymValue result = this.LookupWithoutManifesting(arg, function);

      isPlaceHolder = result == null;
      return result ?? this.bottom_place_holder;
    }

    private SymValue LookupWithoutManifesting(SymValue sv, Constant function)
    {
      if (sv == null)
        return null;
      sv = Find(sv);
      SymValue symValue = this.term_map[sv, function];

      if (symValue == null)
        return null;
      return Find(symValue);
    }

    private SymValue LookupWithoutManifesting(SymValue[] args, Constant function)
    {
      int length = args.Length;
      for (int i = 0; i < length; i++)
        args[i] = Find(args[i]);
      return FindCandidate(args, function);
    }

    public SymValue LookupOrManifest(Constant function, SymValue arg, out bool fresh)
    {
      int oldCnt = this.id_generator;
      SymValue result = this[function, arg];
      
      fresh = oldCnt < this.id_generator;
      return result;
    }

    public void AssumeEqual(SymValue v1, SymValue v2)
    {
      var workList = new WorkList<EqPair> ();
      SymValue sv1 = Find (v1);
      SymValue sv2 = Find (v2);
      PushEquality (workList, sv1, sv2);
      if (!workList.IsEmpty ())
        AddEqualityUpdate (sv1, sv2);

      DrainEqualityWorkList (workList);
    }

    public bool IsEqual(SymValue v1, SymValue v2)
    {
      return Find (v1) == Find (v2);
    }

    public void Eliminate(Constant function, SymValue arg)
    {
      SymValue value = Find (arg);
      DoubleImmutableMap<SymValue, Constant, SymValue> newTermMap = this.term_map.Remove (value, function);
      if (newTermMap == this.term_map)
        return;
      this.term_map = newTermMap;
      AddEliminateEdgeUpdate (value, function);
    }

    public void Eliminate(Constant function)
    {
      this.term_map = this.term_map.Remove (this.const_root, function);
      AddEliminateEdgeUpdate (this.const_root, function);
    }

    public void EliminateAll(SymValue arg)
    {
      SymValue value = Find (arg);
      AddEliminateAllUpdate (value);
      this.term_map = this.term_map.RemoveAll (value);
      this[arg] = this.underlying_top_value;
    }

    public SymValue FreshSymbol()
    {
      return new SymValue (++this.id_generator);
    }

    public EGraph<Constant, AbstractValue> Join(EGraph<Constant, AbstractValue> that, out IMergeInfo mergeInfo, bool widen)
    {
      EGraph<Constant, AbstractValue> egraph = this;
      int updateSize;
      EGraph<Constant, AbstractValue> commonTail = ComputeCommonTail (egraph, that, out updateSize);
      bool flag = true;
      if (commonTail == null)
        flag = false;


      EGraph<Constant, AbstractValue> result;
      MergeState mergeState;

      result = new EGraph<Constant, AbstractValue> (commonTail);
      mergeState = new MergeState (result, egraph, that, widen);
      mergeState.ReplayEliminations (commonTail);
      mergeState.AddMapping (egraph.const_root, that.const_root, result.const_root);
      mergeState.JoinSymbolicValue (egraph.const_root, that.const_root, result.const_root);
      mergeState.Commit ();

      mergeInfo = mergeState;

      return result;
    }

    public IEnumerable<Constant> Functions(SymValue symbol)
    {
      return this.term_map.Keys2 (Find (symbol));
    }

    public IEnumerable<EGraphTerm<Constant>> EqTerms(SymValue sv)
    {
      foreach (var term in this.eq_term_map[Find (sv)].AsEnumerable ()) if (TryLookup (term.Function, term.Args) == sv) yield return term;
    }

    public IEnumerable<EGraphTerm<Constant>> EqMultiTerms(SymValue sv)
    {
      EGraphTerm<Constant> term = this.eq_multi_term_map[sv];
      if (term.Args != null && this.IsValidMultiTerm(term))
        yield return term;
    }

    private bool IsOldSymbol(SymValue sv)
    {
      if (this.parent == null)
        return false;
      return sv.UniqueId <= this.parent.LastSymbolId;
    }

    private SymValue Find(SymValue symbol)
    {
      SymValue forwarded = this.forw_map[symbol];
      if (forwarded == null)
        return symbol;
      return Find (forwarded);
    }

    private void DrainEqualityWorkList(WorkList<EqPair> workList)
    {
      while (!workList.IsEmpty ()) {
        EqPair eqPair = workList.Pull ();
        SymValue sv1 = Find (eqPair.v1);
        SymValue sv2 = Find (eqPair.v2);
        if (sv1 != sv2) {
          if (sv1.UniqueId < sv2.UniqueId) {
            SymValue tmp = sv1;
            sv1 = sv2;
            sv2 = tmp;
          }

          foreach (Constant function in Functions (sv1)) {
            SymValue v2 = LookupWithoutManifesting (sv1, function);
            if (v2 == null)
              this[sv2, function] = this[sv1, function];
            else
              PushEquality (workList, this[sv1, function], v2);
          }
          AbstractValue thisValue = this[sv1];
          AbstractValue thatValue = this[sv2];
          foreach (var elem in this.eq_term_map[sv1].AsEnumerable ())
            this.eq_term_map = this.eq_term_map.Add (sv2, this.eq_term_map[sv2].Cons (elem));
          this.forw_map = this.forw_map.Add (sv1, sv2);
          this[sv2] = thisValue.Meet (thatValue);
        }
      }
    }

    

    private void PushEquality(WorkList<EqPair> workList, SymValue sv1, SymValue sv2)
    {
      if (sv1 != sv2) 
        workList.Add (new EqPair (sv1, sv2));
    }

    private static EGraph<Constant, AbstractValue> ComputeCommonTail(EGraph<Constant, AbstractValue> g1, EGraph<Constant, AbstractValue> g2, out int updateSize)
    {
      EGraph<Constant, AbstractValue> graph1 = g1;
      EGraph<Constant, AbstractValue> graph2 = g2;
      while (graph1 != graph2) {
        if (graph1 == null)
          break;
        if (graph2 == null) {
          graph1 = null;
          break;
        }
        if (graph1.history_size > graph2.history_size)
          graph1 = graph1.parent;
        else if (graph2.history_size > graph1.history_size)
          graph2 = graph2.parent;
        else {
          graph1 = graph1.parent;
          graph2 = graph2.parent;
        }
      }
      EGraph<Constant, AbstractValue> tail = graph1;
      int historySize = tail != null ? tail.history_size : 0;
      updateSize = g1.history_size + g2.history_size - 2*historySize;
      return tail;
    }

    private SymValue TryLookup(Constant function, params SymValue[] args)
    {
      if (args.Length == 0 || args.Length == 1)
        return LookupWithoutManifesting (this.const_root, function);
      return LookupWithoutManifesting (args, function);
    }

    private SymValue FindCandidate(SymValue[] args, Constant function)
    {
      int length = args.Length;
      var multiEdge = new MultiEdge (function, 0, length);
      for (LispList<SymValue> list = this.multi_term_map[args[0], multiEdge]; list != null; list = list.Tail) {
        EGraphTerm<Constant> term = this.eq_multi_term_map[list.Head];
        if (term.Args.Length == length) {
          bool found = true;
          for (int i = 0; i < length; ++i) {
            if (Find (term.Args[i]) != args[i]) {
              found = false;
              break;
            }
          }
          if (found)
            return list.Head;
        }
      }
      return null;
    }
    #endregion

    #region Merge updates
    private void AddUpdate(Update update)
    {
      this.updates = this.updates.Cons(update);
    }

    private void AddAbstractValueUpdate(SymValue sv)
    {
      if (this.IsOldSymbol(sv))
        this.AddUpdate(new MergeState.AbstractValueUpdate(sv));
    }

    private void AddEqualityUpdate(SymValue sv1, SymValue sv2)
    {
      if (this.IsOldSymbol(sv1) && this.IsOldSymbol(sv2))
        this.AddUpdate(new MergeState.EqualityUpdate(sv1, sv2));
    }

    private void AddEdgeUpdate(SymValue sv, Constant function)
    {
      if (this.IsOldSymbol(sv))
        this.AddUpdate(new MergeState.EdgeUpdate(sv, function));
    }

    private void AddEliminateAllUpdate(SymValue sv)
    {
      if (this.IsOldSymbol(sv))
        foreach (var function in this.term_map.Keys2(sv))
          this.AddUpdate(new MergeState.EliminateEdgeUpdate(sv, function));
    }

    private void AddEliminateEdgeUpdate(SymValue sv, Constant function)
    {
      if (this.IsOldSymbol(sv))
        AddUpdate(new MergeState.EliminateEdgeUpdate(sv, function));
    }

    private void AddMultiEdgeUpdate(SymValue[] from, Constant function)
    {
      for (int i = 0; i < from.Length; i++)
        if (!this.IsOldSymbol(from[i]))
          return;
      this.AddUpdate(new MergeState.MultiEdgeUpdate(from, function));
    }
    #endregion

    #region Nested type: EqPair
    public struct EqPair : IEquatable<EqPair>
    {
      public readonly SymValue v1;
      public readonly SymValue v2;

      public EqPair(SymValue v1, SymValue v2)
      {
        this.v1 = v1;
        this.v2 = v2;
      }

      #region Implementation of IEquatable<EGraph<Constant,AbstractValue>.EqPair>
      public bool Equals(EqPair other)
      {
        return (v1 == other.v1 && v2 == other.v2);
      }
      #endregion

      public override bool Equals(object obj)
      {
        if (obj is EqPair)
          return Equals ((EqPair) obj);
        return false;
      }

      public override int GetHashCode()
      {
        return v1 == null ? 1 : v1.GlobalId + this.v2.GlobalId;
      }
    }
    #endregion

    #region Nested type: MergeState
    private class MergeState : IMergeInfo
    {
      public readonly EGraph<Constant, AbstractValue> Graph1;
      public readonly EGraph<Constant, AbstractValue> Graph2;
      private readonly DoubleDictionary<SymValue, SymValue, int> PendingCounts;
      public readonly EGraph<Constant, AbstractValue> Result;
      private readonly HashSet<Tuple<SymValue, SymValue, MultiEdge>> VisitedMultiEdges = new HashSet<Tuple<SymValue, SymValue, MultiEdge>> ();
      private readonly int lastCommonVariable;

      private readonly HashSet<SymValue> manifested = new HashSet<SymValue> ();
      private readonly bool widen;
      private DoubleImmutableMap<SymValue, SymValue, SymValue> Map;
      private LispList<Tuple<SymValue, SymValue, SymValue>> tuples;
      private IImmutableSet<SymValue> visitedKey1;

      public MergeState(EGraph<Constant, AbstractValue> result, EGraph<Constant, AbstractValue> g1, EGraph<Constant, AbstractValue> g2, bool widen)
      {
        this.Result = result;
        this.Graph1 = g1;
        this.Graph2 = g2;
        this.Map = DoubleImmutableMap<SymValue, SymValue, SymValue>.Empty ();
        this.PendingCounts = new DoubleDictionary<SymValue, SymValue, int> ();
        this.visitedKey1 = ImmutableSet<SymValue>.Empty ();
        this.Changed = false;
        this.lastCommonVariable = result.id_generator;
        this.widen = widen;
      }

      public bool Changed { get; private set; }

      public IEnumerable<Tuple<SymValue, SymValue, SymValue>> MergeTriples
      {
        get { return this.tuples.AsEnumerable (); }
      }

      private bool UpdatePendingCount(SymValue xi, SymValue yi, int arity)
      {
        int result = 0;
        this.PendingCounts.TryGetValue (xi, yi, out result);
        result = result + 1;

        this.PendingCounts[xi, yi] = result;
        if (result == arity)
          return true;
        return false;
      }

      public bool IsCommon(SymValue sv)
      {
        return sv.UniqueId <= this.lastCommonVariable;
      }

      public bool AreCommon(SymValue[] svs)
      {
        return svs.All (sv => IsCommon (sv));
      }

      public void JoinSymbolicValue(SymValue sv1, SymValue sv2, SymValue r)
      {
        if (this.Graph2.HasAllBottomFields (sv2)) {
          if (sv1 != null) {
            foreach (Constant function in this.Graph1.term_map.Keys2 (sv1)) {
              SymValue v1 = this.Graph1.LookupWithoutManifesting (sv1, function);
              bool isPlaceHolder;
              SymValue v2 = this.Graph2.LookupOrBottomPlaceHolder (sv2, function, out isPlaceHolder);
              if (!isPlaceHolder || function.KeepAsBottomField) {
                SymValue r1 = AddJointEdge (v1, v2, function, r);
                if (r1 != null)
                  JoinSymbolicValue (v1, v2, r1);
              }
            }
          }
        } else if (!this.widen && this.Graph1.HasAllBottomFields (sv1)) {
          Changed = true;
          if (sv2 != null) {
            foreach (Constant function in this.Graph2.term_map.Keys2 (sv2)) {
              bool isPlaceHolder;
              SymValue v1 = this.Graph1.LookupOrBottomPlaceHolder (sv1, function, out isPlaceHolder);
              SymValue v2 = this.Graph2.LookupWithoutManifesting (sv2, function);
              if (!isPlaceHolder || function.KeepAsBottomField) {
                SymValue r1 = AddJointEdge (v1, v2, function, r);
                if (r1 != null) JoinSymbolicValue (v1, v2, r1);
              }
            }
          }
        } else {
          IEnumerable<Constant> functions;
          if (this.widen) {
            if (this.Graph1.term_map.Keys2Count (sv1) <= this.Graph2.term_map.Keys2Count (sv2)) functions = this.Graph1.term_map.Keys2 (sv1);
            else {
              functions = this.Graph2.term_map.Keys2 (sv2);
              Console.WriteLine ("---EGraph changed because G2 has fewer keys for {0} that {1} in G1", sv2, sv1);
              Changed = true;
            }
          } else if (this.Graph1.term_map.Keys2Count (sv1) < this.Graph2.term_map.Keys2Count (sv2)) {
            functions = this.Graph2.term_map.Keys2 (sv2);
            Console.WriteLine ("---EGraph changed because G1 has fewer keys for {0} that {1} in G2", sv1, sv2);
            Changed = true;
          } else
            functions = this.Graph1.term_map.Keys2 (sv1);

          foreach (Constant function in functions) {
            SymValue v1 = this.Graph1.LookupWithoutManifesting (sv1, function);
            SymValue v2 = this.Graph2.LookupWithoutManifesting (sv2, function);
            if (v1 == null) {
              if (!this.widen && function.ManifestField)
                Changed = true;
              else
                continue;
            }
            if (v2 == null && (this.widen || !function.ManifestField))
              Changed = true;
            else {
              SymValue r1 = AddJointEdge (v1, v2, function, r);
              if (r1 != null)
                JoinSymbolicValue (v1, v2, r1);
            }
          }
        }

        JoinMultiEdges (sv1, sv2);
      }

      private void JoinMultiEdges(SymValue sv1, SymValue sv2)
      {
        if (sv1 == null || sv2 == null)
          return;
        IEnumerable<MultiEdge> edges = this.Graph1.multi_term_map.Keys2Count (sv1) > this.Graph2.multi_term_map.Keys2Count (sv2)
                                         ? this.Graph2.multi_term_map.Keys2 (sv2)
                                         : this.Graph1.multi_term_map.Keys2 (sv1);
        foreach (MultiEdge edge in edges) JoinMultiEdge (sv1, sv2, edge);
      }

      private void JoinMultiEdge(SymValue sv1, SymValue sv2, MultiEdge edge)
      {
        var key = new Tuple<SymValue, SymValue, MultiEdge> (sv1, sv2, edge);
        if (this.VisitedMultiEdges.Contains (key))
          return;
        this.VisitedMultiEdges.Add (key);
        LispList<SymValue> list1 = this.Graph1.multi_term_map[sv1, edge];
        LispList<SymValue> list2 = this.Graph2.multi_term_map[sv2, edge];
        if (list2.IsEmpty ())
          return;
        foreach (SymValue v1 in list1.AsEnumerable ()) {
          foreach (SymValue v2 in list2.AsEnumerable ()) {
            if (UpdatePendingCount (v1, v2, edge.Arity)) {
              EGraphTerm<Constant> term1 = this.Graph1.eq_multi_term_map[v1];
              EGraphTerm<Constant> term2 = this.Graph2.eq_multi_term_map[v2];
              if (term1.Args != null && term2.Args != null) {
                var resultRoots = new SymValue[term1.Args.Length];
                for (int i = 0; i < resultRoots.Length; i++)
                  resultRoots[i] = this.Map[term1.Args[i], term2.Args[i]];
                SymValue r = AddJointEdge (v1, v2, edge.Function, resultRoots);
                if (r != null)
                  JoinSymbolicValue (sv1, sv2, r);
              } else
                break;
            }
          }
        }
      }

      private SymValue AddJointEdge(SymValue v1, SymValue v2, Constant function, SymValue[] resultRoots)
      {
        SymValue result = LookupMap (v1, v2);
        bool flag = false;
        if (result == null) {
          if (VisitedKey1 (v1, v2)) {
            Changed = true;
            if (v1 == null || v2 == null)
              return null;
          }
          flag = true;
          result = v1 == null || v1.UniqueId > this.lastCommonVariable || v1 != v2 ? this.Result.FreshSymbol () : v1;
          AddMapping (v1, v2, result);
        } else if (this.Result.LookupWithoutManifesting (resultRoots, function) == result)
          return null;
        this.Result[resultRoots, function] = result;
        AbstractValue g1AbstractValue = G1AbstractValue (v1);
        AbstractValue g2AbstractValue = G2AbstractValue (v2);
        
        bool weaker;
        AbstractValue joinValue = g1AbstractValue.Join (g2AbstractValue, this.widen, out weaker);

        this.Result[result] = joinValue;
        if (weaker) {
          Console.WriteLine (string.Format ("----EGraph changed due to join of abstract values of [{0}, {1}] (prev {2}, new {3}, join {4}", v1, v2, g1AbstractValue, g2AbstractValue, joinValue));
          this.Changed = true;
        }

        return flag ? result : null;
      }

      private AbstractValue G2AbstractValue(SymValue v2)
      {
        if (v2 == null)
          return this.Graph2.underlying_top_value.ForManifestedField ();
        return this.Graph2[v2];
      }

      private AbstractValue G1AbstractValue(SymValue v1)
      {
        if (v1 == null)
          return this.Graph1.underlying_top_value.ForManifestedField ();
        return this.Graph1[v1];
      }

      public void AddMapping(SymValue v1, SymValue v2, SymValue result)
      {
        if (v2 == null)
          this.visitedKey1 = this.visitedKey1.Add (v1);
        else if (v1 != null)
          this.Map = this.Map.Add (v1, v2, result);
        else
          this.visitedKey1 = this.visitedKey1.Add (v2);
        AddTuple (v1, v2, result);
      }

      private void AddTuple(SymValue v1, SymValue v2, SymValue result)
      {
        this.tuples = this.tuples.Cons (new Tuple<SymValue, SymValue, SymValue> (v1, v2, result));
      }

      private bool VisitedKey1(SymValue v1, SymValue v2)
      {
        if (v1 == null)
          return this.visitedKey1.Contains (v2);
        if (!this.visitedKey1.Contains (v1))
          return this.Map.ContainsKey1 (v1);

        return true;
      }

      private SymValue LookupMap(SymValue v1, SymValue v2)
      {
        if (v1 == null || v2 == null)
          return null;
        return this.Map[v1, v2];
      }

      private SymValue AddJointEdge(SymValue v1, SymValue v2, Constant function, SymValue resultRoot)
      {
        SymValue result = LookupMap (v1, v2);
        bool flag = false;
        if (result == null) {
          if (VisitedKey1 (v1, v2)) {
            Changed = true;
            if (v1 == null) {
              if (this.manifested.Contains (v2))
                return null;
              this.manifested.Add (v2);
            }
            if (v2 == null) {
              if (this.manifested.Contains (v1))
                return null;
              this.manifested.Add (v1);
            }
          }
          flag = true;
          result = v1 == null || v1.UniqueId > this.lastCommonVariable || v1 != v2 ? this.Result.FreshSymbol () : v1;
          AddMapping (v1, v2, result);
        } else if (this.Result.LookupWithoutManifesting (resultRoot, function) == null)
          return null;
        this.Result[resultRoot, function] = result;
        AbstractValue thisAV = G1AbstractValue (v1);
        AbstractValue thatAV = G2AbstractValue (v2);

        bool weaker;
        AbstractValue join = thisAV.Join (thatAV, this.widen, out weaker);
        this.Result[result] = join;

        if (weaker) {
          Console.WriteLine(string.Format("----EGraph changed due to join of abstract values of [{0}, {1}] (prev {2}, new {3}, join {4}", v1, v2, thisAV, thatAV, join));
          this.Changed = true;
        }

        return flag ? result : null;
      }

      public void Replay(EGraph<Constant, AbstractValue> common)
      {
        PrimeMapWithCommon ();
        ReplayEliminations (this.Graph1.updates, common.updates);
        ReplayEliminations (this.Graph2.updates, common.updates);
      }

      private void PrimeMapWithCommon()
      {
        LispList<SymValue> rest = null;
        foreach (SymValue sv in this.Graph1.eq_term_map.Keys) {
          if (IsCommon (sv) && (this.Graph2.eq_term_map.ContainsKey (sv) || this.Graph2.eq_multi_term_map.ContainsKey (sv))) {
            if (this.Graph1.multi_term_map.ContainsKey1 (sv))
              rest = rest.Cons (sv);
            AddMapping (sv, sv, sv);
          }
        }
        foreach (SymValue sv in this.Graph1.eq_multi_term_map.Keys) {
          if (IsCommon (sv) && (this.Graph2.eq_term_map.ContainsKey (sv) || this.Graph2.eq_multi_term_map.ContainsKey (sv)) && this.Map[sv, sv] == null) {
            if (this.Graph1.multi_term_map.ContainsKey1 (sv))
              rest = rest.Cons (sv);
            AddMapping (sv, sv, sv);
          }
        }
        while (rest != null) {
          SymValue sv = rest.Head;
          rest = rest.Tail;
          foreach (MultiEdge edge in this.Graph1.multi_term_map.Keys2 (sv)) JoinMultiEdge (sv, sv, edge);
        }
      }

      private void Replay(LispList<Update> updates, LispList<Update> common)
      {
        for (Update update = Update.Reverse (updates, common); update != null; update = update.Next)
          update.Replay (this);
      }

      public void ReplayEliminations(EGraph<Constant, AbstractValue> common)
      {
        ReplayEliminations (this.Graph1.updates, common.updates);
        ReplayEliminations (this.Graph2.updates, common.updates);
      }

      private void ReplayEliminations(LispList<Update> updates, LispList<Update> common)
      {
        for (Update update = Update.Reverse (updates, common); update != null; update = update.Next)
          update.ReplayElimination (this);
      }

      public void Commit()
      {
        if (Changed)
          return;

        bool needContinue = false;
        foreach (var edge in this.Graph1.ValidMultiTerms) {
          EGraphTerm<Constant> term = edge.Value;
          var args = new SymValue[term.Args.Length];

          for (int i = 0; i < args.Length; ++i) {
            SymValue sv = term.Args[i];
            if (VisitedKey1 (sv, null)) {
              if (this.Map.Keys2 (sv) != null && this.Map.Keys2 (sv).Count () == 1)
                args[i] = this.Map[sv, this.Map.Keys2 (sv).First ()];
            } else {
              needContinue = true;
              break;
            }

            if (args[i] == null) {
              Changed = true;
              return;
            }
          }

          if (needContinue)
            continue;

          SymValue symbol = this.Result.LookupWithoutManifesting (args, term.Function);
          if (symbol != null) {
            SymValue key = edge.Key;
            if (this.Map.Keys2 (key) != null && this.Map.Keys2 (key).Count () == 1 && this.Map[key, this.Map.Keys2 (key).First ()] == symbol)
              continue;
          }

          Changed = true;
          return;
        }
      }

      public class EdgeUpdate : Update
      {
        private readonly SymValue from;
        private readonly Constant function;

        public EdgeUpdate(SymValue from, Constant function)
        {
          this.from = from;
          this.function = function;
        }

        #region Overrides of Update
        public override void Replay(MergeState merge)
        {
          if (!merge.IsCommon (this.from))
            return;

          SymValue sv1 = merge.Graph1.LookupWithoutManifesting (this.from, this.function);
          SymValue sv2 = merge.Graph2.LookupWithoutManifesting (this.from, this.function);
          if (sv1 == null) {
            if (this.function.KeepAsBottomField && merge.Graph1.HasAllBottomFields (this.from))
              sv1 = merge.Graph1.bottom_place_holder;
            else {
              if (sv2 == null || merge.widen || !this.function.ManifestField)
                return;
              merge.Changed = true;
            }
          }
          if (sv2 == null) {
            if (this.function.KeepAsBottomField && merge.Graph2.HasAllBottomFields(this.from))
              sv2 = merge.Graph2.bottom_place_holder;
            else
            {
              if (merge.widen || !this.function.ManifestField)
                return;
              merge.Changed = true;
              return;
            }
          }

          SymValue r = merge.AddJointEdge (sv1, sv2, this.function, this.from);
          if (r == null || r.UniqueId <= merge.lastCommonVariable)
            return;

          merge.JoinSymbolicValue (sv1, sv2, r);
        }

        public override void ReplayElimination(MergeState merge)
        {}
        #endregion
      }

      public class MultiEdgeUpdate : Update
      {
        private readonly Constant function;
        private readonly SymValue[] @from;

        public MultiEdgeUpdate(SymValue[] from, Constant function)
        {
          this.function = function;
          this.from = from;
        }

        #region Overrides of Update
        public override void Replay(MergeState merge)
        {
          int len = this.from.Length;
          for (int i = 0; i < len; i++) {
            SymValue sv = this.from[i];
            if (merge.IsCommon(sv))
              merge.JoinMultiEdge (sv, sv, new MultiEdge (this.function, i, len));
          }
        }

        public override void ReplayElimination(MergeState merge)
        {
        }
        #endregion
      }

      public class AbstractValueUpdate : Update
      {
        private readonly SymValue sv;

        public AbstractValueUpdate(SymValue sv)
        {
          this.sv = sv;
        }

        #region Overrides of Update
        public override void Replay(MergeState merge)
        {
          if (!merge.IsCommon(sv))
            return;
          AbstractValue av1 = merge.Graph1[sv];
          AbstractValue av2 = merge.Graph2[sv];
          AbstractValue result = merge.Result[sv];
          bool weaker;
          AbstractValue join = av1.Join (av2, merge.widen, out weaker);

          if (weaker) {
            Console.WriteLine ("----EGraph changed during AbstractValueUpdate {3} due to weaker abstractValue join (prev {0}, new {1}, result {2}", av1, av2, result, sv);
            merge.Changed = true;
          }
          if (join.Equals (result))
            return;

          merge.Result[sv] = join;
        }

        public override void ReplayElimination(MergeState merge)
        {
          if (!merge.IsCommon(this.sv))
            return;

          AbstractValue av1 = merge.Graph1[this.sv];
          if (av1.IsTop)
            merge.Result[this.sv] = av1;
          else {
            AbstractValue av2 = merge.Graph2[this.sv];
            if (av2.IsTop)
              merge.Result[this.sv] = av2;
          }
        }
        #endregion
      }

      public class EqualityUpdate : Update
      {
        private readonly SymValue sv1;
        private readonly SymValue sv2;

        public EqualityUpdate(SymValue sv1, SymValue sv2)
        {
          this.sv1 = sv1;
          this.sv2 = sv2;
        }

        #region Overrides of Update
        public override void Replay(MergeState merge)
        {
          if (!merge.IsCommon(sv1) || !merge.IsCommon(sv2) || (!merge.Graph1.IsEqual(sv1, sv2) || merge.Result.IsEqual(sv1, sv2)))
            return;
          if (merge.Graph2.IsEqual (sv1, sv2))
            merge.Result.AssumeEqual (sv1, sv2);
          else
            merge.Changed = true;
        }

        public override void ReplayElimination(MergeState merge)
        {}
        #endregion
      }

      public class EliminateEdgeUpdate : Update
      {
        private readonly SymValue sv;
        private readonly Constant function;

        public EliminateEdgeUpdate(SymValue sv, Constant function)
        {
          this.sv = sv;
          this.function = function;
        }

        #region Overrides of Update
        public override void ReplayElimination(MergeState merge)
        {
          if (!merge.IsCommon(this.sv))
            return;
          merge.Result.Eliminate (this.function, sv);
        }

        public override void Replay(MergeState merge)
        {
          if (!merge.IsCommon(this.sv))
            return;

          SymValue sv1 = merge.Graph1.LookupWithoutManifesting (sv, function);
          SymValue sv2 = merge.Graph2.LookupWithoutManifesting (sv, function);
          if (sv1 != null && sv2 != null)
            return;
          if (sv1 != null)
            merge.Changed = true;
          if (merge.Result.LookupWithoutManifesting(sv, function) == null)
            return;

          merge.Result.Eliminate (function, sv);
        }
        #endregion
      }
    }
    #endregion

    #region Nested type: MultiEdge
    public struct MultiEdge : IEquatable<MultiEdge>
    {
      public readonly int Arity;
      public readonly Constant Function;
      public readonly int Index;

      public MultiEdge(Constant function, int index, int arity)
      {
        this.Function = function;
        this.Index = index;
        this.Arity = arity;
      }

      #region Implementation of IEquatable<MultiEdge>
      public bool Equals(MultiEdge other)
      {
        return true;
      }
      #endregion

      public override bool Equals(object obj)
      {
        if (obj is MultiEdge)
          return Equals ((MultiEdge) obj);
        return false;
      }

      public override int GetHashCode()
      {
        return this.Arity*13 + this.Index;
      }

      public override string ToString()
      {
        return string.Format ("[{0}:{1}]", this.Function, this.Index);
      }
    }
    #endregion

    #region Nested type: Update
    private abstract class Update
    {
      private Update next;

      public Update Next
      {
        get { return this.next; }
      }

      public abstract void Replay(MergeState merge);
      public abstract void ReplayElimination(MergeState merge);

      public static Update Reverse(LispList<Update> updates, LispList<Update> common)
      {
        Update update = null;
        for (; updates != common; updates = updates.Tail) {
          Update head = updates.Head;
          head.next = update;
          update = head;
        }
        return update;
      }
    }
    #endregion
  }
}