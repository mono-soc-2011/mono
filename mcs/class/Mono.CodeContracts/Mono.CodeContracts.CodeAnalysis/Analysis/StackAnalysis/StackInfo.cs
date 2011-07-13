namespace Mono.CodeContracts.CodeAnalysis.Analysis.StackAnalysis
{
  public struct StackInfo
  {
    private StackInfo<object> stack;

    public int Depth
    {
      get { return stack.Depth; }
    }

    public object this[int offset]
    {
      get { return this.stack[offset]; }
    }

    public StackInfo(int depth, int capacity)
    {
      stack = new StackInfo<object> (depth, capacity);
    }

    private StackInfo(StackInfo<object> copy)
    {
      this.stack = copy;
    }

    public StackInfo Pop(int slots)
    {
      return new StackInfo(this.stack.Pop (slots));
    }

    public StackInfo Push()
    {
      this.stack.Push (null);
      return this;
    }
    public StackInfo PushThis()
    {
      this.stack.Push (true);
      return this;
    }
    public StackInfo Push<Method>(Method method)
    {
      this.stack.Push (method);
      return this;
    }
    public void Adjust(int delta)
    {
      if (delta == 0)
        return;
      if (delta < 0)
        this.stack.Pop (-delta);
      for (int i = 0; i < delta; ++i)
        this.Push ();
    }

    public bool IsThis(int offset)
    {
      return this.As<bool> (offset);
    }

    public bool TryGet<T>(int offset, out T target)
    {
      var o = this[offset];
      if (o is T) {
        target = (T) o;
        return true;
      }
      target = default(T);
      return false;
    }

    private T As<T>(int offset)
    {
      var o = this[offset];
      return o is T ? (T) o : default(T);
    }

    public StackInfo Copy()
    {
      return new StackInfo(new StackInfo<object> (this.stack));
    }

    public override string ToString()
    {
      return this.stack.ToString ();
    }
  }

  public struct StackInfo<T>
  {
    private int depth;
    private T[] stack;

    public int Depth 
    {
      get { return this.depth; }
    }
    public T this[int offset]
    {
      get { int index = this.depth - 1 - offset;
        if (index >= 0 && index < this.stack.Length)
          return this.stack[index];
        return default(T);
      }
    }

    public StackInfo(int depth, int capacity)
    {
      this.depth = depth;
      this.stack = new T[capacity];
    }

    public StackInfo(StackInfo<T> that)
    {
      this.depth = that.depth;
      this.stack = (T[]) that.stack.Clone ();
    }

    public StackInfo<T> Pop(int slots)
    {
      for (int idx = this.depth - slots; idx < this.depth; ++idx) {
        if (idx < this.stack.Length)
          this.stack[idx] = default(T);
      }
      this.depth -= slots;
      return this;
    } 

    public void Push(T info)
    {
      int index = this.depth;
      if (index < this.stack.Length)
        this.stack[index] = info;
      ++this.depth;
    }

    public override string ToString()
    {
      return depth.ToString ();
    } 
  }
}