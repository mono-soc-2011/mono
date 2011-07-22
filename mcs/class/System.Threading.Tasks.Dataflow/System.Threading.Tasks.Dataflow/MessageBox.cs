// MessageBox.cs
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
	/* In MessageBox we store message that have been offered to us so that they can be
	 * later processed
	 */
	internal class MessageBox<TInput>
	{
		readonly CompletionHelper compHelper;
		readonly BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();

		public MessageBox (BlockingCollection<TInput> messageQueue, CompletionHelper compHelper)
		{
			this.compHelper = compHelper;
			this.messageQueue = messageQueue;
		}

		public DataflowMessageStatus OfferMessage (ITargetBlock<TInput> target,
		                                           DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				return DataflowMessageStatus.Declined;

			if (consumeToAccept) {
				bool consummed;
				if (!source.ReserveMessage (messageHeader, target))
					return DataflowMessageStatus.NotAvailable;
				messageValue = source.ConsumeMessage (messageHeader, target, out consummed);
				// TODO: find correct behavior
				if (!consummed)
					return DataflowMessageStatus.NotAvailable;
			}

			try {
				messageQueue.Add (messageValue);
			} catch (InvalidOperationException) {
				// This is triggered either if the underlying collection didn't accept the item
				// or if the messageQueue has been marked complete, either way it corresponds to a false
				return DataflowMessageStatus.DecliningPermanently;
			}
			EnsureProcessing ();
			return DataflowMessageStatus.Accepted;
		}

		protected virtual void EnsureProcessing ()
		{

		}

		public void Complete ()
		{
			// Make message queue complete
			messageQueue.CompleteAdding ();
			if (messageQueue.IsCompleted)
				compHelper.Complete ();
		}
	}
}

#endif
