using System.Text.RegularExpressions;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Customers;

public enum PersonType
{
    INDIVIDUAL,
    COMPANY
}

public sealed class TaxId : ValueObject
{
    public string Value { get; private set; } = null!;
    public PersonType PersonType { get; private set; }

    private TaxId() { } // required for EF Core materialization

    public TaxId(string input, PersonType personType)
    {
        var digits = OnlyDigits(input);
        PersonType = personType;

        if (personType == PersonType.INDIVIDUAL)
        {
            if (!IsValidCpf(digits))
                throw new DomainException("Invalid CPF.");
        }
        else
        {
            if (!IsValidCnpj(digits))
                throw new DomainException("Invalid CNPJ.");
        }

        Value = digits;
    }
    private static string OnlyDigits(string input)
    {
        return Regex.Replace(input, "[^0-9]", "");
    }

    // ================= CPF =================
    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        if (AllDigitsEqual(cpf))
            return false;

        var firstDigit = CalculateCpfDigit(cpf[..9], 10);
        var secondDigit = CalculateCpfDigit(cpf[..10], 11);

        return cpf.EndsWith($"{firstDigit}{secondDigit}");
    }

    private static int CalculateCpfDigit(string baseDigits, int weightStart)
    {
        var sum = 0;

        for (int i = 0; i < baseDigits.Length; i++)
        {
            sum += (baseDigits[i] - '0') * (weightStart - i);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    // ================= CNPJ =================
    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return false;

        if (AllDigitsEqual(cnpj))
            return false;

        var firstDigit = CalculateCnpjDigit(cnpj[..12]);
        var secondDigit = CalculateCnpjDigit(cnpj[..12] + firstDigit);

        return cnpj.EndsWith($"{firstDigit}{secondDigit}");
    }

    private static int CalculateCnpjDigit(string baseDigits)
    {
        int[] weights = baseDigits.Length == 12
            ? new[] { 5,4,3,2,9,8,7,6,5,4,3,2 }
            : new[] { 6,5,4,3,2,9,8,7,6,5,4,3,2 };

        var sum = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            sum += (baseDigits[i] - '0') * weights[i];
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static bool AllDigitsEqual(string value)
    {
        return value.All(c => c == value[0]);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return PersonType;
    }

    public override string ToString()
    {
        return Value;
    }
}