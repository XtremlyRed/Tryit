using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest.Command
{
    [TestClass]
    public class BindingCommandTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            // Reset global exception handler before each test
            BindingCommand.SetGlobalCommandExceptionCallback(null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullExecute_ThrowsArgumentNullException()
        {
            // Arrange & Act
            new BindingCommand(null!);
        }

        [TestMethod]
        public void Execute_ActionIsCalled()
        {
            // Arrange
            var executed = false;
            Action executeAction = () => executed = true;
            var command = new BindingCommand(executeAction);

            // Act
            command.Execute();

            // Assert
            Assert.IsTrue(executed, "The execute action should have been called.");
        }

        [TestMethod]
        public void CanExecute_NoPredicate_ReturnsTrue()
        {
            // Arrange
            var command = new BindingCommand(() => { });

            // Act
            var canExecute = command.CanExecute();

            // Assert
            Assert.IsTrue(
                canExecute,
                "CanExecute should return true when no predicate is provided."
            );
        }

        [TestMethod]
        public void CanExecute_WithPredicate_ReturnsPredicateValue()
        {
            // Arrange
            var canExecutePredicate = false;
            var command = new BindingCommand(() => { }, () => canExecutePredicate);

            // Act & Assert
            Assert.IsFalse(
                command.CanExecute(),
                "CanExecute should return the predicate's value (false)."
            );

            canExecutePredicate = true;
            Assert.IsTrue(
                command.CanExecute(),
                "CanExecute should return the predicate's value (true)."
            );
        }

        [TestMethod]
        public void IsExecuting_ChangesDuringExecution()
        {
            // Arrange
            var mre = new ManualResetEvent(false);
            var command = new BindingCommand(() => mre.WaitOne(100));
            var canExecuteChangedCount = 0;
            command.CanExecuteChanged += (s, e) => canExecuteChangedCount++;

            // Assert initial state
            Assert.IsFalse(command.IsExecuting);

            // Act
            var task = System.Threading.Tasks.Task.Run(() => command.Execute());

            // Assert during execution
            System.Threading.Tasks.Task.Delay(20).Wait(); // Give time for the command to start
            Assert.IsTrue(command.IsExecuting, "IsExecuting should be true during execution.");
            Assert.IsFalse(command.CanExecute(), "CanExecute should be false during execution.");

            mre.Set();
            task.Wait();

            // Assert after execution
            Assert.IsFalse(command.IsExecuting, "IsExecuting should be false after execution.");
            Assert.IsTrue(command.CanExecute(), "CanExecute should be true after execution.");
            Assert.AreEqual(2, canExecuteChangedCount, "CanExecuteChanged should be raised twice.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_ThrowsException_NoGlobalHandler_Rethrows()
        {
            // Arrange
            var command = new BindingCommand(() => throw new InvalidOperationException());

            // Act
            command.Execute();
        }

        [TestMethod]
        public void Execute_ThrowsException_WithGlobalHandler_HandlerIsCalled()
        {
            // Arrange
            Exception? caughtException = null;
            var expectedException = new InvalidOperationException("Test Exception");
            BindingCommand.SetGlobalCommandExceptionCallback(ex => caughtException = ex);
            var command = new BindingCommand(() => throw expectedException);

            // Act
            command.Execute();

            // Assert
            Assert.IsNotNull(caughtException, "Global exception handler should have been called.");
            Assert.AreSame(
                expectedException,
                caughtException,
                "The correct exception should be passed to the handler."
            );
        }

        [TestMethod]
        public void Execute_ThrowsException_IsExecutingResets()
        {
            // Arrange
            var command = new BindingCommand(() => throw new Exception());
            BindingCommand.SetGlobalCommandExceptionCallback(ex => { }); // Suppress rethrow

            // Act
            command.Execute();

            // Assert
            Assert.IsFalse(
                command.IsExecuting,
                "IsExecuting should be reset to false even if an exception occurs."
            );
        }

        [TestMethod]
        public void ImplicitOperator_FromAction_CreatesCommand()
        {
            // Arrange
            var executed = false;
            Action action = () => executed = true;

            // Act
            BindingCommand command = action;
            command.Execute();

            // Assert
            Assert.IsNotNull(command);
            Assert.IsTrue(executed, "The action converted to a command should execute.");
        }
    }
}
