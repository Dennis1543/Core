﻿using System;
using Jamiras.DataModels;
using Jamiras.DataModels.Metadata;
using Jamiras.ViewModels.Converters;
using Jamiras.ViewModels.Fields;

namespace Jamiras.ViewModels.Grid
{
    public class IntegerColumnDefinition : GridColumnDefinition
    {
        public IntegerColumnDefinition(string header, ModelProperty sourceProperty, IntegerFieldMetadata metadata)
            : base(header, sourceProperty)
        {
            _metadata = metadata;
        }

        private readonly IntegerFieldMetadata _metadata;

        private static readonly IConverter _integerConverter = new DelegateConverter(ConvertInteger, null);

        private static string ConvertInteger(object c)
        {
            return c.ToString();
        }

        public static readonly ModelProperty SummarizationProperty =
            ModelProperty.Register(typeof(IntegerColumnDefinition), "Summarization", typeof(Summarization), Summarization.None, OnSummarizationChanged);

        /// <summary>
        /// Gets or sets the type of summarization to perform on this column.
        /// </summary>
        public Summarization Summarization
        {
            get { return (Summarization)GetValue(SummarizationProperty); }
            set { SetValue(SummarizationProperty, value); }
        }

        private static void OnSummarizationChanged(object sender, ModelPropertyChangedEventArgs e)
        {
            var column = (IntegerColumnDefinition)sender;
            switch ((Summarization)e.NewValue)
            {
                case Summarization.None:
                    column.SummarizeFunction = null;
                    break;
                case Summarization.Total:
                    column.SummarizeFunction = TotalIntegers;
                    break;
                default:
                    throw new NotSupportedException("No integer summarizer for " + e.NewValue);
            }
        }

        private static string TotalIntegers(System.Collections.IEnumerable values)
        {
            int total = 0;
            foreach (var f in values)
            {
                if (f is int)
                    total += (int)f;
            }

            return ConvertInteger(total);
        }

        protected override FieldViewModelBase CreateFieldViewModel(GridRowViewModel row)
        {
            if (IsReadOnly)
            {
                var textViewModel = new ReadOnlyTextFieldViewModel(Header);
                textViewModel.IsRightAligned = true;
                textViewModel.SetBinding(ReadOnlyTextFieldViewModel.TextProperty, new ModelBinding(row, SourceProperty, ModelBindingMode.OneWay, _integerConverter));
                return textViewModel;
            }

            var viewModel = new IntegerFieldViewModel(Header, _metadata);
            viewModel.BindValue(row, SourceProperty, ModelBindingMode.TwoWay);
            return viewModel;
        }
    }
}
