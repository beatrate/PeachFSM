# PeachFSM
A HFSM library for Unity inspired by hfsm2.

# Features

## DSL style state machine definition
```cs
var machine =
	FSM.Root()
	[
		// Declare state with children.
		FSM.Composite()
		[
			// Declare the main state representing the parent state itself.
			FSM.State<Main>(),

			// Children states.
			FSM.State<On>(),
			FSM.State<Off>()
		]

	].ToMachine(context);
```

## State API
```cs
using FSM = Beatrate.PeachFSM.Machine<SomeContext, SomeUpdateMode>;

public class SomeContext
{
	public int N = 10;
}

public enum SomeUpdateMode
{
	Foo, Blah
}

public struct SomeEvent
{ }

public class AState : FSM.Base
{
	public AState()
	{
		// Each state needs to declare event types it wants to receive.
		EventFilter.SupportEvent<SomeEvent>();
	}

	// State entry. Supports recursive rerouting of the active state.
	public override void Enter(Control control)
	{
		// Control object lets you access any state instance.
		CState c = control.Access<CState();
		
		// As well as check their active status.
		if(control.IsActive<DState>())
		{
		}
		
		// States can request state changes, Default priority allows the state machine
		// to ignore a request if it's trying to activate a child state that has a
		// different active child state.
		control.ChangeTo<BState>(ChangeStatePriority.Default);
		
		// Always priority forces state transition in any case.
		control.ChangeTo<BState>(ChangeStatePriority.Always);
	}

	// State update. Supports requesting active state changes as well.
	public override void Update(Control control, SomeUpdateMode updateMode)
	{
		if(updateMode == SomeUpdateMode.Foo)
		{ }
	}

	// State exit. Doesn's support state changes.
	public override void Leave(Control control)
	{ }
	
	// Receive supported events in an allocation free manner.
	public override void React<TEvent>(Control control, TEvent e)
	{
		if(e is SomeEvent some)
		{
			// Handle the event.
		}
	}
}
```

## State machine API
```cs
// Initialize the state machine.
machine.Start();
// Can access state instances externally.
AState a = machine.Access<AState>();
// Can also check state active statuses.
if(machine.IsActive<CState>())
{ }
// Force transition externally.
machine.ChangeTo<CState>();
// Update active states.
machine.Update(SomeUpdateMode.Foo);
// Raise events on currently active states.
machine.React(new SomeEvent());
// Shut down the state machine.
machine.Stop();
```
