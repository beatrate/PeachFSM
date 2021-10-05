using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Beatrate.PeachFSM.Tests
{
	public enum StateMachineCallback
	{
		Enter,
		Update,
		Leave,
		React
	}

	public struct StateMachineValidationEntry
	{
		public StateMachineCallback Callback;
		public Type StateType;
		public Type EventType;
	}
	
	public class StateMachineValidationHistory
	{
		public List<StateMachineValidationEntry> Entries = new List<StateMachineValidationEntry>();

		public void Enter<TState>()
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Enter,
				StateType = typeof(TState),
				EventType = null
			});
		}

		public void Update<TState>()
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Update,
				StateType = typeof(TState),
				EventType = null
			});
		}

		public void Leave<TState>()
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Leave,
				StateType = typeof(TState),
				EventType = null
			});
		}

		public void React<TState, TEvent>()
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Leave,
				StateType = typeof(TState),
				EventType = typeof(TEvent)
			});
		}

		public void Enter<TContext, TUpdateMode>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Enter,
				StateType = state.IdentifierType,
				EventType = null
			});
		}

		public void Update<TContext, TUpdateMode>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Update,
				StateType = state.IdentifierType,
				EventType = null
			});
		}

		public void Leave<TContext, TUpdateMode>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.Leave,
				StateType = state.IdentifierType,
				EventType = null
			});
		}

		public void React<TContext, TUpdateMode, TEvent>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			Entries.Add(new StateMachineValidationEntry
			{
				Callback = StateMachineCallback.React,
				StateType = state.IdentifierType,
				EventType = typeof(TEvent)
			});
		}
	}

	public class StateMachineValidationCursor
	{
		private readonly StateMachineValidationHistory history;

		public int Index { get; private set; } = 0;

		public StateMachineValidationCursor(StateMachineValidationHistory history)
		{
			this.history = history;
		}

		public void Enter<TState>()
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.Enter, entry.Callback);
			Assert.AreEqual(typeof(TState), entry.StateType);
		}

		public void Enter<TContext, TUpdateMode>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.Enter, entry.Callback);
			Assert.AreEqual(state.IdentifierType, entry.StateType);
		}

		public void Update<TState>()
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.Update, entry.Callback);
			Assert.AreEqual(typeof(TState), entry.StateType);
		}

		public void Update<TContext, TUpdateMode>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.Update, entry.Callback);
			Assert.AreEqual(state.IdentifierType, entry.StateType);
		}

		public void Leave<TState>()
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.Leave, entry.Callback);
			Assert.AreEqual(typeof(TState), entry.StateType);
		}

		public void Leave<TContext, TUpdateMode>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.Leave, entry.Callback);
			Assert.AreEqual(state.IdentifierType, entry.StateType);
		}

		public void React<TState, TEvent>()
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.React, entry.Callback);
			Assert.AreEqual(typeof(TState), entry.StateType);
			Assert.AreEqual(typeof(TEvent), entry.EventType);
		}

		public void React<TContext, TUpdateMode, TEvent>(Machine<TContext, TUpdateMode>.Base state)
			where TContext : class
			where TUpdateMode : Enum
		{
			var entry = history.Entries[Index++];
			Assert.AreEqual(StateMachineCallback.React, entry.Callback);
			Assert.AreEqual(typeof(TEvent), entry.StateType);
			Assert.AreEqual(typeof(TEvent), entry.EventType);
		}

		public void IsDone()
		{
			Assert.IsTrue(Index > history.Entries.Count - 1);
		}
	}

	public class ValidatedContext
	{
		public StateMachineValidationHistory History { get; set; }

		public ValidatedContext(StateMachineValidationHistory history)
		{
			History = history;
		}
	}

	public class ValidatedBase<TContext, TUpdateMode> : Machine<TContext, TUpdateMode>.Base
		where TContext : ValidatedContext
		where TUpdateMode : Enum
	{
		private Machine<TContext, TUpdateMode>.Base wrappedState = null;

		public override Type IdentifierType => wrappedState.IdentifierType;
		public override bool IsComplex => wrappedState.IsComplex;

		public ValidatedBase(Machine<TContext, TUpdateMode>.Base wrappedState)
		{
			this.wrappedState = wrappedState;
		}

		public override void Initialize(TContext context)
		{
			base.Initialize(context);
			
			wrappedState.Initialize(context);
			EventFilter.SupportEvents(wrappedState.EventFilter);
		}

		public override void Enter(Machine<TContext, TUpdateMode>.Control control)
		{
			Context.History.Enter(wrappedState);
			wrappedState.Enter(control);
		}

		public override void Leave(Machine<TContext, TUpdateMode>.Control control)
		{
			Context.History.Leave(wrappedState);
			wrappedState.Leave(control);
		}

		public override void Update(Machine<TContext, TUpdateMode>.Control control, TUpdateMode updateMode)
		{
			Context.History.Update(wrappedState);
			wrappedState.Update(control, updateMode);
		}

		public override void React<TEvent>(Machine<TContext, TUpdateMode>.Control control, TEvent e)
		{
			Context.History.React<TContext, TUpdateMode, TEvent>(wrappedState);
			wrappedState.React<TEvent>(control, e);
		}
	}

	public static class ValidationUtility
	{
		public static ValidatedBase<TContext, TUpdateMode> V<TContext, TUpdateMode>(this Machine<TContext, TUpdateMode>.Base wrappedState)
			where TContext : ValidatedContext
			where TUpdateMode : Enum
		{
			return new ValidatedBase<TContext, TUpdateMode>(wrappedState);
		}

		public static StateMachineValidationCursor GetCursor(this StateMachineValidationHistory history)
		{
			return new StateMachineValidationCursor(history);
		}
	}
}
