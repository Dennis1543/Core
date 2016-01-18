﻿
namespace Jamiras.DataModels.Metadata
{
    public class StringFieldMetadata : FieldMetadata
    {
        public StringFieldMetadata(string fieldName, int maxLength, FieldAttributes attributes)
            : this(fieldName, maxLength)
        {
            Attributes = attributes;
        }

        public StringFieldMetadata(string fieldName, int maxLength)
            : base(fieldName, typeof(string))
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; private set; }

        public bool IsMultiline { get; set; }

        public override string Validate(ModelBase model, object value)
        {
            string strValue = value as string;
            if (strValue != null && strValue.Length > MaxLength)
                return "{0} cannot exceed " + MaxLength + " characters.";

            return base.Validate(model, value);
        }
    }
}
