using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class DropGestureRecognizerTests : BaseTestFixture
	{
		[Test]
		public void PropertySetters()
		{
			var dropRec = new DropGestureRecognizer() { AllowDrop = true };

			Command cmd = new Command(() => { });
			var parameter = new Object();
			dropRec.AllowDrop = true;
			dropRec.DragOverCommand = cmd;
			dropRec.DragOverCommandParameter = parameter;
			dropRec.DropCommand = cmd;
			dropRec.DropCommandParameter = parameter;

			Assert.AreEqual(true, dropRec.AllowDrop);
			Assert.AreEqual(cmd, dropRec.DragOverCommand);
			Assert.AreEqual(parameter, dropRec.DragOverCommandParameter);
			Assert.AreEqual(cmd, dropRec.DropCommand);
			Assert.AreEqual(parameter, dropRec.DropCommandParameter);
		}

		[Test]
		public void DragOverCommandFires()
		{
			var dropRec = new DropGestureRecognizer() { AllowDrop = true };
			var parameter = new Object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			dropRec.DragOverCommand = cmd;
			dropRec.DragOverCommandParameter = parameter;
			dropRec.SendDragOver(new DragEventArgs(new DataPackage()));

			Assert.AreEqual(parameter, commandExecuted);
		}

		[Test]
		public async Task DropCommandFires()
		{
			var dropRec = new DropGestureRecognizer() { AllowDrop = true };
			var parameter = new Object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			dropRec.DropCommand = cmd;
			dropRec.DropCommandParameter = parameter;
			await dropRec.SendDrop(new DropEventArgs(new DataPackageView(new DataPackage())));

			Assert.AreEqual(commandExecuted, parameter);
		}

		[TestCase(typeof(Entry), "EntryTest")]
		[TestCase(typeof(Label), "LabelTest")]
		[TestCase(typeof(Editor), "EditorTest")]
		[TestCase(typeof(TimePicker), "01:00:00")]
		[TestCase(typeof(CheckBox), "True")]
		[TestCase(typeof(Switch), "True")]
		[TestCase(typeof(RadioButton), "True")]
		public async Task TextPackageCorrectlySetsOnCompatibleTarget(Type fieldType, string result)
		{
			var dropRec = new DropGestureRecognizer() { AllowDrop = true };
			var element = (View)Activator.CreateInstance(fieldType);
			element.GestureRecognizers.Add(dropRec);
			var args = new DropEventArgs(new DataPackageView(new DataPackage() { Text = result }));
			await dropRec.SendDrop(args);
			Assert.AreEqual(element.GetStringValue(), result);
		}

		[TestCase(typeof(DatePicker), "12/12/2020 12:00:00 AM")]
		public void DateTextPackageCorrectlySetsOnCompatibleTarget(Type fieldType, string result)
		{
			var date = DateTime.Parse(result);
			result = date.ToString();
			TextPackageCorrectlySetsOnCompatibleTarget(fieldType, result);
		}

		[Test]
		public async Task HandledTest()
		{
			string testString = "test String";
			var dropTec = new DropGestureRecognizer() { AllowDrop = true };
			var element = new Label();
			element.Text = "Text Shouldn't change";
			var args = new DropEventArgs(new DataPackageView(new DataPackage() { Text = testString }));
			args.Handled = true;
			await dropTec.SendDrop(args);
			Assert.AreNotEqual(element.Text, testString);
		}
	}
}
