using System.Collections.Generic;
using System.Linq;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class DoubleDictionary<A, B, C> : Dictionary<A, Dictionary<B, C>>
  {
    public C this[A a, B b]
    {
      get { return base[a][b]; }
      set
      {
        Dictionary<B, C> dict;
        if (!base.TryGetValue (a, out dict)) {
          dict = new Dictionary<B, C> ();
          base.Add (a, dict);
        }
        dict[b] = value;
      }
    }

    public IEnumerable<A> Keys1
    {
      get { return base.Keys; }
    }

    public void Add(A a, B b, C value)
    {
      Dictionary<B, C> dict;
      if (!base.TryGetValue (a, out dict)) {
        dict = new Dictionary<B, C> ();
        base.Add (a, dict);
      }
      dict.Add (b, value);
    }

    public IEnumerable<B> Keys2(A a)
    {
      Dictionary<B, C> dict;
      if (base.TryGetValue (a, out dict))
        return dict.Keys;
      return Enumerable.Empty<B> ();
    }

    public bool ContainsKey(A a, B b)
    {
      Dictionary<B, C> dict;
      if (!base.TryGetValue (a, out dict))
        return false;

      return dict.ContainsKey (b);
    }

    public bool TryGetValue(A a, B b, out C value)
    {
      Dictionary<B, C> dict;
      if (base.TryGetValue (a, out dict))
        return dict.TryGetValue (b, out value);

      value = default(C);
      return false;
    }
  }
}