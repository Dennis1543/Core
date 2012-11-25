﻿using System.Collections.Generic;
using Jamiras.Services;
using Jamiras.ViewModels;
using Moq;
using NUnit.Framework;

namespace Jamiras.Core.Tests.ViewModels
{
    [TestFixture]
    class DialogViewModelBaseTests
    {
        [SetUp]
        public void Setup()
        {
            _dialogService = new Mock<IDialogService>();
            _viewModel = new TestViewModel(_dialogService.Object);
        }

        private Mock<IDialogService> _dialogService;
        private TestViewModel _viewModel;

        private class TestViewModel : DialogViewModelBase
        {
            public TestViewModel(IDialogService dialogService)
                : base(dialogService)
            {
            }
        }

        [Test]
        public void TestInitialization()
        {
            Assert.That(_viewModel.DialogTitle, Is.Null);
            Assert.That(_viewModel.DialogResult, Is.EqualTo(DialogResult.None));
            Assert.That(_viewModel.OkButtonText, Is.EqualTo("OK"));
            Assert.That(_viewModel.IsCancelButtonVisible, Is.True);
        }

        [Test]
        public void TestDialogTitle()
        {
            List<string> propertiesChanged = new List<string>();
            _viewModel.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.DialogTitle = "Title";
            Assert.That(_viewModel.DialogTitle, Is.EqualTo("Title"));
            Assert.That(propertiesChanged, Contains.Item("DialogTitle"));
        }

        [Test]
        public void TestOkButtonText()
        {
            List<string> propertiesChanged = new List<string>();
            _viewModel.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.OkButtonText = "Save";
            Assert.That(_viewModel.OkButtonText, Is.EqualTo("Save"));
            Assert.That(propertiesChanged, Contains.Item("OkButtonText"));
        }

        [Test]
        public void TestOkCommand()
        {
            List<string> propertiesChanged = new List<string>();
            _viewModel.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.OkCommand.Execute();
            Assert.That(_viewModel.DialogResult, Is.EqualTo(DialogResult.OK));
            Assert.That(propertiesChanged, Contains.Item("DialogResult"));
        }

        private class TestViewModelWithErrors : DialogViewModelBase
        {
            public TestViewModelWithErrors(IDialogService dialogService)
                : base(dialogService)
            {
                AddValidation("Foo", ValidateFoo);
            }

            private string ValidateFoo()
            {
                return "Foo is invalid.";
            }
        }

        [Test]
        public void TestOkCommandWithErrors()
        {
            var viewModel = new TestViewModelWithErrors(_dialogService.Object);
            var error = viewModel.Validate();

            bool errorShown = false;
            _dialogService.Setup(d => d.ShowDialog(It.IsAny<MessageBoxViewModel>())).
                Returns((MessageBoxViewModel mbvm) =>
                    {
                        errorShown = true;
                        Assert.That(mbvm.Message, Is.EqualTo(error));
                        return DialogResult.OK;
                    });

            viewModel.OkCommand.Execute();

            Assert.That(errorShown, Is.True, "Error not shown");
            Assert.That(_viewModel.DialogResult, Is.EqualTo(DialogResult.None));
        }

        [Test]
        public void TestCancelCommand()
        {
            List<string> propertiesChanged = new List<string>();
            _viewModel.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.CancelCommand.Execute();
            Assert.That(_viewModel.DialogResult, Is.EqualTo(DialogResult.Cancel));
            Assert.That(propertiesChanged, Contains.Item("DialogResult"));
        }

        [Test]
        public void TestIsCancelButtonVisible()
        {
            List<string> propertiesChanged = new List<string>();
            _viewModel.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.IsCancelButtonVisible = false;
            Assert.That(_viewModel.IsCancelButtonVisible, Is.False);
            Assert.That(propertiesChanged, Contains.Item("IsCancelButtonVisible"));
        }

        [Test]
        public void TestShowDialogOk()
        {
            _dialogService.Setup(d => d.ShowDialog(_viewModel)).Returns(DialogResult.OK);
            Assert.That(_viewModel.ShowDialog(), Is.EqualTo(DialogResult.OK));
            _dialogService.Verify(d => d.ShowDialog(_viewModel));
        }

        [Test]
        public void TestShowDialogCancel()
        {
            _dialogService.Setup(d => d.ShowDialog(_viewModel)).Returns(DialogResult.Cancel);
            Assert.That(_viewModel.ShowDialog(), Is.EqualTo(DialogResult.Cancel));
            _dialogService.Verify(d => d.ShowDialog(_viewModel));
        }
    }
}
