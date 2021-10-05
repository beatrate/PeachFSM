using NUnit.Framework;
using FSM = Beatrate.PeachFSM.Machine<Beatrate.PeachFSM.Tests.EventTests.EventContext, Beatrate.PeachFSM.SingleUpdateMode>;

namespace Beatrate.PeachFSM.Tests
{
	public class HierarchyTransitionsTests
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
		}

		public class C : FSM.Base
		{

		}

		public class D : FSM.Base
		{

		}
	}
}

