using NUnit.Framework;
using FSM = Beatrate.PeachFSM.Machine<Beatrate.PeachFSM.Tests.EventTests.EventContext, Beatrate.PeachFSM.SingleUpdateMode>;

namespace Beatrate.PeachFSM.Tests
{
	public class EventTests
	{
		public class EventContext : ValidatedContext
		{
			public EventContext(StateMachineValidationHistory history) : base(history)
			{

			}
		}

		public class A : FSM.Base
		{

			public override void Enter(FSM.Control control)
			{
				control.ChangeTo<B>();
			}
		}

		public class B : FSM.Base
		{
			public B()
			{
				EventFilter.SupportEvent<Event>();
			}

			public override void React<TEvent>(FSM.Control control, TEvent e)
			{
				
			}
		}

		public struct Event
		{
			public int A;
		}

		[Test]
		public void CanReactToEvent()
		{
			var history = new StateMachineValidationHistory();
			var context = new EventContext(history);
			var machine =
				FSM.Root()
				[
					FSM.Composite()
					[
						FSM.State<A>().V(),

						FSM.State<B>().V()
					]
				].ToMachine(context);

			machine.Start();
			machine.React(new Event());
			
			var validation = history.GetCursor();
			
			validation.Enter<A>();
			validation.Enter<B>();
			//validation.React<A, Event>();
			validation.React<B, Event>();
			validation.IsDone();
		}
	}
}

