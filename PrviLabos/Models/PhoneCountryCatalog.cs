namespace PrviLabos.Models;

public sealed record PhoneCountryOption(string DialCode, string CountryName, string FlagEmoji)
{
    public string DisplayText => $"{FlagEmoji} {CountryName} (+{DialCode})";
}

public static class PhoneCountryCatalog
{
    public static readonly IReadOnlyList<PhoneCountryOption> Options = new[]
    {
        new PhoneCountryOption("43", "Austrija", "🇦🇹"),
        new PhoneCountryOption("355", "Albanija", "🇦🇱"),
        new PhoneCountryOption("376", "Andora", "🇦🇩"),
        new PhoneCountryOption("32", "Belgija", "🇧🇪"),
        new PhoneCountryOption("387", "Bosna i Hercegovina", "🇧🇦"),
        new PhoneCountryOption("359", "Bugarska", "🇧🇬"),
        new PhoneCountryOption("385", "Hrvatska", "🇭🇷"),
        new PhoneCountryOption("420", "Češka", "🇨🇿"),
        new PhoneCountryOption("45", "Danska", "🇩🇰"),
        new PhoneCountryOption("372", "Estonija", "🇪🇪"),
        new PhoneCountryOption("358", "Finska", "🇫🇮"),
        new PhoneCountryOption("33", "Francuska", "🇫🇷"),
        new PhoneCountryOption("49", "Njemačka", "🇩🇪"),
        new PhoneCountryOption("30", "Grčka", "🇬🇷"),
        new PhoneCountryOption("353", "Irska", "🇮🇪"),
        new PhoneCountryOption("354", "Island", "🇮🇸"),
        new PhoneCountryOption("39", "Italija", "🇮🇹"),
        new PhoneCountryOption("381", "Srbija", "🇷🇸"),
        new PhoneCountryOption("371", "Latvija", "🇱🇻"),
        new PhoneCountryOption("370", "Litva", "🇱🇹"),
        new PhoneCountryOption("352", "Luksemburg", "🇱🇺"),
        new PhoneCountryOption("36", "Mađarska", "🇭🇺"),
        new PhoneCountryOption("356", "Malta", "🇲🇹"),
        new PhoneCountryOption("31", "Nizozemska", "🇳🇱"),
        new PhoneCountryOption("47", "Norveška", "🇳🇴"),
        new PhoneCountryOption("48", "Poljska", "🇵🇱"),
        new PhoneCountryOption("351", "Portugal", "🇵🇹"),
        new PhoneCountryOption("40", "Rumunjska", "🇷🇴"),
        new PhoneCountryOption("421", "Slovačka", "🇸🇰"),
        new PhoneCountryOption("386", "Slovenija", "🇸🇮"),
        new PhoneCountryOption("34", "Španjolska", "🇪🇸"),
        new PhoneCountryOption("46", "Švedska", "🇸🇪"),
        new PhoneCountryOption("41", "Švicarska", "🇨🇭"),
        new PhoneCountryOption("90", "Turska", "🇹🇷"),
        new PhoneCountryOption("44", "Ujedinjeno Kraljevstvo", "🇬🇧"),
        new PhoneCountryOption("1", "Sjedinjene Američke Države", "🇺🇸")
    };

    public static string DefaultDialCode => "385";

    public static string ComposePhoneNumber(string dialCode, string localNumber)
    {
        return $"+{NormalizeDigits(dialCode)} {NormalizeDigits(localNumber)}".Trim();
    }

    public static bool TrySplitPhoneNumber(string? phoneNumber, out string dialCode, out string localNumber)
    {
        dialCode = DefaultDialCode;
        localNumber = string.Empty;

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var normalized = NormalizeDigits(phoneNumber);
        foreach (var option in Options.OrderByDescending(option => option.DialCode.Length))
        {
            if (!normalized.StartsWith(option.DialCode, StringComparison.Ordinal) || normalized.Length <= option.DialCode.Length)
            {
                continue;
            }

            dialCode = option.DialCode;
            localNumber = normalized[option.DialCode.Length..];
            return true;
        }

        if (normalized.StartsWith(DefaultDialCode, StringComparison.Ordinal) && normalized.Length > DefaultDialCode.Length)
        {
            dialCode = DefaultDialCode;
            localNumber = normalized[DefaultDialCode.Length..];
            return true;
        }

        localNumber = normalized;
        return localNumber.Length > 0;
    }

    private static string NormalizeDigits(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        var index = 0;

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                buffer[index++] = character;
            }
        }

        return new string(buffer[..index]);
    }
}