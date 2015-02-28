﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Jamiras.Controls
{
    public class Validation
    {
        public static readonly DependencyProperty ErrorTemplateProperty =
            DependencyProperty.RegisterAttached("ErrorTemplate", typeof(DataTemplate), typeof(Validation), new FrameworkPropertyMetadata(OnErrorTemplateChanged));

        public static DataTemplate GetErrorTemplate(FrameworkElement target)
        {
            return (DataTemplate)target.GetValue(ErrorTemplateProperty);
        }

        public static void SetErrorTemplate(FrameworkElement target, DataTemplate value)
        {
            target.SetValue(ErrorTemplateProperty, value);
        }

        private static void OnErrorTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            var adorner = GetAdorner(frameworkElement);
            if (adorner != null)
                adorner.ContentTemplate = (DataTemplate)e.NewValue;
        }

        private static readonly DependencyProperty AdornerProperty =
            DependencyProperty.RegisterAttached("Adorner", typeof(TemplatedAdorner), typeof(Validation));

        private static TemplatedAdorner GetAdorner(FrameworkElement target)
        {
            return (TemplatedAdorner)target.GetValue(AdornerProperty);
        }

        private static void SetAdorner(FrameworkElement target, TemplatedAdorner value)
        {
            target.SetValue(AdornerProperty, value);
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.RegisterAttached("IsValid", typeof(bool), typeof(Validation),
                new FrameworkPropertyMetadata(true, OnIsValidChanged));

        public static bool GetIsValid(FrameworkElement target)
        {
            return (bool)target.GetValue(IsValidProperty);
        }

        public static void SetIsValid(FrameworkElement target, bool value)
        {
            target.SetValue(IsValidProperty, value);
        }

        private static void OnIsValidChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;

            if ((bool)e.NewValue)
            {
                var adorner = GetAdorner(frameworkElement);
                if (adorner != null)
                    adorner.Visibility = Visibility.Collapsed;
            }
            else
            {
                var adorner = GetAdorner(frameworkElement);
                if (adorner == null)
                {
                    adorner = new TemplatedAdorner(frameworkElement, GetErrorTemplate(frameworkElement));
                    SetAdorner(frameworkElement, adorner);
                }

                adorner.Visibility = Visibility.Visible;
            }
        }

        private class TemplatedAdorner : Adorner
        {
            public TemplatedAdorner(FrameworkElement adornedElement, DataTemplate contentDataTemplate)
                : base(adornedElement)
            {
                _presenter = new ContentPresenter();
                _presenter.ContentTemplate = contentDataTemplate;
                _presenter.DataContext = adornedElement.DataContext;
                _presenter.Content = adornedElement.DataContext;

                var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
                adornerLayer.Add(this);

                AddVisualChild(_presenter);
                AddLogicalChild(_presenter);
            }

            private readonly ContentPresenter _presenter;

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            protected override System.Windows.Media.Visual GetVisualChild(int index)
            {
                return _presenter;
            }

            protected override System.Collections.IEnumerator LogicalChildren
            {
                get { yield return _presenter; }
            }

            protected override Size MeasureOverride(Size constraint)
            {
                return AdornedElement.RenderSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _presenter.Arrange(new Rect(new Point(0, 0), finalSize));
                return finalSize;
            }

            public DataTemplate ContentTemplate
            {
                get { return _presenter.ContentTemplate; }
                set { _presenter.ContentTemplate = value; }
            }
        }
    }
}
