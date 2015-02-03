﻿using System;
using System.Collections.Generic;
using Jamiras.DataModels;
using Jamiras.DataModels.Metadata;

namespace Jamiras.ViewModels.Fields
{
    public class AutoCompleteFieldViewModel : TextFieldViewModel
    {
        public AutoCompleteFieldViewModel(string label, StringFieldMetadata metadata, Func<string, IEnumerable<LookupItem>> searchFunction, Func<int, string> lookupLabelFunction)
            : base(label, metadata)
        {
            IsTextBindingDelayed = true;

            _searchFunction = searchFunction;
            _lookupLabelFunction = lookupLabelFunction;

            // subscribe to our own PropertyChanged so we can benefit from the IsTextBindingDelayed
            AddPropertyChangedHandler(TextProperty, OnTextChanged);
        }

        private readonly Func<string, IEnumerable<LookupItem>> _searchFunction;
        private readonly Func<int, string> _lookupLabelFunction;
        private string _searchText;
        private bool _searchDisabled, _searchPending;

        protected override void OnModelPropertyChanged(ModelPropertyChangedEventArgs e)
        {
            if (e.Property == TextFieldViewModel.TextProperty && !_searchDisabled)
                _searchPending = true;

            base.OnModelPropertyChanged(e);
        }

        private void OnTextChanged(object sender, ModelPropertyChangedEventArgs e)
        {
            if (_searchPending)
            {
                _searchPending = false;
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            lock (_searchFunction)
            {
                if (_searchText == Text)
                    return;

                _searchText = Text;
            }

            var suggestions = String.IsNullOrEmpty(_searchText) ? null : _searchFunction(_searchText);

            lock (_searchFunction)
            {
                if (_searchText == Text)
                    SetValue(SuggestionsProperty, suggestions);
            }
        }

        public static readonly ModelProperty SuggestionsProperty = 
            ModelProperty.Register(typeof(AutoCompleteFieldViewModel), "Suggestions", typeof(IEnumerable<LookupItem>), null);

        /// <summary>
        /// Gets the suggestions matching the current Text value.
        /// </summary>
        public IEnumerable<LookupItem> Suggestions
        {
            get { return (IEnumerable<LookupItem>)GetValue(SuggestionsProperty); }
        }

        public static readonly ModelProperty SelectedIdProperty =
            ModelProperty.Register(typeof(AutoCompleteFieldViewModel), "SelectedId", typeof(int), 0, OnSelectedIdChanged);

        /// <summary>
        /// Gets or sets the unique identifier of the matching item.
        /// </summary>
        public int SelectedId
        {
            get { return (int)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        private static void OnSelectedIdChanged(object sender, ModelPropertyChangedEventArgs e)
        {
            var vm = (AutoCompleteFieldViewModel)sender;
            var id = (int)e.NewValue;

            if (vm.Suggestions != null)
            {
                foreach (var lookupItem in vm.Suggestions)
                {
                    if (lookupItem.Id == id)
                    {
                        vm.SetTextInternal(lookupItem.Label);
                        return;
                    }
                }
            }

            if (id != 0)
            {
                var label = vm._lookupLabelFunction(id);
                vm.SetTextInternal(label);
            }
        }

        private void SetTextInternal(string value)
        {
            _searchDisabled = true;
            try
            {
                _searchPending = false;
                SetValue(TextProperty, value);
            }
            finally
            {
                _searchDisabled = false;
            }
        }

        public static readonly ModelProperty IsMatchRequiredProperty =
            ModelProperty.Register(typeof(FieldViewModelBase), "IsMatchRequired", typeof(bool), false);

        /// <summary>
        /// Gets or sets whether an exact match is required.
        /// </summary>
        public bool IsMatchRequired
        {
            get { return (bool)GetValue(IsMatchRequiredProperty); }
            set { SetValue(IsMatchRequiredProperty, value); }
        }

        protected override string Validate(ModelProperty property, object value)
        {
            if (property == TextProperty && IsMatchRequired)
            {
                if (SelectedId == 0 && !String.IsNullOrEmpty(Text))
                    return String.Format("{0} is not a matching value.", LabelWithoutAccelerators);
            }

            return base.Validate(property, value);
        }

    }
}
