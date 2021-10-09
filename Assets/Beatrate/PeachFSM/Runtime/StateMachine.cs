using Beatrate.Core;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;


namespace Beatrate.PeachFSM
{
	public class EmptyContext
	{

	}

	public enum SingleUpdateMode
	{
		Default = 0
	}

	public class Machine<TContext, TUpdateMode>
		where TContext : class
		where TUpdateMode : System.Enum
	{
		public class DeclarationControl
		{
			public List<Base> Children { get; } = new List<Base>();
			
			public void DeclareChild(Base child)
			{
				Children.Add(child);
			}

			public void DeclareChildren(List<Base> children)
			{
				for(int childIndex = 0; childIndex < children.Count; ++childIndex)
				{
					Children.Add(children[childIndex]);
				}
			}

			public void Clear()
			{
				Children.Clear();
			}
		}

		public class Control
		{
			public enum ChangeStatePriority
			{
				Default = 0,
				Always = 1
			}

			public struct ChangeStateRequest
			{
				public Type StateType { get; private set; }
				public ChangeStatePriority Priority { get; private set; }

				public static ChangeStateRequest Create<TState>(ChangeStatePriority priority) where TState : Base
				{
					return new ChangeStateRequest
					{
						StateType = typeof(TState),
						Priority = priority
					};
				}

				public static ChangeStateRequest Create(Base state, ChangeStatePriority priority)
				{
					return new ChangeStateRequest
					{
						StateType = state.IdentifierType,
						Priority = priority
					};
				}
			}

			private readonly Machine<TContext, TUpdateMode> machine = null;

			public List<ChangeStateRequest> Requests { get; } = new List<ChangeStateRequest>();

			public Control(Machine<TContext, TUpdateMode> machine)
			{
				this.machine = machine;
			}

			public void ChangeTo<TState>(ChangeStatePriority priority = ChangeStatePriority.Default) where TState : Base
			{
				Requests.Add(ChangeStateRequest.Create<TState>(priority));
			}

			public void ChangeTo(Base state, ChangeStatePriority priority = ChangeStatePriority.Default)
			{
				Requests.Add(ChangeStateRequest.Create(state, priority));
			}

			public TState Access<TState>() where TState : Base
			{
				return machine.Access<TState>();
			}

			public bool IsActive<TState>() where TState : Base
			{
				return machine.IsActive<TState>();
			}

			public void Clear()
			{
				Requests.Clear();
			}
		}

		public class EventFilter
		{
			public List<Type> SupportedEvents { get; } = new List<Type>();

			public EventFilter SupportEvent<TEvent>()
			{
				SupportedEvents.Add(typeof(TEvent));
				return this;
			}

			public EventFilter SupportEvents(EventFilter other)
			{
				SupportedEvents.AddRange(other.SupportedEvents);
				return this;
			}

			public bool Supports<TEvent>()
			{
				Type eventType = typeof(TEvent);

				for(int i = 0; i < SupportedEvents.Count; ++i)
				{
					if(SupportedEvents[i] == eventType)
					{
						return true;
					}
				}

				return false;
			}
		}

		public class Base
		{
			public TContext Context { get; private set; }
			public virtual Type IdentifierType => GetType();
			public virtual Base IdentifiedInstance => this;
			public virtual bool IsComplex => false;

			public EventFilter EventFilter { get; } = new EventFilter();

			public virtual void Initialize(TContext context)
			{
				Context = context;
			}

			public virtual void DeclareStructure(DeclarationControl control)
			{

			}

			protected virtual void FilterEvents()
			{

			}

			public virtual void Enter(Control control)
			{

			}

			public virtual void Update(Control control, TUpdateMode updateMode)
			{

			}

			public virtual void Leave(Control control)
			{

			}

			public virtual void React<TEvent>(Control control, TEvent e)
			{

			}
		}

		public class RootState : Base
		{
			private Base state = null;

			public override bool IsComplex => true;
			public override Type IdentifierType => state.IdentifierType;
			public override Base IdentifiedInstance => state;

			public RootState this[Base state]
			{
				get
				{
					Assert.IsNotNull(state);

					this.state = state;

					return this;
				}
			}

			public override void Initialize(TContext context)
			{
				base.Initialize(context);

				state.Initialize(context);
				EventFilter.SupportEvents(state.EventFilter);
			}

			public override void DeclareStructure(DeclarationControl control)
			{
				state.DeclareStructure(control);
			}

			public override void Enter(Control control)
			{
				state.Enter(control);
			}

			public override void Leave(Control control)
			{
				state.Leave(control);
			}

			public override void Update(Control control, TUpdateMode updateMode)
			{
				state.Update(control, updateMode);
			}

			public override void React<TEvent>(Control control, TEvent e)
			{
				state.React(control, e);
			}
		}

		public class CompositeState : Base
		{
			private Base topState = null;
			private List<Base> children = new List<Base>();

			public override Type IdentifierType => topState.IdentifierType;
			public override Base IdentifiedInstance => topState;
			public override bool IsComplex => true;

			public CompositeState this[Base topState, params Base[] children]
			{
				get
				{
					Assert.IsNotNull(topState);
					Assert.IsNotNull(children);
					Assert.AreNotEqual(0, children.Length);

					this.topState = topState;
					this.children.AddRange(children);
					
					return this;
				}
			}

			public override void Initialize(TContext context)
			{
				base.Initialize(context);
				topState.Initialize(context);
				EventFilter.SupportEvents(topState.EventFilter);
			}

			public override void DeclareStructure(DeclarationControl control)
			{
				control.DeclareChildren(children);
				children.Clear();
				children = null;
			}

			public override void Enter(Control control)
			{
				topState.Enter(control);
			}

			public override void Leave(Control control)
			{
				topState.Leave(control);
			}

			public override void Update(Control control, TUpdateMode updateMode)
			{
				topState.Update(control, updateMode);
			}

			public override void React<TEvent>(Control control, TEvent e)
			{
				topState.React(control, e);
			}
		}

		private class HierarchyNode
		{
			public Base Parent { get; set; } = null;
			public List<Base> Children { get; } = new List<Base>();
		}

		private Base rootState = null;
		private List<Base> states = new List<Base>();
		private Dictionary<Type, int> stateIndices = new Dictionary<Type, int>();
		private List<HierarchyNode> hierarchyNodes = new List<HierarchyNode>();
		private List<Base> stateStack = new List<Base>();

		private List<Control> recycledControls = new List<Control>();

		public TContext Context { get; private set; }

		public Machine(RootState root, TContext context)
		{
			rootState = root;
			Context = context;

			BuildHierarchy();
			Initialize();
		}

		public static RootState Root()
		{
			return new RootState();
		}

		public static TState State<TState>() where TState : Base, new()
		{
			return new TState();
		}

		public static CompositeState Composite()
		{
			return new CompositeState();
		}

		public TState Access<TState>() where TState : Base
		{
			Type stateType = typeof(TState);
			Base state = null;
			if(stateIndices.TryGetValue(stateType, out int stateIndex))
			{
				state = states[stateIndex];
			}

			Assert.IsNotNull(state);

			return (TState)state.IdentifiedInstance;
		}

		public bool IsActive<TState>() where TState : Base
		{
			return IsActive(typeof(TState));
		}

		public bool IsActive(Base state)
		{
			return IsActive(state.IdentifierType);
		}

		private bool IsActive(Type stateType)
		{
			if(!stateIndices.TryGetValue(stateType, out int stateIndex))
			{
				stateIndex = -1;
			}

			Base state = stateIndex == -1 ? null : states[stateIndex];
			Assert.IsNotNull(state);

			for(int i = 0; i < stateStack.Count; ++i)
			{
				if(state.IdentifierType == stateStack[i].IdentifierType)
				{
					return true;
				}
			}

			return false;
		}

		private void BuildHierarchy()
		{
			var stack = new Stack<Base>();
			stack.Push(rootState);
			var declarationControl = new DeclarationControl();

			while(stack.Count != 0)
			{
				Base state = stack.Pop();
				// Only happens for the root state.
				if(!stateIndices.ContainsKey(state.IdentifierType))
				{
					stateIndices.Add(state.IdentifierType, states.Count);
					states.Add(state);
					hierarchyNodes.Add(new HierarchyNode());
				}

				HierarchyNode hierarchyNode = hierarchyNodes[stateIndices[state.IdentifierType]];
				declarationControl.Clear();
				state.DeclareStructure(declarationControl);

				for(int childIndex = 0; childIndex < declarationControl.Children.Count; ++childIndex)
				{
					Base child = declarationControl.Children[childIndex];
					stateIndices.Add(child.IdentifierType, states.Count);
					states.Add(child);
					hierarchyNodes.Add(new HierarchyNode { Parent = state });

					stack.Push(child);

					hierarchyNode.Children.Add(child);
				}

				declarationControl.Clear();
			}
		}

		private void Initialize()
		{
			for(int stateIndex = 0; stateIndex < states.Count; ++stateIndex)
			{
				Base state = states[stateIndex];
				state.Initialize(Context);
			}
		}

		public void Start()
		{
			if(IsActive(rootState))
			{
				return;
			}

			ChangeTo(rootState);
		}

		public void Stop()
		{
			if(!IsActive(rootState))
			{
				return;
			}

			PopStatesUpToDepth(0);
		}

		public void Update(TUpdateMode updateMode)
		{
			if(!IsActive(rootState))
			{
				return;
			}

			ProcessUpdates(updateMode);
		}

		public void ChangeTo<TState>() where TState : Base
		{
			var control = GetControl();
			control.ChangeTo<TState>();
			ProcessTransitions(control);
			RecycleControl(control);
		}

		public void React<TEvent>(TEvent e)
		{
			var control = GetControl();
			var stateStackCopy = BeListPool<Base>.Get();
			stateStackCopy.AddRange(stateStack);

			for(int depth = 0; depth < stateStack.Count; ++depth)
			{
				if(stateStack[depth] != stateStackCopy[depth])
				{
					break;
				}

				control.Clear();
				Base state = stateStack[depth];

				if(state.EventFilter.Supports<TEvent>())
				{
					state.React(control, e);
					ProcessTransitions(control);
				}
				
			}

			BeListPool<Base>.Return(stateStack);
			RecycleControl(control);
		}

		private void ChangeTo(Base state)
		{
			var control = GetControl();
			control.ChangeTo(state);
			ProcessTransitions(control);
			RecycleControl(control);
		}

		private void ProcessUpdates(TUpdateMode updateMode)
		{
			var control = GetControl();
			for(int depth = 0; depth < stateStack.Count; ++depth)
			{
				Base state = stateStack[depth];
				control.Clear();
				state.Update(control, updateMode);
				ProcessTransitions(control);
			}

			RecycleControl(control);
		}

		private void ProcessTransitions(Control control)
		{
			if(control.Requests.Count == 0)
			{
				control.Clear();
				return;
			}

			while(control.Requests.Count != 0)
			{
				var request = control.Requests[0];
				int targetDepth = FindInactiveDepth(request.StateType);
				Base targetState = states[stateIndices[request.StateType]];

				if(targetDepth < stateStack.Count && stateStack[targetDepth] == targetState)
				{
					control.Clear();
					return;
				}

				PopStatesUpToDepth(targetDepth);
				var path = BeListPool<Base>.Get();
				FindActivationPath(targetState, path, out int innermostActiveIndex);
				int startDepth = innermostActiveIndex == -1 ? 0 : innermostActiveIndex + 1;

				for(int i = startDepth; i < path.Count; ++i)
				{
					control.Clear();

					Base state = path[i];
					Base nextInPath = i < path.Count - 1 ? path[i + 1] : null;
					state.Enter(control);
					stateStack.Add(state);

					if(control.Requests.Count != 0)
					{
						var chainedRequest = control.Requests[0];
						if(i < path.Count - 1 && chainedRequest.Priority == Control.ChangeStatePriority.Default)
						{
							// Ignore.
						}
						else
						{
							break;
						}
					}

					control.Clear();
				}

				BeListPool<Base>.Return(path);
			}
		}

		private void FindActivationPath(Base state, List<Base> path, out int innermostActiveIndex)
		{
			path.Clear();

			Base current = state;

			while(current != null)
			{
				path.Add(current);
				current = hierarchyNodes[stateIndices[current.IdentifierType]].Parent;
			}

			path.Reverse();
			innermostActiveIndex = -1;

			for(int i = path.Count - 1; i >= 0; --i)
			{
				if(i < stateStack.Count && stateStack[i] == path[i])
				{
					innermostActiveIndex = i;
					break;
				}
			}
		}

		private int FindInactiveDepth(Type stateType)
		{
			int stateIndex = stateIndices[stateType];
			Base current = states[stateIndex];
			int depth = 0;

			while(true)
			{
				current = hierarchyNodes[stateIndices[current.IdentifierType]].Parent;
				if(current != null)
				{
					++depth;
				}
				else
				{
					break;
				}
			}

			return depth;
		}

		private void PopStatesUpToDepth(int outerDepth)
		{
			var control = GetControl();

			for(int depth = stateStack.Count - 1; depth >= 0 && depth >= outerDepth; --depth)
			{
				Base state = stateStack[depth];
				stateStack.RemoveAt(depth);
				state.Leave(control);
				control.Clear();
			}

			RecycleControl(control);
		}

		private Control GetControl()
		{
			if(recycledControls.Count != 0)
			{
				var control = recycledControls[recycledControls.Count - 1];
				recycledControls.RemoveAt(recycledControls.Count - 1);
				return control;
			}

			return new Control(this);
		}

		private void RecycleControl(Control control)
		{
			control.Clear();
			recycledControls.Add(control);
		}
	}

	public static class MachineUtility
	{
		public static Machine<TContext, TUpdateMode> ToMachine<TContext, TUpdateMode>(this Machine<TContext, TUpdateMode>.RootState state, TContext context)
			where TContext : class
			where TUpdateMode : System.Enum
		{
			return new Machine<TContext, TUpdateMode>(state, context);
		}
	}
}
