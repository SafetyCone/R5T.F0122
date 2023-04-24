using System;


namespace R5T.F0122
{
    public static class Instances
    {
        public static Z0000.ICharacters Characters => Z0000.Characters.Instance;
        public static F0000.IEnumerableOperator EnumerableOperator => F0000.EnumerableOperator.Instance;
        public static F0121.IMemberNameOperator MemberNameOperator => F0121.MemberNameOperator.Instance;
        public static IOperations Operations => F0122.Operations.Instance;
        public static F0000.IStringOperator StringOperator => F0000.StringOperator.Instance;
        public static F0000.IStrings Strings => F0000.Strings.Instance;
    }
}