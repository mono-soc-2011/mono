using System;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public struct Dummy : IEquatable<Dummy>
  {
    public static readonly Dummy Value;

    public bool Equals(Dummy other)
    {
      return true;
    }
  }
}