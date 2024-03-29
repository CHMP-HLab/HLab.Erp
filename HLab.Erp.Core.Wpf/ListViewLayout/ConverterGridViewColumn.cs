// -- FILE ------------------------------------------------------------------
// name       : ConverterGridViewColumn.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace HLab.Erp.Core.ListViewLayout
{

	// ------------------------------------------------------------------------
	public abstract class ConverterGridViewColumn : GridViewColumn, IValueConverter
	{

		// ----------------------------------------------------------------------
		protected ConverterGridViewColumn( Type bindingType )
		{
			if ( bindingType == null )
			{
				throw new ArgumentNullException( "bindingType" );
			}

			this.bindingType = bindingType;

			// binding
			Binding binding = new Binding();
			binding.Mode = BindingMode.OneWay;
			binding.Converter = this;
			DisplayMemberBinding = binding;
		} // ConverterGridViewColumn

		// ----------------------------------------------------------------------
		public Type BindingType
		    // BindingType
		    => bindingType;

	    // ----------------------------------------------------------------------
		protected abstract object ConvertValue( object value );

		// ----------------------------------------------------------------------
		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( !bindingType.IsInstanceOfType( value ) )
			{
				throw new InvalidOperationException();
			}
			return ConvertValue( value );
		} // IValueConverter.Convert

		// ----------------------------------------------------------------------
		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		} // IValueConverter.ConvertBack

		// ----------------------------------------------------------------------
		// members
        readonly Type bindingType;

	} // class ConverterGridViewColumn

} // namespace Itenso.Windows.Controls.ListViewLayout
// -- EOF -------------------------------------------------------------------
