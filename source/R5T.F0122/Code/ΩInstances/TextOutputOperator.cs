using System;


namespace R5T.F0122
{
    public class TextOutputOperator : ITextOutputOperator
    {
        #region Infrastructure

        public static ITextOutputOperator Instance { get; } = new TextOutputOperator();


        private TextOutputOperator()
        {
        }

        #endregion
    }
}
