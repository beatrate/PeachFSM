using NUnit.Framework;

using FSM = Beatrate.PeachFSM.Machine<Beatrate.PeachFSM.Tests.SimpleTests.LightContext, Beatrate.PeachFSM.SingleUpdateMode>;

namespace Beatrate.PeachFSM.Tests
{
	public class SimpleTests
	{
		public class LightContext : ValidatedContext
		{
			public bool SwitchOn = false;

			public LightContext(StateMachineValidationHistory history) : base(history)
			{
				
			}
		}

		public class Main : FSM.Base
		{
			public override void Enter(FSM.Control control)
			{
				control.ChangeTo<Off>();
			}
		}

		public class On : FSM.Base
		{

		}

		public class Off : FSM.Base
		{
			public override void Update(FSM.Control control, SingleUpdateMode updateMode)
			{
				if(Context.SwitchOn)
				{
					control.ChangeTo<On>();
				}
			}
		}

		[Test]
		public void TestSimple()
		{
			var history = new StateMachineValidationHistory();
			var context = new LightContext(history);
			var machine =
				FSM.Root()
				[
					FSM.Composite()
					[
						FSM.State<Main>(),

						FSM.State<On>(),
						FSM.State<Off>()
					]

				].ToMachine(context);

			
			machine.Start();
			Assert.IsTrue(machine.IsActive<Main>());
			Assert.IsTrue(machine.IsActive<Off>());

			context.SwitchOn = true;
			machine.Update(default);
			Assert.IsTrue(machine.IsActive<On>());
			Assert.IsFalse(machine.IsActive<Off>());

			var validation = history.GetCursor();
			validation.Enter<Main>();
			validation.Enter<Off>();

			validation.Update<Main>();
			validation.Update<Off>();
			validation.Leave<Off>();
			validation.Enter<On>();
			validation.IsDone();
		}
	}
}

