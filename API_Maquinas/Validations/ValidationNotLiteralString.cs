using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.Validations
{
    public class ValidationNotLiteralString : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                return !string.Equals(str, "string", StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"El campo {name} no puede ser 'string'.";
        }
    }
}
