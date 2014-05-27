﻿using System;
using System.Collections.Generic;
using Jamiras.DataModels;
using Jamiras.ViewModels;
using Jamiras.ViewModels.Converters;
using NUnit.Framework;

namespace Jamiras.Core.Tests.ViewModels
{
    [TestFixture]
    class ViewModelBaseTests
    {
        private class TestModel : ModelBase
        {
            public static readonly ModelProperty StrProperty = ModelProperty.Register(typeof(TestModel), "Str", typeof(string), "Happy");

            public string Str
            {
                get { return (string)GetValue(StrProperty); }
                set { SetValue(StrProperty, value); }
            }

            public static readonly ModelProperty IntegerProperty = ModelProperty.Register(typeof(TestModel), "Integer", typeof(int), 0);

            public int Integer
            {
                get { return (int)GetValue(IntegerProperty); }
                set { SetValue(IntegerProperty, value); }
            }
        }

        private class TestViewModel : ViewModelBase
        {
            public static readonly ModelProperty TextProperty = ModelProperty.Register(typeof(TestViewModel), "Text", typeof(string), null);

            public string Text
            {
                get { return (string)GetValue(TextProperty); }
                set { SetValue(TextProperty, value); }
            }

            public static readonly ModelProperty IntegerProperty = ModelProperty.Register(typeof(TestViewModel), "Integer", typeof(int), 1);

            public int Integer
            {
                get { return (int)GetValue(IntegerProperty); }
                set { SetValue(IntegerProperty, value); }
            }
        }

        [SetUp]
        public void Setup()
        {
            _model = new TestModel();
            _viewModel = new TestViewModel();
        }

        private TestModel _model;
        private TestViewModel _viewModel;

        [Test]
        public void TestInitialization()
        {
            Assert.That(_viewModel.Text, Is.Null);
            Assert.That(_viewModel.Integer, Is.EqualTo(1));
        }

        [Test]
        public void TestSetBinding()
        {
            _model.Str = "Banana";
            Assert.That(_viewModel.Text, Is.Null);

            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.StrProperty));
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            _model.Str = "Strawberry";
            Assert.That(_viewModel.Text, Is.EqualTo("Strawberry"));
        }

        [Test]
        public void TestChangeBinding()
        {
            _model.Str = "Banana";
            Assert.That(_viewModel.Text, Is.Null);

            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.StrProperty));
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            var model2 = new TestModel { Str = "Apple" };
            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(model2, TestModel.StrProperty));
            Assert.That(_viewModel.Text, Is.EqualTo("Apple"));

            _model.Str = "Strawberry";
            Assert.That(_viewModel.Text, Is.EqualTo("Apple"));

            model2.Str = "Kiwi";
            Assert.That(_viewModel.Text, Is.EqualTo("Kiwi"));
        }

        [Test]
        public void TestClearBinding()
        {
            _model.Str = "Banana";
            Assert.That(_viewModel.Text, Is.Null);

            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.StrProperty));
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            _viewModel.SetBinding(TestViewModel.TextProperty, null);
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            _model.Str = "Strawberry";
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));
        }

        [Test]
        public void TestGetBinding()
        {
            var binding = new ModelBinding(_model, TestModel.StrProperty);
            _viewModel.SetBinding(TestViewModel.TextProperty, binding);

            Assert.That(_viewModel.GetBinding(TestViewModel.TextProperty), Is.SameAs(binding));
            Assert.That(_viewModel.GetBinding(TestViewModel.IntegerProperty), Is.Null);
        }

        [Test]
        public void TestBindingModeOneWay()
        {
            _model.Str = "Banana";
            Assert.That(_viewModel.Text, Is.Null);

            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.StrProperty, ModelBindingMode.OneWay));
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            _model.Str = "Strawberry";
            Assert.That(_viewModel.Text, Is.EqualTo("Strawberry"));

            var propertiesChanged = new List<string>();
            _model.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.Text = "Apple";
            Assert.That(_model.Str, Is.EqualTo("Strawberry"), "model should not have been updated");
            Assert.That(propertiesChanged, Has.No.Member("Str"), "model should not have been updated");

            _viewModel.Commit();
            Assert.That(_model.Str, Is.EqualTo("Strawberry"), "model should not have been committed");
            Assert.That(propertiesChanged, Has.No.Member("Str"), "model should not have been committed");
        }

        [Test]
        public void TestBindingModeTwoWay()
        {
            _model.Str = "Banana";
            Assert.That(_viewModel.Text, Is.Null);

            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.StrProperty, ModelBindingMode.TwoWay));
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            _model.Str = "Strawberry";
            Assert.That(_viewModel.Text, Is.EqualTo("Strawberry"));

            var propertiesChanged = new List<string>();
            _model.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.Text = "Apple";
            Assert.That(_model.Str, Is.EqualTo("Apple"), "model should have been updated");
            Assert.That(propertiesChanged, Has.Member("Str"), "model should have been updated");
        }

        [Test]
        public void TestBindingModeCommitted()
        {
            _model.Str = "Banana";
            Assert.That(_viewModel.Text, Is.Null);

            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.StrProperty, ModelBindingMode.Committed));
            Assert.That(_viewModel.Text, Is.EqualTo("Banana"));

            _model.Str = "Strawberry";
            Assert.That(_viewModel.Text, Is.EqualTo("Strawberry"));

            var propertiesChanged = new List<string>();
            _model.PropertyChanged += (o, e) => propertiesChanged.Add(e.PropertyName);

            _viewModel.Text = "Apple";
            Assert.That(_model.Str, Is.EqualTo("Strawberry"), "model should not have been updated");
            Assert.That(propertiesChanged, Has.No.Member("Str"), "model should not have been updated");

            _viewModel.Commit();
            Assert.That(_model.Str, Is.EqualTo("Apple"), "model should have been updated");
            Assert.That(propertiesChanged, Has.Member("Str"), "model should have been updated");
        }

        internal class NumberToStringConverter : IConverter
        {
            public string Convert(ref object value)
            {
                if (value is int)
                {
                    value = value.ToString();
                    return null;
                }

                return "only int supported";
            }

            public string ConvertBack(ref object value)
            {
                int iVal;
                if (Int32.TryParse(value.ToString(), out iVal))
                {
                    value = iVal;
                    return null;
                }

                return "parse error";
            }
        }

        [Test]
        public void TestBindingConverter()
        {
            _viewModel.SetBinding(TestViewModel.TextProperty, new ModelBinding(_model, TestModel.IntegerProperty, new NumberToStringConverter()));
            Assert.That(_viewModel.Text, Is.EqualTo("0"));

            _model.Integer = 99;
            Assert.That(_viewModel.Text, Is.EqualTo("99"));

            _viewModel.Text = "123";
            Assert.That(_model.Integer, Is.EqualTo(123));

            _viewModel.Text = "abc";
            Assert.That(_model.Integer, Is.EqualTo(123));
        }
    }
}
