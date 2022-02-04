using Debex.Convert.Views.Pages;

namespace Debex.Convert.BL.Stages.CleanFormatsStage
{
    internal enum ResultCode
    {
        Correct,
        Corrected,
        Empty,
        Error,
        RepeatAfter
    }

    internal class FormatResult
    {
        public object Data { get; set; }
        public ResultCode Result { get; set; }
        public string Message { get; set; }


        public static FormatResult Correct(object data, string message = null) => new() { Result = ResultCode.Correct, Data = data, Message = message };
        public static FormatResult Corrected(object data, string message = null) => new() { Result = ResultCode.Corrected, Data = data, Message = message };
        public static FormatResult RepeatAfter(object data, string message = null) => new() { Result = ResultCode.RepeatAfter, Data = data, Message = message };

        public static FormatResult Empty() => new() { Result = ResultCode.Empty };
        public static FormatResult Error(object data, string message = null) => new() { Result = ResultCode.Error, Data = data, Message = message };
    }

}