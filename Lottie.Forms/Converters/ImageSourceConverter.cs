using System;
using System.Globalization;
using Xamarin.Forms;

namespace Lottie.Forms.Converters
{
	public class ImageSourceConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            return sourceType == typeof(string);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertFromInvariantString(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ConvertFromInvariantString(string value)
        {
            if (!(value is string text))
                return null;

            if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
            {
                return uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase) ? ImageSource.FromFile(uri.LocalPath) : ImageSource.FromUri(uri);
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                return ImageSource.FromFile(text);
            }

            throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(ImageSource)}");
        }
    }

}
