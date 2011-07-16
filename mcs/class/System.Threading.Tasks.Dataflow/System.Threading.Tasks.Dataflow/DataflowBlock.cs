// IPropagatorBlock.cs
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
	public static class DataflowBlock
	{
		static DataflowMessageHeader globalHeader = new DataflowMessageHeader ();

		public static IObservable<TOutput> AsObservable<TOutput> (this ISourceBlock<TOutput> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ObservableDataflowBlock<TOutput> (source);
		}

		public static IObserver<TInput> AsObserver<TInput> (this ITargetBlock<TInput> target)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			return new ObserverDataflowBlock<TInput> (target);
		}

		public static Task<int> Choose<T1, T2> (ISourceBlock<T1> source1, Action<T1> action1, ISourceBlock<T2> source2, Action<T2> action2)
		{
			return Choose (source1, action1, source2, action2, DataflowBlockOptions.Unbounded);
		}

		public static Task<int> Choose<T1, T2> (ISourceBlock<T1> source1,
		                                        Action<T1> action1,
		                                        ISourceBlock<T2> source2,
		                                        Action<T2> action2,
		                                        DataflowBlockOptions dataflowBlockOptions)
		{
			throw new NotImplementedException ();
		}

		public static Task<int> Choose<T1, T2, T3> (ISourceBlock<T1> source1,
		                                            Action<T1> action1,
		                                            ISourceBlock<T2> source2,
		                                            Action<T2> action2,
		                                            ISourceBlock<T3> source3,
		                                            Action<T3> action3)
		{
			return Choose (source1, action1, source2, action2, source3, action3, DataflowBlockOptions.Unbounded);
		}

		public static Task<int> Choose<T1, T2, T3> (ISourceBlock<T1> source1,
		                                            Action<T1> action1,
		                                            ISourceBlock<T2> source2,
		                                            Action<T2> action2,
		                                            ISourceBlock<T3> source3,
		                                            Action<T3> action3,
		                                            DataflowBlockOptions dataflowBlockOptions)
		{
			throw new NotImplementedException ();		
		}

		public static IPropagatorBlock<TInput, TOutput> Encapsulate<TInput, TOutput> (ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
		{
			throw new NotImplementedException ();
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
		{
			return source.LinkTo (target, (_) => true);
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target, Predicate<TOutput> predicate)
		{
			return source.LinkTo (target, predicate, true);
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source,
		                                           ITargetBlock<TOutput> target,
		                                           Predicate<TOutput> predicate,
		                                           bool discardsMessages)
		{
			return source.LinkTo (target, false);
		}

		public static Task<bool> OutputAvailableAsync<TOutput> (this ISourceBlock<TOutput> source)
		{
			throw new NotImplementedException ();
		}

		public static bool Post<TInput> (this ITargetBlock<TInput> target, TInput item)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			return target.OfferMessage (globalHeader.Increment (), item, null, false) == DataflowMessageStatus.Accepted;
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source)
		{
			throw new NotImplementedException ();
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source)
		{
			throw new NotImplementedException ();
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public static bool TryReceive<TOutput> (this IReceivableSourceBlock<TOutput> source, out TOutput item)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
