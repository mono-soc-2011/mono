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
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class BufferBlock<T> : IPropagatorBlock<T, T>, ITargetBlock<T>, IDataflowBlock, ISourceBlock<T>, IReceivableSourceBlock<T>
	{
		static readonly DataflowBlockOptions defaultOptions = new DataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		BlockingCollection<T> messageQueue = new BlockingCollection<T> ();
		MessageBox<T> messageBox;
		MessageVault<T> vault;
		DataflowBlockOptions dataflowBlockOptions;

		// With each call to LinkTo, targets get added and when the current one is disposed, the next in line is activated
		TargetBuffer<T> targets = new TargetBuffer<T> ();

		public BufferBlock () : this (defaultOptions)
		{
			
		}

		public BufferBlock (DataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.dataflowBlockOptions = dataflowBlockOptions;
			this.messageBox = new PassingMessageBox<T> (messageQueue, compHelper, ProcessQueue, dataflowBlockOptions);
			this.vault = new MessageVault<T> ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           T messageValue,
		                                           ISourceBlock<T> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (this, messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<T> target, bool unlinkAfterOne)
		{
			var result = targets.AddTarget (target, unlinkAfterOne);
			ProcessQueue ();

			return result;
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
		{
			return vault.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<T> filter, out T item)
		{
			// TODO
			item = default(T);
			return false;
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			// TODO
			items = null;
			return false;
		}

		void ProcessQueue ()
		{
			ITargetBlock<T> target;
			if ((target = targets.Current) == null)
				return;
			T input;
			while (messageQueue.TryTake (out input))
				target.OfferMessage (messageBox.GetNextHeader (), input, this, false);
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

		public int Count {
			get {
				return messageQueue.Count;
			}
		}
	}
}

#endif
