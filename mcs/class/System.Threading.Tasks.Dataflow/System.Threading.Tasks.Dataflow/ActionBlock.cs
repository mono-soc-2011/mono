// ActionBlock.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class ActionBlock<TInput> : ITargetBlock<TInput>, IDataflowBlock
	{
		static readonly ExecutionDataflowBlockOptions defaultOptions = new ExecutionDataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		Action<TInput> action;
		ExecutionDataflowBlockOptions dataflowBlockOptions;
		// TODO: take care of options
		BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		AtomicBoolean started = new AtomicBoolean ();
		Action processQueue;

		public ActionBlock (Action<TInput> action) : this (action, defaultOptions)
		{
			
		}

		public ActionBlock (Action<TInput> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			this.action = action;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.processQueue = ProcessQueue;
		}

		public ActionBlock (Func<TInput, Task> action) : this (action, defaultOptions)
		{
			throw new NotImplementedException ();
		}

		public ActionBlock (Func<TInput, Task> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			throw new NotImplementedException ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				return DataflowMessageStatus.Declined;
			try {
				messageQueue.Add (messageValue);
			} catch (InvalidOperationException e) {
				// This is triggered either if the underlying collection didn't accept the item
				// or if the messageQueue has been marked complete, either way it corresponds to a false
				return DataflowMessageStatus.DecliningPermanently;
			}
			EnsureProcessing ();
			return DataflowMessageStatus.Accepted;
		}

		void EnsureProcessing ()
		{
			if (!started.TryRelaxedSet ())
				return;

			Task[] tasks = new Task[dataflowBlockOptions.MaxDegreeOfParallelism];
			for (int i = 0; i < tasks.Length; ++i)
				tasks[i] = Task.Factory.StartNew (processQueue);
			Task.Factory.ContinueWhenAll (tasks, (_) => {
					started.Value = false;
					// Re-run ourselves in case of a race when data is available in the end
					if (messageQueue.Count > 0)
						EnsureProcessing ();
					else if (messageQueue.IsCompleted)
						compHelper.Complete ();
				});
		}

		void ProcessQueue ()
		{
			TInput data;
			while (messageQueue.TryTake (out data))
				action (data);
		}

		public void Complete ()
		{
			// Make message queue complete
			messageQueue.CompleteAdding ();
			if (messageQueue.IsCompleted)
				compHelper.Complete ();
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
