using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.AST;

namespace Mono.CodeContracts.CodeAnalysis.Core.ContractsExtraction
{
  public sealed class PreambleBlock : Block
  {
    public PreambleBlock(List<Statement> statements)
      : base(statements)
    {
      
    }
  }
}