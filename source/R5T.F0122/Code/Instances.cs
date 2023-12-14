using System;


namespace R5T.F0122
{
    public static class Instances
    {
        public static Z0000.ICharacters Characters => Z0000.Characters.Instance;
        public static F0000.IEnumerableOperator EnumerableOperator => F0000.EnumerableOperator.Instance;
        public static L0053.IExceptionOperator ExceptionOperator => L0053.ExceptionOperator.Instance;
        public static F0121.IKindMarkers KindMarkers => F0121.KindMarkers.Instance;
        public static F0121.IMemberNameOperator MemberNameOperator => F0121.MemberNameOperator.Instance;
        public static F0121.IParameterOperator ParameterOperator => F0121.ParameterOperator.Instance;
        public static IOperations Operations => F0122.Operations.Instance;
        public static L0065.ISignatureOperator SignatureOperator => L0065.SignatureOperator.Instance;
        public static L0065.ISignatureStringOperator SignatureStringOperator => L0065.SignatureStringOperator.Instance;
        public static F0000.IStringOperator StringOperator => F0000.StringOperator.Instance;
        public static F0000.IStrings Strings => F0000.Strings.Instance;
        public static L0053.ISwitchOperator SwitchOperator => L0053.SwitchOperator.Instance;
        public static F0000.ITextOperator TextOperator => F0000.TextOperator.Instance;
    }
}