// BufferBlock.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

#if NET_4_0 || MOBILE

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class BufferBlock<T> : IPropagatorBlock<T, T>, ITargetBlock<T>, IDataflowBlock, ISourceBlock<T>, IReceivableSourceBlock<T>
	{
		static readonly DataflowBlockOptions defaultOptions = new DataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		MessageBox<TInput> messageBox;
		DataflowBlockOptions dataflowBlockOptions;

		// With each call to LinkTo, targets get added and when the current one is disposed, the next in line is activated
		TargetBuffer<T> targets;

		public BufferBlock () : this (defaultOptions)
		{
			
		}

		public BufferBlock (DataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException (dataflowBlockOptions);

			this.dataflowBlockOptions = dataflowBlockOptions;
			this.processQueue = ProcessQueue;
			this.messageBox = new PassingMessageBox<TInput> (messageQueue, compHelper, processQueue, dataflowBlockOptions);
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<T> target, bool unlinkAfterOne)
		{
			return targets.AddTarget (target, unlinkAfterOne);
		}

		void ProcessQueue ()
		{
			
		}

		public void Complete ()
		{
			messageBox.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.Fault (ex);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}
	}
}

#endif
