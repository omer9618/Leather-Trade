using System;
using System.ComponentModel.DataAnnotations;

public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null) return true;
        DateTime dateValue;
        if (DateTime.TryParse(value.ToString(), out dateValue))
        {
            return dateValue.Date > DateTime.UtcNow.Date;
        }
        return false;
    }
} 