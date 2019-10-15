﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace HLab.Erp.Core.Wpf
{
    public class TextBoxAdvanced : TextBox
    {

        public TextBoxAdvanced()
        {
            SetBindingDefault();
        }

        public static readonly DependencyProperty DefaultBackgroundProperty = DependencyProperty.Register(nameof(DefaultBackground), typeof(object), typeof(TextBoxAdvanced), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public static readonly DependencyProperty DefaultForegroundProperty = DependencyProperty.Register(nameof(DefaultForeground), typeof(object), typeof(TextBoxAdvanced), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public static readonly DependencyProperty EditBackgroundProperty = DependencyProperty.Register(nameof(EditBackground), typeof(object), typeof(TextBoxAdvanced), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public static readonly DependencyProperty EditForegroundProperty = DependencyProperty.Register(nameof(EditForeground), typeof(object), typeof(TextBoxAdvanced), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        [Bindable(true)]
        [Category("Appearance")]
        public object DefaultBackground
        {
            get => (Brush)GetValue(DefaultBackgroundProperty); set => SetValue(DefaultBackgroundProperty, value);
        }

        [Bindable(true)]
        [Category("Appearance")]
        public object DefaultForeground
        {
            get => (Brush)GetValue(DefaultForegroundProperty); set => SetValue(DefaultForegroundProperty, value);
        }

        [Bindable(true)]
        [Category("Appearance")]
        public object EditBackground
        {
            get => (Brush)GetValue(EditBackgroundProperty); set => SetValue(EditBackgroundProperty, value);
        }

        [Bindable(true)]
        [Category("Appearance")]
        public object EditForeground
        {
            get => (Brush)GetValue(EditForegroundProperty); set => SetValue(EditForegroundProperty, value);
        }

        private void SetBindingEdit()
        {
            var b = new Binding(nameof(EditBackground)) {Source = this};
            var f = new Binding(nameof(EditForeground)) {Source = this};
            BindingOperations.SetBinding(this, BackgroundProperty, b);
            BindingOperations.SetBinding(this, ForegroundProperty, f);    
        }

        private void SetBindingDefault()
        {
            var b = new Binding(nameof(DefaultBackground)) { Source = this };
            var f = new Binding(nameof(DefaultForeground)) { Source = this };
            BindingOperations.SetBinding(this, BackgroundProperty, b);
            BindingOperations.SetBinding(this, ForegroundProperty, f);           
        }

         protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            SetBindingEdit();
            base.OnGotKeyboardFocus(e);
        }
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            SetBindingDefault();
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            var pos = CaretIndex;

            var v = Text;

            var v2 = v.Replace(">=", "≥")
            .Replace("<=", "≤")
            .Replace("+-", "±")
            .Replace("!=", "≠")
            .Replace("~=", "≈")
            .Replace("/8", "∞");
            Text = v2;

            CaretIndex = pos - (v.Length - v2.Length);

            base.OnTextChanged(e);
        }
    }
}
