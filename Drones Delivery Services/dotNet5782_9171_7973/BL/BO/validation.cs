﻿using System;
using System.Text.RegularExpressions;

namespace BO
{
    static class Validation
    {
        internal static bool IsValidPhone(string phone)
        {
            foreach (char ch in phone)
            {
                if (ch == '-') continue;
                if (!char.IsDigit(ch)) return false;
            }
            return true;
        }

        internal static bool IsValidMail(string mail)
        {
            if (string.IsNullOrEmpty(mail)) return false;

            Regex mailRegex = new(@"^\w{4,}@gmail[.]com$");
            return mailRegex.IsMatch(mail);  
        }

        internal static bool IsValidLongitude(double longitude)
        {
            return longitude >= -180 && longitude <= 180;
        }

        internal static bool IsValidLatitude(double lat)
        {
            return lat >= -90 && lat <= 90;
        }

        internal static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= 3 || name.Length >= 15) return false;

            foreach (char ch in name)
            {
                if (ch == ' ') continue;
                if (!char.IsLetter(ch)) return false;
            }
            return true;
        }

        internal static bool IsValidModel(string model)
        {
            return !string.IsNullOrEmpty(model) && model.Length > 3 && model.Length < 15;
        }

        internal static bool IsValidBattery(double battery)
        {
            return battery >= 0 && battery <= 100;
        }

        internal static bool IsValidEnumOption<T>(int option)
        {
            return option >= 0 && option < Enum.GetValues(typeof(T)).Length;
        }
    }
}
