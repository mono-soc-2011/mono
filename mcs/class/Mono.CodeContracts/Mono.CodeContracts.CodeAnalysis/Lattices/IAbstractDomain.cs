namespace Mono.CodeContracts.CodeAnalysis.Lattices
{

  /// <summary>
  /// Represents abstraction of concrete value
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IAbstractDomain<T>
  {
    /// <summary>
    /// Represents universe set (which holds every value)
    /// </summary>
    T Top { get; }

    /// <summary>
    /// Represents empty set (which holds nothing)
    /// </summary>
    T Bottom { get; }

    /// <summary>
    /// Is this value a universe set
    /// </summary>
    bool IsTop { get; }
    /// <summary>
    /// Is this value an empty set
    /// </summary>
    bool IsBottom { get; }

    /// <summary>
    /// Returns a union of this and that
    /// </summary>
    /// <param name="that"></param>
    /// <param name="widening">Specifies that widening-join operator must be used</param>
    /// <param name="weaker">Returns that result domain is weaker than this</param>
    /// <returns></returns>
    T Join(T that, bool widening, out bool weaker);
    
    /// <summary>
    /// Returns an intersection of this and that
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    T Meet(T that);

    bool LessEqual(T that);

    T ImmutableVersion();
  }
}
