// TransformManyBlock.cs
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
	public sealed class TransformManyBlock<TInput, TOutput> :
		IPropagatorBlock<TInput, TOutput>, ITargetBlock<TInput>, IDataflowBlock, ISourceBlock<TOutput>, IReceivableSourceBlock<TOutput>
	{
		static readonly ExecutionDataflowBlockOptions defaultOptions = new ExecutionDataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		MessageBox<TInput> messageBox;
		MessageVault<TOutput> vault;
		ExecutionDataflowBlockOptions dataflowBlockOptions;
		readonly Func<TInput, IEnumerable<TOutput>> transformer;
		TargetBuffer<TOutput> targets = new TargetBuffer<TOutput> ();

		public TransformManyBlock (Func<TInput, IEnumerable<TOutput>> transformer) : this (transformer, defaultOptions)
		{

		}

		public TransformManyBlock (Func<TInput, IEnumerable<TOutput>> transformer, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.transformer = transformer;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.messageBox = new ExecutingMessageBox<TInput> (messageQueue, compHelper, TransformProcess, dataflowBlockOptions);
			this.vault = new MessageVault<TOutput> ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (this, messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<TOutput> target, bool unlinkAfterOne)
		{
			return targets.AddTarget (target, unlinkAfterOne);
		}

		public TOutput ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
		{
			return vault.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<TOutput> filter, out TOutput item)
		{
			// TODO
			item = default(TOutput);
			return false;
		}

		public bool TryReceiveAll (out IList<TOutput> items)
		{
			// TODO
			items = null;
			return false;
		}

		void TransformProcess ()
		{
			ITargetBlock<TOutput> target;
			TInput input;

			while (messageQueue.TryTake (out input) && (target = targets.Current) != null)
				foreach (var item in transformer (input))
					target.OfferMessage (messageBox.GetNextHeader (), item, this, false);
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
