using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.AST;

namespace Mono.CodeContracts.CodeAnalysis.Core.ContractsExtraction
{
  public static class HelperMethods
  {
    public static Method IsMethodCall(Statement s)
    {
      if (s == null)
        return null;

      var expressionStatement = s as ExpressionStatement;
      if (expressionStatement == null)
        return null;

      var methodCall = expressionStatement.Expression as MethodCall;
      if (methodCall == null)
        return null;

      var binding = methodCall.Callee as MemberBinding;
      if (binding == null)
        return null;

      return binding.BoundMember as Method;
    }

    public static Local ExtractPreamble(Method method, ContractNodes contractNodes, Block contractInitializer, out Block postPreamble)
    {
      postPreamble = null;
      return null;
    }

    public static List<Statement> ExtractContractBlocks(List<Statement> blocks, int firstBlockIndex, int firstStmtIndex, int lastBlockIndex, int lastStmtIndex)
    {
      List<Statement> result = new List<Statement> ();
      Block firstBlock = (Block) blocks[firstBlockIndex];
      Block block = new Block (new List<Statement> ());
      if (firstBlock != null) {
        int cnt = firstBlockIndex == lastBlockIndex ? lastStmtIndex + 1 : firstBlock.Statements.Count;
        for (int i = firstStmtIndex; i < cnt; i++) {
          Statement stmt = firstBlock.Statements[i];
          block.Statements.Add (stmt);
          if (stmt != null)
            firstBlock.Statements[i] = null;
        }
      }
      result.Add (block);
      int nextIndex = firstBlockIndex + 1;
      if (nextIndex > lastBlockIndex)
        return result;
      Block newLastBlock = null;
      int lastFullBlockIndex = lastBlockIndex - 1;
      Block lastBlock = (Block) blocks[lastBlockIndex];
      if (lastBlock != null && lastStmtIndex == lastBlock.Statements.Count - 1) {
        lastFullBlockIndex = lastBlockIndex;
      } else {
        newLastBlock = new Block (new List<Statement> ());
        if (block.Statements != null && block.Statements.Count > 0) {
          Branch branch = block.Statements[block.Statements.Count - 1] as Branch;
          if (branch != null && branch.Target != null && branch.Target == lastBlock)
            branch.Target = newLastBlock;
        }
      }

      for (; nextIndex < lastFullBlockIndex; ++nextIndex) {
        Block curBlock = (Block) blocks[nextIndex];
        result.Add (curBlock);
        if (curBlock != null) {
          blocks[nextIndex] = null;
          if (newLastBlock != null && curBlock.Statements != null && curBlock.Statements.Count > 0) {
            Branch branch = curBlock.Statements[curBlock.Statements.Count - 1] as Branch;
            if (branch != null && branch.Target != null && branch.Target == lastBlock)
              branch.Target = newLastBlock;
          }
        }
      }

      if (newLastBlock != null) {
        for (int i = 0; i < lastStmtIndex + 1; i++) {
          newLastBlock.Statements.Add (lastBlock.Statements[i]);
          lastBlock.Statements[i] = null;
        }

        result.Add (newLastBlock);
      }
      return result;
    }

    public static bool IsCompilerGenerated(TypeNode type)
    {
      throw new System.NotImplementedException ();
    }

    public static int FindNextRealStatement(List<Statement> stmts, int beginIndex)
    {
      if (stmts == null || stmts.Count <= beginIndex)
        return -1;
      int index = beginIndex;
      while (index < stmts.Count && (stmts[index] == null || stmts[index].NodeType == NodeType.Nop))
        ++index;
      return index;
    }
  }
}