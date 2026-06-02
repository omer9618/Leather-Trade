using System;
using System.ComponentModel.DataAnnotations;

public class PastOrTodayDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null) return true;
        DateTime dateValue;
        if (DateTime.TryParse(value.ToString(), out dateValue))
        {
            return dateValue.Date <= DateTime.UtcNow.Date;
        }
        return false;
    }
} 